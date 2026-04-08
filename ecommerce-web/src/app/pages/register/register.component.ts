import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1>Create account</h1>
    <p class="muted">Register as a customer, then sign in to shop.</p>

    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>
        Username
        <input type="text" formControlName="userName" autocomplete="username" />
      </label>
      <label>
        Email
        <input type="email" formControlName="email" autocomplete="email" />
      </label>
      <label>
        Password
        <input type="password" formControlName="password" autocomplete="new-password" />
      </label>
      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      @if (ok()) {
        <p class="ok">{{ ok() }}</p>
      }
      <button type="submit" class="btn primary" [disabled]="form.invalid || busy()">
        {{ busy() ? 'Creating…' : 'Register' }}
      </button>
    </form>
    <p class="extra"><a routerLink="/login">Sign in</a> · <a routerLink="/">Home</a></p>
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
      .ok {
        color: #4ade80;
        margin: 0;
        font-size: 0.9rem;
      }
      .extra {
        margin-top: 1.25rem;
        font-size: 0.9rem;
      }
    `
  ]
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly ok = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    userName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.busy.set(true);
    this.error.set(null);
    this.ok.set(null);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.ok.set('Account created. You can sign in now.');
        this.busy.set(false);
        setTimeout(() => void this.router.navigate(['/login']), 1200);
      },
      error: (err: { status?: number }) => {
        if (err?.status === 409) {
          this.error.set('That username is already taken.');
        } else {
          this.error.set('Registration failed. Check the form and try again.');
        }
        this.busy.set(false);
      }
    });
  }
}
