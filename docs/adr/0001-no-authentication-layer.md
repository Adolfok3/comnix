# 1. No built-in authentication or API key validation

Date: 2026-07-17

## Status

Accepted

## Context

comnix exists to turn a plain HTTP `GET` into a pre-configured SSH command, optimized for three things: running on **private/trusted networks** (homelab LAN, VPN), staying **fast and lightweight** (Native AOT, ~32 MB image, minimal middleware), and supporting **high-throughput, low-friction triggers** from clients that can't do anything more sophisticated than a `GET` request — phone home-screen shortcuts, browser bookmarklets, Home Assistant `rest_command`, physical buttons wired to an ESP8266, etc.

An API key (e.g. an `X-Api-Key` header) was considered as a lightweight app-level safeguard. It was rejected:

- **Most of comnix's target clients can't reliably send custom headers.** A phone home-screen shortcut, a bookmarklet, or a simple GET-triggered button almost always ends up encoding any credential in the **query string** instead (`?key=...`), not a header.
- **A query-string credential isn't a meaningful secret.** It ends up in browser history (synced across devices), reverse-proxy and web server access logs, `Referer` headers, shell history, and screenshots/shared links of automations. Anyone who sees the URL once has the "key" forever, and rotating it means updating every shortcut/bookmarklet/automation that uses it.
- **It would create a false sense of security.** Shipping something labeled "authentication" that is trivially exposed by the exact usage pattern the app is built around is worse than shipping nothing — operators might skip real network-level protection because they believe the app already handles it.
- **It adds weight for a project whose whole pitch is staying minimal and fast.** Token validation, secret storage/rotation, and the associated middleware run counter to the "tiny image, zero dependencies, out of your way" design goal.

## Decision

comnix will **not** implement any built-in authentication, authorization, or API key mechanism. It is designed, documented, and expected to run **only on a trusted network boundary** (LAN, VPN, or behind a reverse proxy that itself handles authentication). This is treated as a hard deployment requirement, not an optional hardening step.

## Consequences

- **The network boundary is the entire security model.** Anyone who can reach the container's port can invoke any route in `commands.json` — there is no secondary gate. This must be communicated unambiguously in the README and cannot be treated as a minor caveat.
- Responsibility for authentication/access control is explicitly pushed to infrastructure the operator already controls (VPN, reverse proxy with auth, firewall rules) rather than reimplemented inside comnix. This keeps comnix simple but means it is **not safe to expose directly to the internet under any circumstances**.
- Because there's no per-caller identity, comnix also has no concept of audit/attribution (who triggered a command) — only that a request happened. Anyone wanting per-user accountability needs to add it at the reverse-proxy layer (e.g. logging authenticated identity there).
- Since the command set is fixed and pre-configured by whoever controls `commands.json` (not parameterized by request input), the practical risk of unauthenticated access is "an unauthorized party can trigger one of the pre-configured actions" rather than arbitrary remote code execution — this shapes the risk as closer to "misuse of a physical button" than "unauthenticated RCE," which is part of why the private-network trust model is considered acceptable for this project's scope.
- **This decision is conditional on the private-network-only scope.** If comnix's scope ever expands to include direct internet exposure, multi-tenant use, or per-user attribution, this ADR must be revisited — the tradeoffs above assume a single trusted operator on a single trusted network.

## Security recommendations for operators

Since comnix has no internal gate, these are not optional extras — they *are* the security model:

- **Never publish the port to the public internet.** Don't `-p 0.0.0.0:5000:5000` on an internet-facing host, and don't port-forward it on your router.
- **Bind to a trusted interface only.** Prefer `-p 127.0.0.1:5000:5000` or your VPN interface's address over `0.0.0.0`, and rely on the VPN/LAN for reachability instead of exposing on every interface.
- **Put a VPN in front of remote access.** Tailscale, WireGuard, or similar — so only devices already authenticated onto your private network can reach comnix at all.
- **Or put an authenticating reverse proxy in front.** Caddy/Traefik/nginx with Authelia, Authentik, or basic auth if you need comnix reachable from outside a pure VPN setup.
- **Isolate it from untrusted network segments.** Don't run comnix on the same network segment as guest Wi-Fi or an untrusted IoT VLAN.
- **Lock down `ssh.json` on the host.** `chmod 600` on the config directory/files, prefer SSH key auth over password, and protect the private key file the same way.
- **Apply least privilege on the SSH target.** Use a dedicated SSH user scoped to only what `commands.json` needs, rather than a broadly privileged account — this limits the blast radius if `commands.json` itself is ever compromised or misconfigured.
- **Keep `commands.json` intentional.** Every entry is something anyone on the trusted network can trigger with no further check — don't add commands you wouldn't be comfortable with any device on that network running.
