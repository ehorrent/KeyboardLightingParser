module Utils

open System
open System.Linq
open System.Text.RegularExpressions

// Regex helper
let regex (s:string) = new Regex(s)

// Linq helpers
let select  f xs = Enumerable.Select(xs, new Func<_,_>(f))
let orderBy f xs = Enumerable.OrderBy(xs, new Func<_,_>(f))
