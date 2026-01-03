export interface Order {
  id: string;
  buyerId: string;
  orderDate: string;
  status: OrderStatus;
  total: number;
  shippingAddress: Address;
  paymentMethod: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  pictureUrl: string;
  unitPrice: number;
  quantity: number;
}

export interface Address {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

export enum OrderStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Shipped = 'Shipped',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled'
}

export interface CreateOrderRequest {
  buyerId: string;
  shippingAddress: Address;
  paymentMethod: string;
}
