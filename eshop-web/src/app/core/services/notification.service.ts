import { Injectable, signal, computed, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  Notification,
  NotificationCount,
  CreateNotificationDto,
} from '../models/notification.model';

declare const signalR: any;

@Injectable({
  providedIn: 'root',
})
export class NotificationService implements OnDestroy {
  private apiUrl = environment.notificationApiUrl;
  private hubUrl = environment.notificationHubUrl;
  private hubConnection: any = null;
  private destroy$ = new Subject<void>();

  // Signals pour l'état réactif
  private _notifications = signal<Notification[]>([]);
  private _unreadCount = signal<number>(0);
  private _isConnected = signal<boolean>(false);
  private _connectionError = signal<string | null>(null);

  // Exposer en lecture seule
  readonly notifications = this._notifications.asReadonly();
  readonly unreadCount = this._unreadCount.asReadonly();
  readonly isConnected = this._isConnected.asReadonly();
  readonly connectionError = this._connectionError.asReadonly();

  // Computed signals
  readonly hasUnread = computed(() => this._unreadCount() > 0);
  readonly recentNotifications = computed(() =>
    this._notifications().slice(0, 5)
  );

  // Event emitter pour les nouvelles notifications
  private newNotificationSubject = new BehaviorSubject<Notification | null>(
    null
  );
  newNotification$ = this.newNotificationSubject.asObservable();

  constructor(private http: HttpClient) {}

  ngOnDestroy(): void {
    this.disconnect();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Connexion SignalR
  async connect(userId: string): Promise<void> {
    if (this.hubConnection) {
      await this.disconnect();
    }

    try {
      // Chargement dynamique de SignalR (pour éviter les erreurs si non installé)
      if (typeof signalR === 'undefined') {
        console.warn(
          'SignalR not loaded. Using polling mode for notifications.'
        );
        this._connectionError.set('SignalR non disponible');
        // Fallback : charger les notifications périodiquement
        this.startPolling(userId);
        return;
      }

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.hubUrl}?userId=${userId}`)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Écouter les événements
      this.hubConnection.on(
        'ReceiveNotification',
        (notification: Notification) => {
          this.handleNewNotification(notification);
        }
      );

      this.hubConnection.on('UnreadCountUpdated', (count: number) => {
        this._unreadCount.set(count);
      });

      this.hubConnection.onreconnecting(() => {
        this._isConnected.set(false);
        this._connectionError.set('Reconnexion en cours...');
      });

      this.hubConnection.onreconnected(() => {
        this._isConnected.set(true);
        this._connectionError.set(null);
      });

      this.hubConnection.onclose(() => {
        this._isConnected.set(false);
      });

      await this.hubConnection.start();
      this._isConnected.set(true);
      this._connectionError.set(null);

      // Charger les notifications initiales
      await this.loadNotifications(userId);
      await this.loadUnreadCount(userId);
    } catch (error) {
      console.error('Erreur de connexion SignalR:', error);
      this._connectionError.set('Impossible de se connecter au serveur');
      this._isConnected.set(false);
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
      } catch (error) {
        console.error('Erreur lors de la déconnexion:', error);
      }
      this.hubConnection = null;
    }
    this._isConnected.set(false);
  }

  private startPolling(userId: string): void {
    // Mode polling si SignalR n'est pas disponible
    this.loadNotifications(userId);
    this.loadUnreadCount(userId);

    // Polling toutes les 30 secondes
    setInterval(() => {
      this.loadNotifications(userId);
      this.loadUnreadCount(userId);
    }, 30000);
  }

  private handleNewNotification(notification: Notification): void {
    this._notifications.update((notifications) => [
      notification,
      ...notifications,
    ]);
    this._unreadCount.update((count) => count + 1);
    this.newNotificationSubject.next(notification);
  }

  // API calls
  async loadNotifications(
    userId: string,
    skip = 0,
    take = 20
  ): Promise<void> {
    try {
      const notifications = await this.http
        .get<Notification[]>(
          `${this.apiUrl}/notifications/user/${userId}?skip=${skip}&take=${take}`
        )
        .toPromise();
      this._notifications.set(notifications || []);
    } catch (error) {
      console.error('Erreur lors du chargement des notifications:', error);
    }
  }

  async loadUnreadCount(userId: string): Promise<void> {
    try {
      const count = await this.http
        .get<NotificationCount>(
          `${this.apiUrl}/notifications/user/${userId}/count`
        )
        .toPromise();
      this._unreadCount.set(count?.unreadCount || 0);
    } catch (error) {
      console.error('Erreur lors du chargement du compteur:', error);
    }
  }

  getNotifications(
    userId: string,
    skip = 0,
    take = 20
  ): Observable<Notification[]> {
    return this.http.get<Notification[]>(
      `${this.apiUrl}/notifications/user/${userId}?skip=${skip}&take=${take}`
    );
  }

  getUnreadNotifications(userId: string): Observable<Notification[]> {
    return this.http.get<Notification[]>(
      `${this.apiUrl}/notifications/user/${userId}/unread`
    );
  }

  getNotificationCount(userId: string): Observable<NotificationCount> {
    return this.http.get<NotificationCount>(
      `${this.apiUrl}/notifications/user/${userId}/count`
    );
  }

  createNotification(dto: CreateNotificationDto): Observable<Notification> {
    return this.http.post<Notification>(`${this.apiUrl}/notifications`, dto);
  }

  markAsRead(notificationId: string): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/notifications/${notificationId}/read`,
      {}
    );
  }

  async markAsReadAndUpdate(notificationId: string): Promise<void> {
    try {
      await this.http
        .put<void>(`${this.apiUrl}/notifications/${notificationId}/read`, {})
        .toPromise();

      this._notifications.update((notifications) =>
        notifications.map((n) =>
          n.id === notificationId ? { ...n, isRead: true } : n
        )
      );
      this._unreadCount.update((count) => Math.max(0, count - 1));
    } catch (error) {
      console.error('Erreur lors du marquage comme lu:', error);
    }
  }

  markAllAsRead(userId: string): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/notifications/user/${userId}/read-all`,
      {}
    );
  }

  async markAllAsReadAndUpdate(userId: string): Promise<void> {
    try {
      await this.http
        .put<void>(`${this.apiUrl}/notifications/user/${userId}/read-all`, {})
        .toPromise();

      this._notifications.update((notifications) =>
        notifications.map((n) => ({ ...n, isRead: true }))
      );
      this._unreadCount.set(0);
    } catch (error) {
      console.error('Erreur lors du marquage de tout comme lu:', error);
    }
  }

  deleteNotification(notificationId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/notifications/${notificationId}`
    );
  }

  async deleteNotificationAndUpdate(notificationId: string): Promise<void> {
    try {
      const notification = this._notifications().find(
        (n) => n.id === notificationId
      );
      await this.http
        .delete<void>(`${this.apiUrl}/notifications/${notificationId}`)
        .toPromise();

      this._notifications.update((notifications) =>
        notifications.filter((n) => n.id !== notificationId)
      );

      if (notification && !notification.isRead) {
        this._unreadCount.update((count) => Math.max(0, count - 1));
      }
    } catch (error) {
      console.error('Erreur lors de la suppression:', error);
    }
  }

  deleteAllNotifications(userId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/notifications/user/${userId}/all`
    );
  }

  // Test endpoint
  sendTestNotification(userId: string): Observable<Notification> {
    return this.http.post<Notification>(
      `${this.apiUrl}/notifications/test/${userId}`,
      {}
    );
  }
}
