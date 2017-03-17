[Building]
`MSBuild`
(The above command implicitly grabs the only existing .sln file. It expands to: `MSBuild.exe .\MSBuildCppUnix.sln /t:Build`) 

[Repository Layout (Outdated; UPDATE)]

MSBuild
	LLVM
	MSVC
	$(OS).props
Source
	DotNet
	Samples

Vars.proj (Transitive)

Build (Generated)
	$(Platform)
		$(Configuration)
Target (Generated)
	$(Platform)
	$(Configuration)

#Sample commit to test email notifications.