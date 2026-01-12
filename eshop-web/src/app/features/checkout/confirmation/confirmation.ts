import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models/order.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './confirmation.html',
  styleUrls: ['./confirmation.scss']
})
export class Confirmation implements OnInit {
  order = signal<Order | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  orderId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) {}

  ngOnInit() {
    this.orderId = this.route.snapshot.paramMap.get('id');

    if (this.orderId) {
      this.loadOrder(this.orderId);
    } else {
      this.error.set('No order ID provided');
      this.loading.set(false);
    }
  }

  private loadOrder(orderId: string) {
    this.orderService.getOrderById(orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.error.set('Failed to load order details');
        this.loading.set(false);
      }
    });
  }

  getImageUrl(pictureUrl: string | undefined): string {
    if (!pictureUrl) return '/assets/images/placeholder.jpg';
    if (pictureUrl.startsWith('http')) return pictureUrl;
    return `${environment.catalogApiUrl}${pictureUrl}`;
  }

  continueShopping() {
    this.router.navigate(['/catalog']);
  }

  viewOrders() {
    this.router.navigate(['/orders']);
  }

  printOrder() {
    window.print();
  }

  get orderTotal(): number {
    const order = this.order();
    if (!order) return 0;
    return order.subtotal;
  }

  get orderTax(): number {
    const order = this.order();
    if (!order) return 0;
    return order.subtotal * 0.1;
  }
}
