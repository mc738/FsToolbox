namespace FsToolbox.UnitTests.Paths


module GenericPath =

    open FsToolbox.Paths.GenericPath
    open FsToolbox.Paths.GenericPath.Parsing
    open FsToolbox.Testing.MsTest.Assertions
    open Microsoft.VisualStudio.TestTools.UnitTesting

    

    [<TestClass>]
    type GenericPathTests() =


        [<TestMethod>]
        member this.``Parse basic path``() =
            let expected =
                [ { Selector = SelectorToken.Child "foo"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "bar"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.foo.bar"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member this.``Parse array slice``() =
            let expected =
                [ { Selector = SelectorToken.Child "foo"
                    Filter = None
                    ArraySelector = Some "2:4" }
                  { Selector = SelectorToken.Child "bar"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.foo[2:4].bar"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member this.``Parse filter``() =
            let expected =
                [ { Selector = SelectorToken.Child "foo"
                    Filter = Some "@.bar =~ '^TEST%'"
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "bar"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.foo[?(@.bar =~ '^TEST%')].bar"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member this.``Parse filter and array slice``() =
            let expected =
                [ { Selector = SelectorToken.Child "foo"
                    Filter = Some "@.bar =~ '^TEST%'"
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "bar"
                    Filter = None
                    ArraySelector = Some "1:4" }
                  { Selector = SelectorToken.Child "baz"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.foo[?(@.bar =~ '^TEST%')].bar[1:4].baz"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member this.``Parse delimited filter value``() =
            let expected =
                [ { Selector = SelectorToken.Child "foo"
                    Filter = Some "@.bar =~ '^TEST)]%'"
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "bar"
                    Filter = None
                    ArraySelector = Some "1:4" }
                  { Selector = SelectorToken.Child "baz"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.foo[?(@.bar =~ '^TEST)]%')].bar[1:4].baz"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member this.``Parse delimited name``() =
            let expected =
                [ { Selector = SelectorToken.Child "'foo.baz'"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "bar"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.'foo.baz'.bar"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member _.``Parse single character selector name``() =
            // $.store.f.book

            let expected =
                [ { Selector = SelectorToken.Child "store"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "f"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "book"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.store.f.book"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member _.``Parse single character selector name at end``() =
            // $.store.f.book

            let expected =
                [ { Selector = SelectorToken.Child "store"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "books"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "f"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.store.books.f"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member _.``Parse single character selector name with filter``() =
            // $.store.f.book

            let expected =
                [ { Selector = SelectorToken.Child "store"
                    Filter = None
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "f"
                    Filter = Some "@.price<10"
                    ArraySelector = None }
                  { Selector = SelectorToken.Child "book"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.store.f[?(@.price<10)].book"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member _.``Parse basic child union``() =
            let expected =
                [ { Selector = SelectorToken.ChildUnion "a,b"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.a,b"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")
          
        
        [<TestMethod>]
        member _.``Parse basic child union with filter array selector``() =
            let expected =
                [ { Selector = SelectorToken.ChildUnion "a,b"
                    Filter = None
                    ArraySelector = Some "1" } ]

            let path = "$.a,b[1]"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")

        [<TestMethod>]
        member _.``Parse basic child union with delimited name``() =
            let expected =
                [ { Selector = SelectorToken.ChildUnion "'a,b',c"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.'a,b',c"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")
            
        
        [<TestMethod>]
        member _.``Parse basic child union with delimited name and array selector``() =
            let expected =
                [ { Selector = SelectorToken.ChildUnion "'a,b',c"
                    Filter = None
                    ArraySelector = Some "1" } ]

            let path = "$.'a,b',c[1]"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")
            
            
        [<TestMethod>]
        member _.``Parse basic child with delimited name including comma``() =
            let expected =
                [ { Selector = SelectorToken.Child "'a,b'"
                    Filter = None
                    ArraySelector = None } ]

            let path = "$.'a,b'"
            let result = parse path '$' true

            match result with
            | ParserResult.Success actual -> Assert.AreEqual(expected, actual)
            | _ -> Assert.Fail($"Parsing failed. Error: '{result}'")
            
        [<TestMethod>]
        member _.``Create child union selector``() =
            
            let expected = Selector.ChildUnion [ "a"; "b" ]
            
            let actual = Selector.FromToken (SelectorToken.ChildUnion "a,b")
            
            assertEqual expected actual
            
            
        [<TestMethod>]
        member _.``Create child union selector with delimited characters``() =
            
            let expected = Selector.ChildUnion [ "a,b"; "c" ]
            
            let actual = Selector.FromToken (SelectorToken.ChildUnion "'a,b',c")
            
            assertEqual expected actual
            