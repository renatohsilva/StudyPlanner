import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

const TOKEN_KEY = 'studyplanner.token';
const USER_ID_KEY = 'studyplanner.authUserId';
const NAME_KEY = 'studyplanner.authName';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly userId = signal<string | null>(localStorage.getItem(USER_ID_KEY));
  readonly name = signal<string | null>(localStorage.getItem(NAME_KEY));
  readonly isAuthenticated = computed(() => this.token() !== null);

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE_URL}/auth/register`, request)
      .pipe(tap((response) => this.persistSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE_URL}/auth/login`, request)
      .pipe(tap((response) => this.persistSession(response)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_ID_KEY);
    localStorage.removeItem(NAME_KEY);
    this.token.set(null);
    this.userId.set(null);
    this.name.set(null);
  }

  private persistSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_ID_KEY, response.userId);
    localStorage.setItem(NAME_KEY, response.name);
    this.token.set(response.token);
    this.userId.set(response.userId);
    this.name.set(response.name);
  }
}
