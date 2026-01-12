import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, CreateOrderRequest, CheckoutRequest } from '../models/order.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = `${environment.orderingApiUrl}/orders`;

  constructor(private http: HttpClient) {}

  getOrders(customerId: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/customer/${customerId}`);
  }

  getOrderById(orderId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${orderId}`);
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.apiUrl, request);
  }

  /**
   * Creates an order from checkout with full details
   * Returns the created order ID
   */
  checkout(request: CheckoutRequest): Observable<string> {
    return this.http.post<string>(this.apiUrl, request);
  }

  cancelOrder(orderId: string, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${orderId}/cancel`, { reason });
  }

  submitOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${orderId}/submit`, {});
  }
}
