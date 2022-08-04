namespace Fable.SimpleHttp

open System
open Browser
open Browser.Types
open Fable.Core

#if !FABLE_COMPILER
open System.Net.Http
#endif

module Blob =
    /// Creates a Blob from the given input string
    [<Emit("new Blob([$0], { 'type':'text/plain' })")>]
    let fromText (value: string) : Blob = jsNative

    [<Emit("window.URL.createObjectURL($0)")>]
    let createObjectURL (blob: Blob) : string = jsNative

    /// Download a Blob
    let download (blob: Blob) (fileName: string) =
        let element = unbox<HTMLAnchorElement> (document.createElement "a")
        element.target <- "_blank"
        element.href <- createObjectURL(blob)
        element.setAttribute("download", fileName)
        document.body.appendChild(element) |> ignore
        element.click()
        document.body.removeChild(element) |> ignore

module File =
    /// Creates a File from the given input string and file name
    [<Emit("new File([$0], $1, { 'type':'text/plain' })")>]
    let fromText (value: string) (fileName: string) : File = jsNative


/// Utility functions to work with blob and file APIs.
module FileReader =
    /// Asynchronously reads the blob data content as string
    let readBlobAsText (blob: Blob) : Async<string> =
        Async.FromContinuations <| fun (resolve, _, _) ->
            let reader = FileReader.Create()
            reader.onload <- fun _ ->
                if reader.readyState = FileReaderState.DONE
                then resolve (unbox reader.result)

            reader.readAsText(blob)

    /// Asynchronously reads the blob data content as string
    let readFileAsText (file: File) : Async<string> =
        Async.FromContinuations <| fun (resolve, _, _) ->
            let reader = FileReader.Create()
            reader.onload <- fun _ ->
                if reader.readyState = FileReaderState.DONE
                then resolve (unbox reader.result)

            reader.readAsText(file)

module FormData =

    /// Creates a new FormData object
    [<Emit("new FormData()")>]
    let create() : FormData = jsNative

    /// Appends a key-value pair to the form data
    let append (key:string) (value:string) (form : FormData) : FormData =
        form.append(key, value)
        form

    /// Appends a file to the form data
    let appendFile (key: string) (file: File) (form: FormData) : FormData =
        form.append (key, file)
        form

    /// Appends a named file to the form data
    let appendNamedFile (key: string) (fileName: string) (file: File) (form: FormData) : FormData =
        form.append (key, file, fileName)
        form

    /// Appends a blog to the form data
    let appendBlob (key: string) (blob: Blob) (form: FormData) : FormData =
        form.append (key, blob)
        form

    /// Appends a blog to the form data
    let appendNamedBlob (key: string) (fileName: string) (blob: Blob) (form: FormData) : FormData =
        form.append (key, blob, fileName)
        form

module Headers =
    let contentType value = Header("Content-Type", value)
    let accept value = Header("Accept", value)
    let acceptCharset value = Header("Accept-Charset", value)
    let acceptEncoding value = Header("Accept-Encoding", value)
    let acceptLanguage value = Header("Accept-Language", value)
    let acceptDateTime value = Header("Accept-Datetime", value)
    let authorization value = Header("Authorization", value)
    let cacheControl value = Header("Cache-Control", value)
    let connection value = Header("Connection", value)
    let cookie value = Header("Cookie", value)
    let contentMD5 value = Header("Content-MD5", value)
    let date value = Header("Date", value)
    let expect value = Header("Expect", value)
    let ifMatch value = Header("If-Match", value)
    let ifModifiedSince value = Header("If-Modified-Since", value)
    let ifNoneMatch value = Header("If-None-Match", value)
    let ifRange value = Header("If-Range", value)
    let IfUnmodifiedSince value = Header("If-Unmodified-Since", value)
    let maxForwards value = Header("Max-Forwards", value)
    let origin value = Header ("Origin", value)
    let pragma value = Header("Pragma", value)
    let proxyAuthorization value = Header("Proxy-Authorization", value)
    let range value = Header("Range", value)
    let referer value = Header("Referer", value)
    let userAgent value = Header("User-Agent", value)
    let create key value = Header(key, value)

module Http =

    let private defaultRequest =
        { url = "";
          method = HttpMethod.GET
          headers = []
          withCredentials = false
          overridenMimeType = None
          overridenResponseType = None
          timeout = None
          content = BodyContent.Empty }

    let private emptyResponse =
        { statusCode = 0
          responseText = ""
          responseType = ""
          responseUrl = ""
          responseHeaders = Map.empty
          content = ResponseContent.Text "" }

    let private splitAt (delimiter: string) (input: string) : string [] =
        if String.IsNullOrEmpty input then [| input |]
        else input.Split([| delimiter |], StringSplitOptions.None)

    let private serializeMethod = function
        | HttpMethod.GET -> "GET"
        | HttpMethod.POST -> "POST"
        | HttpMethod.PATCH -> "PATCH"
        | HttpMethod.PUT -> "PUT"
        | HttpMethod.DELETE -> "DELETE"
        | HttpMethod.OPTIONS -> "OPTIONS"
        | HttpMethod.HEAD -> "HEAD"

    /// Starts the configuration of the request with the specified url
    let request (url: string) : HttpRequest =
        { defaultRequest with url = url }

    /// Sets the Http method of the request
    let method httpVerb (req: HttpRequest) =
        { req with method = httpVerb }

    /// Appends a header to the request configuration
    let header (singleHeader: Header) (req: HttpRequest) =
        { req with headers = List.append req.headers [singleHeader] }

    /// Appends a list of headers to the request configuration
    let headers (values: Header list) (req: HttpRequest)  =
        { req with headers = List.append req.headers values }

    /// Enables cross-site credentials such as cookies
    let withCredentials (enabled: bool) (req: HttpRequest) =
        { req with withCredentials = enabled }

    /// Enables Http request timeout
    let withTimeout (timeoutInMilliseconds: int) (req: HttpRequest) =
        { req with timeout = Some timeoutInMilliseconds}

    /// Specifies a MIME type other than the one provided by the server to be used instead when interpreting the data being transferred in a request. This may be used, for example, to force a stream to be treated and parsed as "text/xml", even if the server does not report it as such.
    let overrideMimeType (value: string) (req: HttpRequest) =
        { req with overridenMimeType = Some value }

    /// Change the expected response type from the server
    let overrideResponseType (value: ResponseTypes) (req: HttpRequest) =
        { req with overridenResponseType = Some value }

    /// Sets the body content of the request
    let content (bodyContent: BodyContent) (req: HttpRequest) : HttpRequest =
        { req with content = bodyContent }

    /// Sends the request to the server, this function does not throw
    let send (req: HttpRequest) : Async<HttpResponse> =
#if FABLE_COMPILER
        Async.FromContinuations <| fun (resolve, reject, _) ->
            let xhr = XMLHttpRequest.Create()
            xhr.``open``(serializeMethod req.method, req.url)
            xhr.onreadystatechange <- fun _ ->
                if xhr.readyState = ReadyState.Done
                then resolve {
                    responseText =
                        match xhr.responseType with
                        | "" -> xhr.responseText
                        | "text" -> xhr.responseText
                        | _ -> ""

                    statusCode = int xhr.status
                    responseType = xhr.responseType
                    content =
                        match xhr.responseType with
                        | ("" | "text") -> ResponseContent.Text xhr.responseText
                        | "arraybuffer" -> ResponseContent.ArrayBuffer (unbox xhr.response)
                        | "blob" -> ResponseContent.Blob (unbox xhr.response)
                        | _ -> ResponseContent.Unknown xhr.response

                    responseHeaders =
                        xhr.getAllResponseHeaders()
                        |> splitAt "\r\n"
                        |> Array.choose (fun headerLine ->
                            let parts = splitAt ":" headerLine
                            match List.ofArray parts with
                            | key :: rest ->  Some (key.ToLower(), (String.concat ":" rest).Trim())
                            | otherwise -> None)
                        |> Map.ofArray

                    responseUrl = xhr.responseURL
                }

            for (Header(key, value)) in req.headers do
                xhr.setRequestHeader(key, value)

            xhr.withCredentials <- req.withCredentials

            match req.overridenMimeType with
            | Some mimeType -> xhr.overrideMimeType(mimeType)
            | None -> ()

            match req.overridenResponseType with
            | Some ResponseTypes.Text -> xhr.responseType <- "text"
            | Some ResponseTypes.Blob -> xhr.responseType <- "blob"
            | Some ResponseTypes.ArrayBuffer -> xhr.responseType <- "arraybuffer"
            | None -> ()

            match req.timeout with
            | Some timeout -> xhr.timeout <- timeout
            | None -> ()

            match req.content with 
            | BodyContent.Empty -> xhr.send()
            | BodyContent.Text value -> xhr.send(value)
            | BodyContent.Form formData -> xhr.send(formData)
            | BodyContent.Binary blob -> xhr.send(blob)
#else
        async {
            try
                use requestMessage = new HttpRequestMessage()
                requestMessage.RequestUri <- Uri(req.url)
                requestMessage.Method <-
                    match req.method with
                    | HttpMethod.GET     -> HttpMethod.Get
                    | HttpMethod.POST    -> HttpMethod.Post
                    | HttpMethod.PUT     -> HttpMethod.Put
                    | HttpMethod.PATCH   -> HttpMethod "PATCH"
                    | HttpMethod.DELETE  -> HttpMethod.Delete
                    | HttpMethod.HEAD    -> HttpMethod.Head
                    | HttpMethod.OPTIONS -> HttpMethod.Options
                req.headers
                |> Seq.iter (fun (Header (key, value)) ->
                    requestMessage.Headers.Add(key, value))
                use content =
                    match req.content with
                    | BodyContent.Text text -> new StringContent(text)
                    | BodyContent.Empty -> null
                    | _ -> failwith "Only BodyContent.Text is supported in the dotnet implementation"
                requestMessage.Content <- content

                use client = new HttpClient()

                match req.timeout with
                | Some timeout -> client.Timeout <- TimeSpan.FromMilliseconds(timeout)
                | None -> ()

                let! response = client.SendAsync requestMessage |> Async.AwaitTask
                let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                let headers =
                    response.Headers
                    |> Seq.choose (fun kv ->
                        kv.Value
                        |> Seq.tryLast
                        |> Option.map (fun value -> kv.Key, value))
                    |> Map.ofSeq

                return
                    { statusCode = int response.StatusCode
                      responseText = responseBody
                      responseType = "text"
                      responseHeaders = headers
                      responseUrl = req.url
                      content = ResponseContent.Text responseBody }
            with
            // We're catching a lot here to mimic the behaviour of the JS
            // implementation, which isn't able to expose the kind of error.
            | :? ArgumentException ->
                return emptyResponse // invalid uri
            | :? HttpRequestException
            | :? AggregateException as aggrEx when (aggrEx.InnerException :? HttpRequestException) ->
                return emptyResponse // connection errors
        }
#endif

    /// Safely sends a GET request and returns a tuple(status code * response text). This function does not throw.
    let get url : Async<int * string> =
        async {
            let! response =
                request url
                |> method HttpMethod.GET
                |> send
            return response.statusCode, response.responseText
        }

    /// Safely sends a PUT request and returns a tuple(status code * response text). This function does not throw.
    let put url (data: string) : Async<int * string> =
        async {
            let! response =
                request url
                |> method HttpMethod.PUT
                |> content (BodyContent.Text data)
                |> send
            return response.statusCode, response.responseText
        }

    /// Safely sends a DELETE request and returns a tuple(status code * response text). This function does not throw.
    let delete url : Async<int * string> =
        async {
            let! response =
                request url
                |> method HttpMethod.DELETE
                |> send
            return response.statusCode, response.responseText
        }

    /// Safely sends a PATCH request and returns a tuple(status code * response text). This function does not throw.
    let patch url (data: string) : Async<int * string> =
        async {
            let! response =
                request url
                |> method HttpMethod.PATCH
                |> content (BodyContent.Text data)
                |> send
            return response.statusCode, response.responseText
        }

    /// Safely sends a POST request and returns a tuple(status code * response text). This function does not throw.
    let post url (data: string) : Async<int * string> =
        async {
            let! response =
                request url
                |> method HttpMethod.POST
                |> content (BodyContent.Text data)
                |> send
            return response.statusCode, response.responseText
        }
