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
    let listening, server = startWebServerAsync suaveConfig webApp
    
    Async.Start server
    printfn "Web server started"

    printfn "Getting server ready to listen for reqeusts"
    listening
    |> Async.RunSynchronously
    |> ignore
    
    printfn "Server listening to requests"

    let mutable autoClose = false
    let driversDir = root </> "server" </> "drivers"
    let options = FirefoxOptions()
    match Array.contains "--headless" argv with 
    | true -> 
        autoClose <- true 
        options.AddArgument("--headless")
        options.AddArgument("--no-sandbox")
    | false -> () 

    printfn "Starting FireFox Driver"
    use driver = new FirefoxDriver(driversDir, options)
    driver.Url <- sprintf "http://localhost:%d/index.html" port
    
    let mutable testsFinishedRunning = false

    while not testsFinishedRunning do
      // give tests time to run
      printfn "Tests have not finished running yet"
      printfn "Waiting for another 5 seconds"
      Threading.Thread.Sleep(5 * 1000)
      try 
        driver.FindElementByClassName("failed") |> ignore
        testsFinishedRunning <- true
      with 
        | _ -> ()

    let passedTests = unbox<string> (driver.ExecuteScript("return JSON.stringify(passedTests, null, 4);"))
    let failedTests = unbox<string> (driver.ExecuteScript("return JSON.stringify(failedTests, null, 4);"))
    Console.ForegroundColor <- ConsoleColor.Green
    printfn "Tests Passed: \n%s" passedTests
    Console.ForegroundColor <- ConsoleColor.Red
    printfn "Tests Failed: \n%s" failedTests
    Console.ResetColor()
    let failed = driver.FindElementByClassName("failed")
    let success = driver.FindElementByClassName("passed")

    let failedText = failed.Text

    printfn ""
    printfn "Passed: %s" success.Text
    printfn "Failed: %s" failed.Text

    if autoClose 
    then 
      cts.Cancel()
      driver.Quit()
    else 
      printfn "Finished testing, press any key to continue..."
      Console.ReadKey() |> ignore
    
    try 
      let failedCount = int failedText
      if failedCount <> 0 then 1
      else 0
    with
    | e ->
        printfn "Error occured while parsing the number of failed tests"
        printfn "%s\n%s" e.Message e.StackTrace
        1 // return an integer exit code