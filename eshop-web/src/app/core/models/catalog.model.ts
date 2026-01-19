export interface CatalogItem {
  id: string;
  name: string;
  description: string;
  price: number;
  pictureUri: string;
  availableStock: number;
  onReorder: boolean;
  catalogTypeId: string;
  catalogTypeName: string;
  catalogBrandId: string;
  catalogBrandName: string;
  specifications?: { [key: string]: string }; // Spécifications dynamiques
  // Review Statistics
  averageRating: number;
  reviewCount: number;
  // Audit
  createdAt: Date;
  createdBy: string;
  modifiedAt?: Date;
  modifiedBy?: string;
}

export interface CatalogType {
  id: string;
  type: string;
}

export interface CatalogBrand {
  id: string;
  brand: string;
}

export interface PaginatedItems<T> {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
}

export interface CreateCatalogItemRequest {
  name: string;
  description?: string;
  price: number;
  pictureFileName: string;
  catalogTypeId: string;
  catalogBrandId: string;
  availableStock: number;
  restockThreshold: number;
  maxStockThreshold: number;
  specifications?: { [key: string]: string }; // Spécifications dynamiques (optionnel)
}

export interface SearchSuggestion {
  id: string;
  name: string;
  category: string;
  brand: string;
  price: number;
  pictureUri: string;
  type: 'product' | 'category' | 'brand';
}

export interface HomeRecommendations {
  topRated: CatalogItem[];
  newArrivals: CatalogItem[];
  bestSellers: CatalogItem[];
}
