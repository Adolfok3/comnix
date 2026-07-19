<p align="center">
  <img src="https://raw.githubusercontent.com/Adolfok3/comnix/main/assets/icon.png" alt="comnix" width="100" height="100" />
</p>

<h1 align="center">comnix</h1>

<p align="center"><strong>Run remote SSH commands with a plain HTTP <code>GET</code> — straight from your homelab, no bloat.</strong></p>

comnix turns `GET /api/{route}` requests into pre-configured SSH commands against your homelab servers. Native AOT, ~9.5 MB to pull (~32 MB on disk), zero external dependencies — just the container and two JSON files.

## Quick start

```bash
docker run -d \
  --name comnix \
  -p 5000:5000 \
  -v ./config:/app/config \
  adolfok3/comnix:latest
```

Mount a directory at `/app/config` with:

- **`ssh.json`** — one or more named SSH connections (host, user, password or private key).
- **`commands.json`** — routes mapped to shell commands, each optionally tied to a connection.
- **`response.html`** *(optional)* — custom HTML/CSS/JS response instead of the default JSON.

If these files don't exist yet, comnix creates empty defaults on first run.

## Security

comnix has **no built-in authentication or API key validation** — this is intentional, not an oversight. It's meant to run only behind a trusted network boundary (your LAN, a VPN, or a reverse proxy with authentication in front). **Never expose the port directly to the internet.**

## Full documentation

For the complete configuration reference, Docker Compose example, response templating, and security rationale, see the [full README on GitHub](https://github.com/Adolfok3/comnix#readme).
