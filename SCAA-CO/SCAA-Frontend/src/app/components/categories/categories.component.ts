import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CategoryForCreatingDto,
  CategoryForGettingDto,
  CategoryForUpdatingDto
} from '../../models/category.model';
import { CategoryService } from '../../services/category.service';

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
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly editingId = signal<number | null>(null);
  readonly searchTerm = signal('');

  readonly filteredCategories = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const all = this.categories();
    if (!term) {
      return all;
    }
    return all.filter(c => c.categoryName?.toLowerCase().includes(term));
  });

  newCategoryName = '';
  editCategoryName = '';

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.error.set(null);

    this.categoryService.getAll().subscribe({
      next: data => {
        this.categories.set(data);
        this.loading.set(false);
      },
      error: err => {
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
      next: created => {
        this.categories.update(list => [...list, created]);
        this.newCategoryName = '';
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
        this.categories.update(list => list.filter(c => c.id !== id));
      },
      error: err => this.error.set(this.extractError(err))
    });
  }

  clearSearch(): void {
    this.searchTerm.set('');
  }

  dismissError(): void {
    this.error.set(null);
  }

  private extractError(err: unknown): string {
    if (err && typeof err === 'object' && 'message' in err) {
      return String((err as { message: unknown }).message);
    }
    return 'An unexpected error occurred.';
  }
}
