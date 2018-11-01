namespace Fable.SimpleHttp
 
type HttpMethod = 
    | GET
    | POST
    | PUT
    | PATCH 
    | DELELE
    | HEAD
    | OPTIONS

type Header = Header of string * string 

[<RequireQualifiedAccess>]
type BodyContent = 
    | Empty
    | Text of string 
    | Binary of Fable.Import.Browser.Blob
    | Form of Fable.Import.Browser.FormData 

[<RequireQualifiedAccess>]
type ResponseTypes = 
    | Text 
    | Blob
    | ArrayBuffer 

type HttpRequest = {
    url: string 
    method: HttpMethod 
    headers: Header list
    overridenMimeType: Option<string> 
    overridenResponseType: Option<ResponseTypes>
    content: BodyContent
} 

[<RequireQualifiedAccess>]
type ResponseContent = 
    | Text of string 
    | Blob of Fable.Import.Browser.Blob 
    | ArrayBuffer of Fable.Import.JS.ArrayBuffer
    | Unknown of obj

type HttpResponse = {
    statusCode: int 
    responseText: string
    responseType: string 
    responseHeaders: Map<string, string>
    content : ResponseContent
} 