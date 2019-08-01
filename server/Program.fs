// Learn more about F# at http://fsharp.org

open System
open Suave
open Suave.Successful
open System.IO
open OpenQA.Selenium
open OpenQA.Selenium.Firefox
open System.Threading
open Suave.Logging
open Suave.Operators
open Suave.Filters
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open System.Text
open Fable.Remoting.Json
open Newtonsoft.Json
open Newtonsoft.Json
open PuppeteerSharp

let (</>) x y = Path.Combine(x, y)
let converter = FableJsonConverter() :> JsonConverter
let serialize x = JsonConvert.SerializeObject(x, converter)

let rec findRoot dir =
    if File.Exists(System.IO.Path.Combine(dir, "paket.dependencies"))
    then dir
    else
        let parent = Directory.GetParent(dir)
        if isNull parent then
            failwith "Couldn't find root directory"
        findRoot parent.FullName

[<EntryPoint>]
let main argv =
    let runningTests = Array.contains "--testing" argv

    let cwd = Directory.GetCurrentDirectory()
    let root = findRoot cwd
    let rnd = new Random()
    let randomPort = rnd.Next(5000, 9000)
    let port =
      if not runningTests
      then 8085
      else randomPort

    let cts = new CancellationTokenSource()

    let suaveConfig =
        { defaultConfig with
            homeFolder = Some (root </> "public")
            bindings   = [ HttpBinding.createSimple HTTP "127.0.0.1" port ]
            bufferSize = 2048
            cancellationToken = cts.Token }

    let utf8 (bytes: byte []) = Encoding.UTF8.GetString bytes


    let okBytes (input: byte []) : WebPart =
      fun ctx -> async {
        let nextCtx = { ctx  with response = { ctx.response  with HttpResult.content = HttpContent.Bytes input; HttpResult.status = HTTP_200.status } }
        return Some nextCtx
      }

    let webApp =
      choose [
        GET >=> Files.browseHome
        GET >=> path "/api/get-first" >=> OK "first"
        POST >=> path "/api/post-echo" >=> request (fun r -> OK (utf8 r.rawForm))
        GET >=> path "/api/echo-headers" >=> request (fun r -> OK (serialize r.headers))
        POST >=> path "/api/echo-form" >=> request (fun r ->

          OK (serialize [r.fieldData "firstName" |> Choice.toResult ; r.fieldData "lastName" |> Choice.toResult])
         )
        POST >=> path "/api/echoBinary" >=> request (fun r -> okBytes r.rawForm) >=> Writers.setMimeType "application/octet-stream"
        OK "Not Found" >=> Writers.setStatus HttpCode.HTTP_404
      ]

    if not runningTests
    then
      printfn "Starting web server..."
      startWebServer suaveConfig webApp
      0
    else
    printfn "Starting Integration Tests"
    printfn ""
    printfn "========== SETUP =========="
    printfn ""
    printfn "Downloading chromium browser..."
    let browserFetcher = BrowserFetcher()
    browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore
    printfn "Chromium browser downloaded"

    let listening, server = startWebServerAsync suaveConfig webApp

    Async.Start server
    printfn "Web server started"

    printfn "Getting server ready to listen for reqeusts"
    listening
    |> Async.RunSynchronously
    |> ignore

    printfn "Server listening to requests"

    let launchOptions = LaunchOptions()
    launchOptions.ExecutablePath <- browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision)
    launchOptions.Headless <- true

    async {
        use! browser = Async.AwaitTask(Puppeteer.LaunchAsync(launchOptions))
        use! page = Async.AwaitTask(browser.NewPageAsync())
        printfn ""
        printfn "Navigating to http://localhost:%d/index.html" port
        let! _ = Async.AwaitTask (page.GoToAsync (sprintf "http://localhost:%d/index.html" port))
        let stopwatch = System.Diagnostics.Stopwatch.StartNew()
        let toArrayFunction = """
        window.domArr = function(elements) {
            var arr = [ ];
            for(var i = 0; i < elements.length;i++) arr.push(elements.item(i));
            return arr;
        };
        """

        let getResultsFunctions = """
        window.getTests = function() {
            var tests = document.querySelectorAll("div.passed, div.executing, div.failed, div.pending");
            return domArr(tests).map(function(test) {
                var name = test.getAttribute('data-test')
                var type = test.classList[0]
                var module =
                    type === 'failed'
                    ? test.parentNode.parentNode.parentNode.getAttribute('data-module')
                    : test.parentNode.parentNode.getAttribute('data-module')
                return [name, type, module];
            });
        }
        """
        let! _ = Async.AwaitTask (page.EvaluateExpressionAsync(toArrayFunction))
        let! _ = Async.AwaitTask (page.EvaluateExpressionAsync(getResultsFunctions))
        let! _ = Async.AwaitTask (page.WaitForExpressionAsync("document.getElementsByClassName('executing').length === 0"))
        stopwatch.Stop()
        printfn "Finished running tests, took %d ms" stopwatch.ElapsedMilliseconds
        let passingTests = "document.getElementsByClassName('passed').length"
        let! passedTestsCount = Async.AwaitTask (page.EvaluateExpressionAsync<int>(passingTests))
        let failingTests = "document.getElementsByClassName('failed').length"
        let! failedTestsCount = Async.AwaitTask (page.EvaluateExpressionAsync<int>(failingTests))
        let pendingTests = "document.getElementsByClassName('pending').length"
        let! pendingTestsCount = Async.AwaitTask(page.EvaluateExpressionAsync<int>(pendingTests))
        let! testResults = Async.AwaitTask (page.EvaluateExpressionAsync<string [] []>("window.getTests()"))
        printfn ""
        printfn "========== SUMMARY =========="
        printfn ""
        printfn "Total test count %d" (passedTestsCount + failedTestsCount + pendingTestsCount)
        printfn "Passed tests %d" passedTestsCount
        printfn "Failed tests %d" failedTestsCount
        printfn "Skipped tests %d" pendingTestsCount
        printfn ""
        printfn "========== TESTS =========="
        printfn ""
        let moduleGroups = testResults |> Array.groupBy (fun arr -> arr.[2])

        for (moduleName, tests) in moduleGroups do
            for test in tests do
                let name = test.[0]
                let testType = test.[1]

                match testType with
                | "passed" ->
                    Console.ForegroundColor <- ConsoleColor.Green
                    printfn "√ %s / %s" moduleName name
                | "failed" ->
                    Console.ForegroundColor <- ConsoleColor.Red
                    printfn "X %s / %s" moduleName name
                | "pending" ->
                    Console.ForegroundColor <- ConsoleColor.Blue
                    printfn "~ %s / %s" moduleName name
                | other ->
                    printfn "~ %s / %s" moduleName name

        Console.ResetColor()
        printfn ""
        printfn "Stopping web server..."
        cts.Cancel()
        printfn "Exit code: %d" failedTestsCount
        return failedTestsCount
    }

    |> Async.RunSynchronously