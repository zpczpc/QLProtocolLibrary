# Changelog

All notable changes to this project will be documented in this file.

## [0.5.0] - 2026-04-22

### Added

- dedicated `0x32` command-forwarding builder: `QlProtocolCommandBuilder.BuildForward(...)`
- dedicated `0x32` hex helper: `QlProtocolCommandBuilder.BuildForwardHex(...)`
- dedicated `0x32` parse helpers: `ReadForwardPortId()` and `ReadForwardContent()`
- source demo coverage for the new `0x32` APIs
- unit tests that validate the documented `0x32` forwarding example

### Changed

- refreshed root README, package README, and bilingual API docs for the `0.5.0` release
- clarified the `0x32` frame structure as `DataLength + PortId + ForwardedContent`
- kept the external NuGet demo pinned to `0.4.0` until the new package is published, while documenting the direct `0.5.0` upgrade path

## [0.4.0] - 2026-04-14

### Changed

- refactored the protocol core to follow the main application-layer document structure based on `DeviceAddress + FunctionCode + FunctionData + CRC16`
- removed the old `MN`-based packet model from the current code path
- aligned read/write examples with the protocol document samples such as `10 00 00 01 03 00 00 00 01 43 21`
- updated payload encoding rules to distinguish protocol control fields from payload value fields
- rewrote README, API docs, sample READMEs, and publishing checklists to match the current implementation

### Added

- tests that validate document-based read/write packet examples
- demo output that directly matches documented packet samples

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
