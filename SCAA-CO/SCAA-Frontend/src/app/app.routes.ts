import { Routes } from '@angular/router';

import { CategoriesComponent } from './components/categories/categories.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { authGuard, publicOnlyGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'categories' },
  { path: 'login', component: LoginComponent, canActivate: [publicOnlyGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [publicOnlyGuard] },
  { path: 'categories', component: CategoriesComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'categories' }
];
