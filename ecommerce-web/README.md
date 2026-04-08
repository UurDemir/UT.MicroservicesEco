# E‑Commerce Lesson — Angular front-end

Angular 19 SPA: public **catalog** and **backoffice** (admin) for the microservices demo.

## Run (development)

1. Start the stack from **Aspire AppHost** so **ApiGateway** is running.
2. Edit **`proxy.conf.json`**: set `"target"` to your gateway base URL (from the Aspire dashboard, e.g. `https://localhost:5xxxx`).
3. From this folder:

```bash
npm install
ng serve
```

Open `http://localhost:4200/`.

- **Storefront**: `/`, `/products` (add to basket when signed in), **`/register`**, **`/login`**, **`/basket`** (review lines, shipping address, **place order** → `POST /api/orders`, then basket lines are cleared).
- **Backoffice**: `/backoffice/login` — seeded admin **`admin`** / **`Admin123!`**

## API

All HTTP calls use paths like `/api/...`; the dev server proxies them to the gateway (`proxy.conf.json`). JWT is stored in `localStorage` and sent as `Authorization: Bearer`. Each request gets an `X-Correlation-ID` header.

## Build

```bash
ng build
```

For a non-proxied deployment, set `apiBaseUrl` in `src/environments/environment.ts` if the SPA is hosted on a different origin than the gateway (and ensure CORS on the gateway).
