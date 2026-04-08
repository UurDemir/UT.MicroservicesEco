import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-backoffice-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="bo-shell">
      <aside class="side">
        <div class="side-head">Backoffice</div>
        <nav>
          <a routerLink="/backoffice/dashboard" routerLinkActive="active">Dashboard</a>
          <a routerLink="/backoffice/products" routerLinkActive="active">Products</a>
          <a routerLink="/backoffice/orders" routerLinkActive="active">Orders</a>
          <a routerLink="/backoffice/deliveries" routerLinkActive="active">Deliveries</a>
        </nav>
        <button type="button" class="logout" (click)="logout()">Sign out</button>
      </aside>
      <section class="bo-main">
        <router-outlet />
      </section>
    </div>
  `,
  styles: [
    `
      .bo-shell {
        display: flex;
        min-height: 100vh;
      }
      .side {
        width: 200px;
        background: var(--surface);
        border-right: 1px solid var(--border);
        padding: 1rem;
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }
      .side-head {
        font-weight: 700;
        font-size: 1rem;
      }
      .side nav {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
      }
      .side a {
        color: var(--muted);
        text-decoration: none;
        padding: 0.35rem 0.25rem;
        border-radius: 4px;
      }
      .side a.active,
      .side a:hover {
        color: var(--accent);
        background: rgba(59, 130, 246, 0.08);
      }
      .logout {
        margin-top: auto;
        align-self: stretch;
        padding: 0.5rem;
        border: 1px solid var(--border);
        border-radius: 6px;
        background: transparent;
        cursor: pointer;
        color: var(--text);
      }
      .logout:hover {
        border-color: var(--danger);
        color: var(--danger);
      }
      .bo-main {
        flex: 1;
        padding: 1.25rem 1.5rem;
        overflow: auto;
      }
    `
  ]
})
export class BackofficeLayoutComponent {
  private readonly auth = inject(AuthService);

  logout(): void {
    this.auth.logout('/backoffice/login');
  }
}
