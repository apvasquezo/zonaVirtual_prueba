export enum Perfil {
  Pagador = 1,
  Comercio = 2
}

export interface CheckUsuarioResponse {
  existePersona: boolean;
  tieneCuenta: boolean;
  nombreSugerido?: string;
}

export interface LoginResponse {
  token: string;
  expiracion: string;
  perfil: Perfil;
  nombre: string;
  referenciaId: number;
}

export interface ComercioListItem {
  id: number;
  comercioCodigo: string;
  comercioNombre: string;
  comercioNit: string;
}

export interface Transaccion {
  id: number;
  transCodigo: number;
  transMedioPago: number;
  transMedioPagoNombre: string;
  transEstado: number;
  transEstadoNombre: string;
  transTotal: number;
  transFecha: string;
  transConcepto: string;
  comercioNombre: string;
  usuarioPagadorNombre: string;
}

export interface TransaccionesComercioResponse {
  transacciones: Transaccion[];
  totalTransacciones: number;
}

export const ESTADOS = [
  { valor: 1, nombre: 'Aprobada' },
  { valor: 1000, nombre: 'Rechazada' },
  { valor: 999, nombre: 'Pendiente' },
  { valor: 1001, nombre: 'Rechazada SR' }
];

export const MEDIOS_PAGO = [
  { valor: 32, nombre: 'Tarjeta de Crédito' },
  { valor: 29, nombre: 'PSE' },
  { valor: 41, nombre: 'Gana' },
  { valor: 42, nombre: 'Caja' }
];
