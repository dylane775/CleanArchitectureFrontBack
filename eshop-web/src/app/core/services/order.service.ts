import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, CreateOrderRequest } from '../models/order.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = environment.orderingApiUrl;

  constructor(private http: HttpClient) {}

  getOrders(buyerId: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/orders/buyer/${buyerId}`);
  }

  getOrderById(orderId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/orders/${orderId}`);
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/orders`, request);
  }

  cancelOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/orders/${orderId}/cancel`, {});
  }
}
