import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from './api-config.service';

interface AuthResponse {
  token: string;
  name: string;
  email: string;
}

export interface CurrentUser {
  name: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'cv_token';
  private readonly USER_KEY = 'cv_user';

  currentUser = signal<CurrentUser | null>(this.loadUserFromStorage());
  isAuthenticated = computed(() => this.currentUser() !== null);

  constructor(private http: HttpClient, private config: ApiConfigService) {}

  private get apiUrl() { return this.config.apiUrl; }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, { email, password });
  }

  register(name: string, email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register`, { name, email, password });
  }

  updateProfile(name: string): Observable<AuthResponse> {
    return this.http.put<AuthResponse>(`${this.apiUrl}/auth/profile`, { name });
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/auth/password`, { currentPassword, newPassword });
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  saveSession(res: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify({ name: res.name, email: res.email }));
    this.currentUser.set({ name: res.name, email: res.email });
  }

  private loadUserFromStorage(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(this.USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
