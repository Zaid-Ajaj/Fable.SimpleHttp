namespace Fable.SimpleHttp

type HttpMethod =
    | GET
    | POST
    | PUT
    | PATCH
    | DELETE
    | HEAD
    | OPTIONS

type Header = Header of string * string

[<RequireQualifiedAccess>]
type BodyContent =
    | Empty
    | Text of string
    | Binary of Browser.Types.Blob
    | Form of Browser.Types.FormData

[<RequireQualifiedAccess>]
type ResponseTypes =
    | Text
    | Blob
    | ArrayBuffer

type HttpRequest = {
    url: string
    method: HttpMethod
    headers: Header list
    withCredentials: bool
    overridenMimeType: Option<string>
    overridenResponseType: Option<ResponseTypes>
    timeout: Option<int>
    content: BodyContent
}

[<RequireQualifiedAccess>]
type ResponseContent =
    | Text of string
    | Blob of Browser.Types.Blob
    | ArrayBuffer of Fable.Core.JS.ArrayBuffer
    | Unknown of obj

type HttpResponse = {
    statusCode: int
    responseText: string
    responseType: string
    responseUrl: string
    responseHeaders: Map<string, string>
    content : ResponseContent
}
