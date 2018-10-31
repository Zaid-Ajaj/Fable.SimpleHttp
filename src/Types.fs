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
    | Form of Fable.Import.Browser.FormData 

type HttpRequest = {
    url: string 
    method: HttpMethod 
    headers: Header list 
    content: BodyContent
} 

type HttpResponse = {
    statusCode: int 
    responseText: string
    responseType: string 
    responseHeaders: Map<string, string>
} 