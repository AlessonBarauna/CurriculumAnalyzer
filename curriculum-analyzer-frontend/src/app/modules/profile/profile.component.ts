import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../shared/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent {
  profileForm: FormGroup;
  passwordForm: FormGroup;
  savingProfile = signal(false);
  savingPassword = signal(false);

  constructor(
    private fb: FormBuilder,
    public authService: AuthService,
    private toast: ToastService
  ) {
    this.profileForm = this.fb.group({
      name: [authService.currentUser()?.name ?? '', [Validators.required, Validators.minLength(2)]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword:     ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    const { name } = this.profileForm.value;

    this.savingProfile.set(true);
    this.authService.updateProfile(name).subscribe({
      next: (res) => {
        this.authService.saveSession(res);
        this.savingProfile.set(false);
        this.toast.success('Nome atualizado com sucesso!');
      },
      error: (err) => {
        this.savingProfile.set(false);
        this.toast.error(err?.error?.error || 'Erro ao atualizar perfil.');
      }
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;
    const { currentPassword, newPassword, confirmPassword } = this.passwordForm.value;

    if (newPassword !== confirmPassword) {
      this.toast.error('As novas senhas não coincidem.');
      return;
    }

    this.savingPassword.set(true);
    this.authService.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.passwordForm.reset();
        this.savingPassword.set(false);
        this.toast.success('Senha alterada com sucesso!');
      },
      error: (err) => {
        this.savingPassword.set(false);
        this.toast.error(err?.error?.error || 'Erro ao alterar senha.');
      }
    });
  }
}
