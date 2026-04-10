# QLProtocolLibrary

[![CI](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/zpczpc/QLProtocolLibrary/actions/workflows/dotnet-ci.yml)
[![NuGet](https://img.shields.io/nuget/v/QLProtocolLibrary?logo=nuget)](https://www.nuget.org/packages/QLProtocolLibrary)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

[中文](README.md) | English

`QLProtocolLibrary` is a C# library for the QL device communication protocol. Its goal is to hide protocol details so callers can start using the protocol without first studying the full protocol document.

Typical use cases:

- WinForms upper-computer applications
- TCP communication services
- desktop tools that need quick protocol integration
- teams that want to ship protocol capability as a reusable NuGet package

## Core value

This library focuses on two things:

1. frame building
2. frame parsing

It supports two usage styles at the same time:

- high-level business APIs that hide register addresses
- generic protocol APIs that expose address- and type-based access

## Installation

```bash
dotnet add package QLProtocolLibrary
```

## Fastest start

```csharp
using QLProtocolLibrary;

var command = QlProtocolKnownCommands.BuildReadDeviceTime("1001");
Console.WriteLine(QlHexConverter.ToHexString(command));
```

## Quick example

```csharp
using QLProtocolLibrary;

var command = QlProtocolKnownCommands.BuildReadDeviceTime("1001");

// send command and receive response frame
QlProtocolFrame frame = QlProtocolParser.Parse(responseBytes);

if (QlProtocolKnownParsers.TryParseDeviceTime(frame, out var deviceTime))
{
    Console.WriteLine(deviceTime);
}
```

## Documentation index

- NuGet package README: [src/QLProtocolLibrary/README.md](src/QLProtocolLibrary/README.md)
- API reference (Chinese): [docs/API.zh-CN.md](docs/API.zh-CN.md)
- API reference (English): [docs/API.en.md](docs/API.en.md)
- Publishing checklist (Chinese): [docs/PUBLISHING.zh-CN.md](docs/PUBLISHING.zh-CN.md)
- Publishing checklist (English): [docs/PUBLISHING.en.md](docs/PUBLISHING.en.md)
- Contributing guide: [CONTRIBUTING.md](CONTRIBUTING.md)
- Changelog: [CHANGELOG.md](CHANGELOG.md)
- Source-based demo: [examples/QLProtocolLibrary.Demo/Program.cs](examples/QLProtocolLibrary.Demo/Program.cs)
- NuGet usage sample: [examples/QLProtocolLibrary.NuGetDemo/Program.cs](examples/QLProtocolLibrary.NuGetDemo/Program.cs)

## Current capabilities

- full frame building
- full frame parsing
- CRC16 validation
- TCP sticky-packet splitting
- high-level known-command APIs
- high-level typed business parsers
- generic typed payload readers
- known-operation catalog and unified response router

## Repository layout

- `src/QLProtocolLibrary`: library source
- `examples/QLProtocolLibrary.Demo`: source-based sample
- `examples/QLProtocolLibrary.NuGetDemo`: NuGet usage sample
- `tests/QLProtocolLibrary.Tests`: unit tests
- `docs`: open-source and publishing documents

## Project links

- GitHub repository: https://github.com/zpczpc/QLProtocolLibrary
- Issue tracker: https://github.com/zpczpc/QLProtocolLibrary/issues
- NuGet package: https://www.nuget.org/packages/QLProtocolLibrary
- License: MIT, see `LICENSE`

See: [docs/PUBLISHING.en.md](docs/PUBLISHING.en.md)
