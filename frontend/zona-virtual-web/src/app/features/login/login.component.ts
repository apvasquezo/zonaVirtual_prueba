import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Perfil } from '../../core/models/models';

type Etapa = 'seleccion' | 'login' | 'registrar';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  Perfil = Perfil;
  etapa = signal<Etapa>('seleccion');
  cargando = signal(false);
  error = signal<string | null>(null);
  mensajeExito = signal<string | null>(null);

  perfil: Perfil = Perfil.Pagador;

  // login
  email = '';
  loginPassword = '';

  // registro
  identificador = '';
  nombre = '';
  registroEmail = '';
  password = '';
  comercioDireccion = '';

  constructor(private auth: AuthService, private router: Router) {}

  elegirPerfil(p: Perfil) {
    this.perfil = p;
    this.etapa.set('login');
    this.error.set(null);
    this.mensajeExito.set(null);
    this.email = '';
    this.loginPassword = '';
  }

  volverASeleccion() {
    this.etapa.set('seleccion');
    this.error.set(null);
    this.mensajeExito.set(null);
  }

  irARegistro() {
    this.etapa.set('registrar');
    this.error.set(null);
    this.mensajeExito.set(null);
    this.identificador = '';
    this.nombre = '';
    this.registroEmail = '';
    this.password = '';
    this.comercioDireccion = '';
  }

  irALogin() {
    this.etapa.set('login');
    this.error.set(null);
    this.mensajeExito.set(null);
  }

  ingresar() {
    if (!this.email.trim() || !this.loginPassword) {
      this.error.set('Ingresa tu usuario y contraseña.');
      return;
    }
    this.error.set(null);
    this.mensajeExito.set(null);
    this.cargando.set(true);
    this.auth.login(this.perfil, this.email.trim(), this.loginPassword).subscribe({
      next: () => this.redirigir(),
      error: () => {
        this.cargando.set(false);
        this.error.set('Usuario o contraseña incorrectos.');
      }
    });
  }

  registrar() {
    if (!this.identificador.trim() || !this.nombre.trim() || !this.registroEmail.trim() || this.password.length < 6) {
      this.error.set('Completa identificación, nombre, correo y una contraseña de al menos 6 caracteres.');
      return;
    }
    this.error.set(null);
    this.cargando.set(true);
    const emailRegistrado = this.registroEmail.trim();

    this.auth.registrar({
      perfil: this.perfil,
      identificador: this.identificador.trim(),
      nombre: this.nombre.trim(),
      email: emailRegistrado,
      password: this.password,
      comercioDireccion: this.perfil === Perfil.Comercio ? this.comercioDireccion.trim() : undefined
    }).subscribe({
      next: () => {
        this.cargando.set(false);
        this.email = emailRegistrado;
        this.loginPassword = '';
        this.mensajeExito.set('Cuenta creada correctamente. Ahora ingresa con tu usuario y contraseña.');
        this.etapa.set('login');
      },
      error: err => {
        this.cargando.set(false);
        this.error.set(err?.error?.mensaje ?? 'No se pudo crear la cuenta.');
      }
    });
  }

  private redirigir() {
    this.cargando.set(false);
    this.router.navigate([this.perfil === Perfil.Pagador ? '/pagador' : '/comercio']);
  }
}