import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ComercioListItem, Transaccion, TransaccionesComercioResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class TransaccionService {
  private baseUrl = `${environment.apiUrl}/transacciones`;

  constructor(private http: HttpClient) {}

  listarComercios(): Observable<ComercioListItem[]> {
    return this.http.get<ComercioListItem[]>(`${this.baseUrl}/comercios`);
  }

  listarPorPagador(): Observable<Transaccion[]> {
    return this.http.get<Transaccion[]>(`${this.baseUrl}/pagador`);
  }

  crearPago(payload: { transCodigo: number; transMedioPago: number; comercioId: number; transTotal: number; transConcepto: string; }): Observable<Transaccion> {
    return this.http.post<Transaccion>(`${this.baseUrl}/pagador`, payload);
  }

  listarPorComercio(filtros: { fecha?: string; transCodigo?: number; usuarioNombre?: string }): Observable<TransaccionesComercioResponse> {
    let params = new HttpParams();
    if (filtros.fecha) params = params.set('fecha', filtros.fecha);
    if (filtros.transCodigo) params = params.set('transCodigo', filtros.transCodigo);
    if (filtros.usuarioNombre) params = params.set('usuarioNombre', filtros.usuarioNombre);
    return this.http.get<TransaccionesComercioResponse>(`${this.baseUrl}/comercio`, { params });
  }

  actualizar(id: number, payload: { transMedioPago: number; transEstado: number; transTotal: number; transConcepto: string; }): Observable<Transaccion> {
    return this.http.put<Transaccion>(`${this.baseUrl}/comercio/${id}`, payload);
  }
}
