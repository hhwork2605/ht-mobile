import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ButtonModule, InputTextModule, PasswordModule, MessageModule],
  template: `
    <div class="login-wrap">
      <div class="login-card">
        <div class="brand">
          <div class="brand-logo"><i class="pi pi-shop"></i></div>
          <div>
            <div class="brand-name">HtMobile</div>
            <div class="brand-sub">Trang quản trị</div>
          </div>
        </div>

        <h1 class="title">Đăng nhập</h1>
        <p class="subtitle">Dùng tài khoản quản trị để tiếp tục.</p>

        @if (error()) {
          <p-message severity="error" [text]="error()!" styleClass="w-full mb-3" />
        }

        <form (ngSubmit)="submit()">
          <label class="lbl">Email</label>
          <input pInputText type="email" name="email" [(ngModel)]="email" class="w-full mb-3"
                 placeholder="admin@htmobile.local" autocomplete="username" />

          <label class="lbl">Mật khẩu</label>
          <p-password name="password" [(ngModel)]="password" [feedback]="false" [toggleMask]="true"
                      styleClass="w-full mb-4" inputStyleClass="w-full" placeholder="••••••••" />

          <p-button type="submit" label="Đăng nhập" [loading]="loading()" styleClass="w-full" />
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-wrap { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: var(--ht-page); padding: 1rem; }
    .login-card { width: 100%; max-width: 380px; background: #fff; border: 1px solid var(--ht-line); border-radius: 16px; padding: 28px; box-shadow: 0 4px 24px rgba(20,23,26,.06); }
    .brand { display: flex; align-items: center; gap: 10px; margin-bottom: 22px; }
    .brand-logo { width: 36px; height: 36px; border-radius: 10px; background: var(--ht-brand); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 18px; }
    .brand-name { font-weight: 700; font-size: 16px; }
    .brand-sub { font-size: 11px; color: var(--ht-ink4); font-weight: 500; }
    .title { font-size: 20px; font-weight: 700; margin: 0 0 4px; }
    .subtitle { font-size: 13px; color: var(--ht-ink5); margin: 0 0 20px; }
    .lbl { display: block; font-size: 12.5px; font-weight: 600; color: var(--ht-ink5); margin-bottom: 6px; }
    .w-full { width: 100%; }
    .mb-3 { margin-bottom: 14px; } .mb-4 { margin-bottom: 20px; }
  `],
})
export class LoginComponent {
  email = 'admin@htmobile.local';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private auth: AuthService, private router: Router) {}

  submit(): void {
    this.error.set(null);
    this.loading.set(true);
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/products']),
      error: (e) => {
        this.error.set(e?.error?.message ?? 'Đăng nhập thất bại.');
        this.loading.set(false);
      },
    });
  }
}
