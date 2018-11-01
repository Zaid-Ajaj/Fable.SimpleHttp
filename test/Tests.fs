module Tests

open QUnit
open Fable.SimpleHttp
open Fable.SimpleJson 

registerModule "Simple Http Tests"
 
setTimeout 5000

testCaseAsync "Http.get returns text when status is OK" <| fun test ->
    async {
        let! text = Http.get "/api/get-first"
        do test.areEqual text "first"
    }

testCaseAsync "Http.get throws when status code is not OK" <| fun test ->
    async {
        let! result = Async.Catch (Http.get "/api/not-existent") 
        match result with 
        | Choice1Of2 text -> test.failwith "Exected ERROR!"
        | Choice2Of2 error -> test.passWith error.Message
    }

testCaseAsync "Http.get returns text when status is OK" <| fun test ->
    async {
        let! text = Http.get "/api/get-first"
        do test.areEqual text "first"
    }

testCaseAsync "Http.getSafe resolves correctly when response is 200" <| fun test ->
    async {
        let! (status, responseText) = Http.getSafe "/api/get-first"
        test.areEqual 200 status 
        test.areEqual "first" responseText 
    }

testCaseAsync "Http.getSafe resolves correctly when response is 404" <| fun test ->
    async {
        let! (status, responseText) = Http.getSafe "/api/not-existent" 
        test.areEqual status 404
        test.areEqual responseText "Not Found"
    }

testCaseAsync "Http.post resolves correctly when resposne is 200" <| fun test ->
    async {
        let input = "my data"
        let! responseText = Http.post "/api/post-echo" input
        test.areEqual input responseText
    }

testCaseAsync "Http.post throws when resposne is 404" <| fun test ->
    async {
        let input = "data"
        let! responseText = Http.post "/api/post-echo" input
        test.areEqual input responseText
    }

testCaseAsync "Http.postSafe, well, safely resolves when response is 200" <| fun test ->
    async {
        let input = "data"
        let! (statuscode, responseText) = Http.postSafe "/api/post-echo" input 
        test.areEqual 200 statuscode 
        test.areEqual input responseText
    }

testCaseAsync "Headers can be round-tripped" <| fun test ->
    async {
        let! response = 
          Http.request "/api/echo-headers"
          |> Http.method GET
          |> Http.header (Headers.authorization "Bearer: <token>")
          |> Http.header (Headers.contentType "application/json")
          |> Http.send 

        test.areEqual 200 response.statusCode
        let headers = Json.parseAs<Map<string, string>> response.responseText
        match Map.tryFind "authorization" headers, Map.tryFind "content-type" headers with 
        | Some "Bearer: <token>", Some "application/json" -> test.pass() 
        | otherwise -> test.unexpected otherwise
    }

testCaseAsync "Body content can be round-tripped" <| fun test ->
    async {
        let! response = 
          Http.request "/api/post-echo"
          |> Http.method POST
          |> Http.content (BodyContent.Text "hello")
          |> Http.send 

        test.areEqual 200 response.statusCode
        test.areEqual "hello" response.responseText
    }

testCaseAsync "Empty body content is allowed" <| fun test ->
    async {
        let! response = 
          Http.request "/api/post-echo"
          |> Http.method POST
          |> Http.content BodyContent.Empty
          |> Http.send 

        test.areEqual 200 response.statusCode
        test.areEqual "" response.responseText
    }

testCaseAsync "Form data can be round-tripped" <| fun test ->
    async {
        let formData = 
            FormData.create()
            |> FormData.append "firstName" "Zaid"
            |> FormData.append "lastName" "Ajaj"
        
        let! response = 
            Http.request "/api/echo-form"
            |> Http.method POST 
            |> Http.content (BodyContent.Form formData) 
            |> Http.send 

        test.areEqual 200 response.statusCode 
        let form = Json.parseAs<Choice<string, string> list> response.responseText
        match form with 
        | [ Choice1Of2 "Zaid"; Choice1Of2 "Ajaj" ] -> test.pass()
        | otherwise -> test.unexpected otherwise
    }  

testCaseAsync "Binary blob data can be roundtripped" <| fun test -> 
    async {
        let blob = Blob.fromText "hello!"

        let! response = 
            Http.request "/api/echoBinary"
            |> Http.method POST 
            |> Http.overrideResponseType ResponseTypes.Blob
            |> Http.content (BodyContent.Binary blob)
            |> Http.send 

        test.areEqual 200 response.statusCode
        test.areEqual "" response.responseText

        match response.content with 
        | ResponseContent.Blob result -> 
            let! content = FileReader.readBlobAsText result 
            test.areEqual "hello!" content 
        | _ -> 
            test.failwith "Expected to read binary data"

        Map.tryFind "content-type" response.responseHeaders 
        |> function 
            | Some "application/octet-stream" -> test.pass()
            | _ -> test.unexpected response.responseHeaders
    }