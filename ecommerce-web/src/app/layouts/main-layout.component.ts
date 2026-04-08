import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell">
      <header class="top">
        <a routerLink="/" class="brand">E‑Commerce Demo</a>
        <nav>
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Home</a>
          <a routerLink="/products" routerLinkActive="active">Catalog</a>
          @if (auth.isAuthenticated()) {
            <a routerLink="/basket" routerLinkActive="active">Basket</a>
            <span class="hi">Hi, {{ auth.getUserDisplayName() }}</span>
            <button type="button" class="nav-btn" (click)="signOut()">Sign out</button>
          } @else {
            <a routerLink="/login" routerLinkActive="active">Sign in</a>
            <a routerLink="/register">Register</a>
          }
          <a routerLink="/backoffice/login" class="admin-link">Backoffice</a>
        </nav>
      </header>
      <main class="content">
        <router-outlet />
      </main>
      <footer class="foot">Gateway-first · Angular front-end</footer>
    </div>
  `,
  styles: [
    `
      .shell {
        min-height: 100vh;
        display: flex;
        flex-direction: column;
      }
      .top {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0.75rem 1.25rem;
        background: var(--surface);
        border-bottom: 1px solid var(--border);
      }
      .brand {
        font-weight: 700;
        font-size: 1.1rem;
        color: var(--text);
        text-decoration: none;
      }
      nav {
        display: flex;
        gap: 1rem;
      }
      nav a {
        color: var(--muted);
        text-decoration: none;
        font-size: 0.95rem;
      }
      nav a.active,
      nav a:hover {
        color: var(--accent);
      }
      .hi {
        font-size: 0.85rem;
        color: var(--muted);
        max-width: 8rem;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
      .nav-btn {
        padding: 0.35rem 0.65rem;
        border-radius: 6px;
        border: 1px solid var(--border);
        background: transparent;
        color: var(--muted);
        font-size: 0.85rem;
        cursor: pointer;
      }
      .nav-btn:hover {
        color: var(--text);
        border-color: var(--muted);
      }
      .admin-link {
        font-weight: 600;
      }
      .content {
        flex: 1;
        padding: 1.5rem;
        max-width: 1100px;
        width: 100%;
        margin: 0 auto;
      }
      .foot {
        padding: 0.75rem;
        text-align: center;
        font-size: 0.8rem;
        color: var(--muted);
        border-top: 1px solid var(--border);
      }
    `
  ]
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);

  signOut(): void {
    this.auth.logout('/login');
  }
}
