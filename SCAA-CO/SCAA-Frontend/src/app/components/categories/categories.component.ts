import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CategoryForCreatingDto,
  CategoryForGettingDto,
  CategoryForUpdatingDto
} from '../../models/category.model';
import { CategoryService } from '../../services/category.service';

type SortField = 'id' | 'categoryName';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css'
})
export class CategoriesComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);

  readonly categories = signal<CategoryForGettingDto[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly sortBy = signal<SortField>('id');
  readonly ascending = signal(false);

  readonly editingId = signal<number | null>(null);
  readonly searchTerm = signal('');

  newCategoryName = '';
  editCategoryName = '';

  readonly pageSizeOptions = [5, 10, 25, 50] as const;

  readonly totalPages = computed(() => {
    const size = this.pageSize();
    const total = this.totalCount();
    return size > 0 ? Math.max(1, Math.ceil(total / size)) : 1;
  });

  readonly filteredCategories = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const all = this.categories();
    if (!term) {
      return all;
    }
    return all.filter(c => c.categoryName?.toLowerCase().includes(term));
  });

  /** Compact page list with ellipsis sentinels (null) for jumps. */
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
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.error.set(null);

    this.categoryService
      .getAll({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy(),
        ascending: this.ascending()
      })
      .subscribe({
        next: data => {
          this.categories.set(data.items ?? []);
          this.totalCount.set(data.totalCount ?? 0);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          // Backend throws NotFound when zero items match — treat as empty.
          if (err.status === 404) {
            this.categories.set([]);
            this.totalCount.set(0);
            this.loading.set(false);
            return;
          }
          this.error.set(this.extractError(err));
          this.loading.set(false);
        }
      });
  }

  createCategory(): void {
    const name = this.newCategoryName.trim();
    if (!name) {
      return;
    }

    const dto: CategoryForCreatingDto = { categoryName: name };
    this.categoryService.create(dto).subscribe({
      next: () => {
        this.newCategoryName = '';
        // Re-fetch so the new item lands on the correct page in the correct sort order.
        this.loadCategories();
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  startEdit(category: CategoryForGettingDto): void {
    this.editingId.set(category.id);
    this.editCategoryName = category.categoryName;
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editCategoryName = '';
  }

  saveEdit(): void {
    const id = this.editingId();
    const name = this.editCategoryName.trim();
    if (id === null || !name) {
      return;
    }

    const dto: CategoryForUpdatingDto = { id, categoryName: name };
    this.categoryService.update(dto).subscribe({
      next: updated => {
        this.categories.update(list =>
          list.map(c => (c.id === updated.id ? updated : c))
        );
        this.cancelEdit();
        // If sorting by name, the position likely changed — refresh.
        if (this.sortBy() === 'categoryName') {
          this.loadCategories();
        }
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  deleteCategory(id: number): void {
    if (!confirm('Delete this category?')) {
      return;
    }

    this.categoryService.delete(id).subscribe({
      next: () => {
        // Reload — handles page-shrink (e.g. last item on last page).
        const remainingOnPage = this.categories().length - 1;
        if (remainingOnPage === 0 && this.pageNumber() > 1) {
          this.pageNumber.set(this.pageNumber() - 1);
        }
        this.loadCategories();
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  goToPage(page: number | null): void {
    if (page === null) return;
    const clamped = Math.min(Math.max(1, page), this.totalPages());
    if (clamped === this.pageNumber()) return;
    this.pageNumber.set(clamped);
    this.loadCategories();
  }

  changePageSize(size: number): void {
    if (size === this.pageSize()) return;
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadCategories();
  }

  toggleSort(field: SortField): void {
    if (this.sortBy() === field) {
      this.ascending.set(!this.ascending());
    } else {
      this.sortBy.set(field);
      this.ascending.set(true);
    }
    this.pageNumber.set(1);
    this.loadCategories();
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
