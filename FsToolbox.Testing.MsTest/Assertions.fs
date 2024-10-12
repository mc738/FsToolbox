namespace FsToolbox.Testing.MsTest

open Microsoft.VisualStudio.TestTools.UnitTesting

module Assertions =

    let assertEqual<'T> (expected: 'T) (actual: 'T) = Assert.AreEqual(expected, actual)

    let assertEqualSeq<'T> (expected: 'T seq) (actual: 'T seq) =
        // NOTE - Seq.toList is used because it evaluated the seqs which is needed for Assert.AreEqual()
        Assert.AreEqual(expected |> Seq.toList, actual |> Seq.toList)

    let areEqual<'T> (expected: 'T) (actual: 'T) =
        assertEqual expected actual
        actual

    let areEqualSeq (expected: 'T) (actual: 'T) =
        assertEqualSeq expected actual
        actual

    let assertNotEqual<'T> (expected: 'T) (actual: 'T) = Assert.AreNotEqual(expected, actual)

    let assertNotEqualSeq<'T> (expected: 'T seq) (actual: 'T seq) =
        Assert.AreNotEqual(expected |> Seq.toList, actual |> Seq.toList)

    let areNotEqual<'T> (expected: 'T) (actual: 'T) =
        assertNotEqual expected actual
        actual

    let areNotEqualSeq (expected: 'T) (actual: 'T) =
        assertNotEqualSeq expected actual
        actual

    let assertSame<'T> (expected: 'T) (actual: 'T) = Assert.AreSame(expected, actual)

    let assertSameSeq<'T> (expected: 'T seq) (actual: 'T seq) =
        Assert.AreSame(expected |> Seq.toList, actual |> Seq.toList)

    let areSame<'T> (expected: 'T) (actual: 'T) =
        assertSame expected actual
        actual

    let areSameSeq (expected: 'T) (actual: 'T) =
        assertSameSeq expected actual
        actual

    let assertNotSame<'T> (expected: 'T) (actual: 'T) = Assert.AreNotSame(expected, actual)

    let assertNotSameSeq<'T> (expected: 'T seq) (actual: 'T seq) =
        Assert.AreNotSame(expected |> Seq.toList, actual |> Seq.toList)

    let areNotSame<'T> (expected: 'T) (actual: 'T) =
        assertNotSame expected actual
        actual

    let areNotSameSeq<'T> (expected: 'T seq) (actual: 'T seq) =
        assertNotSameSeq expected actual
        actual

    let unwrap<'T, 'U> (result: Result<'T, 'U>) =
        match result with
        | Ok r -> r
        | Error e ->
            Assert.Fail("Unwrapped result returned a error", e)

            Operators.Unchecked.defaultof<'T                    >

    let unwrapError<'T, 'U> (result: Result<'T, 'U>) =
        match result with
        | Ok r ->
            Assert.Fail("Unwrapped result returned a ok", r)
            Operators.Unchecked.defaultof<'U>
        | Error e -> e

    let unwrapSome<'T> (value: 'T option) =
        match value with
        | Some v -> v
        | None ->
            Assert.Fail("Unwrapped option returned none")
            Operators.Unchecked.defaultof<'T>

    let unwrapNone<'T> (value: 'T option) =
        match value with
        | Some v -> Assert.Fail("Unwrapped option returned some")
        | None -> ()

    let chainAssertions<'T> (asserts: ('T -> 'T -> unit) list) (expected: 'T) (actual: 'T) =
        asserts |> List.iter (fun assertion -> assertion expected actual)

    let bespoke<'TInput, 'TOutput> (onFailure: unit -> unit) (predict: 'TInput -> Result<'TOutput, unit>) (input: 'TInput) =
        
        
        
        ()
        
        