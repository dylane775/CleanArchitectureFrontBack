export interface BasketItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  pictureUrl: string;
}

export interface Basket {
  id: string;
  buyerId: string;
  items: BasketItem[];
  totalPrice: number;
}

export interface AddBasketItemRequest {
  productId: string;
  quantity: number;
}

export interface UpdateBasketItemRequest {
  quantity: number;
}
