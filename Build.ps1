$repositoryRoot = "$(Get-Location)"

# Deploy Dummy Task library.
Set-Location $repositoryRoot\Source\Samples\DummyTask
dotnet restore
dotnet publish -o $repositoryRoot\Target\DotNet\Debug\DummyTask

# Deploy LLVM Task library.
Set-Location $repositoryRoot\Source\Tasks
dotnet restore
dotnet publish -o $repositoryRoot\Target\DotNet\Debug\Tasks

Set-Location $repositoryRoot