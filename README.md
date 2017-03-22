[Building]
`dotnet build`
(The above command implicitly grabs the only existing .sln file. It expands to: `dotnet msbuild .\MSBuildCppUnix.sln /t:Build`)

[Repository Layout (Outdated)]

MSBuild
	LLVM
	MSVC
	$(OSPlatform).props
Source
	Samples
		Vars.proj
	Tasks

Build (Generated)
	$(Platform)
		$(Configuration)
Target (Generated)
	$(Platform)
		$(Configuration)
