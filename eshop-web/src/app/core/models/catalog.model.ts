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
}
