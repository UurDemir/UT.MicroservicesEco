TLS files are not committed (*.pem is gitignored).

Local Aspire: default mount is this folder, or set env REVERSE_PROXY_BINDMOUNT to any directory with:
  fullchain.pem  — origin / LE public chain
  privkey.pem    — private key

Published Compose: set REVERSE_PROXY_BINDMOUNT in .env (e.g. ./ssl next to docker-compose.yaml); same file names.
Cloudflare: SSL/TLS > Origin Server > Create certificate.
