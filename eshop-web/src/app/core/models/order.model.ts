export interface Order {
  id: string;
  customerId: string;
  orderStatus: string;
  totalAmount: number;
  orderDate: string;
  deliveryDate?: string;
  shippingAddress: string;
  billingAddress: string;
  paymentMethod: string;
  customerEmail: string;
  customerPhone?: string;
  items: OrderItem[];
  totalItemCount: number;
  subtotal: number;
  totalDiscount: number;
  createdAt: string;
  createdBy: string;
  modifiedAt?: string;
  modifiedBy?: string;
}

export interface OrderItem {
  id: string;
  catalogItemId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  pictureUrl?: string;
  discount: number;
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

// Enhanced models for checkout process
export interface CheckoutRequest {
  customerId: string;
  shippingAddress: string;
  billingAddress: string;
  paymentMethod: string;
  customerEmail: string;
  customerPhone?: string;
  items: CheckoutItem[];
}

export interface CheckoutItem {
  catalogItemId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  pictureUrl?: string;
  discount: number;
}

export interface CheckoutFormData {
  shippingAddress: Address;
  billingAddress: Address;
  sameAsBilling: boolean;
  paymentMethod: PaymentMethod;
  cardNumber?: string;
  cardName?: string;
  cardExpiry?: string;
  cardCvv?: string;
  email: string;
  phone?: string;
}

export interface PaymentMethod {
  type: 'CreditCard' | 'DebitCard' | 'PayPal' | 'BankTransfer' | 'CashOnDelivery';
  label: string;
}

export const PAYMENT_METHODS: PaymentMethod[] = [
  { type: 'CreditCard', label: 'Credit Card' },
  { type: 'DebitCard', label: 'Debit Card' },
  { type: 'PayPal', label: 'PayPal' },
  { type: 'BankTransfer', label: 'Bank Transfer' },
  { type: 'CashOnDelivery', label: 'Cash on Delivery' }
];

// Helper function to format address as string for backend
export function formatAddressAsString(address: Address): string {
  return `${address.street}, ${address.city}, ${address.state}, ${address.zipCode}, ${address.country}`;
}
