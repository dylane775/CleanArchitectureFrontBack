import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatTooltipModule
  ],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayout {
  sidenavOpened = signal(true);

  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      route: '/admin/dashboard',
      icon: 'dashboard',
      description: 'Overview and statistics'
    },
    {
      label: 'Products',
      route: '/admin/products',
      icon: 'inventory_2',
      description: 'Manage catalog items'
    },
    {
      label: 'Brands',
      route: '/admin/brands',
      icon: 'label',
      description: 'Manage product brands'
    },
    {
      label: 'Categories',
      route: '/admin/categories',
      icon: 'category',
      description: 'Manage product categories'
    },
    {
      label: 'Orders',
      route: '/admin/orders',
      icon: 'receipt_long',
      description: 'View and manage orders'
    },
    {
      label: 'Users',
      route: '/admin/users',
      icon: 'people',
      description: 'Manage user accounts'
    }
  ];

  constructor(private router: Router) {}

  toggleSidenav(): void {
    this.sidenavOpened.set(!this.sidenavOpened());
  }

  navigateToHome(): void {
    this.router.navigate(['/']);
  }

  isActive(route: string): boolean {
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }
}
