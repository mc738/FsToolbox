namespace FsToolbox.Core

open System.Security.Cryptography

module Array =

    let randomItem<'T> (a: 'T array) = a[RandomNumberGenerator.GetInt32(0, a.Length)]

