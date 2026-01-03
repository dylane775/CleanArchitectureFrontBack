import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CatalogItem, CatalogType, CatalogBrand, PaginatedItems, CreateCatalogItemRequest } from '../models/catalog.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly apiUrl = environment.catalogApiUrl;

  constructor(private http: HttpClient) {}

  getCatalogItems(pageIndex: number = 1, pageSize: number = 10): Observable<PaginatedItems<CatalogItem>> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedItems<CatalogItem>>(`${this.apiUrl}/catalog/items`, { params });
  }

  getCatalogItemById(id: string): Observable<CatalogItem> {
    return this.http.get<CatalogItem>(`${this.apiUrl}/catalog/items/${id}`);
  }

  getCatalogItemsByBrand(brandId: string): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>(`${this.apiUrl}/catalog/items/brand/${brandId}`);
  }

  getCatalogItemsByType(typeId: string): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>(`${this.apiUrl}/catalog/items/type/${typeId}`);
  }

  getCatalogTypes(): Observable<CatalogType[]> {
    return this.http.get<CatalogType[]>(`${this.apiUrl}/catalog/types`);
  }

  getCatalogBrands(): Observable<CatalogBrand[]> {
    return this.http.get<CatalogBrand[]>(`${this.apiUrl}/catalog/brands`);
  }

  createCatalogItem(request: CreateCatalogItemRequest): Observable<CatalogItem> {
    return this.http.post<CatalogItem>(`${this.apiUrl}/catalog/items`, request);
  }

  updateCatalogItem(id: string, request: Partial<CreateCatalogItemRequest>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/catalog/items/${id}`, request);
  }

  deleteCatalogItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/catalog/items/${id}`);
  }
}
