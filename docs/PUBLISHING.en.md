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

Recommended additional fields:

- `PackageProjectUrl`
- `RepositoryUrl`
- `RepositoryBranch`
- `PackageLicenseExpression` or `PackageLicenseFile`
- `PackageIcon`
- `PackageReleaseNotes`

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

## 4. Example checks

Make sure the sample project still runs:

- `examples/QLProtocolLibrary.Demo`

## 5. Build checks

Run at least:

```bash
dotnet build .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release
dotnet pack .\src\QLProtocolLibrary\QLProtocolLibrary.csproj -c Release -o .\artifacts
```

## 6. NuGet page experience

Verify that:

- the first README screen is clear enough
- the installation command is visible
- high-level APIs are explained clearly
- both “address-free usage” and “generic typed usage” are documented
- example code can be copied directly

## 7. Post-publish smoke test

After publishing, verify immediately:

1. `dotnet add package QLProtocolLibrary`
2. create a clean sample project
3. call `QlKnownOperations.DeviceTime.BuildRead("1001")`
4. call `QlProtocolParser.Parse(...)`
5. verify XML documentation appears correctly in the IDE
