$repositoryRoot = Get-Location

# Deploy Dummy Task library.
##Set-Location ("$repositoryRoot/Source/Samples/DummyTask" -replace '/', [IO.Path]::DirectorySeparatorChar)
##dotnet restore
##dotnet publish -o ("$repositoryRoot/Target/DotNet/Debug/DummyTask" -replace '/', [IO.Path]::DirectorySeparatorChar)

$project = ('Source/Tasks' -replace '/', [IO.Path]::DirectorySeparatorChar)
# Deploy LLVM Task library.
dotnet restore $project
dotnet build $project -f netstandard1.3 -o ('Target/DotNet/Debug/Tasks' -replace '/', [IO.Path]::DirectorySeparatorChar)