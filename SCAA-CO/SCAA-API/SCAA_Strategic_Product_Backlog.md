# SCAA-API — Strategic Product Backlog & Business Analysis

**Analyst:** Senior Product Owner / Business Systems Analyst  
**Date:** 2026-05-23  
**Codebase:** .NET 10 Web API — SCAA (Supply Chain & Commerce Analytics Application)  
**Stack:** EF Core · SQL Server · Mapster · Swagger (JWT configured)

---

## Executive Summary

SCAA-API is an e-commerce and supply chain management backend covering **7 core entities**: Category, Customer, Inventory, Order, OrderItem, Product, and Supplier. The repository layer is well-structured with a solid generic base, but **the system currently has zero controllers, zero DTOs, zero service layer, broken mapping config, and no authentication enforcement** — meaning no business logic has been implemented yet. This is the ideal moment to shape the product correctly before the frontend is built.

The analysis below identifies **critical data integrity bugs**, **missing business workflows**, and **20 ranked strategic tasks** that collectively transform this foundation into a production-grade, revenue-generating platform.

---

## 🔴 Critical Bugs Discovered During Analysis

These must be resolved before any feature work begins.

### BUG-05 — No Authentication Enforcement Despite JWT Being Configured
**Severity:** High — Swagger is configured with Bearer token security, but `UseAuthentication()` is missing from `Program.cs` and there is no User/Role entity.  
**Fix:** Add Identity or a custom auth entity, add `UseAuthentication()`, and protect endpoints with `[Authorize]`.

---

## Domain Model Map

```
Supplier ──< Product >──── Category
                │
                ▼
           Inventory  (AvailableQty per Product)
                │
           Product ──< OrderItem >── Order ──── Customer
```

**Identified Gaps in Domain Model:**
- No `Payment` entity (no payment status, method, or transaction reference)
- No `ShipmentTracking` entity (no address, carrier, or tracking number)
- No `ProductReview` entity (no customer feedback loop)
- No `Discount/Coupon` entity (Discount is a raw field with no rule engine)
- No `AuditLog` entity (no change history)
- No `Notification` entity (no customer communication)
- `Supplier` has only a name — no contact info, address, or lead time

---

## Strategic Product Backlog — 20 Tasks

Tasks are ranked by business value: **Critical → High → Medium → Low**.

---

### TASK-01 — Implement Order Lifecycle State Machine

**Category:** Operational Efficiency / Strategic Feature

#### Business Problem
`Order.Status` is an unconstrained `varchar(50)`. Any string can be saved. There are no rules about which transitions are legal (e.g., can a Delivered order go back to Pending?). There is no timestamp capturing when each status change occurred. The business has no operational visibility into order flow.

#### Proposed Solution
Replace the raw string with a proper `OrderStatus` enum: `Pending → Processing → Shipped → Delivered | Cancelled | Refunded`. Add an `OrderStatusHistory` table to log every transition with a timestamp and optional operator note. Enforce transition rules in a service layer.

#### Business Value
**High** — This is the heartbeat of an e-commerce business. Without controlled order states, fulfilment teams have no reliable workflow and customers get no accurate status updates.

#### Tables Used
- `Orders`
- `OrderStatusHistory` *(new)*

#### API Services Impacted
- OrderService *(new)*
- OrderController *(new)*

#### Required Business Logic
- Valid transitions matrix: Pending → Processing, Processing → Shipped, Shipped → Delivered, any non-Delivered → Cancelled
- Block illegal transitions and return a 422 with a descriptive error
- Auto-stamp `StatusChangedAt` on every transition
- Record `ChangedByUserId` (operator) for audit purposes

#### Suggested DB Changes
```sql
ALTER TABLE Orders ADD COLUMN StatusChangedAt DATETIME NULL;
CREATE TABLE OrderStatusHistory (
  Id INT IDENTITY PRIMARY KEY,
  OrderId INT NOT NULL REFERENCES Orders(Id),
  PreviousStatus VARCHAR(50),
  NewStatus VARCHAR(50),
  ChangedAt DATETIME NOT NULL DEFAULT GETDATE(),
  Note VARCHAR(500),
  ChangedByUserId INT NULL
);
```

#### Suggested Endpoints
- `PATCH /api/orders/{id}/status` — transition to new status
- `GET /api/orders/{id}/status-history` — view full state timeline

#### Acceptance Criteria
- [ ] Only valid status transitions are accepted; invalid transitions return 422
- [ ] Every status change is recorded in `OrderStatusHistory`
- [ ] `GET /api/orders/{id}/status-history` returns the full timeline
- [ ] Cancelled orders cannot be transitioned to any other state
- [ ] `StatusChangedAt` is always set on the `Order` row

#### Priority: **Critical**
#### Complexity: **Medium**

---

### TASK-02 — Inventory Deduction Engine on Order Placement

**Category:** Process Automation / Data Quality

#### Business Problem
When an order is placed, `Inventory.AvailableQuantity` is never automatically reduced. A customer can place 100 orders for a product with only 1 unit in stock. There is no stock reservation, no oversell prevention, and no out-of-stock guard.

#### Proposed Solution
Implement an atomic inventory deduction service that fires when an order transitions from `Pending` to `Processing`. Before accepting an order, validate that `Inventory.AvailableQuantity >= OrderItem.Quantity` for every line item. Use a database transaction to prevent race conditions. Restore stock when an order is Cancelled.

#### Business Value
**High** — Overselling is a direct financial liability and a customer experience failure. Stock accuracy is foundational to every other inventory feature.

#### Tables Used
- `Inventories`
- `OrderItems`
- `Orders`

#### API Services Impacted
- InventoryService *(new)*
- OrderService *(new)*

#### Required Business Logic
- Pre-order stock check: `Inventory.AvailableQuantity >= requested quantity` (per line item)
- Atomic deduction wrapped in a `DbTransaction`
- On order cancellation, restore `AvailableQuantity`
- Raise a low-stock event when `AvailableQuantity` falls below a configurable threshold

#### Suggested Endpoints
- `GET /api/inventory/low-stock?threshold=10` — list products below threshold
- `POST /api/inventory/{productId}/restock` — add stock (admin only)

#### Acceptance Criteria
- [ ] Placing an order reduces `Inventory.AvailableQuantity` for each line item
- [ ] An order with insufficient stock is rejected with a 409 Conflict
- [ ] Cancelling an order restores stock
- [ ] The deduction is atomic — no partial deductions on multi-item orders
- [ ] A restock endpoint allows authorized users to replenish inventory

#### Priority: **Critical**
#### Complexity: **Medium**

---

### TASK-03 — OrderAmount Auto-Calculation & Validation

**Category:** Data Quality / Business Intelligence

#### Business Problem
`Order.OrderAmount` is stored as a raw decimal with no enforced relationship to `OrderItems`. In the seed data, Order 1 has `OrderAmount = 199.97` and its items are `(2 × 79.99) + (5 × 12.99) = 224.93` — a discrepancy of $24.96. This means revenue figures in any report will be wrong. The `Discount` field relationship to the final amount is also undefined.

#### Proposed Solution
Remove `OrderAmount` as an input field. Make it a **computed, server-side derived value**: `SUM(OrderItem.Quantity × OrderItem.PricePerUnit) × (1 - DiscountPercentage/100)`. Expose it as a read-only property on the Order DTO. Add a DB constraint or computed column for integrity.

#### Business Value
**High** — Every revenue report, margin analysis, and customer invoice depends on this being correct. Incorrect totals are a compliance and trust risk.

#### Tables Used
- `Orders`
- `OrderItems`

#### API Services Impacted
- OrderService *(new)*
- ReportingService *(new)*

#### Required Business Logic
- On `SaveChanges`, recalculate `OrderAmount = SUM(Quantity × PricePerUnit) × (1 - Discount/100)`
- `PricePerUnit` on `OrderItem` must be locked at the product's price at the time of ordering (not the current price — prices change)
- Expose a `SubTotal`, `DiscountAmount`, and `FinalTotal` breakdown on the Order DTO

#### Suggested Endpoints
- `GET /api/orders/{id}` — returns recalculated totals on every fetch
- `GET /api/orders/{id}/invoice` — formatted invoice with line items

#### Acceptance Criteria
- [ ] `OrderAmount` can never be manually set via API input
- [ ] `OrderAmount` always equals the sum of line items after discount
- [ ] `PricePerUnit` on `OrderItem` is the price at time of order, unaffected by later price changes
- [ ] The invoice endpoint returns a structured breakdown
- [ ] Existing seed data discrepancy is flagged in a data repair script

#### Priority: **Critical**
#### Complexity: **Medium**

---

### TASK-04 — Add Audit Fields to All Entities

**Category:** Compliance & Audit

#### Business Problem
No entity has `CreatedAt`, `UpdatedAt`, `CreatedBy`, or `UpdatedBy` fields. There is no way to know when a record was created, last modified, or by whom. This fails basic compliance requirements (GDPR, SOX, financial auditing) and makes debugging production incidents impossible.

#### Proposed Solution
Create an `IAuditable` interface with `CreatedAt`, `UpdatedAt`, `CreatedByUserId`, `UpdatedByUserId`. Apply to all 7 entities. Override `SaveChangesAsync` in `ApplicationDbContext` to auto-populate these fields before every save.

#### Business Value
**High** — Audit trails are non-negotiable for any production system handling financial transactions and personal customer data.

#### Tables Used
- `Categories`, `Customers`, `Inventories`, `Orders`, `OrderItems`, `Products`, `Suppliers`

#### Suggested DB Changes
```sql
-- Add to ALL tables:
CreatedAt    DATETIME NOT NULL DEFAULT GETDATE(),
UpdatedAt    DATETIME NULL,
CreatedBy    VARCHAR(100) NULL,
UpdatedBy    VARCHAR(100) NULL
```

#### Suggested Endpoints
- `GET /api/audit/orders/{id}` — full change log for an order

#### Acceptance Criteria
- [ ] All 7 entities have `CreatedAt` and `UpdatedAt`
- [ ] `CreatedAt` is set on insert and never updated
- [ ] `UpdatedAt` is set on every update automatically via `SaveChangesAsync` override
- [ ] Existing records have `CreatedAt` backfilled with a migration default

#### Priority: **Critical**
#### Complexity: **Low**

---

### TASK-05 — Authentication & Role-Based Authorization

**Category:** Strategic Feature / Compliance

#### Business Problem
Swagger is configured with JWT Bearer token security, but `UseAuthentication()` is not in the middleware pipeline and there is no `User`, `Role`, or Identity entity. Every future endpoint will be publicly accessible without auth. Customer data, pricing, and inventory are all exposed.

#### Proposed Solution
Add ASP.NET Core Identity or a lightweight custom auth system. Define roles: `Admin`, `WarehouseManager`, `SalesAgent`, `Customer` (self-service). Add `UseAuthentication()` and `UseAuthorization()` to the pipeline. Protect all endpoints with role-appropriate `[Authorize]` attributes.

#### Business Value
**High** — No product can go to production without authentication. This also enables per-role dashboards, customer self-service, and per-user audit trails.

#### Tables Used
- `Users` *(new)*
- `Roles` *(new)*
- `UserRoles` *(new)*

#### Suggested Endpoints
- `POST /api/auth/register`
- `POST /api/auth/login` — returns JWT
- `POST /api/auth/refresh`
- `GET /api/auth/me`

#### Acceptance Criteria
- [ ] Unauthenticated requests to protected endpoints return 401
- [ ] Unauthorized role access returns 403
- [ ] JWT token contains `userId`, `email`, `role` claims
- [ ] Admin role can access all endpoints
- [ ] Customer role can only access their own orders and profile

#### Priority: **Critical**
#### Complexity: **High**

---

### TASK-06 — Real-Time Inventory Dashboard (KPI)

**Category:** Business Intelligence

#### Business Problem
There is no business-facing view of inventory health. Management cannot see which products are running low, which are overstocked, or which product-supplier combinations are at risk. `Product.Quantity` vs `Inventory.AvailableQuantity` mismatch means even basic stock figures are unreliable.

#### Proposed Solution
Build an Inventory Intelligence API that aggregates stock levels, sell-through rates, and days-of-stock-remaining per product and category. Include supplier lead time (once Supplier is enriched with contact fields).

#### Business Value
**High** — Inventory mismanagement is one of the top causes of revenue loss in e-commerce. A single stockout on a high-velocity product is immediately measurable in lost sales.

#### Tables Used
- `Inventories`
- `Products`
- `Categories`
- `OrderItems`
- `Suppliers`

#### Suggested Endpoints
- `GET /api/dashboard/inventory-summary` — total SKUs, low-stock count, out-of-stock count
- `GET /api/dashboard/inventory/by-category` — breakdown per category
- `GET /api/inventory/low-stock?threshold={n}` — configurable threshold
- `GET /api/inventory/out-of-stock` — zero-quantity products
- `GET /api/inventory/velocity` — units sold per product in last 30/60/90 days

#### Acceptance Criteria
- [ ] Dashboard returns accurate counts of low-stock and out-of-stock products
- [ ] Velocity report shows units sold per product per time window
- [ ] All metrics are computed from live data, not cached stale values
- [ ] Results support pagination and category filtering

#### Priority: **High**
#### Complexity: **Medium**

---

### TASK-07 — Customer Order History & Lifetime Value

**Category:** Business Intelligence / Revenue Opportunities

#### Business Problem
Customer data exists (name, city, email, phone, `LastLoginDate`) but there is no way to query a customer's full order history, total spend, average order value, or last purchase date. The `LastLoginDate` field exists on `Customer` but it is never systematically updated (no auth system yet). There is no customer segmentation or value scoring.

#### Proposed Solution
Build a Customer Intelligence API that computes LTV (Lifetime Value), average order value, purchase frequency, and days since last order per customer. Add a customer tier system (Bronze/Silver/Gold) based on spend thresholds.

#### Business Value
**High** — Customer LTV is the single most important commercial metric for a retail business. Identifying top customers enables loyalty programs, targeted discounts, and retention campaigns.

#### Tables Used
- `Customers`
- `Orders`
- `OrderItems`

#### Suggested Endpoints
- `GET /api/customers/{id}/order-summary` — total orders, total spend, avg order value, last order date
- `GET /api/customers/top?limit=10` — top customers by spend
- `GET /api/customers/inactive?daysSinceOrder=30` — customers with no recent order
- `GET /api/dashboard/customers` — aggregate: new vs returning, city breakdown, avg LTV

#### Acceptance Criteria
- [ ] `GET /api/customers/{id}/order-summary` returns accurate spend totals
- [ ] Top-customers endpoint returns correctly ranked results
- [ ] Inactive customers endpoint identifies churn risk correctly
- [ ] Customer city distribution is aggregated for geographic analysis

#### Priority: **High**
#### Complexity: **Medium**

---

### TASK-08 — Supplier Enrichment & Supplier Performance Tracking

**Category:** Strategic Feature / Operational Efficiency

#### Business Problem
`Supplier` has only `SupplierName`. There is no contact email, phone, address, lead time, payment terms, or quality rating. Operations cannot contact a supplier, calculate reorder timing, or evaluate supplier performance. When inventory runs low, there is no automated reorder trigger.

#### Proposed Solution
Enrich the `Supplier` entity with operational fields. Add a `SupplierOrder` entity to track restocking orders to suppliers. Track on-time delivery rate and price stability per supplier.

#### Business Value
**High** — Supply chain resilience depends on supplier data quality. Enriching this entity unlocks automated reorder workflows, supplier scorecards, and lead-time-aware inventory alerts.

#### Tables Used
- `Suppliers`
- `Products`
- `Inventories`

#### Suggested DB Changes
```sql
ALTER TABLE Suppliers ADD
  ContactEmail    VARCHAR(100) NULL,
  ContactPhone    VARCHAR(20)  NULL,
  Address         VARCHAR(200) NULL,
  LeadTimeDays    INT          NULL,
  PaymentTerms    VARCHAR(100) NULL,
  Rating          DECIMAL(3,2) NULL  -- 0.00 to 5.00
;
```

#### Suggested Endpoints
- `GET /api/suppliers/{id}/products` — all products from this supplier
- `GET /api/suppliers/{id}/performance` — avg lead time, delivery rate, price changes
- `GET /api/suppliers/reorder-candidates` — suppliers with products below reorder threshold

#### Acceptance Criteria
- [ ] Supplier record accepts and stores contact fields
- [ ] Products can be filtered by supplier
- [ ] Reorder-candidates endpoint uses `Inventory.AvailableQuantity` + configurable threshold

#### Priority: **High**
#### Complexity: **Low**

---

### TASK-09 — Revenue & Sales Analytics API

**Category:** Business Intelligence

#### Business Problem
There is no sales reporting capability. Management cannot see daily/weekly/monthly revenue, top-selling products, revenue by category, or discount impact. `Order.Discount` and `Order.OrderAmount` exist but are not aggregated anywhere.

#### Proposed Solution
Build a Sales Analytics service that computes revenue KPIs from existing `Orders` and `OrderItems` data, grouped by time period, product, category, and customer segment.

#### Business Value
**High** — Revenue visibility is the primary management dashboard. Without it, business decisions are made blind.

#### Tables Used
- `Orders`
- `OrderItems`
- `Products`
- `Categories`
- `Customers`

#### Suggested Endpoints
- `GET /api/reports/revenue?period=monthly&year=2026` — revenue by month
- `GET /api/reports/top-products?limit=10&period=30d` — best sellers
- `GET /api/reports/revenue-by-category` — category performance
- `GET /api/reports/discount-impact` — total discount given vs revenue
- `GET /api/reports/daily-orders?from=&to=` — order volume trend

#### Acceptance Criteria
- [ ] Revenue figures match `SUM(OrderAmount)` for Delivered + Shipped orders only
- [ ] Top-products report accounts for quantity sold and revenue generated
- [ ] Discount impact report shows total discount amount and % of gross revenue
- [ ] All reports support date range filtering

#### Priority: **High**
#### Complexity: **Medium**

---

### TASK-10 — Product Catalogue Enrichment

**Category:** Strategic Feature / Revenue Opportunities

#### Business Problem
`Product` currently has only `ProductName`, `Price`, `Quantity`, `CategoryId`, and `SupplierId`. There is no SKU, description, image URL, weight, dimensions, or active/discontinued flag. The system cannot support a product catalogue, search, or filtering. A product cannot be retired without being deleted — which would break historical orders.

#### Proposed Solution
Enrich the `Product` entity with commercial and operational fields. Add an `IsActive` soft-delete flag. Introduce `SKU` as a unique business identifier (distinct from the database `Id`).

#### Business Value
**High** — Every frontend product page, search feature, and catalogue export depends on rich product data. SKUs are required for warehouse management and supplier ordering.

#### Tables Used
- `Products`
- `Categories`
- `Suppliers`

#### Suggested DB Changes
```sql
ALTER TABLE Products ADD
  SKU            VARCHAR(50)   NULL UNIQUE,
  Description    VARCHAR(1000) NULL,
  ImageUrl       VARCHAR(500)  NULL,
  WeightKg       DECIMAL(8,3)  NULL,
  IsActive       BIT           NOT NULL DEFAULT 1,
  CostPrice      DECIMAL(18,2) NULL,  -- supplier cost (for margin calculation)
  ReorderLevel   INT           NULL   -- triggers low-stock alert
;
```

#### Suggested Endpoints
- `GET /api/products?category=&supplier=&minPrice=&maxPrice=&inStock=true` — filtered catalogue
- `GET /api/products/{id}` — product detail with inventory level
- `GET /api/products/sku/{sku}` — lookup by SKU
- `PATCH /api/products/{id}/deactivate` — soft delete

#### Acceptance Criteria
- [ ] SKU is unique and validated on insert/update
- [ ] `IsActive = false` products are excluded from catalogue endpoints but preserved for order history
- [ ] `CostPrice` enables a `GET /api/products/margins` endpoint for profitability analysis
- [ ] Products can be filtered by category, supplier, price range, and stock status

#### Priority: **High**
#### Complexity: **Low-Medium**

---

### TASK-11 — Soft Delete Strategy Across All Entities

**Category:** Data Quality / Compliance

#### Business Problem
No entity has a soft-delete mechanism. Deleting a `Customer` would orphan their `Orders`. Deleting a `Product` would orphan `OrderItems`. Deleting a `Supplier` would orphan `Products`. Hard deletes in a transactional commerce system break referential integrity and destroy historical data.

#### Proposed Solution
Add `IsDeleted BIT NOT NULL DEFAULT 0` and `DeletedAt DATETIME NULL` to all entities. Override all `GetAllAsync` calls with a global query filter `WHERE IsDeleted = 0`. Provide an undelete endpoint for admins.

#### Business Value
**High** — Data preservation is critical for auditing, financial reporting, and legal compliance. Accidental or malicious hard deletes should be recoverable.

#### Tables Used
- All 7 tables

#### Suggested DB Changes
```sql
-- Add to ALL tables:
IsDeleted  BIT       NOT NULL DEFAULT 0,
DeletedAt  DATETIME  NULL,
DeletedBy  VARCHAR(100) NULL
```

#### Suggested Endpoints
- `DELETE /api/customers/{id}` — soft delete (sets IsDeleted = 1)
- `POST /api/customers/{id}/restore` — admin only, sets IsDeleted = 0

#### Acceptance Criteria
- [ ] Soft-deleted records are excluded from all list queries automatically via EF global filter
- [ ] Hard DELETE at the database level is blocked via application layer
- [ ] Admin restore endpoint reactivates a deleted record
- [ ] Cascade soft-delete: deleting a Customer soft-deletes their open Orders

#### Priority: **High**
#### Complexity: **Low**

---

### TASK-12 — Discount Rule Engine

**Category:** Strategic Feature / Revenue Opportunities

#### Business Problem
`Order.Discount` is a raw decimal with no business rules. There is no way to define discount policies (e.g., "10% off for Gold customers", "free shipping over $200", "bulk discount for 5+ units"). Discounts are manually entered with no validation, no maximum cap, and no audit trail.

#### Proposed Solution
Create a `DiscountRule` entity with configurable rules. The order service applies rules automatically based on customer tier, cart value, and item quantity. Store the applied rule reference on the order for audit.

#### Business Value
**High** — Promotions and discounts are primary levers for revenue management. A rules engine enables marketing campaigns without code changes.

#### Tables Used
- `Orders`
- `Customers`
- `Products`
- `DiscountRules` *(new)*

#### Suggested DB Changes
```sql
CREATE TABLE DiscountRules (
  Id              INT IDENTITY PRIMARY KEY,
  RuleName        VARCHAR(100) NOT NULL,
  DiscountType    VARCHAR(20)  NOT NULL, -- 'Percentage', 'FixedAmount', 'FreeShipping'
  DiscountValue   DECIMAL(18,2) NOT NULL,
  MinOrderValue   DECIMAL(18,2) NULL,
  MinQuantity     INT           NULL,
  CustomerTier    VARCHAR(20)   NULL,  -- Bronze, Silver, Gold
  ValidFrom       DATETIME      NULL,
  ValidTo         DATETIME      NULL,
  IsActive        BIT           NOT NULL DEFAULT 1
);
ALTER TABLE Orders ADD AppliedDiscountRuleId INT NULL REFERENCES DiscountRules(Id);
```

#### Suggested Endpoints
- `GET /api/discount-rules` — list all active rules
- `POST /api/discount-rules` — create rule (admin)
- `POST /api/orders/{id}/apply-discount?ruleId=` — manually apply rule

#### Acceptance Criteria
- [ ] Discount rules are applied automatically during order creation
- [ ] Applied rule is stored and visible on the order
- [ ] Expired rules (`ValidTo < NOW`) are not applied
- [ ] A maximum discount cap of 50% is enforced

#### Priority: **High**
#### Complexity: **High**

---

### TASK-13 — Customer Registration Date & Engagement Metrics

**Category:** Business Intelligence / Data Quality

#### Business Problem
`Customer.LastLoginDate` is stored but never reliably updated (no auth system). There is no `RegistrationDate`. The system cannot calculate how long a customer has been active, their engagement rate, or identify at-risk churning customers. `LastLoginDate` defaults to `DateTime.Now.Date` on construction — meaning every new customer record looks like it logged in today even if created via import.

#### Proposed Solution
Add `RegistrationDate` to `Customer`. Wire `LastLoginDate` to the authentication system (set on successful JWT login). Add engagement scoring based on order frequency and recency.

#### Business Value
**Medium-High** — Customer lifecycle data (acquisition, activation, retention, churn) is the foundation of any CRM strategy.

#### Tables Used
- `Customers`
- `Orders`

#### Suggested DB Changes
```sql
ALTER TABLE Customers ADD
  RegistrationDate  DATETIME NOT NULL DEFAULT GETDATE(),
  IsActive          BIT NOT NULL DEFAULT 1,
  CustomerTier      VARCHAR(20) NULL  -- Bronze, Silver, Gold
;
```

#### Suggested Endpoints
- `GET /api/customers/{id}/engagement` — login frequency, order frequency, days since last order
- `GET /api/customers/churn-risk` — inactive customers (no order in N days)

#### Acceptance Criteria
- [ ] `RegistrationDate` is set on customer creation and never changes
- [ ] `LastLoginDate` is updated only via the auth flow
- [ ] Churn-risk endpoint flags customers with no order in the last 60 days
- [ ] Customer tier is computed from total historical spend

#### Priority: **Medium-High**
#### Complexity: **Low**

---

### TASK-14 — Order Search & Advanced Filtering

**Category:** API Improvements / Operational Efficiency

#### Business Problem
The generic repository has paging, filtering, and sorting but no domain-specific order search. Operations staff need to find orders by customer name, status, date range, or order amount. Currently there is no way to do this without a full table scan.

#### Proposed Solution
Build a rich `OrderQuery` endpoint with filtering by status, customer, date range, amount range, and text search. Support sorting by date, amount, and status.

#### Business Value
**Medium** — Customer service and fulfilment teams spend significant time searching for orders. A 10-second search saved 100 times a day is material operational efficiency.

#### Tables Used
- `Orders`
- `Customers`
- `OrderItems`

#### Suggested Endpoints
- `GET /api/orders?status=&customerId=&from=&to=&minAmount=&maxAmount=&page=&pageSize=&sort=`
- `GET /api/orders/search?q=` — full-text search on customer name or order ID

#### Acceptance Criteria
- [ ] All filter parameters work independently and in combination
- [ ] Pagination metadata (`totalCount`, `totalPages`, `currentPage`) is returned
- [ ] Response time < 500ms for standard queries with indexes in place
- [ ] Default sort is `OrderDate DESC`

#### Priority: **Medium**
#### Complexity: **Low**

---

### TASK-15 — Product Category Management with Hierarchy Support

**Category:** Strategic Feature / API Improvements

#### Business Problem
`Category` has only `Id` and `CategoryName`. There is no parent category (no hierarchy), no category description, no active/inactive flag, and no product count. The system cannot support a multi-level catalogue (e.g., Electronics → Mobile Phones → Smartphones).

#### Proposed Solution
Add a `ParentCategoryId` self-referencing FK to support two-level hierarchy. Add `Description` and `IsActive`. Expose a category tree endpoint for frontend navigation menus.

#### Business Value
**Medium** — Category hierarchy is required by virtually every e-commerce storefront for navigation, search filtering, and merchandising.

#### Tables Used
- `Categories`
- `Products`

#### Suggested DB Changes
```sql
ALTER TABLE Categories ADD
  ParentCategoryId  INT NULL REFERENCES Categories(Id),
  Description       VARCHAR(500) NULL,
  IsActive          BIT NOT NULL DEFAULT 1,
  SortOrder         INT NOT NULL DEFAULT 0
;
```

#### Suggested Endpoints
- `GET /api/categories/tree` — full hierarchy as nested JSON
- `GET /api/categories/{id}/products` — all products in category (including subcategories)

#### Acceptance Criteria
- [ ] Categories support one level of parent-child hierarchy
- [ ] `GET /api/categories/tree` returns correctly nested structure
- [ ] Deactivating a parent category deactivates all children
- [ ] Products in subcategories are returned when querying the parent

#### Priority: **Medium**
#### Complexity: **Low**

---

### TASK-16 — Automated Low-Stock Alert System

**Category:** Process Automation / Operational Efficiency

#### Business Problem
There is no mechanism to notify warehouse managers when a product's `Inventory.AvailableQuantity` drops below a threshold. The `Product` entity has no `ReorderLevel` field. Stockouts are only discovered after a customer order fails, causing revenue loss and customer dissatisfaction.

#### Proposed Solution
Add `ReorderLevel` and `ReorderQuantity` to `Product`. Run a scheduled background job (`IHostedService`) every hour that queries `Inventory` records where `AvailableQuantity <= ReorderLevel` and emits alerts (initially to an `Alerts` log table; later to email/webhook).

#### Business Value
**Medium** — Proactive stock management prevents revenue loss from stockouts and reduces emergency restocking costs.

#### Tables Used
- `Inventories`
- `Products`
- `Suppliers`
- `StockAlerts` *(new)*

#### Suggested DB Changes
```sql
CREATE TABLE StockAlerts (
  Id             INT IDENTITY PRIMARY KEY,
  ProductId      INT NOT NULL REFERENCES Products(Id),
  AlertType      VARCHAR(50) NOT NULL,  -- 'LowStock', 'OutOfStock'
  TriggeredAt    DATETIME NOT NULL DEFAULT GETDATE(),
  AvailableQty   INT NOT NULL,
  ReorderLevel   INT NOT NULL,
  IsAcknowledged BIT NOT NULL DEFAULT 0,
  AcknowledgedAt DATETIME NULL
);
```

#### Suggested Endpoints
- `GET /api/alerts/stock` — active unacknowledged stock alerts
- `PATCH /api/alerts/stock/{id}/acknowledge` — mark as handled

#### Acceptance Criteria
- [ ] Background job runs on configurable interval
- [ ] Alerts are created only once per product per threshold breach (no duplicates until restocked)
- [ ] Acknowledging an alert suppresses re-alerting until stock changes again
- [ ] Alert includes supplier contact info for immediate reorder action

#### Priority: **Medium**
#### Complexity: **Medium**

---

### TASK-17 — Data Integrity Consistency Check Endpoints (Admin)

**Category:** Data Quality

#### Business Problem
Multiple data integrity issues exist in the current seed data and schema design: `Order.OrderAmount` doesn't match `SUM(OrderItems)`, `Product.Quantity` doesn't match `Inventory.AvailableQuantity`, and `Order.LastLoginDate` is misnamed causing semantic confusion. There is no admin tooling to detect or repair these inconsistencies.

#### Proposed Solution
Build an admin-only Data Health API that runs consistency checks across the database and returns a diagnostic report with findings and repair suggestions.

#### Business Value
**Medium** — Data quality underpins every business intelligence report. Invisible corruption in financial figures is a liability. Admins need visibility and tools to repair data without direct database access.

#### Tables Used
- `Orders`, `OrderItems`, `Products`, `Inventories`

#### Suggested Endpoints
- `GET /api/admin/data-health/order-amounts` — orders where `OrderAmount != computed total`
- `GET /api/admin/data-health/inventory-sync` — products where `Product.Quantity != Inventory.AvailableQuantity`
- `GET /api/admin/data-health/orphan-records` — order items with no matching order; inventories with no product
- `POST /api/admin/data-health/repair` — attempt auto-repair of detected inconsistencies

#### Acceptance Criteria
- [ ] All health check endpoints are accessible only to the `Admin` role
- [ ] Order-amounts check correctly identifies all mismatched rows
- [ ] Orphan-records check detects FK violations that slipped past EF constraints
- [ ] Repair endpoint logs all changes made for audit purposes

#### Priority: **Medium**
#### Complexity: **Medium**

---

### TASK-18 — Order Return & Refund Workflow

**Category:** Strategic Feature / Operational Efficiency

#### Business Problem
Once an order is `Delivered`, there is no way to process a return or refund. There is no `ReturnRequest` entity, no partial return (return one item from a multi-item order), no refund amount calculation, and no stock restoration on return. This means any post-delivery issue requires direct database manipulation.

#### Proposed Solution
Add a `ReturnRequest` entity and workflow: Customer requests return → Admin reviews → Return approved → Stock restored → Refund issued (recorded in system). Support partial returns at the `OrderItem` level.

#### Business Value
**Medium** — Return processing is a mandatory capability for any e-commerce business. Absence of it drives customer service costs and erodes trust.

#### Tables Used
- `Orders`
- `OrderItems`
- `Inventories`
- `ReturnRequests` *(new)*

#### Suggested DB Changes
```sql
CREATE TABLE ReturnRequests (
  Id              INT IDENTITY PRIMARY KEY,
  OrderId         INT NOT NULL REFERENCES Orders(Id),
  OrderItemId     INT NULL REFERENCES OrderItems(Id),
  RequestedAt     DATETIME NOT NULL DEFAULT GETDATE(),
  Reason          VARCHAR(500) NOT NULL,
  Status          VARCHAR(50) NOT NULL DEFAULT 'Pending',
  RefundAmount    DECIMAL(18,2) NULL,
  ProcessedAt     DATETIME NULL,
  ProcessedBy     INT NULL
);
```

#### Suggested Endpoints
- `POST /api/orders/{id}/return` — create return request
- `GET /api/returns?status=` — list return requests (admin)
- `PATCH /api/returns/{id}/approve` — approve and process refund
- `PATCH /api/returns/{id}/reject`

#### Acceptance Criteria
- [ ] Return requests can only be created for `Delivered` orders
- [ ] Approving a return restores `Inventory.AvailableQuantity`
- [ ] Partial returns (single order item) are supported
- [ ] Refund amount is auto-calculated from `PricePerUnit × ReturnQuantity`

#### Priority: **Medium**
#### Complexity: **High**

---

### TASK-19 — Supplier Product Catalogue & Price History

**Category:** Business Intelligence / Compliance

#### Business Problem
`Product.Price` is a single current value with no history. When an `OrderItem.PricePerUnit` is recorded, there is no way to verify what the product's price was at that moment or trace price changes over time. Suppliers can change prices and there is no record of negotiations or historical pricing.

#### Proposed Solution
Create a `ProductPriceHistory` table. Every time `Product.Price` is updated, insert a row into the history table with the old price, new price, effective date, and operator. This enables margin analysis, pricing trend reports, and audit compliance.

#### Business Value
**Medium** — Price history is essential for financial auditing, supplier negotiations, and accurate margin reporting. It costs very little to implement but is very expensive to reconstruct retroactively.

#### Tables Used
- `Products`
- `Suppliers`
- `ProductPriceHistory` *(new)*

#### Suggested DB Changes
```sql
CREATE TABLE ProductPriceHistory (
  Id          INT IDENTITY PRIMARY KEY,
  ProductId   INT NOT NULL REFERENCES Products(Id),
  OldPrice    DECIMAL(18,2) NOT NULL,
  NewPrice    DECIMAL(18,2) NOT NULL,
  ChangedAt   DATETIME NOT NULL DEFAULT GETDATE(),
  ChangedBy   VARCHAR(100) NULL,
  Note        VARCHAR(500) NULL
);
```

#### Suggested Endpoints
- `GET /api/products/{id}/price-history` — full pricing timeline
- `GET /api/reports/price-changes?from=&to=` — all price changes in period

#### Acceptance Criteria
- [ ] Every `Product.Price` update triggers a `ProductPriceHistory` insert (via EF interceptor or service layer)
- [ ] `OrderItem.PricePerUnit` is validated against the price history for the order date
- [ ] Price history is read-only (no updates or deletes allowed)

#### Priority: **Medium**
#### Complexity: **Low**

---

### TASK-20 — Geographic Sales Intelligence

**Category:** Business Intelligence / Revenue Opportunities

#### Business Problem
`Customer.City` is stored for all 5 seed customers and presumably all future customers. This geographic data is never analyzed. Management cannot see which cities generate the most revenue, which cities have the highest order value, or which geographic markets are underperforming.

#### Proposed Solution
Build a Geographic Analytics API that aggregates order revenue, customer count, and order frequency by city. This data supports marketing spend allocation, regional promotions, and logistics planning.

#### Business Value
**Low-Medium** — Geographic segmentation is a secondary intelligence layer but directly informs marketing budget allocation and regional strategy without requiring any new data collection.

#### Tables Used
- `Customers`
- `Orders`
- `OrderItems`

#### Suggested Endpoints
- `GET /api/reports/revenue-by-city` — revenue, order count, customer count per city
- `GET /api/reports/top-cities?limit=10` — highest revenue cities
- `GET /api/customers?city={city}` — filter customers by city

#### Acceptance Criteria
- [ ] Revenue-by-city correctly sums `Order.OrderAmount` for non-cancelled orders per city
- [ ] Results support sorting by revenue, order count, and customer count
- [ ] Cities with zero orders in the period are excluded from results

#### Priority: **Low-Medium**
#### Complexity: **Low**

---

## Priority Matrix Summary

| # | Task | Category | Priority | Complexity | Business Value |
|---|------|----------|----------|------------|----------------|
| BUG-05 | Add `UseAuthentication()` to pipeline | Security | 🔴 Critical | Low | High |
| TASK-01 | Order Lifecycle State Machine | Operational | 🔴 Critical | Medium | High |
| TASK-02 | Inventory Deduction Engine | Automation | 🔴 Critical | Medium | High |
| TASK-03 | OrderAmount Auto-Calculation | Data Quality | 🔴 Critical | Medium | High |
| TASK-04 | Audit Fields on All Entities | Compliance | 🔴 Critical | Low | High |
| TASK-05 | Authentication & RBAC | Security | 🔴 Critical | High | High |
| TASK-06 | Inventory Dashboard KPIs | BI | 🟠 High | Medium | High |
| TASK-07 | Customer LTV & Order History | BI/Revenue | 🟠 High | Medium | High |
| TASK-08 | Supplier Enrichment | Strategic | 🟠 High | Low | High |
| TASK-09 | Revenue & Sales Analytics | BI | 🟠 High | Medium | High |
| TASK-10 | Product Catalogue Enrichment | Strategic | 🟠 High | Low-Med | High |
| TASK-11 | Soft Delete Strategy | Compliance | 🟠 High | Low | High |
| TASK-12 | Discount Rule Engine | Revenue | 🟠 High | High | High |
| TASK-13 | Customer Engagement Metrics | BI | 🟡 Med-High | Low | Medium |
| TASK-14 | Order Search & Filtering | API | 🟡 Medium | Low | Medium |
| TASK-15 | Category Hierarchy | Strategic | 🟡 Medium | Low | Medium |
| TASK-16 | Low-Stock Alert System | Automation | 🟡 Medium | Medium | Medium |
| TASK-17 | Data Health Admin Endpoints | Data Quality | 🟡 Medium | Medium | Medium |
| TASK-18 | Return & Refund Workflow | Strategic | 🟡 Medium | High | Medium |
| TASK-19 | Product Price History | Compliance | 🟡 Medium | Low | Medium |
| TASK-20 | Geographic Sales Intelligence | BI | 🟢 Low-Med | Low | Medium |

---

## Recommended Sprint Order

### Sprint 0 — Foundation (Before Any Feature Work)
Fix all 5 bugs. Implement auth entity. Add audit fields. Implement mapping config. Add soft-delete. This sprint produces zero visible features but makes the system safe to build on.

### Sprint 1 — Core Commerce Workflow
TASK-01 (Order State Machine) + TASK-02 (Inventory Deduction) + TASK-03 (OrderAmount Calculation) + TASK-05 (Auth & RBAC). This sprint produces a working, correct order lifecycle.

### Sprint 2 — Product & Supplier Depth
TASK-08 (Supplier Enrichment) + TASK-10 (Product Enrichment) + TASK-15 (Category Hierarchy) + TASK-14 (Order Search). This sprint produces a proper product catalogue.

### Sprint 3 — Business Intelligence
TASK-06 (Inventory Dashboard) + TASK-07 (Customer LTV) + TASK-09 (Revenue Analytics) + TASK-13 (Engagement Metrics). This sprint produces the management dashboard.

### Sprint 4 — Automation & Advanced Features
TASK-12 (Discount Engine) + TASK-16 (Stock Alerts) + TASK-18 (Returns) + TASK-19 (Price History) + TASK-20 (Geographic BI) + TASK-17 (Data Health).

---

## Missing Tables Checklist (Future Epics)

| Entity | Purpose | Trigger |
|--------|---------|---------|
| `Users` | Authentication & profiles | Sprint 0 |
| `Roles` / `UserRoles` | RBAC | Sprint 0 |
| `OrderStatusHistory` | State machine audit trail | Sprint 1 |
| `StockAlerts` | Low-stock notifications | Sprint 4 |
| `ReturnRequests` | Return & refund workflow | Sprint 4 |
| `DiscountRules` | Rule-based promotions | Sprint 3 |
| `ProductPriceHistory` | Pricing audit trail | Sprint 4 |
| `Payments` | Payment status & method tracking | Future |
| `Shipments` | Carrier, tracking, delivery address | Future |
| `ProductReviews` | Customer ratings & feedback | Future |
| `Notifications` | Email/SMS/push communication log | Future |
| `SupplierOrders` | Restock orders placed to suppliers | Future |

---

*Generated by Senior Product Owner / Business Analyst — SCAA-API v1.0 Pre-Launch Audit — 2026-05-23*
