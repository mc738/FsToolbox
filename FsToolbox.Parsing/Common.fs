namespace FsToolbox.Parsing

open System

[<AutoOpen>]
module Common =

    [<RequireQualifiedAccess>]
    type ReadResult =
        | Found of StartIndex: int * EndIndex: int
        | NotFound
        | OutOfBounds


    type ParsableInput =
        { Input: string
          Position: int }

        static member Create(input) = { Input = input; Position = 0 }

        member pi.IsInBounds(i) = i >= 0 && i < pi.Input.Length

        member pi.InBounds() = pi.IsInBounds(pi.Position)

        member pi.GetChar(i) =
            match pi.IsInBounds(i) with
            | true -> Some pi.Input.[i]
            | false -> None

        member pi.LookAhead(i) =
            match pi.IsInBounds(i) with
            | true -> Some pi.Input.[i]
            | false -> None

        member pi.CheckChar(c: char, i: int) =
            match pi.IsInBounds(i) with
            | true -> pi.Input.[i] = c
            | false -> false

        member pi.IsChar(c: char) = pi.CheckChar(c, pi.Position)

        member pi.NextIsChar(c: char) = pi.CheckChar(c, pi.Position + 1)

        member pi.Is2Chars(c1, c2) =
            // Bit of a hack.
            // (c1 = c2 && pi.CheckChar(c1, pi.Position + 3)) is to deal with cases like "{{This is {{nested}}}}". Without it the result would be "This is {{nested}" (missing last `}`)
            match
                pi.CheckChar(c1, pi.Position),
                pi.CheckChar(c2, pi.Position + 1),
                (c1 = c2 && pi.CheckChar(c1, pi.Position + 3))
            with
            | true, true, false -> true
            | _ -> false


        member pi.IsString(startIndex: int, str: string, ?comparison: StringComparison) =
            let sc = comparison |> Option.defaultValue StringComparison.Ordinal
            let endIndex = startIndex + str.Length - 1

            match pi.GetSlice(startIndex, endIndex) with
            | Some s when String.Equals(s, str, sc) -> ReadResult.Found(startIndex, endIndex)
            | Some _ -> ReadResult.NotFound
            | None -> ReadResult.OutOfBounds


        member pi.ReadUntil(chars: char array) =


            ()

        member pi.NextNonNested() = ()
            (*
            let oc1, oc2 = pi.OpenDelimiter.[0], pi.OpenDelimiter.[1]
            let cc1, cc2 = pi.CloseDelimiter.[0], pi.CloseDelimiter.[1]

            let rec handler (pi: ParsableInput, nestingCount) =
                match pi.InBounds(), pi.Is2Chars(oc1, oc2), pi.Is2Chars(cc1, cc2) with
                | true, true, _ -> handler (pi.Advance(1), nestingCount + 1)
                | true, _, true ->
                    match nestingCount = 1 with
                    | true -> Some pi.Position
                    | false -> handler (pi.Advance(1), nestingCount - 1)
                | true, false, false -> handler (pi.Advance(1), nestingCount)
                | false, _, _ -> None

            handler (pi, 0)
            *)

        member pi.GetSlice(startIndex, endIndex) =
            match pi.IsInBounds(startIndex), pi.IsInBounds(endIndex), startIndex < endIndex with
            | true, true, true -> Some pi.Input.[startIndex..endIndex]
            | _ -> None

        member pi.Advance(x) = { pi with Position = pi.Position + x }

        member pi.SetPosition(x) = { pi with Position = x }

        member pi.CurrentChar =
            match pi.InBounds() with
            | true -> pi.Input.[pi.Position]
            | false -> '\u0000'
