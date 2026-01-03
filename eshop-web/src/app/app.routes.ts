import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home').then(m => m.Home)
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'catalog',
    loadComponent: () => import('./features/catalog/catalog-list/catalog-list').then(m => m.CatalogList)
  },
  {
    path: 'product/:id',
    loadComponent: () => import('./features/product-detail/product-detail').then(m => m.ProductDetail)
  },
  {
    path: 'basket',
    loadComponent: () => import('./features/basket/basket').then(m => m.Basket),
    canActivate: [authGuard]
  },
  {
    path: 'orders',
    loadComponent: () => import('./features/orders/orders').then(m => m.Orders),
    canActivate: [authGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/profile/profile').then(m => m.Profile),
    canActivate: [authGuard]
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/admin/dashboard/admin-dashboard').then(m => m.AdminDashboard)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/admin/products/product-list').then(m => m.ProductList)
      },
      {
        path: 'products/new',
        loadComponent: () => import('./features/admin/products/product-form').then(m => m.ProductForm)
      },
      {
        path: 'products/edit/:id',
        loadComponent: () => import('./features/admin/products/product-form').then(m => m.ProductForm)
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/catalog'
  }
];
