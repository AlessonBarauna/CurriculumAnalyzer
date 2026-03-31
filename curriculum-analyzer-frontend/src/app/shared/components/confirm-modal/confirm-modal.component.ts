import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-modal.component.html',
  styleUrls: ['./confirm-modal.component.scss']
})
export class ConfirmModalComponent {
  visible = signal(false);
  message = signal('');
  private resolver?: (value: boolean) => void;

  open(message: string): Promise<boolean> {
    this.message.set(message);
    this.visible.set(true);
    return new Promise(resolve => { this.resolver = resolve; });
  }

  confirm(): void {
    this.visible.set(false);
    this.resolver?.(true);
  }

  cancel(): void {
    this.visible.set(false);
    this.resolver?.(false);
  }
}
