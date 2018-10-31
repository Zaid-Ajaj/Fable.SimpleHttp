namespace Fable.SimpleHttp 

open Fable.Import.Browser
open Fable.Core.Exceptions
open Fable.Core 

module FormData = 

    [<Emit("new FormData()")>]
    /// Creates a new FormData object
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
    let create key value = Header(key, value)

module Http = 
    let private defaultRequest = 
        { url = ""; 
          method = HttpMethod.GET
          headers = []
          content = BodyContent.Empty }

    [<Emit("$1.split($0)")>]
    let private splitAt (delimeter: string) (input: string) : string [] = jsNative

    let private serializeMethod = function 
        | HttpMethod.GET -> "GET"
        | HttpMethod.POST -> "POST"
        | HttpMethod.PATCH -> "PATCH"
        | HttpMethod.PUT -> "PUT"
        | HttpMethod.DELELE -> "DELETE"
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
    
    /// Sends the request to the server
    let send (req: HttpRequest) : Async<HttpResponse> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``(serializeMethod req.method, req.url)
            xhr.onreadystatechange <- fun _ ->
                if int xhr.readyState = 4 (* DONE *)
                then resolve {
                    responseText = xhr.responseText
                    statusCode = int xhr.status 
                    responseType = xhr.responseType
                    responseHeaders = 
                        xhr.getAllResponseHeaders()
                        |> splitAt "\r\n"
                        |> Array.choose (fun headerLine -> 
                            let parts = splitAt ":" headerLine 
                            match List.ofArray parts with 
                            | key :: rest ->  Some (key.ToLower(), String.concat ":" rest)
                            | otherwise -> None)
                        |> Map.ofArray
                }

            for (Header(key, value)) in req.headers do
                xhr.setRequestHeader(key, value) 

            match req.method, req.content with 
            | GET, _ -> xhr.send(None) 
            | _, BodyContent.Empty -> xhr.send(None)
            | _, BodyContent.Text value -> xhr.send(value)
            | _, BodyContent.Form formData -> xhr.send(formData)

    let content (bodyContent: BodyContent) (req: HttpRequest) : HttpRequest = 
        { req with content = bodyContent }

    let get url : Async<string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("GET", url)
            xhr.onreadystatechange <- fun _ ->
              if int xhr.readyState = 4 (* DONE *)
              then if xhr.status = 200.0
                   then resolve xhr.responseText 
                   else 
                       let error = sprintf "Server responded with %d Error (%s) for GET request at %s" 
                       let errorMsg = error (int xhr.status) xhr.statusText url
                       reject (new System.Exception(errorMsg)) 
            
            xhr.send(None)      

    let getSafe url : Async<int * string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("GET", url)
            xhr.onreadystatechange <- fun _ ->
                if int xhr.readyState = 4 (* DONE *)
                then resolve (int xhr.status, xhr.responseText)
            xhr.send(None) 

    let put url (data: string) : Async<string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("PUT", url)
            xhr.onreadystatechange <- fun _ ->
              if int xhr.readyState = 4 (* DONE *)
              then if xhr.status = 200.0
                   then resolve xhr.responseText 
                   else 
                       let error = sprintf "Server responded with %d Error (%s) for GET request at %s" 
                       let errorMsg = error (int xhr.status) xhr.statusText url
                       reject (new System.Exception(errorMsg)) 
            
            xhr.send(data)      

    let putSafe url (date: string): Async<int * string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("PUT", url)
            xhr.onreadystatechange <- fun _ ->
                if int xhr.readyState = 4 (* DONE *)
                then resolve (int xhr.status, xhr.responseText)
            xhr.send(None) 

    let patch url (data: string) : Async<string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("PUT", url)
            xhr.onreadystatechange <- fun _ ->
              if int xhr.readyState = 4 (* DONE *)
              then if xhr.status = 200.0
                   then resolve xhr.responseText 
                   else 
                       let error = sprintf "Server responded with %d Error (%s) for GET request at %s" 
                       let errorMsg = error (int xhr.status) xhr.statusText url
                       reject (new System.Exception(errorMsg)) 
            
            xhr.send(data)      

    let patchSafe url (data: string) : Async<int * string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("PUT", url)
            xhr.onreadystatechange <- fun _ ->
                if int xhr.readyState = 4 (* DONE *)
                then resolve (int xhr.status, xhr.responseText)
            xhr.send(data) 

    let post url (data:string) : Async<string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("POST", url)
            xhr.onreadystatechange <- fun _ ->
              if int xhr.readyState = 4 (* DONE *)
              then if xhr.status = 200.0
                   then resolve xhr.responseText 
                   else 
                       let error = sprintf "Server responded with %d Error (%s) for POST request at %s" 
                       let errorMsg = error (int xhr.status) xhr.statusText url
                       reject (new System.Exception(errorMsg)) 
            xhr.send(data)    

    let postSafe url (data: string) : Async<int * string> = 
        Async.FromContinuations <| fun (resolve, reject, _) ->  
            let xhr = XMLHttpRequest.Create()
            xhr.``open``("POST", url)
            xhr.onreadystatechange <- fun _ ->
                if int xhr.readyState = 4 (* DONE *)
                then resolve (int xhr.status, xhr.responseText)
            xhr.send(data) 