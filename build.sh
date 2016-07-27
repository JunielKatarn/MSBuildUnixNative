#!/bin/bash

repositoryRoot="`pwd`"

# Deploy Dummmy Task library.
cd $repositoryRoot/Source/Samples/DummyTask
dotnet restore
dotnet publish -o $repositoryRoot/Target/DotNet/Debug/DummyTask

# Deploy LLVM Task library.
cd $repositoryRoot/Source/Tasks
dotnet restore
dotnet publish -o $repositoryRoot/Target/DotNet/Debug/Tasks

cd $repositoryRoot
