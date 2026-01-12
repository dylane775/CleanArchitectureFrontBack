import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../core/models/order.model';
import { environment } from '../../../environments/environment';

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
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class Orders implements OnInit {
  orders = signal<Order[]>([]);
  loading = signal(false);

  constructor(
    private authService: AuthService,
    private orderService: OrderService,
    private router: Router,
    private snackBar: MatSnackBar
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
    console.log('Loading orders for user:', user.id);

    // Call real API to get orders
    this.orderService.getOrders(user.id).subscribe({
      next: (orders) => {
        console.log('Orders received:', orders);
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading orders:', error);
        this.loading.set(false);
        this.snackBar.open('Failed to load orders. Please try again.', 'Close', {
          duration: 4000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
      }
    });
  }

  viewOrderDetails(orderId: string): void {
    this.router.navigate(['/checkout/confirmation', orderId]);
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
