namespace Fable.SimpleHttp

open Fable.Import.JS
open Fable.Import.Browser
 
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
}