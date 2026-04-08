import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';

interface DeliveryDto {
  id: string;
  orderId: string;
  address: string;
  status: string;
  createdAtUtc: string;
}

@Component({
  selector: 'app-deliveries-admin',
  imports: [DatePipe],
  template: `
    <h1>Deliveries</h1>
    <p class="muted">GET /api/deliveries</p>
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
              <th>Order id</th>
              <th>Address</th>
              <th>Status</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            @for (d of deliveries(); track d.id) {
              <tr>
                <td class="mono">{{ d.id.slice(0, 8) }}…</td>
                <td class="mono">{{ d.orderId.slice(0, 8) }}…</td>
                <td>{{ d.address }}</td>
                <td>{{ d.status }}</td>
                <td>{{ d.createdAtUtc | date: 'short' }}</td>
              </tr>
            } @empty {
              <tr>
                <td colspan="5">No deliveries.</td>
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
export class DeliveriesAdminComponent {
  private readonly http = inject(HttpClient);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly deliveries = signal<DeliveryDto[]>([]);

  constructor() {
    this.http.get<DeliveryDto[]>('/api/deliveries').subscribe({
      next: (data) => {
        this.deliveries.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load deliveries.');
        this.loading.set(false);
      }
    });
  }
}
