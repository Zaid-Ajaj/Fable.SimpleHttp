namespace Fable.SimpleHttp 

open Fable.Import.Browser

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

    let getReq (url: string) : HttpRequest =
        { defaultRequest with url = url } 

    let header (singleHeader: Header) (req: HttpRequest) = 
        { req with headers = List.append req.headers [singleHeader] }

    let headers (values: Header list) (req: HttpRequest)  = 
        { req with headers = List.append req.headers values }
    
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