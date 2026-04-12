Place TLS files here for local Aspire runs (bind-mounted into nginx):
  fullchain.pem  — Cloudflare Origin Certificate (PEM) or public cert chain
  privkey.pem    — matching private key

Cloudflare: SSL/TLS > Origin Server > Create certificate, copy PEM + key.
Published Compose: set REVERSE_PROXY_BINDMOUNT_0=./ssl in .env (see deploy/env.example) and put these files in that folder on the server.
