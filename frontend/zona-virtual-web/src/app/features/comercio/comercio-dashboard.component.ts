import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { TransaccionService } from '../../core/services/transaccion.service';
import { ESTADOS, MEDIOS_PAGO, Transaccion } from '../../core/models/models';

@Component({
  selector: 'app-comercio-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comercio-dashboard.component.html',
  styleUrl: './comercio-dashboard.component.css'
})
export class ComercioDashboardComponent implements OnInit {
  estados = ESTADOS;
  mediosPago = MEDIOS_PAGO;

  transacciones = signal<Transaccion[]>([]);
  total = signal(0);
  cargando = signal(false);

  filtroFecha = '';
  filtroCodigo: number | null = null;
  filtroUsuario = '';

  editando = signal<Transaccion | null>(null);
  editMedioPago: number | null = null;
  editEstado: number | null = null;
  editTotal: number | null = null;
  editConcepto = '';
  editError = signal<string | null>(null);
  guardando = signal(false);

  constructor(
    public auth: AuthService,
    private transaccionService: TransaccionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.buscar();
  }

  buscar() {
    this.cargando.set(true);
    this.transaccionService.listarPorComercio({
      fecha: this.filtroFecha || undefined,
      transCodigo: this.filtroCodigo ?? undefined,
      usuarioNombre: this.filtroUsuario || undefined
    }).subscribe({
      next: res => {
        this.transacciones.set(res.transacciones);
        this.total.set(res.totalTransacciones);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  limpiarFiltros() {
    this.filtroFecha = '';
    this.filtroCodigo = null;
    this.filtroUsuario = '';
    this.buscar();
  }

  puedeEditar(t: Transaccion): boolean {
    return t.transEstado !== 1; // 1 = Aprobada
  }

  abrirEdicion(t: Transaccion) {
    this.editando.set(t);
    this.editMedioPago = t.transMedioPago;
    this.editEstado = t.transEstado;
    this.editTotal = t.transTotal;
    this.editConcepto = t.transConcepto;
    this.editError.set(null);
  }

  cerrarEdicion() {
    this.editando.set(null);
  }

  guardarEdicion() {
    const t = this.editando();
    if (!t || !this.editMedioPago || !this.editEstado || !this.editTotal || !this.editConcepto.trim()) {
      this.editError.set('Completa todos los campos.');
      return;
    }
    this.guardando.set(true);
    this.transaccionService.actualizar(t.id, {
      transMedioPago: this.editMedioPago,
      transEstado: this.editEstado,
      transTotal: this.editTotal,
      transConcepto: this.editConcepto.trim()
    }).subscribe({
      next: () => {
        this.guardando.set(false);
        this.cerrarEdicion();
        this.buscar();
      },
      error: err => {
        this.guardando.set(false);
        this.editError.set(err?.error?.mensaje ?? 'No se pudo actualizar la transacción.');
      }
    });
  }

  estadoClase(estado: number): string {
    if (estado === 1) return 'badge-approved';
    if (estado === 999) return 'badge-pending';
    return 'badge-rejected';
  }

  salir() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
