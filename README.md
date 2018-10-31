# Fable.SimpleHttp [![Build Status](https://travis-ci.org/Zaid-Ajaj/Fable.SimpleHttp.svg?branch=master)](https://travis-ci.org/Zaid-Ajaj/Fable.SimpleHttp) [![Build status](https://ci.appveyor.com/api/projects/status/i17usjpn7bbiwm9n?svg=true)](https://ci.appveyor.com/project/Zaid-Ajaj/fable-SimpleHttp) [![Nuget](https://img.shields.io/nuget/v/Fable.SimpleHttp.svg?maxAge=0&colorB=brightgreen)](https://www.nuget.org/packages/Fable.SimpleHttp)

A library for easily making Http requests in Fable projects.

### Installation
Install from nuget using paket
```sh
paket add nuget Fable.SimpleHttp --project path/to/YourProject.fsproj 
```
Make sure the references are added to your paket files
```sh
# paket.dependencies (solution-wide dependencies)
nuget Fable.SimpleHttp

# paket.refernces (project-specific dependencies)
Fable.SimpleHttp
```

## Building and running tests
Requirements

 - Dotnet core 2.1+
 - Mono 5.0+
 - Node 10.0+


Running the watching the tests live 
```sh
./build.sh RunLiveTests 
```
Building the tests and running them using QUnut cli runner
```sh
./build.sh RunTests
```
or just `Ctrl + Shift + B` to run the cli tests as a VS Code task
