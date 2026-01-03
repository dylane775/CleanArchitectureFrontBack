import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, User, RefreshTokenRequest } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.identityApiUrl;
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  currentUser = signal<User | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, request)
      .pipe(
        tap(response => this.handleAuthResponse(response))
      );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register`, request)
      .pipe(
        tap(response => this.handleAuthResponse(response))
      );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const request: RefreshTokenRequest = { refreshToken };
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/refresh`, request)
      .pipe(
        tap(response => this.handleAuthResponse(response))
      );
  }

  logout(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);

    // If backend doesn't return user object, decode it from JWT token
    let user = response.user;
    if (!user) {
      user = this.decodeUserFromToken(response.accessToken);
    }

    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
    this.isAuthenticated.set(true);
  }

  private loadUserFromStorage(): void {
    const token = this.getAccessToken();
    const userStr = localStorage.getItem('user');

    if (token) {
      try {
        // Vérifier si le token n'est pas expiré
        if (this.isTokenExpired(token)) {
          console.log('Token expiré, déconnexion...');
          this.logout();
          return;
        }

        // Try to parse user from localStorage, or decode from token if invalid
        let user: User | null = null;
        if (userStr && userStr !== 'undefined') {
          try {
            user = JSON.parse(userStr) as User;
          } catch {
            // If parsing fails, decode from token
            user = this.decodeUserFromToken(token);
            localStorage.setItem('user', JSON.stringify(user));
          }
        } else {
          // If user is missing or "undefined", decode from token
          user = this.decodeUserFromToken(token);
          localStorage.setItem('user', JSON.stringify(user));
        }

        this.currentUser.set(user);
        this.isAuthenticated.set(true);
      } catch (error) {
        console.error('Erreur lors du chargement de l\'utilisateur depuis le storage:', error);
        this.logout();
      }
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expirationDate = new Date(payload.exp * 1000);
      return expirationDate < new Date();
    } catch (error) {
      return true;
    }
  }

  private decodeUserFromToken(token: string): User {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      // Extract user information from JWT claims
      return {
        id: payload.nameid || payload.sub || '',
        email: payload.email || '',
        firstName: payload.FirstName || payload.given_name || '',
        lastName: payload.LastName || payload.family_name || '',
        roles: Array.isArray(payload.role) ? payload.role : (payload.role ? [payload.role] : [])
      };
    } catch (error) {
      console.error('Error decoding user from token:', error);
      throw new Error('Invalid token');
    }
  }
}
