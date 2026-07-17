<p align="center">
  <img src="assets/icon.png" alt="comnix" width="120" height="120" />
</p>

<h1 align="center">comnix</h1>

<p align="center"><strong>Run remote SSH commands with a plain HTTP <code>GET</code> — straight from your homelab, no bloat.</strong></p>

<p align="center">
  <a href="https://github.com/Adolfok3/comnix/actions/workflows/main.yml"><img src="https://img.shields.io/github/actions/workflow/status/Adolfok3/comnix/main.yml?branch=main&label=build&logo=github" alt="Build" /></a>
  <a href="https://github.com/Adolfok3/comnix/releases"><img src="https://img.shields.io/github/v/release/Adolfok3/comnix?label=release&logo=github&sort=semver" alt="Release" /></a>
  <a href="https://hub.docker.com/r/adolfok3/comnix"><img src="https://img.shields.io/docker/image-size/adolfok3/comnix/latest?label=image%20size&logo=docker&color=informational" alt="Docker Image Size" /></a>
  <a href="https://hub.docker.com/r/adolfok3/comnix"><img src="https://img.shields.io/docker/pulls/adolfok3/comnix?logo=docker" alt="Docker Pulls" /></a>
  <a href="tests/Comnix.Tests"><img src="https://img.shields.io/badge/tests-passing-brightgreen?logo=xunit" alt="Tests" /></a>
  <a href="https://dotnet.microsoft.com"><img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET" /></a>
  <a href="src/Comnix/Comnix.csproj"><img src="https://img.shields.io/badge/native-AOT-blue" alt="Native AOT" /></a>
  <a href="src/Comnix/Dockerfile"><img src="https://img.shields.io/badge/platform-linux--musl--x64-orange?logo=alpinelinux&logoColor=white" alt="Platform" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/Adolfok3/comnix" alt="License" /></a>
</p>

## Why comnix

**comnix is a tiny container that turns a plain HTTP `GET` into a remote SSH command.** Drop it into your homelab, mount a volume with your SSH connection profiles and the commands you want to expose, and every route becomes a one-shot remote action — no client library, no auth handshake, no runtime to babysit.

The whole point is to stay out of the way:

| | |
|---|---|
| **Multiple SSH profiles** | Define as many named servers as you want in `ssh.json` (different hosts, users, ports, keys). Each route in `commands.json` picks which one it runs against. |
| **Route → command mapping** | Every entry in `commands.json` maps a URL segment to a shell command. No code, no scripting language — just JSON. |
| **~32 MB image** | Published with .NET 10 Native AOT, self-contained, on top of `runtime-deps:alpine`. No SDK, no JIT, no dead weight. |
| **Zero external dependencies** | No database, queue, or sidecar service needed. Just the container and two JSON files. |
| **Configuration via volume** | Profiles and commands live in JSON files mounted as a volume — edit them and the change applies immediately, no image rebuild. |
| **Customizable response** | Optionally returns HTML (with CSS/JS) instead of JSON — great for bookmarklets, home-screen shortcuts, and automations. |

Ideal for triggering everyday homelab tasks — restarting a service, running a backup script, sending a Wake-on-LAN, checking disk usage — from a phone shortcut, a browser bookmarklet, a Home Assistant button, or anything that can make a `GET` request.

## Table of contents

- [How it works](#how-it-works)
- [Running with Docker](#running-with-docker)
- [Running with Docker Compose](#running-with-docker-compose)
- [Configuration files](#configuration-files)
  - [`ssh.json`](#sshjson)
  - [`commands.json`](#commandsjson)
  - [`response.html` (optional)](#responsehtml-optional)
- [Calling the API](#calling-the-api)
- [Security](#security)
- [Building the image locally](#building-the-image-locally)
- [License](#license)

## How it works

comnix exposes a single HTTP route:

```
GET /api/{route}
```

Each `{route}` is mapped, via `commands.json`, to a shell command executed over SSH against a server described in `ssh.json`. The response carries the execution result — JSON by default, or HTML if you supply a `response.html` template (see below).

```
GET /api/restart-nginx  ─────▶  comnix  ─────▶  ssh user@server "systemctl restart nginx"
                                                            │
                                 ◀──────────────────────────┘
                     { "success": true, "result": "" }
```

## Running with Docker

```bash
docker run -d \
  --name comnix \
  -p 5000:5000 \
  -v ./config:/app/config \
  adolfok3/comnix:latest
```

- **Port**: the API listens on port `5000` inside the container (`ASPNETCORE_HTTP_PORTS=5000`). Map it to whichever host port you prefer.
- **Volume**: mount a host directory at `/app/config` — that's where comnix looks for `ssh.json`, `commands.json`, and optionally `response.html`.
- If `ssh.json`/`commands.json` don't exist in the volume on first run, comnix creates empty defaults automatically (`{"ssh": {}}` and `{"commands": []}`), ready for you to edit.

## Running with Docker Compose

```yaml
services:
  comnix:
    image: adolfok3/comnix:latest
    container_name: comnix
    restart: unless-stopped
    ports:
      - "5000:5000"
    volumes:
      - ./config:/app/config
```

## Configuration files

All the files below live in the directory mounted at `/app/config`.

> [!NOTE]
> These files are watched in real time — changes may take a few seconds to take effect, no container restart needed.

### `ssh.json`

Defines one or more named **SSH connections**, referenced by the commands in `commands.json`.

```json
{
  "ssh": {
    "default": {
      "host": "192.168.1.10",
      "user": "homelab",
      "port": 22,
      "password": "my-password"
    },
    "nas": {
      "host": "192.168.1.20",
      "user": "admin",
      "port": 2222,
      "keyPath": "/app/config/keys/nas_id_rsa",
      "commandTimeout": 60
    }
  }
}
```

| Field | Required | Default | Description |
|---|---|---|---|
| `host` | Yes | — | Address of the SSH server. |
| `user` | Yes | — | Username for authentication. |
| `port` | No | `22` | SSH service port. |
| `password` | No* | — | Password for the user. |
| `keyPath` | No* | — | Path (inside the container) to a private key file. Takes priority over `password` if set. |
| `commandTimeout` | No | `120` | Timeout in seconds for command execution. |

\* provide either `password` **or** `keyPath` — if using a key, mount the key file via volume as well (e.g. `./keys:/app/config/keys`).

Each connection's key (`"default"`, `"nas"`, ...) is the name that entries in `commands.json` use to pick where to run.

### `commands.json`

Defines the available **routes** and the command executed for each one.

```json
{
  "commands": [
    { "route": "restart-nginx", "command": "sudo systemctl restart nginx" },
    { "route": "disk-usage", "command": "df -h" },
    { "route": "wake-nas", "command": "wakeonlan AA:BB:CC:DD:EE:FF", "connection": "nas" }
  ]
}
```

| Field | Required | Default | Description |
|---|---|---|---|
| `route` | Yes | — | Segment after `/api/`. Requests to `GET /api/restart-nginx` execute this entry's command. Matching is case-insensitive. |
| `command` | Yes | — | Shell command executed on the remote server. |
| `connection` | No | `default` | Name of the connection (key from `ssh.json`) used for this route. |

### `response.html` (optional)

By default, every call returns a plain JSON payload:

```json
{ "success": true, "result": "command output" }
```

If you drop a `response.html` file at `/app/config/response.html`, comnix returns **that HTML** instead of JSON — with two placeholders substituted automatically:

| Placeholder | Replaced with |
|---|---|
| `{{success}}` | `true` or `false` |
| `{{result}}` | Command output (HTML-encoded) |

This makes room for a response with your own look and feel — CSS, animations, even JavaScript behavior. A classic homelab use case: use comnix as the target of a phone home-screen shortcut and auto-close the tab right after execution, so no result screen is left hanging around:

```html
<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <style>
      body {
        font-family: system-ui, sans-serif;
        background: #111;
        color: #eee;
        display: flex;
        align-items: center;
        justify-content: center;
        height: 100vh;
        margin: 0;
      }
      .icon { font-size: 3rem; }
    </style>
  </head>
  <body>
    <span class="icon">{{success}}</span>
    <script>
      // fire the action and disappear — no result screen taking up space
      setTimeout(() => window.close(), 300);
    </script>
  </body>
</html>
```

Any valid HTML/CSS/JS works — comnix just does a text substitution before returning the page.

## Calling the API

Anything capable of making a `GET` request can act as a comnix "client": `curl`, a browser bookmarklet, an iOS/Android home-screen shortcut, a Home Assistant `rest_command`, a physical button wired to an ESP8266, etc.

```bash
curl http://homelab.local:5000/api/restart-nginx
```

```json
{ "success": true, "result": "" }
```

## Security

comnix **does not implement its own authentication** — it assumes it's already running behind a trusted network (your LAN, a VPN like Tailscale/WireGuard, or a reverse proxy with authentication in front). Don't expose the port directly to the internet without an additional layer of protection.

## Building the image locally

```bash
docker build -f src/Comnix/Dockerfile -t comnix .
```

## License

Distributed under the [MIT](LICENSE) license.
