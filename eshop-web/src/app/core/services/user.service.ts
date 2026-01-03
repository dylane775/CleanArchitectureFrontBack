import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/auth.model';
import { environment } from '../../../environments/environment';

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = environment.identityApiUrl;

  constructor(private http: HttpClient) {}

  getUserProfile(userId: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/users/${userId}`);
  }

  updateProfile(userId: string, request: UpdateProfileRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/users/${userId}`, request);
  }

  changePassword(userId: string, request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/${userId}/change-password`, request);
  }

  deleteAccount(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${userId}`);
  }
}
