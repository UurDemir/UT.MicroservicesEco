import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { forkJoin, of, concatMap } from 'rxjs';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import type { BasketItemDto, CreateOrderRequest } from '../../core/models/basket.models';

@Component({
  selector: 'app-basket',
  imports: [ReactiveFormsModule, CurrencyPipe, RouterLink],
  template: `
    <h1>Your basket</h1>

    @if (loadError()) {
      <p class="error">{{ loadError() }}</p>
    } @else if (loading()) {
      <p>Loading…</p>
    } @else if (items().length === 0) {
      <p class="muted">Your basket is empty. <a routerLink="/products">Browse products</a></p>
    } @else {
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Product</th>
              <th>Unit price</th>
              <th>Qty</th>
              <th>Line total</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (line of items(); track line.id) {
              <tr>
                <td>{{ line.productName }}</td>
                <td>{{ line.unitPrice | currency: 'USD' }}</td>
                <td class="qty">
                  <input
                    type="number"
                    min="1"
                    [value]="line.quantity"
                    #qtyInput
                  />
                  <button type="button" class="btn sm" (click)="updateQty(line.id, qtyInput.valueAsNumber)">
                    Update
                  </button>
                </td>
                <td>{{ line.quantity * line.unitPrice | currency: 'USD' }}</td>
                <td>
                  <button type="button" class="link danger" (click)="removeLine(line.id)">Remove</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <p class="total">
        Total: <strong>{{ cartTotal() | currency: 'USD' }}</strong>
      </p>

      <section class="checkout">
        <h2>Shipping</h2>
        <form [formGroup]="checkoutForm" (ngSubmit)="placeOrder()">
          <label>
            Address
            <textarea
              formControlName="shippingAddress"
              rows="3"
              placeholder="Street, city, postal code…"
            ></textarea>
          </label>
          @if (orderError()) {
            <p class="error">{{ orderError() }}</p>
          }
          @if (orderOk()) {
            <p class="ok">{{ orderOk() }}</p>
          }
          <button type="submit" class="btn primary" [disabled]="checkoutForm.invalid || orderBusy()">
            {{ orderBusy() ? 'Placing order…' : 'Place order' }}
          </button>
        </form>
      </section>
    }
  `,
  styles: [
    `
      h1 {
        margin-top: 0;
      }
      h2 {
        font-size: 1rem;
        margin: 1.5rem 0 0.75rem;
      }
      .muted {
        color: var(--muted);
      }
      .muted a {
        color: var(--accent);
      }
      .error {
        color: var(--danger);
      }
      .ok {
        color: #4ade80;
      }
      .table-wrap {
        overflow: auto;
      }
      table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.9rem;
      }
      th,
      td {
        border-bottom: 1px solid var(--border);
        padding: 0.5rem 0.35rem;
        text-align: left;
      }
      .qty {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 0.35rem;
      }
      .qty input {
        width: 4rem;
        padding: 0.35rem;
        border-radius: 4px;
        border: 1px solid var(--border);
        background: var(--bg);
        color: var(--text);
      }
      .btn.sm {
        padding: 0.3rem 0.5rem;
        font-size: 0.8rem;
      }
      .link {
        background: none;
        border: none;
        color: var(--accent);
        cursor: pointer;
        text-decoration: underline;
        font-size: inherit;
      }
      .link.danger {
        color: var(--danger);
      }
      .total {
        margin-top: 1rem;
        font-size: 1.1rem;
      }
      .checkout textarea {
        width: 100%;
        max-width: 28rem;
        padding: 0.5rem;
        border-radius: 6px;
        border: 1px solid var(--border);
        background: var(--bg);
        color: var(--text);
        resize: vertical;
      }
      .checkout label {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
        font-size: 0.85rem;
        color: var(--muted);
        margin-bottom: 0.75rem;
      }
    `
  ]
})
export class BasketComponent {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly items = signal<BasketItemDto[]>([]);
  readonly orderBusy = signal(false);
  readonly orderError = signal<string | null>(null);
  readonly orderOk = signal<string | null>(null);

  readonly checkoutForm = this.fb.nonNullable.group({
    shippingAddress: ['', [Validators.required, Validators.minLength(5)]]
  });

  constructor() {
    this.reload();
  }

  cartTotal(): number {
    return this.items().reduce((sum, l) => sum + l.quantity * l.unitPrice, 0);
  }

  basketBasePath(): string {
    const u = this.auth.getBasketUserName();
    if (!u) {
      return '';
    }
    return `/api/baskets/${encodeURIComponent(u)}`;
  }

  reload(): void {
    const u = this.auth.getBasketUserName();
    if (!u) {
      this.loadError.set('Not signed in.');
      this.loading.set(false);
      return;
    }
    this.loading.set(true);
    this.loadError.set(null);
    this.http.get<BasketItemDto[]>(`${this.basketBasePath()}`).subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('Could not load basket.');
        this.loading.set(false);
      }
    });
  }

  updateQty(lineId: string, qty: number): void {
    if (!Number.isFinite(qty) || qty < 1) {
      return;
    }
    this.http.put<BasketItemDto>(`/api/baskets/items/${lineId}`, { quantity: qty }).subscribe({
      next: () => this.reload(),
      error: () => this.loadError.set('Could not update quantity (stock may be insufficient).')
    });
  }

  removeLine(lineId: string): void {
    this.http.delete(`/api/baskets/items/${lineId}`).subscribe({
      next: () => this.reload(),
      error: () => this.loadError.set('Could not remove line.')
    });
  }

  placeOrder(): void {
    if (this.checkoutForm.invalid) {
      return;
    }
    const userName = this.auth.getBasketUserName();
    if (!userName) {
      return;
    }
    const lines = this.items();
    if (lines.length === 0) {
      return;
    }

    const body: CreateOrderRequest = {
      userName,
      shippingAddress: this.checkoutForm.getRawValue().shippingAddress.trim(),
      lines: lines.map((l) => ({
        productId: l.productId,
        quantity: l.quantity,
        unitPrice: l.unitPrice
      }))
    };

    this.orderBusy.set(true);
    this.orderError.set(null);
    this.orderOk.set(null);

    this.http.post<unknown>('/api/orders', body).pipe(
      concatMap(() => {
        const dels = lines.map((l) => this.http.delete(`/api/baskets/items/${l.id}`));
        return dels.length ? forkJoin(dels) : of(null);
      })
    ).subscribe({
      next: () => {
        this.orderOk.set('Order placed successfully.');
        this.orderBusy.set(false);
        this.items.set([]);
        this.checkoutForm.reset();
      },
      error: () => {
        this.orderError.set(
          'Order failed (stock, validation, or server error). Your basket was not cleared.'
        );
        this.orderBusy.set(false);
      }
    });
  }
}
