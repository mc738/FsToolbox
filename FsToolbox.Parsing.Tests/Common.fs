namespace FsToolbox.Parsing.Tests

open FsToolbox.Parsing
open Microsoft.VisualStudio.TestTools.UnitTesting


[<TestClass>]
type ParsableInputTests() =
    
    
    [<TestMethod>]
    member _.``IsString found``() =
        let expected = StringMatchResult.Match { StartIndex =  0; EndIndex = 2 }
        
        let pi = ParsableInput.Create "foo"
        
        let actual = pi.IsString(0, "foo")
            
        Assert.AreEqual(expected, actual)
        
    [<TestMethod>]
    member _.``IsString not found``() =
        let expected = StringMatchResult.NonMatch
        
        let pi = ParsableInput.Create "foo"
        
        let actual = pi.IsString(0, "bar")
            
        Assert.AreEqual(expected, actual)
        
    
    [<TestMethod>]
    member _.``IsString out of bounds``() =
        let expected = StringMatchResult.OutOfBounds
        
        let pi = ParsableInput.Create "foo"
        
        let actual = pi.IsString(0, "not found")
            
        Assert.AreEqual(expected, actual)

