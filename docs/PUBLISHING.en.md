# Publishing Checklist

Use this file as the final checklist before publishing to a public NuGet feed.

## 1. Package metadata

Make sure these fields are correct:

- `PackageId`
- `Version`
- `Authors`
- `Company`
- `Description`
- `PackageTags`
- `PackageReadmeFile`
- whether `Version` has been incremented for this release

Recommended additional fields:

- `PackageProjectUrl`
- `RepositoryUrl`
- `RepositoryBranch`
- `PackageLicenseExpression` or `PackageLicenseFile`
- `PackageIcon`
- `PackageReleaseNotes`

Recommended public values for this repository:

- `PackageProjectUrl`: `https://github.com/zpczpc/QLProtocolLibrary`
- `RepositoryUrl`: `https://github.com/zpczpc/QLProtocolLibrary`
- `PackageLicenseExpression`: `MIT`

## 2. Open-source information

Before publishing publicly, confirm:

- license type
- whether outside contributions are accepted
- whether issues are public
- whether a roadmap is public
- whether any internal or business restrictions need to be documented

## 3. Documentation checks

At minimum, verify:

- root `README.md`
- package README: `src/QLProtocolLibrary/README.md`
- English docs: `README.en.md` and `src/QLProtocolLibrary/README.en.md`
- `docs/API.zh-CN.md`
- `docs/API.en.md`
- `CHANGELOG.md`
- `CONTRIBUTING.md`

Focus points:

- the docs clearly describe the main packet structure as `DeviceAddress + FunctionCode + FunctionData + CRC`
- no stale `MN` or `AA 55 ... BB 55` descriptions remain
- the optional envelope is explained as optional, not as the default packet model
- byte-order notes clearly explain that payload layout depends on function code and data type

## 4. Example checks

Make sure the sample projects still run:

- `examples/QLProtocolLibrary.Demo`
- `examples/QLProtocolLibrary.NuGetDemo`

Focus points:

- examples use `uint deviceAddress`
- examples match the protocol document samples
- examples no longer depend on the old `mn` style API

## 5. Build checks

Run at least:

```bash
dotnet restore .\QLProtocolLibrary.sln
dotnet build .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release
dotnet test .\tests\QLProtocolLibrary.Tests\QLProtocolLibrary.Tests.csproj -c Release
dotnet pack .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release -o .\artifacts
```

## 6. NuGet page experience

Verify that:

- the first README screen is clear enough
- the installation command is visible
- the main packet structure is clearly described
- the optional envelope is clearly marked as optional
- the example code can be copied directly

## 7. Post-publish smoke test

After publishing, verify immediately:

1. `dotnet add package QLProtocolLibrary`
2. create a clean sample project
3. call `QlProtocolCommandBuilder.BuildRead(0x10000001, 0x0000, 0x0001)`
4. call `QlProtocolParser.Parse(...)`
5. verify XML documentation appears correctly in the IDE

## 8. Before the next release

If the repository contains new changes after the last NuGet release, increment the package version and update `CHANGELOG.md` before publishing again.
