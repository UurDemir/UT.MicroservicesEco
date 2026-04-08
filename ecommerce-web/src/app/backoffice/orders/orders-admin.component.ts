import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CurrencyPipe, DatePipe } from '@angular/common';

interface OrderLineDto {
  id: string;
  orderId: string;
  productId: string;
  quantity: number;
  unitPrice: number;
}

interface OrderDto {
  id: string;
  userName: string;
  totalAmount: number;
  shippingAddress: string;
  status: string;
  createdAtUtc: string;
  lines?: OrderLineDto[];
}

@Component({
  selector: 'app-orders-admin',
  imports: [CurrencyPipe, DatePipe],
  template: `
    <h1>Orders</h1>
    <p class="muted">GET /api/orders (read-only list for training).</p>
    @if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (loading()) {
      <p>Loading…</p>
    } @else {
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Id</th>
              <th>Customer</th>
              <th>Total</th>
              <th>Status</th>
              <th>Created</th>
              <th>Lines</th>
            </tr>
          </thead>
          <tbody>
            @for (o of orders(); track o.id) {
              <tr>
                <td class="mono">{{ o.id.slice(0, 8) }}…</td>
                <td>{{ o.userName }}</td>
                <td>{{ o.totalAmount | currency: 'USD' }}</td>
                <td>{{ o.status }}</td>
                <td>{{ o.createdAtUtc | date: 'short' }}</td>
                <td>{{ o.lines?.length ?? 0 }}</td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6">No orders.</td>
              </tr>
            }
          </tbody>
        </table>
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
      }
      .error {
        color: var(--danger);
      }
      .table-wrap {
        overflow: auto;
      }
      table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.85rem;
      }
      th,
      td {
        border-bottom: 1px solid var(--border);
        padding: 0.45rem 0.35rem;
        text-align: left;
      }
      .mono {
        font-family: ui-monospace, monospace;
        font-size: 0.75rem;
      }
    `
  ]
})
export class OrdersAdminComponent {
  private readonly http = inject(HttpClient);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly orders = signal<OrderDto[]>([]);

  constructor() {
    this.http.get<OrderDto[]>('/api/orders').subscribe({
      next: (data) => {
        this.orders.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load orders.');
        this.loading.set(false);
      }
    });
  }
}
