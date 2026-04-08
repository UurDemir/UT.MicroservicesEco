import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import type { ProductDto } from '../../core/models/product.models';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-catalog',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  template: `
    <h1>Product catalog</h1>
    <p class="muted">Browse products. Sign in to add items to your basket.</p>

    @if (!auth.isAuthenticated()) {
      <p class="banner">
        <a routerLink="/login" [queryParams]="{ returnUrl: '/products' }">Sign in</a>
        or
        <a routerLink="/register">register</a>
        to shop.
      </p>
    }

    @if (feedback()) {
      <p class="feedback">{{ feedback() }}</p>
    }

    @if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (loading()) {
      <p>Loading…</p>
    } @else {
      <div class="grid">
        @for (p of products(); track p.id) {
          <article class="card">
            <h2>{{ p.name }}</h2>
            <p class="price">{{ p.price | currency: 'USD' }}</p>
            <p class="meta">Stock: {{ p.stock }}</p>
            <p class="meta small">{{ p.createdAtUtc | date: 'medium' }}</p>
            @if (auth.isAuthenticated()) {
              <div class="buy">
                <label class="qty-label">
                  Qty
                  <input
                    type="number"
                    min="1"
                    [max]="p.stock"
                    [value]="1"
                    #qty
                  />
                </label>
                <button
                  type="button"
                  class="btn primary"
                  [disabled]="p.stock < 1 || addingId() === p.id"
                  (click)="addToBasket(p, qty.valueAsNumber || 1)"
                >
                  {{ addingId() === p.id ? 'Adding…' : 'Add to basket' }}
                </button>
              </div>
            }
          </article>
        } @empty {
          <p>No products yet.</p>
        }
      </div>
    }
  `,
  styles: [
    `
      h1 {
        margin-top: 0;
      }
      .muted {
        color: var(--muted);
        margin-bottom: 0.5rem;
      }
      .banner {
        background: var(--surface);
        border: 1px solid var(--border);
        padding: 0.65rem 1rem;
        border-radius: 8px;
        margin-bottom: 1rem;
        font-size: 0.95rem;
      }
      .banner a {
        font-weight: 600;
      }
      .feedback {
        color: #4ade80;
        margin-bottom: 0.75rem;
      }
      .error {
        color: var(--danger);
      }
      .grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
        gap: 1rem;
      }
      .card {
        border: 1px solid var(--border);
        border-radius: 8px;
        padding: 1rem;
        background: var(--surface);
        display: flex;
        flex-direction: column;
      }
      .card h2 {
        margin: 0 0 0.5rem;
        font-size: 1.05rem;
      }
      .price {
        font-weight: 700;
        font-size: 1.1rem;
        margin: 0.25rem 0;
      }
      .meta {
        margin: 0.15rem 0;
        color: var(--muted);
        font-size: 0.9rem;
      }
      .meta.small {
        font-size: 0.75rem;
      }
      .buy {
        margin-top: auto;
        padding-top: 0.75rem;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .qty-label {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        font-size: 0.8rem;
        color: var(--muted);
      }
      .qty-label input {
        width: 4rem;
        padding: 0.35rem;
        border-radius: 4px;
        border: 1px solid var(--border);
        background: var(--bg);
        color: var(--text);
      }
    `
  ]
})
export class CatalogComponent {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly products = signal<ProductDto[]>([]);
  readonly addingId = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);

  constructor() {
    this.http.get<ProductDto[]>('/api/products').subscribe({
      next: (data) => {
        this.products.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load products. Is the gateway running and proxy configured?');
        this.loading.set(false);
      }
    });
  }

  addToBasket(product: ProductDto, quantity: number): void {
    const user = this.auth.getBasketUserName();
    if (!user || quantity < 1 || product.stock < quantity) {
      return;
    }
    this.addingId.set(product.id);
    this.feedback.set(null);
    const url = `/api/baskets/${encodeURIComponent(user)}/items`;
    this.http.post(url, { productId: product.id, quantity }).subscribe({
      next: () => {
        this.addingId.set(null);
        this.error.set(null);
        this.feedback.set(`Added ${quantity}× ${product.name} to basket.`);
      },
      error: () => {
        this.addingId.set(null);
        this.error.set('Could not add to basket (stock or auth).');
      }
    });
  }
}
