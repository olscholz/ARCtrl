﻿module JsonSchema.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open ISADotNet.Json

let tests_FableBindings = testList "FableBindings" [ 
    testCase "Hello World" (fun () -> 
        let actual = Fable.JsonValidation.helloWorld()
        let expected = "Hello World"
        Expect.equal actual expected "Test if js validator is correctly referenced with hello world example"
    )
    testAsync "Minimal example" { 
        let commentSchemaURL = "https://raw.githubusercontent.com/HLWeil/isa-specs/anyof/source/_static/isajson/comment_schema.json"
        let commentInstance = """{
            "name": "velit amet",
            "value": "minim ut reprehenderit cillum commodo"
        }"""
        let! isValid, errorList = Fable.validate commentSchemaURL commentInstance
        Expect.equal isValid true "Test if json is correctly validated."
        Expect.equal errorList Array.empty "Test for error list"
    }
    testAsync "Minimal example - fail" { 
        let commentSchemaURL = "https://raw.githubusercontent.com/HLWeil/isa-specs/anyof/source/_static/isajson/comment_schema.json"
        let commentInstance = """{
            "name": 12,
            "value": "minim ut reprehenderit cillum commodo"
        }"""
        let! isValid, errorList = Fable.validate commentSchemaURL commentInstance
        Expect.equal isValid false "Test if json is correctly validated."
        Expect.equal errorList.Length 1 "Test for error list"
    }
]

open ISADotNet

let tests_Process =     

    testSequenced <| testList "Process" [

        testAsync "ProcessDateCorrect" { 
            
            let! vr = Validation.validateProcess TestFiles.Validation.processDate

            Expect.isTrue vr.Success (sprintf "Process schema validation should have succeded but did not: %A" (vr.GetErrors()))

        }

        testAsync "ProcessDateTimeCorrect" {
            
            let! vr = Validation.validateProcess TestFiles.Validation.processDateTime

            Expect.isTrue vr.Success (sprintf "Process schema validation should have succeded but did not: %A" (vr.GetErrors()))

        }

        testAsync "ProcessDateWrong" {
            
            let! vr = Validation.validateProcess TestFiles.Validation.processDateWrong

            Expect.isFalse vr.Success "Process schema validation should have failed but did not"

        }

        testAsync "ProcessAdditionalField" {
            
            let! vr = Validation.validateProcess TestFiles.Validation.processAdditionalField

            Expect.isFalse vr.Success "Process schema validation should have failed but did not"

        }

        testAsync "ProcessBroken" {
            
            let! vr = Validation.validateProcess TestFiles.Validation.processBroken

            Expect.isFalse vr.Success "Process schema validation should have failed but did not"

        }
    ]

let tests_ProcessParameterValue =     

    testSequenced <| testList "ProcessParameterValue" [

        testAsync "PPVUnit" { 
            
            let! vr = Validation.validateProcessParameterValue TestFiles.Validation.ppvUnit

            Expect.isTrue vr.Success (sprintf "Process parameter value schema validation should have succeded but did not: %A" (vr.GetErrors()))

        }

        testAsync "PPVOntology" { 
            
            let! vr = Validation.validateProcessParameterValue TestFiles.Validation.ppvOntology

            Expect.isTrue vr.Success (sprintf "Process parameter value schema validation should have succeded but did not: %A" (vr.GetErrors()))

        }

        testAsync "PPVUriWrong" { 
            
            let! vr = Validation.validateProcessParameterValue TestFiles.Validation.ppvUriWrong

            Expect.isFalse vr.Success "Process schema validation should have failed but did not"
            
        }

        testAsync "PPVAdditionalField" { 
            
            let! vr = Validation.validateProcessParameterValue TestFiles.Validation.ppvAdditionalField

            Expect.isFalse vr.Success "Process schema validation should have failed but did not"

        }

        testAsync "PPVBroken" { 
            
            let! vr = Validation.validateProcessParameterValue TestFiles.Validation.ppvBroken

            Expect.isFalse vr.Success "Process schema validation should have failed but did not"

        }
    ]

let main = 
    testList "JsonSchema-Validation" [
        #if FABLE_COMPILER
        tests_FableBindings
        #endif
        tests_Process
        tests_ProcessParameterValue
    ]