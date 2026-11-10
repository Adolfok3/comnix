# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [2.0.0] - 2026-11-10

### Changed

- Migrated to .NET 11.

### Added

- Enabled the `runtime-async` runtime feature.

## [1.0.1] - 2026-11-08

### Security

- Updated vulnerable package versions.

## [1.0.0] - 2026-07-17

### Added

- Initial release of comnix, a Native AOT container for executing configured SSH commands through HTTP `GET` routes.
- Route-to-command mapping and multiple named SSH connections, supporting password and private-key authentication.
- Optional custom HTML responses through `response.html`.
- Live reload of `commands.json` and `ssh.json` configuration files.
