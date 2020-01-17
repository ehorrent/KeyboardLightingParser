module KeyboardLightingParserTests

open Parser
open NUnit.Framework

// Helper functions
let shouldSucceed parseResult = 
  match parseResult with
  | Ok _ -> Assert.Pass()
  | Error e -> Assert.Fail("Should not return an error")

let shouldFail parseResult = 
  match parseResult with
  | Ok _ -> Assert.Fail("Should return a parsing error")
  | Error e -> Assert.Pass()

[<Test>]
let ``For the same key, last effect should be used`` () =
  let entry = [
    "a"
    "wave"
    "green, blue, red"
    "a"
    "static"
    "green"
  ]

  match parse entry with
  | Ok [Static ("a", Green)] -> Assert.Pass()
  | Ok _ -> Assert.Fail("Invalid parsing result")
  | Error e -> Assert.Fail(e)

[<Test>]
let ``Parse should fail with static effects containing multiple colors`` () =
  let entry = ["a"; "static"; "green, blue"]
  shouldFail (parse entry)

[<Test>]
let ``Parse should succeed with static effects containing a single color`` () =
  let entry = ["a"; "static"; "green"]
  shouldSucceed (parse entry)

[<Test>]
let ``Parse should fail with disco effects containing not enough colors`` () =
  let entry = ["a"; "disco"; "green, blue"]
  shouldFail (parse entry)

[<Test>]
let ``Parse should fail with disco effects containing too many colors`` () =
  let entry = ["a"; "disco"; "green, blue, red, yellow"]
  shouldFail (parse entry)

[<Test>]
let ``Parse should succeed with disco effects containing 3 colors`` () =
  let entry = ["a"; "disco"; "green, blue, red"]
  shouldSucceed (parse entry)

[<Test>]
let ``Parse should return valid colors`` () =
  let entry = ["a"; "wave"; "green, blue, red"]
  match parse entry with
  | Ok [Wave ("a", [Red; Blue; Green])] -> Assert.Pass()
  | Ok _ -> Assert.Fail("Invalid parsing result")
  | Error e -> Assert.Fail(e)

[<Test>]
let ``Parse should return corresponding effects`` () =
  let staticEntry = ["a"; "static"; "green"]
  match parse staticEntry with
  | Ok [Static _] -> Assert.Pass()
  | Ok _ -> Assert.Fail("Invalid parsing result")
  | Error e -> Assert.Fail(e)

  let waveEntry = ["a"; "wave"; "green, blue, red"]
  match parse waveEntry with
  | Ok [Wave _] -> Assert.Pass()
  | Ok _ -> Assert.Fail("Invalid parsing result")
  | Error e -> Assert.Fail(e)

  let discoEntry = ["a"; "disco"; "green, blue, red"]
  match parse discoEntry with
  | Ok [Disco _] -> Assert.Pass()
  | Ok _ -> Assert.Fail("Invalid parsing result")
  | Error e -> Assert.Fail(e)

[<Test>]
let ``Parse should fail with wave effects containing no color`` () =
  let entry = ["a"; "disco"; ""]
  shouldFail (parse entry)

[<Test>]
let ``Parse should succeed with wave effects containing multiple colors`` () =
  let entry = ["a"; "wave"; "green, blue, red, yellow, orange"]
  shouldSucceed (parse entry)

[<Test>]
let ``Parse should fail with an invalid color`` () =
  let entry = ["a"; "static"; "greenss"]
  shouldFail (parse entry)

[<Test>]
let ``Parse should succeed with a valid color`` () =
  let entry = ["a"; "wave"; "green, blue, red, yellow, orange"]
  shouldSucceed (parse entry)

[<Test>]
let ``Parse should fail with an invalid key`` () =
  let entry = ["1"; "static"; "green"]
  shouldFail (parse entry)

[<Test>]
let ``Parse should succeed with valid keys`` () =
  let entry = [
    "a"; "static"; "green";
    "b"; "static"; "green";
    "c"; "static"; "green";
    "d"; "static"; "green";
    "e"; "static"; "green";
    "f"; "static"; "green";
    "g"; "static"; "green";
    "h"; "static"; "green";
    "i"; "static"; "green";
    "j"; "static"; "green";
    "k"; "static"; "green";
    "l"; "static"; "green";
    "m"; "static"; "green";
  ]

  shouldSucceed (parse entry)

[<Test>]
let ``Parse should be case insensitive`` () =
  let entry = ["A"; "sTatiC"; "grEEn";]
  shouldSucceed (parse entry)