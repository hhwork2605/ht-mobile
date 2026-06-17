import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../core/auth.service';

interface NavItem { label: string; icon: string; link?: string; disabled?: boolean; }
interface NavGroup { title: string; items: NavItem[]; }

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ButtonModule],
  template: `
    <div class="shell">
      <aside class="sidebar">
        <div class="brand">
          <div class="brand-logo"><i class="pi pi-shop"></i></div>
          <div class="leading">
            <div class="brand-name">HtMobile</div>
            <div class="brand-sub">Trang quản trị</div>
          </div>
        </div>

        <nav class="nav">
          @for (g of groups; track g.title) {
            <div class="nav-group">
              <div class="nav-title">{{ g.title }}</div>
              @for (it of g.items; track it.label) {
                @if (it.disabled) {
                  <span class="nav-item disabled"><i class="pi {{ it.icon }}"></i>{{ it.label }}<span class="soon">Sắp có</span></span>
                } @else {
                  <a class="nav-item" [routerLink]="it.link" routerLinkActive="active">
                    <i class="pi {{ it.icon }}"></i>{{ it.label }}
                  </a>
                }
              }
            </div>
          }
        </nav>
      </aside>

      <div class="main">
        <header class="topbar">
          <div class="page-title">Quản trị</div>
          <div class="user">
            <div class="user-info">
              <div class="user-name">{{ auth.user()?.fullName || 'Admin' }}</div>
              <div class="user-mail">{{ auth.user()?.email }}</div>
            </div>
            <p-button icon="pi pi-sign-out" severity="secondary" [text]="true" (onClick)="logout()" pTooltip="Đăng xuất" />
          </div>
        </header>
        <main class="content"><router-outlet /></main>
      </div>
    </div>
  `,
  styles: [`
    .shell { display: flex; height: 100vh; overflow: hidden; }
    .sidebar { width: 244px; flex: none; background: #fff; border-right: 1px solid var(--ht-line); display: flex; flex-direction: column; }
    .brand { height: 60px; display: flex; align-items: center; gap: 10px; padding: 0 18px; border-bottom: 1px solid var(--ht-line); }
    .brand-logo { width: 32px; height: 32px; border-radius: 10px; background: var(--ht-brand); color: #fff; display: flex; align-items: center; justify-content: center; }
    .brand-name { font-weight: 700; font-size: 15px; }
    .brand-sub { font-size: 10.5px; color: var(--ht-ink4); font-weight: 500; }
    .nav { flex: 1; overflow-y: auto; padding: 12px; }
    .nav-title { font-size: 10.5px; font-weight: 700; text-transform: uppercase; letter-spacing: .7px; color: var(--ht-ink3); padding: 12px 12px 6px; }
    .nav-item { display: flex; align-items: center; gap: 12px; height: 38px; padding: 0 12px; border-radius: 10px; font-size: 13.5px; font-weight: 500; color: var(--ht-ink5); text-decoration: none; cursor: pointer; margin-bottom: 2px; }
    .nav-item:hover { background: var(--ht-surf2); color: var(--ht-ink); }
    .nav-item.active { background: var(--ht-brand); color: #fff; }
    .nav-item.disabled { color: var(--ht-ink3); cursor: not-allowed; }
    .nav-item .soon { margin-left: auto; font-size: 10px; background: var(--ht-surf2); padding: 1px 6px; border-radius: 8px; }
    .main { flex: 1; min-width: 0; display: flex; flex-direction: column; }
    .topbar { height: 60px; flex: none; background: #fff; border-bottom: 1px solid var(--ht-line); display: flex; align-items: center; justify-content: space-between; padding: 0 22px; }
    .page-title { font-size: 17px; font-weight: 700; }
    .user { display: flex; align-items: center; gap: 12px; }
    .user-info { text-align: right; }
    .user-name { font-size: 13px; font-weight: 600; }
    .user-mail { font-size: 11px; color: var(--ht-ink4); }
    .content { flex: 1; overflow-y: auto; padding: 22px; }
  `],
})
export class ShellComponent {
  groups: NavGroup[] = [
    { title: 'Tổng quan', items: [{ label: 'Bảng điều khiển', icon: 'pi-th-large', link: '/dashboard' }] },
    { title: 'Bán hàng', items: [
      { label: 'Sản phẩm', icon: 'pi-box', link: '/products' },
      { label: 'Đơn hàng', icon: 'pi-shopping-cart', link: '/orders' },
      { label: 'Khuyến mãi', icon: 'pi-megaphone', link: '/promotions' },
    ] },
    { title: 'Quản trị', items: [{ label: 'Người dùng & quyền', icon: 'pi-users', link: '/users' }] },
  ];

  constructor(public auth: AuthService, private router: Router) {}

  logout(): void { this.auth.logout(); this.router.navigate(['/login']); }
}
