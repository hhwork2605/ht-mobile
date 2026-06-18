import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { ShellComponent } from './layout/shell.component';
import { LoginComponent } from './pages/login/login.component';
import { ProductsListComponent } from './pages/products/products-list.component';
import { ProductFormComponent } from './pages/products/product-form.component';
import { OrdersListComponent } from './pages/orders/orders-list.component';
import { OrderDetailComponent } from './pages/orders/order-detail.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ReportsComponent } from './pages/reports/reports.component';
import { PromotionsListComponent } from './pages/promotions/promotions-list.component';
import { PromotionFormComponent } from './pages/promotions/promotion-form.component';
import { UsersListComponent } from './pages/users/users-list.component';
import { CategoriesListComponent } from './pages/categories/categories-list.component';
import { CategoryFormComponent } from './pages/categories/category-form.component';
import { InventoryComponent } from './pages/inventory/inventory.component';
import { SeoComponent } from './pages/seo/seo.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'products', component: ProductsListComponent },
      { path: 'products/new', component: ProductFormComponent },
      { path: 'products/:id', component: ProductFormComponent },
      { path: 'categories', component: CategoriesListComponent },
      { path: 'categories/new', component: CategoryFormComponent },
      { path: 'categories/:id', component: CategoryFormComponent },
      { path: 'inventory', component: InventoryComponent },
      { path: 'orders', component: OrdersListComponent },
      { path: 'orders/:id', component: OrderDetailComponent },
      { path: 'promotions', component: PromotionsListComponent },
      { path: 'promotions/new', component: PromotionFormComponent },
      { path: 'promotions/:id', component: PromotionFormComponent },
      { path: 'users', component: UsersListComponent },
      { path: 'seo', component: SeoComponent },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    ],
  },
  { path: '**', redirectTo: '' },
];
