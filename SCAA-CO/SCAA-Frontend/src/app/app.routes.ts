import { Routes } from '@angular/router';

import { CategoriesComponent } from './components/categories/categories.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'categories' },
  { path: 'categories', component: CategoriesComponent }
];
