[Repository Layout]

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