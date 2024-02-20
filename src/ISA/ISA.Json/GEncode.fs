﻿namespace ARCtrl.ISA.Json

open Thoth.Json.Core

open Fable.Core
open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
module GEncode = 

    [<Emit("$1[$0]")>]
    let getFieldFable (name : string) (object : 'T) = jsNative

    /// Try to get a property value from a record by its name 
    let inline tryGetPropertyValue (name : string) (object : 'T) =
        #if FABLE_COMPILER
            getFieldFable name object
        #else
        let property = 
            FSharp.Reflection.FSharpType.GetRecordFields(object.GetType())
            |> Array.tryFind (fun property -> property.Name.Contains name)
        property
        |> Option.bind (fun property -> 
            match FSharp.Reflection.FSharpValue.GetRecordField (object,property) with
            | ARCtrl.ISA.Aux.Update.SomeObj o -> 
                Some o
            | o when isNull o -> 
                None
            | o -> 
                Some o
        )
        #endif

    let inline toJsonString spaces (value : Json) = 
        #if FABLE_COMPILER
        Thoth.Json.JavaScript.Encode.toString spaces value
        #else
        Thoth.Json.Newtonsoft.Encode.toString spaces value
        #endif

    let inline includeString (value : string) = 
        Json.String value

    let inline choose (kvs : (string * Json) list) = 
        kvs
        |> List.choose (fun (k,v) -> 
            if v = Encode.nil then None
            else Some (k,v)
        )

    /// Try to encode the given object using the given encoder, or return Encode.nil if the object is null
    ///
    /// If the object is a sequence, encode each element using the given encoder and return the resulting sequence
    let tryIncludeObj name (encoder : obj -> Json) (value : obj option) = 
        name,
        match value with
        #if FABLE_COMPILER
        | Some (:? System.Collections.IEnumerable as v) ->                  
            !!Seq.map encoder v |> Encode.seq
        #else
        | Some(:? seq<obj> as os) ->                 
            Seq.map encoder os |> Encode.seq
        #endif
        | Some(o) -> encoder o
        | _ -> Encode.nil

    /// Try to encode the given object using the given encoder, or return Encode.nil if the object is null
    let tryInclude name (encoder : 'Value -> Json) (value : 'Value option) = 
        name,
        match value with
        | Some(o) -> encoder o
        | _ -> Encode.nil

    /// Try to encode the given object using the given encoder, or return Encode.nil if the object is null
    let tryIncludeSeq name (encoder : 'Value -> Json) (value : #seq<'Value> option) = 
        name,
        match value with
        | Some(os) -> Seq.map encoder os |> Encode.seq
        | _ -> Encode.nil


    let tryIncludeArray name (encoder : 'Value -> Json) (value : 'Value array option) = 
        name,
        match value with
        | Some(os) -> os |> Array.map encoder |> Encode.array
        | _ -> Encode.nil

    let tryIncludeList name (encoder : 'Value -> Json) (value : 'Value list option) = 
        name,
        match value with
        | Some(os) -> os |> List.map encoder |> Encode.list
        | _ -> Encode.nil

    // This seems to fail because due to dotnet not able to match the boxed lists against nongeneric System.Collections.IEnumerable
    //
    // ->    ///// Try to encode the given object using the given encoder, or return Encode.nil if the object is null
    // ->    /////
    // ->    ///// If the object is a sequence, encode each element using the given encoder and return the resulting sequence
    // ->    //let tryInclude name (encoder : obj -> JsonValue) (value : obj option) = 
    // ->    //    name,
    // ->    //    match value with
    // ->    //    | Some(:? System.Collections.IEnumerable as v) ->          
    // ->    //        let os = 
    // ->    //            #if FABLE_COMPILER
    // ->    //            !!v
    // ->    //            #else
    // ->    //            v |> Seq.cast<obj>
    // ->    //            #endif
    // ->    //        Seq.map encoder os |> Encode.seq
    // ->    //    | Some(o) -> encoder o
    // ->    //    | _ -> Encode.nil