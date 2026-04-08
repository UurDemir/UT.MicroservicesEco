import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-backoffice-login',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="panel">
      <h1>Backoffice sign-in</h1>
      <p class="hint">Use the seeded admin account (default: <code>admin</code> / <code>Admin123!</code>).</p>

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
      <p class="back"><a routerLink="/">← Storefront</a></p>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        min-height: 100vh;
        padding: 2rem 1rem;
        background: linear-gradient(160deg, #0f172a 0%, #1e293b 100%);
      }
      .panel {
        max-width: 380px;
        margin: 0 auto;
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 12px;
        padding: 1.75rem;
      }
      h1 {
        margin: 0 0 0.5rem;
        font-size: 1.35rem;
      }
      .hint {
        font-size: 0.85rem;
        color: var(--muted);
        margin-bottom: 1.25rem;
        line-height: 1.5;
      }
      code {
        font-size: 0.8rem;
        background: rgba(0, 0, 0, 0.2);
        padding: 0.1rem 0.3rem;
        border-radius: 4px;
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
        font-size: 0.9rem;
        margin: 0;
      }
      .back {
        margin-top: 1.25rem;
        font-size: 0.9rem;
      }
      .back a {
        color: var(--accent);
      }
    `
  ]
})
export class BackofficeLoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    userName: ['admin', [Validators.required]],
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
          ret && ret.startsWith('/backoffice') && !ret.includes('login') ? ret : '/backoffice/dashboard';
        void this.router.navigateByUrl(target);
      },
      error: () => {
        this.error.set('Invalid credentials or server unreachable.');
        this.busy.set(false);
      }
    });
  }
}
