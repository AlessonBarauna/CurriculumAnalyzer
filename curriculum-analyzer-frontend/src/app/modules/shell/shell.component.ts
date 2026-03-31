import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';
import { ToastComponent } from '../../shared/components/toast/toast.component';
import { ConfirmModalComponent } from '../../shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastComponent, ConfirmModalComponent],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.scss']
})
export class ShellComponent {
  constructor(public authService: AuthService, private router: Router) {}

  get currentUserFirstName(): string {
    return this.authService.currentUser()?.name?.split(' ')[0] ?? 'Usuário';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}