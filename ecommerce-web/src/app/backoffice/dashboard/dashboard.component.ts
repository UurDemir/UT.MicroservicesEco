import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-backoffice-dashboard',
  imports: [RouterLink],
  template: `
    <h1>Dashboard</h1>
    <p class="muted">Signed in as <strong>{{ displayName }}</strong> · Roles: {{ rolesText() }}</p>
    <ul class="tiles">
      <li><a routerLink="/backoffice/products">Manage products</a></li>
      <li><a routerLink="/backoffice/orders">View orders</a></li>
      <li><a routerLink="/backoffice/deliveries">View deliveries</a></li>
      <li><a routerLink="/products">Open public catalog</a></li>
    </ul>
  `,
  styles: [
    `
      h1 {
        margin-top: 0;
      }
      .muted {
        color: var(--muted);
      }
      .tiles {
        list-style: none;
        padding: 0;
        margin: 1.5rem 0 0;
        display: grid;
        gap: 0.5rem;
        max-width: 22rem;
      }
      .tiles li a {
        display: block;
        padding: 0.65rem 0.85rem;
        border: 1px solid var(--border);
        border-radius: 8px;
        text-decoration: none;
        color: var(--accent);
        background: var(--surface);
      }
      .tiles li a:hover {
        border-color: var(--accent);
      }
    `
  ]
})
export class BackofficeDashboardComponent {
  private readonly auth = inject(AuthService);

  readonly displayName = this.auth.getUserDisplayName();

  rolesText(): string {
    return this.auth.roles().join(', ') || '—';
  }
}
