﻿module TestTasks

open BlackFox.Fake
open Fake.DotNet

open ProjectInfo
open BasicTasks
open Fake.Core

module RunTests = 

    let runTestsUI = BuildTask.create "runTestsUI" [clean; build] {
        let path = "tests/UI"
        Trace.traceImportant "Start UI tests"
        // transpile library for native access
        run dotnet $"fable src/ARCtrl -o {path}/ARCtrl" ""
        GenerateIndexJs.ARCtrl_generate($"{path}/ARCtrl")
        run npx $"cypress run --component -P {path}" ""
    }

    let runTestsJsNative = BuildTask.create "runTestsJSNative" [clean; build] {
        Trace.traceImportant "Start native JavaScript tests"
        for path in ProjectInfo.jsTestProjects do
            // transpile library for native access
            run dotnet $"fable src/ARCtrl -o {path}/ARCtrl" ""
            GenerateIndexJs.ARCtrl_generate($"{path}/ARCtrl")
            run npx $"mocha {path} --timeout 20000" "" 
    }

    let runTestsJs = BuildTask.create "runTestsJS" [clean; build] {
        for path in ProjectInfo.testProjects do
            // transpile js files from fsharp code
            run dotnet $"fable {path} -o {path}/js" ""
            // run mocha in target path to execute tests
            // "--timeout 20000" is used, because json schema validation takes a bit of time.
            run npx $"mocha {path}/js --timeout 20000" ""
    }

    /// <summary>
    /// Until we reach full Py compatibility we use these paths to check only compatible projects
    /// </summary>
    let testProjectsPy = 
        [
            //"tests/ISA/ISA.Tests"
            //"tests/ISA/ISA.Json.Tests"
            //"tests/ISA/ISA.Spreadsheet.Tests"
            "tests/ARCtrl"
        ]

    let runTestsPy = BuildTask.create "runTestsPy" [clean; build] {
        for path in ProjectInfo.testProjects do
            //transpile py files from fsharp code
            run dotnet $"fable {path} -o {path}/py --lang python" ""
            // run pyxpecto in target path to execute tests in python
            run python $"{path}/py/main.py" ""
    }

    let runTestsDotnet = BuildTask.create "runTestsDotnet" [clean; build] {
        let dotnetRun = run dotnet "run"
        testProjects
        |> Seq.iter dotnetRun
    }


module PerformanceReport = 

    let cpu = 
        Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\HARDWARE\\DESCRIPTION\\SYSTEM\\CentralProcessor\\0", "ProcessorNameString", null) :?> string

    let testPerformancePy = BuildTask.create "testPerformancePy" [clean; build] {
        let path = "tests/TestingUtils"
     //transpile py files from fsharp code
        run dotnet $"fable {path} -o {path}/py --lang python" ""
        // run pyxpecto in target path to execute tests in python
        run python $"{path}/py/performance_report.py {cpu}" ""
    }
    let testPerformanceJs = BuildTask.create "testPerformanceJS" [clean; build] {
        let path = "tests/TestingUtils"
        // transpile js files from fsharp code
        run dotnet $"fable {path} -o {path}/js" ""
        // run mocha in target path to execute tests
        run npx $"mocha {path}/js {cpu}" ""
    }
    let testPerformanceDotnet = BuildTask.create "testPerformanceDotnet" [clean; build] {
        let path = "tests/TestingUtils"
        run dotnet $"run --project {path} {cpu}" ""
    }

let perforanceReport = BuildTask.create "PerformanceReport" [PerformanceReport.testPerformancePy; PerformanceReport.testPerformanceJs; PerformanceReport.testPerformanceDotnet] { 
    ()
}

let runTests = BuildTask.create "RunTests" [clean; build; RunTests.runTestsJs; RunTests.runTestsJsNative; RunTests.runTestsPy; RunTests.runTestsDotnet] { 
    ()
}