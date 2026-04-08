import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-storefront-login',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1>Sign in</h1>
    <p class="muted">Use your account to add items to the basket and place orders.</p>

    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>
        Username
        <input type="text" formControlName="userName" autocomplete="username" />
      </label>
      <label>
        Password
        <input type="password" formControlName="password" autocomplete="current-password" />
      </label>
      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      <button type="submit" class="btn primary" [disabled]="form.invalid || busy()">
        {{ busy() ? 'Signing in…' : 'Sign in' }}
      </button>
    </form>
    <p class="extra">
      No account? <a routerLink="/register">Register</a> · <a routerLink="/">Home</a>
    </p>
  `,
  styles: [
    `
      :host {
        display: block;
        max-width: 400px;
      }
      h1 {
        margin-top: 0;
      }
      .muted {
        color: var(--muted);
        margin-bottom: 1.25rem;
      }
      form {
        display: flex;
        flex-direction: column;
        gap: 0.85rem;
      }
      label {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
        font-size: 0.85rem;
        color: var(--muted);
      }
      input {
        padding: 0.55rem 0.65rem;
        border-radius: 6px;
        border: 1px solid var(--border);
        background: var(--bg);
        color: var(--text);
      }
      .error {
        color: var(--danger);
        margin: 0;
        font-size: 0.9rem;
      }
      .extra {
        margin-top: 1.25rem;
        font-size: 0.9rem;
        color: var(--muted);
      }
    `
  ]
})
export class StorefrontLoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    userName: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.busy.set(true);
    this.error.set(null);
    const { userName, password } = this.form.getRawValue();
    this.auth.login({ userName, password }).subscribe({
      next: () => {
        const ret = this.route.snapshot.queryParamMap.get('returnUrl');
        const target =
          ret && ret.startsWith('/') && !ret.startsWith('//') ? ret : '/products';
        void this.router.navigateByUrl(target);
      },
      error: () => {
        this.error.set('Invalid credentials or server unreachable.');
        this.busy.set(false);
      }
    });
  }
}
