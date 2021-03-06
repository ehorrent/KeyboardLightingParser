﻿open System
open Parser

/// Display the result of the parsing
let displayResult (parseResult:ParseResult) = 
  match parseResult with
  | Error e -> Console.WriteLine e
  | Ok keyConfigs -> 
    for keyConfig in keyConfigs do
      match keyConfig with
      | Static(code, color) -> 
        let msg = String.Format("{0}, {1}, {2}", code, "static", color)
        Console.WriteLine msg
      | Wave (code, colors) ->
        let strColors = String.Join(", ", colors)
        let msg = String.Format("{0}, {1}, [{2}]", code, "wave", strColors)
        Console.WriteLine msg
      | Disco (code, color1, color2, color3) ->
        let msg = String.Format("{0}, {1}, [{2}, {3}, {4}]", code, "disco", color1, color2, color3)
        Console.WriteLine msg

[<EntryPoint>]
let main argv =
  match argv |> Array.truncate 1 with
    | [|filename|] -> 
      // Parse the file
      let parseResult = parsefile filename

      // Display the result
      displayResult parseResult

      // Return corresponding code
      match parseResult with
      | Ok _ -> 0
      | Error _ -> 1
    | _ ->
      Console.WriteLine("ERROR: Please provide an input file as argument")
      1
