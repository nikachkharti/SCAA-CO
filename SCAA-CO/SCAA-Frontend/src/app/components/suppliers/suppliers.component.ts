import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  SupplierForCreatingDto,
  SupplierForGettingDto,
  SupplierForUpdatingDto
} from '../../models/supplier.model';
import { SupplierService } from '../../services/supplier.service';

type SortField = 'id' | 'supplierName';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.css'
})
export class SuppliersComponent implements OnInit {
  private readonly supplierService = inject(SupplierService);

  readonly suppliers = signal<SupplierForGettingDto[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly sortBy = signal<SortField>('id');
  readonly ascending = signal(false);

  readonly editingId = signal<number | null>(null);
  readonly searchTerm = signal('');

  newSupplierName = '';
  editSupplierName = '';

  readonly pageSizeOptions = [5, 10, 25, 50] as const;

  readonly totalPages = computed(() => {
    const size = this.pageSize();
    const total = this.totalCount();
    return size > 0 ? Math.max(1, Math.ceil(total / size)) : 1;
  });

  readonly filteredSuppliers = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const all = this.suppliers();
    if (!term) {
      return all;
    }
    return all.filter(s => s.supplierName?.toLowerCase().includes(term));
  });

  readonly pageList = computed<(number | null)[]>(() => {
    const current = this.pageNumber();
    const total = this.totalPages();

    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const pages: (number | null)[] = [1];
    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    if (start > 2) pages.push(null);
    for (let p = start; p <= end; p++) pages.push(p);
    if (end < total - 1) pages.push(null);

    pages.push(total);
    return pages;
  });

  readonly rangeFrom = computed(() => {
    if (this.totalCount() === 0) return 0;
    return (this.pageNumber() - 1) * this.pageSize() + 1;
  });

  readonly rangeTo = computed(() =>
    Math.min(this.pageNumber() * this.pageSize(), this.totalCount())
  );

  ngOnInit(): void {
    this.loadSuppliers();
  }

  loadSuppliers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.supplierService
      .getAll({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy(),
        ascending: this.ascending()
      })
      .subscribe({
        next: data => {
          this.suppliers.set(data.items ?? []);
          this.totalCount.set(data.totalCount ?? 0);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) {
            this.suppliers.set([]);
            this.totalCount.set(0);
            this.loading.set(false);
            return;
          }
          this.error.set(this.extractError(err));
          this.loading.set(false);
        }
      });
  }

  createSupplier(): void {
    const name = this.newSupplierName.trim();
    if (!name) {
      return;
    }

    const dto: SupplierForCreatingDto = { supplierName: name };
    this.supplierService.create(dto).subscribe({
      next: () => {
        this.newSupplierName = '';
        this.loadSuppliers();
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  startEdit(supplier: SupplierForGettingDto): void {
    this.editingId.set(supplier.id);
    this.editSupplierName = supplier.supplierName;
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editSupplierName = '';
  }

  saveEdit(): void {
    const id = this.editingId();
    const name = this.editSupplierName.trim();
    if (id === null || !name) {
      return;
    }

    const dto: SupplierForUpdatingDto = { id, supplierName: name };
    this.supplierService.update(dto).subscribe({
      next: updated => {
        this.suppliers.update(list =>
          list.map(s => (s.id === updated.id ? updated : s))
        );
        this.cancelEdit();
        if (this.sortBy() === 'supplierName') {
          this.loadSuppliers();
        }
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  deleteSupplier(id: number): void {
    if (!confirm('Delete this supplier?')) {
      return;
    }

    this.supplierService.delete(id).subscribe({
      next: () => {
        const remainingOnPage = this.suppliers().length - 1;
        if (remainingOnPage === 0 && this.pageNumber() > 1) {
          this.pageNumber.set(this.pageNumber() - 1);
        }
        this.loadSuppliers();
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  goToPage(page: number | null): void {
    if (page === null) return;
    const clamped = Math.min(Math.max(1, page), this.totalPages());
    if (clamped === this.pageNumber()) return;
    this.pageNumber.set(clamped);
    this.loadSuppliers();
  }

  changePageSize(size: number): void {
    if (size === this.pageSize()) return;
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadSuppliers();
  }

  toggleSort(field: SortField): void {
    if (this.sortBy() === field) {
      this.ascending.set(!this.ascending());
    } else {
      this.sortBy.set(field);
      this.ascending.set(true);
    }
    this.pageNumber.set(1);
    this.loadSuppliers();
  }

  clearSearch(): void {
    this.searchTerm.set('');
  }

  dismissError(): void {
    this.error.set(null);
  }

  private extractError(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      return err.error?.message || err.message || 'Request failed.';
    }
    if (err && typeof err === 'object' && 'message' in err) {
      return String((err as { message: unknown }).message);
    }
    return 'An unexpected error occurred.';
  }
}
