module Tests

open QUnit
open Fable.SimpleHttp

registerModule "Simple Http Tests"
 
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
        let! responseText = Http.post "/api/echo" input
        test.areEqual input responseText
    }

testCaseAsync "Http.post throws when resposne is 404" <| fun test ->
    async {
        let input = "data"
        let! responseText = Http.post "/api/echo" input
        test.areEqual input responseText
    }

testCaseAsync "Http.postSafe, well, safely resolves when response is 200" <| fun test ->
    async {
        let input = "data"
        let! (statuscode, responseText) = Http.postSafe "/api/echo" input 
        test.areEqual 200 statuscode 
        test.areEqual input responseText
    }