module Parser

open System.Collections.Generic
open System
open System.IO
open Utils

// Public models
type Color = Green | Blue | Red | Yellow | Orange

type KeyConfig = 
  | Static of code: string * color: Color
  | Wave of code: string * colors: Color list
  | Disco of code: string * color1: Color * color2: Color * color3: Color

  member self.Key() = 
    match self with
    | Static (code, _) -> code
    | Wave (code, _) -> code
    | Disco (code, _, _, _) -> code

type ParseResult = Result<KeyConfig list,string>

// Private models
type private LineTokens = string list
type private Effect = Static | Wave | Disco

// Parse all supposed keys from a line input
let private parseKeys (keysLine:LineTokens) =
  let rec _parseKeys (keysLine:LineTokens) =
    match keysLine with
    | keyCode :: rest ->
      match regex(@"[A-Za-z]").IsMatch(keyCode) with
      | true -> _parseKeys rest
      | false -> Error ("INVALID: Invalid key token : " + keyCode)
    | [] -> Ok true

  match _parseKeys keysLine with
  | Ok _ -> Ok keysLine
  | Error e -> Error e

// Parse all supposed effects from a line input
let private parseEffects (effectLine:LineTokens) =
  if 1 = effectLine.Length then
    match effectLine.Head with
    | "static" -> Ok Effect.Static
    | "wave" -> Ok Effect.Wave
    | "disco" -> Ok Effect.Disco
    | invalidEffect -> Error ("Unknown effect : " + invalidEffect)
  else
    Error ("INVALID: Only one effect expected by line : " + String.Join("/ ", effectLine))

// Parse all supposed colors from a line input
let private parseColors (colorsLine:LineTokens) =
  let rec _parseColors (colorsLine:LineTokens) (acc : Color list) =
    match colorsLine with
    | "green" :: rest -> _parseColors rest (Green::acc)
    | "blue" :: rest -> _parseColors rest (Blue::acc)
    | "red" :: rest -> _parseColors rest (Red::acc)
    | "yellow" :: rest -> _parseColors rest (Yellow::acc)
    | "orange" :: rest -> _parseColors rest (Orange::acc)
    | invalidColor :: rest -> Error ("INVALID: Invalid color : " + invalidColor)
    | [] -> Ok acc

  _parseColors colorsLine []

// Parse a supposed effect from 3 input lines
let private parseEffect (keysLine:LineTokens) (effectLine:LineTokens) (colorsLine:LineTokens) =
  match parseKeys keysLine with 
  | Error e -> Error e
  | Ok keys ->
    match parseEffects effectLine with 
    | Error e -> Error e
    // We are parsing a static effect
    | Ok Effect.Static ->
      match parseColors colorsLine with
      | Error e -> Error e
      | Ok [color] -> keys |> List.map (fun keyCode -> KeyConfig.Static(keyCode, color)) |> Ok
      | Ok colors -> Error ("INVALID: Static effects are single color only : " + String.Join(" / ", colors))
    // We are parsing a wave effect
    | Ok Effect.Wave ->
      match parseColors colorsLine with
      | Error e -> Error e 
      | Ok colors -> keys |> List.map (fun keyCode -> KeyConfig.Wave(keyCode, colors)) |> Ok
    // We are parsing a disco effect
    | Ok Effect.Disco ->
      match parseColors colorsLine with
      | Error e -> Error e 
      | Ok (color1 :: color2 :: [color3]) -> keys |> List.map (fun keyCode -> KeyConfig.Disco(keyCode, color1, color2, color3)) |> Ok
      | Ok colors -> Error ("INVALID: Disco effects need 3 colors : " + String.Join(" / ", colors))

// Parse all the input tokens and transform them into an ordered KeyConfig list
let private parseTokens (tokens: LineTokens list) =
  // This dictionary will cache all the key settings
  let allKeyConfigs = new Dictionary<string, KeyConfig>()

  // Add key configs to the cache
  let addKeyConfigs (keyConfigs:KeyConfig list) =
    for keyConfig in keyConfigs do
      let keyCode = keyConfig.Key()
      if (allKeyConfigs.ContainsKey(keyCode)) then
        allKeyConfigs.Remove(keyCode) |> ignore
      allKeyConfigs.Add(keyCode, keyConfig)

  let rec _parseEffects (tokens: LineTokens list) =
    // Read tokens by 3 (color / effect / keys)
    match tokens with
    | keysLine :: effectLine :: colorsLine :: rest ->
        match parseEffect keysLine effectLine colorsLine with
        | Ok keyConfigs ->
          addKeyConfigs keyConfigs
          _parseEffects rest
        | Error e -> Error e
    // Not enough line, the effect is incomplete
    | head :: rest ->
        Error "INVALID: Missing lines to complete the config"
    | [] ->
        // Convert back the mutable dictionary into an immutable ordered list
        // We order the key configs items
        allKeyConfigs 
        |> select (fun keyValue -> keyValue.Value)
        |> orderBy (fun keyConfig -> keyConfig.Key()) 
        |> List.ofSeq
        |> Ok

  _parseEffects tokens

// Clean empty tokens, apply lower case
let private cleanInputAndReverse (inputConfig: string list) =
  inputConfig
  |> List.map (
    fun inputLine ->
      inputLine.ToLower()
      |> regex(",").Split
      |> Array.map (fun token -> token.Trim())
      |> Array.filter (String.IsNullOrWhiteSpace >> not)
      |> Array.toList
    )

// Parse lines from a config file
let parse (inputConfig: string list) =
  cleanInputAndReverse inputConfig
  |> parseTokens

// Parse a file
let parsefile filename =
  match File.Exists filename with
  | true -> parse (File.ReadAllLines filename |> Array.toList)
  | false -> Error ("ERROR: Provided input file doesn't exist :" + filename)
