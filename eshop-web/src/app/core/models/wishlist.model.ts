export interface WishlistItem {
  id: string;
  catalogItemId: string;
  productName: string;
  price: number;
  pictureUrl: string;
  brandName: string;
  categoryName: string;
  addedAt: Date;
}

export interface AddToWishlistRequest {
  catalogItemId: string;
  productName: string;
  price: number;
  pictureUrl: string;
  brandName: string;
  categoryName: string;
}

export interface ToggleWishlistResponse {
  added: boolean;
  message: string;
  item?: WishlistItem;
}
