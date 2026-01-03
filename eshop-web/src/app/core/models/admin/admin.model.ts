export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isEmailConfirmed: boolean;
  isActive: boolean;
  roles: string[];
  createdAt: Date;
  lastLoginAt?: Date;
}

export interface Role {
  id: string;
  name: string;
  normalizedName: string;
}

export interface AssignRoleRequest {
  userId: string;
  roleName: string;
}

export interface CatalogBrand {
  id: string;
  brand: string;
  createdAt: Date;
  modifiedAt?: Date;
}

export interface CatalogType {
  id: string;
  type: string;
  createdAt: Date;
  modifiedAt?: Date;
}

export interface CreateCatalogItemRequest {
  name: string;
  description: string;
  price: number;
  pictureFileName: string;
  catalogTypeId: string;
  catalogBrandId: string;
  availableStock: number;
  restockThreshold: number;
  maxStockThreshold: number;
}

export interface UpdateStockRequest {
  productId: string;
  quantity: number;
}

export interface OrderSummary {
  id: string;
  customerId: string;
  orderStatus: string;
  totalAmount: number;
  orderDate: Date;
  deliveryDate?: Date;
  itemCount: number;
  customerEmail: string;
}

export interface OrderDetails {
  id: string;
  customerId: string;
  orderStatus: string;
  totalAmount: number;
  orderDate: Date;
  deliveryDate?: Date;
  shippingAddress: string;
  billingAddress: string;
  paymentMethod: string;
  customerEmail: string;
  customerPhone: string;
  items: OrderItem[];
  createdAt: Date;
  modifiedAt?: Date;
}

export interface OrderItem {
  catalogItemId: string;
  productName: string;
  unitPrice: number;
  discount: number;
  quantity: number;
  pictureUrl: string;
}

export interface AdminStats {
  totalUsers: number;
  totalProducts: number;
  totalOrders: number;
  pendingOrders: number;
  totalRevenue: number;
  recentOrders: OrderSummary[];
}
