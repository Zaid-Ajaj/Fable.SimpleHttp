module Program

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Xml
open System.Xml.Linq
open Fake.IO
open Fake.Core

let path xs = Path.Combine(Array.ofList xs)

let solutionRoot = Files.findParent __SOURCE_DIRECTORY__ "Fable.SimpleHttp.sln";

let src = path [ solutionRoot; "src" ]
let tests = path [ solutionRoot; "tests" ]
let server = path [ solutionRoot; "server" ]

type ShellArgs = { program: string; args: string; cwd: string }

let shell (shellArgs: ShellArgs list) =
    for args in shellArgs do
        printfn "======"
        printfn $"{args.cwd}> {args.program} {args.args}"
        printfn "======"
        let exitCode = Shell.Exec(args.program, args.args, args.cwd)
        if exitCode <> 0 then failwith $"FAILED {args.cwd}> {args.program} {args.args}"
             
             
let dotnet args cwd = {
    program = Tools.dotnet
    args = args
    cwd = cwd
}

let publishNuget projectPath = 
    Shell.cleanDirs [
        Path.Combine(projectPath, "bin")
        Path.Combine(projectPath, "obj")
    ]

    shell [ dotnet "restore --no-cache" projectPath ]
    shell [ dotnet "pack -c Release" projectPath ]
    let nugetKey =
        match Environment.environVarOrNone "NUGET_KEY" with
        | Some nugetKey -> nugetKey
        | None ->
            printfn "The Nuget API key must be set in a NUGET_KEY environmental variable"
            System.Console.Write("Nuget API Key: ")
            System.Console.ReadLine()

    let nupkg =
        Directory.GetFiles(Path.Combine(projectPath, "bin", "Release"))
        |> Seq.head
        |> Path.GetFullPath

    let pushCmd = $"nuget push {nupkg} -s nuget.org -k {nugetKey}" 
    shell [ dotnet pushCmd projectPath ]

let args =
    Environment.GetCommandLineArgs()
    |> Array.skip 1

match args with
| [| "build" |] ->
     shell [
        dotnet "fable clean --yes" src
        dotnet "fable" src
        dotnet "build" src
     ]
     
| [| "publish" |] -> publishNuget src

| otherwise -> printfn $"unknown args %A{otherwise}" 