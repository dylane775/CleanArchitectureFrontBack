import { Component, inject, OnInit, OnDestroy, signal, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Notification, NOTIFICATION_ICONS, NOTIFICATION_COLORS, NotificationType } from '../../../core/models/notification.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.scss'
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);
  private destroy$ = new Subject<void>();

  isOpen = signal(false);
  showToast = signal(false);
  toastNotification = signal<Notification | null>(null);

  // Accès aux signaux du service
  get notifications() { return this.notificationService.recentNotifications; }
  get unreadCount() { return this.notificationService.unreadCount; }
  get hasUnread() { return this.notificationService.hasUnread; }
  get isConnected() { return this.notificationService.isConnected; }

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user) {
      this.notificationService.connect(user.id);

      // Écouter les nouvelles notifications pour le toast
      this.notificationService.newNotification$
        .pipe(takeUntil(this.destroy$))
        .subscribe(notification => {
          if (notification) {
            this.showNotificationToast(notification);
          }
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.notificationService.disconnect();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  toggleDropdown(): void {
    this.isOpen.update(v => !v);
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
    return notifDate.toLocaleDateString('fr-FR');
  }

  async markAsRead(notification: Notification, event: Event): Promise<void> {
    event.stopPropagation();
    if (!notification.isRead) {
      await this.notificationService.markAsReadAndUpdate(notification.id);
    }
  }

  async markAllAsRead(): Promise<void> {
    const user = this.authService.currentUser();
    if (user) {
      await this.notificationService.markAllAsReadAndUpdate(user.id);
    }
  }

  async deleteNotification(notification: Notification, event: Event): Promise<void> {
    event.stopPropagation();
    await this.notificationService.deleteNotificationAndUpdate(notification.id);
  }

  onNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsReadAndUpdate(notification.id);
    }

    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    }

    this.isOpen.set(false);
  }

  viewAllNotifications(): void {
    this.isOpen.set(false);
    this.router.navigate(['/notifications']);
  }

  private showNotificationToast(notification: Notification): void {
    this.toastNotification.set(notification);
    this.showToast.set(true);

    setTimeout(() => {
      this.showToast.set(false);
      this.toastNotification.set(null);
    }, 5000);
  }

  dismissToast(): void {
    this.showToast.set(false);
    this.toastNotification.set(null);
  }

  // Pour les tests
  async sendTestNotification(): Promise<void> {
    const user = this.authService.currentUser();
    if (user) {
      this.notificationService.sendTestNotification(user.id).subscribe();
    }
  }
}
