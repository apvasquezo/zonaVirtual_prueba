import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { TransaccionService } from '../../core/services/transaccion.service';
import { ComercioListItem, MEDIOS_PAGO, Transaccion } from '../../core/models/models';

@Component({
  selector: 'app-pagador-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pagador-dashboard.component.html',
  styleUrl: './pagador-dashboard.component.css'
})
export class PagadorDashboardComponent implements OnInit {
  mediosPago = MEDIOS_PAGO;
  comercios = signal<ComercioListItem[]>([]);
  pagos = signal<Transaccion[]>([]);
  cargando = signal(false);
  error = signal<string | null>(null);
  exito = signal<string | null>(null);

  transCodigo: number | null = null;
  comercioId: number | null = null;
  transMedioPago: number | null = null;
  transTotal: number | null = null;
  transConcepto = '';

  constructor(
    public auth: AuthService,
    private transaccionService: TransaccionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarComercios();
    this.cargarPagos();
  }

  cargarComercios() {
    this.transaccionService.listarComercios().subscribe({
      next: res => this.comercios.set(res)
    });
  }

  cargarPagos() {
    this.cargando.set(true);
    this.transaccionService.listarPorPagador().subscribe({
      next: res => { this.pagos.set(res); this.cargando.set(false); },
      error: () => this.cargando.set(false)
    });
  }

  registrarPago() {
    this.error.set(null);
    this.exito.set(null);

    if (!this.transCodigo || !this.comercioId || !this.transMedioPago || !this.transTotal || !this.transConcepto.trim()) {
      this.error.set('Completa todos los campos del pago.');
      return;
    }

    this.transaccionService.crearPago({
      transCodigo: this.transCodigo,
      comercioId: this.comercioId,
      transMedioPago: this.transMedioPago,
      transTotal: this.transTotal,
      transConcepto: this.transConcepto.trim()
    }).subscribe({
      next: () => {
        this.exito.set('Pago registrado correctamente.');
        this.transCodigo = null;
        this.comercioId = null;
        this.transMedioPago = null;
        this.transTotal = null;
        this.transConcepto = '';
        this.cargarPagos();
      },
      error: err => this.error.set(err?.error?.mensaje ?? 'No se pudo registrar el pago.')
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
