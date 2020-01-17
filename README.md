# KeyboardLightingParser

This project is done with F#, a .net functional programming language. If you are not familiar with this language, here are some references on the "core" features I'm using:

- Most of the functions return a [Result](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/results) type, which allow  to manage errors in a "fluent" way (with pattern matching). I haven't added bind operators on results to be as less cryptic as possible for someone which is not used to the language (see [here](https://fsharpforfunandprofit.com/posts/recipe-part2/) for more details).
- [Pattern matching](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/pattern-matching), [discriminated unions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions).

## Tools

You will need dotnet core sdk 3.1.100 to be able to build and execute the code (should work on 2.* versions but wasn't tested). It can be found [here](https://dotnet.microsoft.com/download) or just by using [chocolatey](https://chocolatey.org/packages/dotnetcore-sdk):

```powershell
choco install dotnetcore-sdk
```

The solution can be opened with vscode (which I used) or visual studio.

## Build and execution

The code was executed on windows, but it should also work on linux...

You can build the code by executing this command:

```powershell
dotnet build
```

Here is he command to execute the sample code:

```powershell
cd src\KeyboardLightingParser

# {your_input_filename} can be a relative path
dotnet run {your_input_filename}

# There is samples in the ./resource/ folder if you want to test
dotnet run .\resources\entry1.txt
```

Here is the command to execute tests:

```powershell
cd src\KeyboardLightingParserTests
dotnet test
```
