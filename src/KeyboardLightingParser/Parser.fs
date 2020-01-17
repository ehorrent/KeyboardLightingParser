module Parser

open System.Collections.Generic
open System
open System.IO
open Utils

// -------------------------------------------------------------------------------------
// Public parser models
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

// -------------------------------------------------------------------------------------
// Private models

type private InputLineTokens = string list
type private Effect = Static | Wave | Disco

// -------------------------------------------------------------------------------------
// Private functions

/// Parse all supposed keys from a line input.
let private parseKeys (keysLine:InputLineTokens) =
  let rec _parseKeys (tokens:InputLineTokens) =
    match tokens with
    | keyCode :: rest ->
      match regex(@"[A-Za-z]").IsMatch(keyCode) with
      | true -> _parseKeys rest
      | false -> Error ("INVALID: Invalid key token : " + keyCode)
    | [] -> Ok true

  match _parseKeys keysLine with
  | Ok _ -> Ok keysLine
  | Error e -> Error e

/// Parse all supposed effects from a line.
let private parseEffects (effectLine:InputLineTokens) =
  if 1 = effectLine.Length then
    // Transform into a typed effect
    match effectLine.Head with
    | "static" -> Ok Effect.Static
    | "wave" -> Ok Effect.Wave
    | "disco" -> Ok Effect.Disco
    | invalidEffect -> Error ("Unknown effect : " + invalidEffect)
  else
    Error ("INVALID: Only one effect expected by line : " + String.Join("/ ", effectLine))

/// Parse all supposed colors from a line.
let private parseColors (colorsLine:InputLineTokens) =
  let rec _parseColors (colorsLine:InputLineTokens) (acc : Color list) =
    // Transform into typed colors
    match colorsLine with
    | "green" :: rest -> _parseColors rest (Blue::acc)
    | "blue" :: rest -> _parseColors rest (Blue::acc)
    | "red" :: rest -> _parseColors rest (Red::acc)
    | "yellow" :: rest -> _parseColors rest (Yellow::acc)
    | "orange" :: rest -> _parseColors rest (Orange::acc)
    | invalidColor :: rest -> Error ("INVALID: Invalid color : " + invalidColor)
    | [] -> Ok acc

  _parseColors colorsLine []

/// Parse a supposed effect from 3 lines.
/// Check the constraints for each effect type (Static / Wave / Disco).
let private parseEffect (keysLine:InputLineTokens) (effectLine:InputLineTokens) (colorsLine:InputLineTokens) =
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

/// Parse all the input tokens and transform them into an ordered KeyConfig list.
/// Use a mutable dictionary internally to be more efficient.
let private parseTokens (tokens: InputLineTokens list) =
  /// This dictionary will cache all the key settings
  let allKeyConfigs = new Dictionary<string, KeyConfig>()

  /// Helper function to add key configs to the cache
  let addKeyConfigs (keyConfigs:KeyConfig list) =
    for keyConfig in keyConfigs do
      let keyCode = keyConfig.Key()
      if (allKeyConfigs.ContainsKey(keyCode)) then
        allKeyConfigs.Remove(keyCode) |> ignore
      allKeyConfigs.Add(keyCode, keyConfig)

  /// Recursive function to parse all the lines
  let rec _parseEffects (allTokens: InputLineTokens list) =
    match allTokens with
    // Read tokens by 3 [keys / effect / color]
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
        // End of the lines, we convert back the mutable dictionary into an immutable ordered list.
        // We also order the key configs items.
        allKeyConfigs 
        |> select (fun keyValue -> keyValue.Value)
        |> orderBy (fun keyConfig -> keyConfig.Key()) 
        |> List.ofSeq
        |> Ok

  _parseEffects tokens

/// Transform the input into "clean" tokens:
/// - remove spaces and empty lines
/// - apply lower case
let private tokenize (inputConfig: string list) =
  inputConfig
  |> List.map (
    fun inputLine ->
      inputLine.ToLower()
      |> regex(",").Split
      |> Array.map (fun token -> token.Trim())
      |> Array.filter (String.IsNullOrWhiteSpace >> not)
      |> Array.toList
    )

// -------------------------------------------------------------------------------------
// Public functions

/// Parse lines containing a keyboard lighting configuration.
let parse (inputConfig: string list) =
  inputConfig
  |> tokenize 
  |> parseTokens

/// Parse a file containing keyboard lighting configuration.
let parsefile filename =
  match File.Exists filename with
  | true -> parse (File.ReadAllLines filename |> Array.toList)
  | false -> Error ("ERROR: Provided input file doesn't exist :" + filename)
