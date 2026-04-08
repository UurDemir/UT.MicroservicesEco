import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';
import { authGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './layouts/main-layout.component';
import { HomeComponent } from './pages/home/home.component';
import { CatalogComponent } from './pages/catalog/catalog.component';
import { StorefrontLoginComponent } from './pages/login/storefront-login.component';
import { RegisterComponent } from './pages/register/register.component';
import { BasketComponent } from './pages/basket/basket.component';
import { BackofficeLayoutComponent } from './backoffice/backoffice-layout.component';
import { BackofficeLoginComponent } from './backoffice/login/login.component';
import { BackofficeDashboardComponent } from './backoffice/dashboard/dashboard.component';
import { ProductsAdminComponent } from './backoffice/products/products-admin.component';
import { OrdersAdminComponent } from './backoffice/orders/orders-admin.component';
import { DeliveriesAdminComponent } from './backoffice/deliveries/deliveries-admin.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'products', component: CatalogComponent },
      { path: 'login', component: StorefrontLoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'basket', component: BasketComponent, canActivate: [authGuard] }
    ]
  },
  { path: 'backoffice/login', component: BackofficeLoginComponent },
  {
    path: 'backoffice',
    component: BackofficeLayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: BackofficeDashboardComponent },
      { path: 'products', component: ProductsAdminComponent },
      { path: 'orders', component: OrdersAdminComponent },
      { path: 'deliveries', component: DeliveriesAdminComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];
