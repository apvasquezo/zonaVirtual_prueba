import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CheckUsuarioResponse, LoginResponse, Perfil } from '../models/models';

const STORAGE_KEY = 'zv_session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = `${environment.apiUrl}/auth`;

  session = signal<LoginResponse | null>(this.loadSession());

  constructor(private http: HttpClient) {}

  verificar(perfil: Perfil, identificador: string): Observable<CheckUsuarioResponse> {
    return this.http.post<CheckUsuarioResponse>(`${this.baseUrl}/verificar`, { perfil, identificador });
  }

  registrar(payload: {
    perfil: Perfil; identificador: string; nombre: string; email: string; password: string; comercioDireccion?: string;
  }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/registro`, payload);
  }

  login(perfil: Perfil, username: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, { perfil, username, password })
      .pipe(tap(res => this.guardarSesion(res)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  private guardarSesion(res: LoginResponse) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(res));
    this.session.set(res);
  }

  private loadSession(): LoginResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) as LoginResponse : null;
  }
}
