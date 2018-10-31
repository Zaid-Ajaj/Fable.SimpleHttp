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