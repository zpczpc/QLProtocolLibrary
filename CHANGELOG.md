# Changelog

All notable changes to this project will be documented in this file.

## [0.3.1] - 2026-04-09

### Added

- external NuGet consumption sample: `examples/QLProtocolLibrary.NuGetDemo`
- test coverage for read request classification and concatenated stream decoding
- GitHub workflow for manual NuGet publishing

### Changed

- public package metadata now includes MIT license, repository URL, package icon, and symbol package output
- root documentation now links directly to NuGet usage samples and GitHub project resources

## [0.3.0] - 2026-04-09

### Added

- SDK-style known operation catalog: `QlKnownOperations`
- unified known response router: `QlProtocolKnownRouter`
- known operation interface and result wrapper
- richer demo coverage for high-level APIs and generic APIs
- repository-level open-source documentation set

## [0.2.0] - 2026-04-08

### Added

- high-level command builders that hide register addresses
- high-level typed parsers for common business frames
- typed result models for known business responses
- bilingual package README
- improved XML documentation for major APIs

## [0.1.0] - 2026-04-08

### Added

- base `netstandard2.0` protocol library
- frame builder and parser
- CRC16 support
- TCP stream decoder
- payload encoding and decoding helpers
- initial NuGet packaging support
