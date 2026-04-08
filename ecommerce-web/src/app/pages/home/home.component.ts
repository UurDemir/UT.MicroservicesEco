import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  template: `
    <section class="hero">
      <h1>Welcome</h1>
      <p class="lead">
        Register or sign in to add products to your basket and place orders through the API gateway. Admins use
        <strong>Backoffice</strong> for catalog management (<code>admin</code> / <code>Admin123!</code>).
      </p>
      <div class="actions">
        <a class="btn primary" routerLink="/products">Browse products</a>
        <a class="btn" routerLink="/register">Register</a>
        <a class="btn" routerLink="/login">Sign in</a>
        <a class="btn" routerLink="/backoffice/login">Admin login</a>
      </div>
    </section>
  `,
  styles: [
    `
      .hero {
        max-width: 36rem;
      }
      h1 {
        margin: 0 0 0.5rem;
        font-size: 2rem;
      }
      .lead {
        color: var(--muted);
        line-height: 1.6;
        margin-bottom: 1.5rem;
      }
      code {
        background: var(--surface);
        padding: 0.1rem 0.35rem;
        border-radius: 4px;
      }
      .actions {
        display: flex;
        flex-wrap: wrap;
        gap: 0.75rem;
      }
    `
  ]
})
export class HomeComponent {}
