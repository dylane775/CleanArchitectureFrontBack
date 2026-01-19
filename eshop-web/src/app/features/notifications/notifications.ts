import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { Notification, NOTIFICATION_ICONS, NOTIFICATION_COLORS, NotificationType } from '../../core/models/notification.model';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notifications.html',
  styleUrl: './notifications.scss'
})
export class NotificationsComponent implements OnInit {
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);

  notifications = signal<Notification[]>([]);
  isLoading = signal(true);
  selectedFilter = signal<'all' | 'unread'>('all');

  get filteredNotifications() {
    const filter = this.selectedFilter();
    const notifs = this.notifications();

    if (filter === 'unread') {
      return notifs.filter(n => !n.isRead);
    }
    return notifs;
  }

  get unreadCount() {
    return this.notifications().filter(n => !n.isRead).length;
  }

  ngOnInit(): void {
    this.loadNotifications();
  }

  async loadNotifications(): Promise<void> {
    const user = this.authService.currentUser();
    if (!user) return;

    this.isLoading.set(true);
    try {
      const notifications = await this.notificationService
        .getNotifications(user.id, 0, 100)
        .toPromise();
      this.notifications.set(notifications || []);
    } catch (error) {
      console.error('Error loading notifications:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  setFilter(filter: 'all' | 'unread'): void {
    this.selectedFilter.set(filter);
  }

  getIcon(type: string): string {
    return NOTIFICATION_ICONS[type as NotificationType] || 'notifications';
  }

  getColor(type: string): string {
    return NOTIFICATION_COLORS[type as NotificationType] || '#666';
  }

  getTimeAgo(date: Date | string): string {
    const now = new Date();
    const notifDate = new Date(date);
    const diffMs = now.getTime() - notifDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'À l\'instant';
    if (diffMins < 60) return `Il y a ${diffMins} min`;
    if (diffHours < 24) return `Il y a ${diffHours}h`;
    if (diffDays < 7) return `Il y a ${diffDays}j`;
    return notifDate.toLocaleDateString('fr-FR', {
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  }

  async markAsRead(notification: Notification): Promise<void> {
    if (notification.isRead) return;

    await this.notificationService.markAsReadAndUpdate(notification.id);
    this.notifications.update(notifs =>
      notifs.map(n => n.id === notification.id ? { ...n, isRead: true } : n)
    );
  }

  async markAllAsRead(): Promise<void> {
    const user = this.authService.currentUser();
    if (!user) return;

    await this.notificationService.markAllAsReadAndUpdate(user.id);
    this.notifications.update(notifs =>
      notifs.map(n => ({ ...n, isRead: true }))
    );
  }

  async deleteNotification(notification: Notification, event: Event): Promise<void> {
    event.stopPropagation();
    await this.notificationService.deleteNotificationAndUpdate(notification.id);
    this.notifications.update(notifs =>
      notifs.filter(n => n.id !== notification.id)
    );
  }

  async deleteAllNotifications(): Promise<void> {
    const user = this.authService.currentUser();
    if (!user) return;

    if (confirm('Êtes-vous sûr de vouloir supprimer toutes les notifications ?')) {
      await this.notificationService.deleteAllNotifications(user.id).toPromise();
      this.notifications.set([]);
    }
  }

  // Test function
  async sendTestNotification(): Promise<void> {
    const user = this.authService.currentUser();
    if (!user) return;

    this.notificationService.sendTestNotification(user.id).subscribe(() => {
      this.loadNotifications();
    });
  }
}
