import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  User,
  Role,
  AssignRoleRequest,
  CatalogBrand,
  CatalogType,
  CreateCatalogItemRequest,
  UpdateStockRequest,
  OrderSummary,
  OrderDetails,
  AdminStats
} from '../../models/admin/admin.model';
import { CatalogItem, PaginatedItems } from '../../models/catalog.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);

  private identityApiUrl = environment.identityApiUrl;
  private catalogApiUrl = environment.catalogApiUrl;
  private orderingApiUrl = environment.orderingApiUrl;

  // ============= USER MANAGEMENT =============
  getAllUsers(page: number = 1, pageSize: number = 10, isActive?: boolean): Observable<PaginatedItems<User>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PaginatedItems<User>>(`${this.identityApiUrl}/users`, { params });
  }

  getUserById(userId: string): Observable<User> {
    return this.http.get<User>(`${this.identityApiUrl}/users/${userId}`);
  }

  getUserRoles(userId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.identityApiUrl}/users/${userId}/roles`);
  }

  // ============= ROLE MANAGEMENT =============
  getAllRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.identityApiUrl}/roles`);
  }

  assignRole(request: AssignRoleRequest): Observable<void> {
    return this.http.post<void>(`${this.identityApiUrl}/roles/assign`, request);
  }

  removeRole(request: AssignRoleRequest): Observable<void> {
    return this.http.post<void>(`${this.identityApiUrl}/roles/remove`, request);
  }

  // ============= PRODUCT MANAGEMENT =============
  createProduct(product: CreateCatalogItemRequest): Observable<CatalogItem> {
    return this.http.post<CatalogItem>(`${this.catalogApiUrl}/catalog/items`, product);
  }

  updateProduct(id: string, product: CreateCatalogItemRequest): Observable<CatalogItem> {
    return this.http.put<CatalogItem>(`${this.catalogApiUrl}/catalog/items/${id}`, product);
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.catalogApiUrl}/catalog/items/${id}`);
  }

  updateStock(request: UpdateStockRequest): Observable<void> {
    return this.http.patch<void>(
      `${this.catalogApiUrl}/catalog/items/${request.productId}/stock`,
      { quantity: request.quantity }
    );
  }

  addStock(productId: string, quantity: number): Observable<void> {
    return this.http.post<void>(
      `${this.catalogApiUrl}/catalog/items/${productId}/stock/add`,
      { quantity }
    );
  }

  removeStock(productId: string, quantity: number): Observable<void> {
    return this.http.post<void>(
      `${this.catalogApiUrl}/catalog/items/${productId}/stock/remove`,
      { quantity }
    );
  }

  // ============= BRAND MANAGEMENT =============
  getAllBrands(): Observable<CatalogBrand[]> {
    return this.http.get<CatalogBrand[]>(`${this.catalogApiUrl}/catalog/brands`);
  }

  createBrand(brand: string): Observable<CatalogBrand> {
    return this.http.post<CatalogBrand>(`${this.catalogApiUrl}/catalog/brands`, { brand });
  }

  updateBrand(id: string, brand: string): Observable<CatalogBrand> {
    return this.http.put<CatalogBrand>(`${this.catalogApiUrl}/catalog/brands/${id}`, { brand });
  }

  deleteBrand(id: string): Observable<void> {
    return this.http.delete<void>(`${this.catalogApiUrl}/catalog/brands/${id}`);
  }

  // ============= TYPE MANAGEMENT =============
  getAllTypes(): Observable<CatalogType[]> {
    return this.http.get<CatalogType[]>(`${this.catalogApiUrl}/catalog/types`);
  }

  createType(type: string): Observable<CatalogType> {
    return this.http.post<CatalogType>(`${this.catalogApiUrl}/catalog/types`, { type });
  }

  updateType(id: string, type: string): Observable<CatalogType> {
    return this.http.put<CatalogType>(`${this.catalogApiUrl}/catalog/types/${id}`, { type });
  }

  deleteType(id: string): Observable<void> {
    return this.http.delete<void>(`${this.catalogApiUrl}/catalog/types/${id}`);
  }

  // ============= ORDER MANAGEMENT =============
  getAllOrders(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(`${this.orderingApiUrl}/orders`);
  }

  getOrderById(orderId: string): Observable<OrderDetails> {
    return this.http.get<OrderDetails>(`${this.orderingApiUrl}/orders/${orderId}`);
  }

  getOrdersByStatus(status: string): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(`${this.orderingApiUrl}/orders/status/${status}`);
  }

  shipOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.orderingApiUrl}/orders/${orderId}/ship`, {});
  }

  deliverOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.orderingApiUrl}/orders/${orderId}/deliver`, {});
  }

  cancelOrder(orderId: string, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.orderingApiUrl}/orders/${orderId}/cancel`, { reason });
  }

  // ============= DASHBOARD STATS =============
  getDashboardStats(): Observable<AdminStats> {
    // This would typically be a dedicated endpoint, but we'll aggregate for now
    return this.http.get<AdminStats>(`${this.identityApiUrl}/admin/stats`);
  }
}
