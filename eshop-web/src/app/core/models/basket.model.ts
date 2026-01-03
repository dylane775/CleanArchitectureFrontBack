export interface BasketItem {
  catalogItemId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  pictureUrl: string;
}

export interface Basket {
  id: string;
  customerId: string;
  items: BasketItem[];
  totalPrice: number;
  itemCount: number;
}

export interface CreateBasketRequest {
  customerId: string;
}

export interface AddBasketItemRequest {
  catalogItemId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  pictureUrl: string;
}

export interface UpdateBasketItemRequest {
  catalogItemId: string;
  newQuantity: number;
}
