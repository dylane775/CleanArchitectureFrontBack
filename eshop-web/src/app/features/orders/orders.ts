import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

interface OrderItem {
  productName: string;
  unitPrice: number;
  quantity: number;
  pictureUrl: string;
}

interface Order {
  id: string;
  orderDate: Date;
  status: string;
  total: number;
  items: OrderItem[];
  shippingAddress: string;
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class Orders implements OnInit {
  orders = signal<Order[]>([]);
  loading = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    const user = this.authService.currentUser();

    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.loading.set(true);

    // TODO: Replace with actual API call when backend is ready
    // Simulate API call with mock data
    setTimeout(() => {
      const mockOrders: Order[] = [
        {
          id: '1',
          orderDate: new Date('2026-01-01'),
          status: 'Delivered',
          total: 299.99,
          shippingAddress: '123 Main St, City, Country',
          items: [
            {
              productName: 'Wireless Headphones',
              unitPrice: 149.99,
              quantity: 2,
              pictureUrl: 'https://via.placeholder.com/100'
            }
          ]
        },
        {
          id: '2',
          orderDate: new Date('2025-12-28'),
          status: 'Shipped',
          total: 599.99,
          shippingAddress: '456 Oak Ave, City, Country',
          items: [
            {
              productName: 'Smart Watch',
              unitPrice: 299.99,
              quantity: 1,
              pictureUrl: 'https://via.placeholder.com/100'
            },
            {
              productName: 'Phone Case',
              unitPrice: 29.99,
              quantity: 1,
              pictureUrl: 'https://via.placeholder.com/100'
            }
          ]
        },
        {
          id: '3',
          orderDate: new Date('2025-12-20'),
          status: 'Processing',
          total: 899.99,
          shippingAddress: '789 Pine Rd, City, Country',
          items: [
            {
              productName: 'Laptop Stand',
              unitPrice: 89.99,
              quantity: 1,
              pictureUrl: 'https://via.placeholder.com/100'
            },
            {
              productName: 'Mechanical Keyboard',
              unitPrice: 159.99,
              quantity: 1,
              pictureUrl: 'https://via.placeholder.com/100'
            }
          ]
        }
      ];

      this.orders.set(mockOrders);
      this.loading.set(false);
    }, 500);
  }

  viewOrderDetails(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered':
        return 'status-delivered';
      case 'shipped':
        return 'status-shipped';
      case 'processing':
        return 'status-processing';
      case 'cancelled':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered':
        return 'check_circle';
      case 'shipped':
        return 'local_shipping';
      case 'processing':
        return 'schedule';
      case 'cancelled':
        return 'cancel';
      default:
        return 'info';
    }
  }

  continueShopping(): void {
    this.router.navigate(['/catalog']);
  }

  getImageUrl(pictureUrl: string): string {
    if (!pictureUrl) {
      return 'https://via.placeholder.com/100';
    }

    // Si l'URL commence déjà par http, la retourner telle quelle
    if (pictureUrl.startsWith('http')) {
      return pictureUrl;
    }

    // Sinon, construire l'URL complète avec le serveur Catalog
    const catalogBaseUrl = environment.catalogApiUrl.replace('/api', '');
    return `${catalogBaseUrl}${pictureUrl}`;
  }
}
