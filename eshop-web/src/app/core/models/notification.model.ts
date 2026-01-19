export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  imageUrl?: string;
  actionUrl?: string;
  relatedEntityId?: string;
  isRead: boolean;
  readAt?: Date;
  createdAt: Date;
  metadata?: Record<string, string>;
}

export type NotificationType =
  | 'ORDER_CONFIRMED'
  | 'ORDER_SHIPPED'
  | 'ORDER_DELIVERED'
  | 'ORDER_CANCELLED'
  | 'PRICE_DROP'
  | 'BACK_IN_STOCK'
  | 'REVIEW_REMINDER'
  | 'PROMO_ALERT'
  | 'WELCOME_MESSAGE'
  | 'PAYMENT_RECEIVED'
  | 'PAYMENT_FAILED';

export interface NotificationCount {
  unreadCount: number;
  totalCount: number;
}

export interface CreateNotificationDto {
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  imageUrl?: string;
  actionUrl?: string;
  relatedEntityId?: string;
  metadata?: Record<string, string>;
}

export const NOTIFICATION_ICONS: Record<NotificationType, string> = {
  ORDER_CONFIRMED: 'check_circle',
  ORDER_SHIPPED: 'local_shipping',
  ORDER_DELIVERED: 'inventory_2',
  ORDER_CANCELLED: 'cancel',
  PRICE_DROP: 'trending_down',
  BACK_IN_STOCK: 'inventory',
  REVIEW_REMINDER: 'rate_review',
  PROMO_ALERT: 'local_offer',
  WELCOME_MESSAGE: 'waving_hand',
  PAYMENT_RECEIVED: 'payments',
  PAYMENT_FAILED: 'error'
};

export const NOTIFICATION_COLORS: Record<NotificationType, string> = {
  ORDER_CONFIRMED: '#4caf50',
  ORDER_SHIPPED: '#2196f3',
  ORDER_DELIVERED: '#8bc34a',
  ORDER_CANCELLED: '#f44336',
  PRICE_DROP: '#ff9800',
  BACK_IN_STOCK: '#9c27b0',
  REVIEW_REMINDER: '#ffeb3b',
  PROMO_ALERT: '#e91e63',
  WELCOME_MESSAGE: '#00bcd4',
  PAYMENT_RECEIVED: '#4caf50',
  PAYMENT_FAILED: '#f44336'
};
