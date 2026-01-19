import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CatalogItem, CatalogType, CatalogBrand, PaginatedItems, CreateCatalogItemRequest, SearchSuggestion, HomeRecommendations } from '../models/catalog.model';
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

  // Recherche avec suggestions (auto-complétion)
  getSearchSuggestions(query: string, limit: number = 8): Observable<SearchSuggestion[]> {
    const params = new HttpParams()
      .set('q', query)
      .set('limit', limit.toString());

    return this.http.get<SearchSuggestion[]>(`${this.apiUrl}/catalog/items/search/suggestions`, { params });
  }

  // Recherche avec pagination complète
  searchCatalogItems(query: string, pageIndex: number = 1, pageSize: number = 10): Observable<PaginatedItems<CatalogItem>> {
    const params = new HttpParams()
      .set('q', query)
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedItems<CatalogItem>>(`${this.apiUrl}/catalog/items/search`, { params });
  }

  // ====================================
  // RECOMMANDATIONS
  // ====================================

  // Récupère les produits similaires
  getRelatedProducts(productId: string, limit: number = 8): Observable<CatalogItem[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<CatalogItem[]>(`${this.apiUrl}/catalog/items/${productId}/related`, { params });
  }

  // Récupère les produits les mieux notés
  getTopRatedProducts(limit: number = 8): Observable<CatalogItem[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<CatalogItem[]>(`${this.apiUrl}/catalog/items/recommendations/top-rated`, { params });
  }

  // Récupère les nouveautés
  getNewArrivals(limit: number = 8): Observable<CatalogItem[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<CatalogItem[]>(`${this.apiUrl}/catalog/items/recommendations/new-arrivals`, { params });
  }

  // Récupère les meilleures ventes
  getBestSellers(limit: number = 8): Observable<CatalogItem[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<CatalogItem[]>(`${this.apiUrl}/catalog/items/recommendations/best-sellers`, { params });
  }

  // Récupère toutes les recommandations pour la page d'accueil
  getHomeRecommendations(limit: number = 8): Observable<HomeRecommendations> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<HomeRecommendations>(`${this.apiUrl}/catalog/items/recommendations/home`, { params });
  }
}
