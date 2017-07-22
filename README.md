# MSBuild UNIX Native

This project adds support for building native code (C/C++) using MSBuild directly on UNIX platforms, using non-MSVC compiler toolsets.

### Source Code

Sources available at: `https://github.com/JunielKatarn/MSBuildUnixNative.git`.

## Building

### Development Environment

These are the currently tested operating systems:
* Windows 10 - Creators Update, using Windows Subsystem for Linux (WSL).
* Ubuntu 16.04
* macOS 10.12 (Sierra)

### Prerequisites
* .NET Core SDK
* LLVM/Clang compiler collection (version 4.0.1).
  * The expected default location is /opt/llvm/4.0.1.
* Ubuntu 16.04:
  * Apt packages
    * `libc6-dev`
    * `libgcc-5-dev`
* (Optional) Visual Studio 2017
  For the best editing/debugging experience, while deploying to WSL.

### Build Steps
1. `dotnet restore`
2. `dotnet msbuild`

### License

MSBuildUnixNative is licensed under the [MIT license](LICENSE).