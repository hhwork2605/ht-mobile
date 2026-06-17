import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { ShellComponent } from './layout/shell.component';
import { LoginComponent } from './pages/login/login.component';
import { ProductsListComponent } from './pages/products/products-list.component';
import { ProductFormComponent } from './pages/products/product-form.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'products', component: ProductsListComponent },
      { path: 'products/new', component: ProductFormComponent },
      { path: 'products/:id', component: ProductFormComponent },
      { path: '', pathMatch: 'full', redirectTo: 'products' },
    ],
  },
  { path: '**', redirectTo: '' },
];
