import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './auth.component.scss'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  name = '';
  email = '';
  password = '';
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  submit(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.register({ email: this.email, password: this.password, name: this.name }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/exam-setup']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Não foi possível criar a conta.');
      }
    });
  }
}
