namespace FsToolbox.Tools

open System

[<RequireQualifiedAccess>]
module Grok =

    open System.Text.RegularExpressions

    [<AutoOpen>]
    module private Internal =
        let private grokRegex = Regex("%{(\\w+):(\\w+)(?::\\w+)?}", RegexOptions.Compiled)

        let private grokRegexWithType =
            Regex("%{(\\w+):(\\w+):(\\w+)?}", RegexOptions.Compiled)

        let private grokRegexWithoutName = Regex("%{(\\w+)}", RegexOptions.Compiled)

        let replaceWithName (patterns: Map<string, string>) (m: Match) =
            let group1 = m.Groups[2]
            let group2 = m.Groups[1]

            match patterns.TryFind group2.Value with
            | Some str -> $"(?<{group1}>({str}))"
            | None -> $"(?<{group1}>)"

        let replaceWithoutName (patterns: Map<string, string>) (m: Match) =
            let group = m.Groups[1]

            match patterns.TryFind group.Value with
            | Some v -> $"({v})"
            | None -> "()"

        let parseGrokString (patterns: Map<string, string>) (str: string) =

            let rec build (pattern: string) =
                let matches = grokRegexWithType.Matches(pattern)

                let newStr =
                    grokRegexWithoutName.Replace(
                        grokRegex.Replace(pattern, replaceWithName patterns),
                        MatchEvaluator(fun m -> replaceWithoutName patterns m)
                    )

                match newStr.Equals(pattern, StringComparison.CurrentCultureIgnoreCase) with
                | true -> newStr
                | false -> build newStr

            build str

        let compileRegex (pattern: string) =
            Regex(pattern, RegexOptions.Compiled ||| RegexOptions.ExplicitCapture)

    type GrokContext =
        { Regex: Regex
          GroupNames: string list }

    let create (patterns: Map<string, string>) (grokStr: string) =
        let compiled =
            parseGrokString patterns grokStr |> compileRegex

        { Regex = compiled
          GroupNames = compiled.GetGroupNames() |> List.ofSeq }
        
    let run (ctx: GrokContext) (str: string) =
        let m = ctx.Regex.Match(str)

        m.Groups
        |> List.ofSeq
        |> List.choose (fun g ->
            match g.Name <> "0" with
            | true ->
                ctx.GroupNames
                |> List.tryFind (fun gn -> gn.Equals(g.Name, StringComparison.CurrentCultureIgnoreCase))
                |> Option.map (fun gn -> gn, g.Value)
            | false -> None)
        |> Map.ofList