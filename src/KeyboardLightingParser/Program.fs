open System
open Parser

let displayResult (result:ParseResult) = 
  match result with
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

  let entry1 = [
    "a, b, c"
    "wave"
    "green, blue"
  ]

  let entry2 = [
    "a, b, c, d"
    "static"
    "green"
    "a, t, v"
    "static"
    "red"
    "d, e, f"
    "wave"
    "red, blue"
    "t, u, v"
    "disco"
    "red, green, orange"
  ]

  let entry3 = [
    "a"
    "static"
    "red, green"
  ]

  // Parse
  let parseResult = parse entry3

  // Display
  displayResult parseResult |> ignore
  
  // Return error code if necessary
  match parseResult with
  | Ok _ -> 0
  | Error _ -> 1

  (*
  match parseColors ["Red"; "Yellow"; "3232"; "Blue"] with
  | Ok _ -> Console.WriteLine "Ok"
  | Error e -> Console.WriteLine e
  *)

  (*
  match parseEffects ["Wave"] with
  | Ok _ -> printf "Ok"
  | Error e -> Console.WriteLine e
  *)
  // We only check the first argument
 (*
  match argv |> Array.truncate 1 with
  | [|filename|] -> 
    parseAndDisplayResult filename
  | _ ->
    Console.WriteLine("Please provide an input file")
    1
  *)
