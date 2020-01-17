module Utils
open System
open System.Linq
open System.Text.RegularExpressions

let rev = System.Linq.Enumerable.Reverse

let regex (s:string) = new Regex(s)

let select  f xs = Enumerable.Select(xs, new Func<_,_>(f))
let orderBy f xs = Enumerable.OrderBy(xs, new Func<_,_>(f))
