import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';

type Mode = 'login' | 'register';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  mode = signal<Mode>('login');
  loading = signal(false);
  errorMessage = signal('');

  loginForm: FormGroup;
  registerForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });

    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  setMode(mode: Mode): void {
    this.mode.set(mode);
    this.errorMessage.set('');
  }

  onLogin(): void {
    if (this.loginForm.invalid) return;
    const { email, password } = this.loginForm.value;

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.login(email, password).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.errorMessage.set(err?.error?.error || 'E-mail ou senha inválidos.');
        this.loading.set(false);
      }
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) return;
    const { name, email, password, confirmPassword } = this.registerForm.value;

    if (password !== confirmPassword) {
      this.errorMessage.set('As senhas não coincidem.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.register(name, email, password).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.errorMessage.set(err?.error?.error || 'Erro ao criar conta. Tente novamente.');
        this.loading.set(false);
      }
    });
  }
}
