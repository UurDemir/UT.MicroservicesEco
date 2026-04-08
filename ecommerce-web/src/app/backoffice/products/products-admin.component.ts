import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import type { ProductDto } from '../../core/models/product.models';

@Component({
  selector: 'app-products-admin',
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
  template: `
    <h1>Products</h1>
    <p class="muted">CRUD via gateway → ProductService (auth required for writes).</p>

    <section class="form-card">
      <h2>{{ editingId() ? 'Edit product' : 'Create product' }}</h2>
      <form [formGroup]="form" (ngSubmit)="save()">
        <label>Name <input type="text" formControlName="name" /></label>
        <label>Price <input type="number" step="0.01" formControlName="price" /></label>
        <label>Stock <input type="number" formControlName="stock" /></label>
        <div class="row">
          <button type="submit" class="btn primary" [disabled]="form.invalid || busy()">Save</button>
          @if (editingId()) {
            <button type="button" class="btn" (click)="cancelEdit()">Cancel</button>
          }
        </div>
      </form>
    </section>

    @if (listError()) {
      <p class="error">{{ listError() }}</p>
    }
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Price</th>
            <th>Stock</th>
            <th>Created</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (p of products(); track p.id) {
            <tr>
              <td>{{ p.name }}</td>
              <td>{{ p.price | currency: 'USD' }}</td>
              <td>{{ p.stock }}</td>
              <td>{{ p.createdAtUtc | date: 'short' }}</td>
              <td class="actions">
                <button type="button" class="link" (click)="startEdit(p)">Edit</button>
                <button type="button" class="link danger" (click)="remove(p.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="5">No products.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [
    `
      h1 {
        margin-top: 0;
      }
      h2 {
        font-size: 1rem;
        margin: 0 0 0.75rem;
      }
      .muted {
        color: var(--muted);
        margin-bottom: 1rem;
      }
      .form-card {
        border: 1px solid var(--border);
        border-radius: 8px;
        padding: 1rem;
        margin-bottom: 1.5rem;
        background: var(--surface);
        max-width: 420px;
      }
      form {
        display: flex;
        flex-direction: column;
        gap: 0.65rem;
      }
      label {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        font-size: 0.85rem;
        color: var(--muted);
      }
      input {
        padding: 0.45rem 0.5rem;
        border-radius: 6px;
        border: 1px solid var(--border);
        background: var(--bg);
        color: var(--text);
      }
      .row {
        display: flex;
        gap: 0.5rem;
        margin-top: 0.25rem;
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
        font-size: 0.9rem;
      }
      th,
      td {
        border-bottom: 1px solid var(--border);
        padding: 0.5rem 0.4rem;
        text-align: left;
      }
      .actions {
        white-space: nowrap;
      }
      .link {
        background: none;
        border: none;
        color: var(--accent);
        cursor: pointer;
        text-decoration: underline;
        margin-right: 0.5rem;
        font-size: inherit;
      }
      .link.danger {
        color: var(--danger);
      }
    `
  ]
})
export class ProductsAdminComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);

  readonly busy = signal(false);
  readonly products = signal<ProductDto[]>([]);
  readonly listError = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0)]],
    stock: [0, [Validators.required, Validators.min(0)]]
  });

  constructor() {
    this.reload();
  }

  reload(): void {
    this.http.get<ProductDto[]>('/api/products').subscribe({
      next: (data) => this.products.set(data),
      error: () => this.listError.set('Failed to load products.')
    });
  }

  startEdit(p: ProductDto): void {
    this.editingId.set(p.id);
    this.form.patchValue({
      name: p.name,
      price: p.price,
      stock: p.stock
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', price: 0, stock: 0 });
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }
    const { name, price, stock } = this.form.getRawValue();
    this.busy.set(true);
    const id = this.editingId();
    const req$ = id
      ? this.http.put<ProductDto>(`/api/products/${id}`, { name, price, stock })
      : this.http.post<ProductDto>('/api/products', { name, price, stock });

    req$.subscribe({
      next: () => {
        this.busy.set(false);
        this.cancelEdit();
        this.reload();
      },
      error: () => {
        this.busy.set(false);
        this.listError.set('Save failed (check admin JWT and gateway).');
      }
    });
  }

  remove(id: string): void {
    if (!confirm('Delete this product?')) {
      return;
    }
    this.http.delete(`/api/products/${id}`).subscribe({
      next: () => this.reload(),
      error: () => this.listError.set('Delete failed.')
    });
  }
}
