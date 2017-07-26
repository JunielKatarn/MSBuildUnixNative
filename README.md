# MSBuild UNIX Native

This project adds support for building native code (C/C++) using MSBuild directly on UNIX platforms, using non-MSVC compiler toolsets.

[![Visual Studio Team Services](https://garage-05.visualstudio.com/_apis/public/build/definitions/8378d9ce-d7ae-453b-a3b7-825306540b1f/4/badge)](https://github.com/JunielKatarn/MSBuildUnixNative/releases)

[![NuGet](https://img.shields.io/nuget/v/LLVM.Build.svg)](https://www.nuget.org/packages/LLVM.Build/absoluteLatest)
[![NuGet](https://img.shields.io/nuget/dt/LLVM.Build.svg)](https://www.nuget.org/api/v2/package/LLVM.Build)

## Building

### Source Code

Sources available at [`https://github.com/JunielKatarn/MSBuildUnixNative`](https://github.com/JunielKatarn/MSBuildUnixNative).

### Development Environment

These are the currently tested operating systems:
* Windows 10 - Creators Update, using Windows Subsystem for Linux (WSL).
* Ubuntu 16.04
* macOS 10.12 (Sierra)

### Prerequisites
* [.NET Core SDK](https://www.microsoft.com/net/download/core)
* LLVM/Clang compiler collection ([version 4.0.0](http://releases.llvm.org/download.html#4.0.0)).
  * Extract at `/opt/llvm/4.0.0`.
* Ubuntu 16.04:
  * `libc6-dev`
  * `libgcc-5-dev`
* (Optional) [Visual Studio 2017](https://www.visualstudio.com/downloads)
  For the best editing/debugging experience, while deploying to WSL.

### Build Steps
1. Clone the repository<br/>
  `git clone git@github.com:JunielKatarn/MSBuildUnixNative.git`
2. `dotnet restore`
3. `dotnet msbuild`

## Usage

1. Create a directory for your project, with two components:
    1. Source files
    2. Project file (i.e. myproject.proj)
    ```
    <Project>
      <Import Project="$(MSBuildToolsPath)\Microsoft.Common.targets" />
      <PropertyGroup>
        <IntDir>obj/</IntDir>
        <OutDir>bin/</OutDir>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="LLVM.Build" />
      </ItemGroup>
      <ItemGroup>
        <ClangCompile Include="source1.cpp" />
        <ClangCompile Include="source2.cpp" />
        ...
        <ClangCompile Include="sourceN.cpp" />
      </ItemGroup>
    </Project>
    ```
2. Restore .NET dependencies.
    ```
    dotnet restore
    ```
3. Build the project.
    ```
    dotnet msbuild
    ```

## License

MSBuildUnixNative is licensed under the [MIT license](LICENSE).