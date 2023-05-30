namespace FsToolbox.S3

open FsToolbox.Core.Results

[<AutoOpen>]
module Common =

    open System
    open System.IO
    open System.Text.Json
    open System.Text.Json.Serialization
    open System.Threading
    open Amazon
    open Amazon.S3
    open Amazon.S3.Model

    type S3BucketOverview = { Name: string; CreatedOn: DateTime }

    [<CLIMutable>]
    type S3Config =
        { [<JsonPropertyName("accessKey")>]
          AccessKey: string
          [<JsonPropertyName("secretKey")>]
          SecretKey: string
          [<JsonPropertyName("regionalEndpoint")>]
          RegionalEndpoint: string
          [<JsonPropertyName("serviceUrl")>]
          ServiceUrl: string }

        static member Load(path: string) =
            match File.Exists path with
            | true -> File.ReadAllText path |> JsonSerializer.Deserialize<S3Config> |> Ok
            | false -> Error $"File '{path}' does not exist."

    type S3Bucket =
        { Name: string
          CreatedOn: DateTime
          Objects: Map<string, S3Object> }

    and S3Object =
        { Key: string
          Size: int64
          Owner: string
          LastModified: DateTime
          ETag: string }

    module private Internal =

        let attemptAsync<'T> (errorDisplayMessage: string) (fn: unit -> Async<Result<'T, FailureResult>>) =
            async {
                try
                    return! fn ()
                with ex ->
                    return
                        { Message = $"Unhandled exception: {ex.Message}"
                          DisplayMessage = errorDisplayMessage
                          Exception = Some ex }
                        |> Error
            }

        let getBucketOverviews (client: AmazonS3Client) =
            let fn _ =
                async {
                    let! r = client.ListBucketsAsync() |> Async.AwaitTask

                    return
                        r.Buckets
                        |> List.ofSeq
                        |> List.map (fun b ->
                            { Name = b.BucketName
                              CreatedOn = b.CreationDate })
                        |> Ok
                }

            attemptAsync "Failed to retrieve buckets" fn

        let getBucketObjects (bucketName: string) (client: AmazonS3Client) =
            let fn _ =
                async {
                    let! r = client.ListObjectsAsync(bucketName) |> Async.AwaitTask

                    return
                        r.S3Objects
                        |> List.ofSeq
                        |> List.map (fun o ->
                            { Key = o.Key
                              Size = o.Size
                              Owner = o.Owner.Id
                              LastModified = o.LastModified
                              ETag = o.ETag })
                        |> Ok
                }

            attemptAsync $"Failed to retrieve objects for bucket `{bucketName}`" fn

        let downloadObject (bucketName: string) (client: AmazonS3Client) (key: string) filePath append =
            let fn _ =
                async {
                    let request = GetObjectRequest()

                    request.BucketName <- bucketName
                    request.Key <- key

                    let! data = client.GetObjectAsync(request) |> Async.AwaitTask

                    data.WriteResponseStreamToFileAsync(filePath, append, CancellationToken.None)
                    |> Async.AwaitTask
                    |> ignore

                    return Ok()
                }

            attemptAsync $"Failed to download object `{key}` for bucket `{bucketName}`" fn

        let writeObjectToStream (bucketName: string) (client: AmazonS3Client) (key: string) (stream: Stream) =
            let fn _ =
                async {
                    let request = GetObjectRequest()

                    request.BucketName <- bucketName
                    request.Key <- key

                    // TODO add error handling!
                    let! data = client.GetObjectAsync(request) |> Async.AwaitTask

                    data.ResponseStream.CopyToAsync stream |> Async.AwaitTask |> ignore

                    return Ok()
                }

            attemptAsync $"Failed to write object `{key}` for bucket `{bucketName}` to stream" fn


        let getObjectStream (bucketName: string) (client: AmazonS3Client) (key: string) =
            let fn _ =
                async {
                    let request = GetObjectRequest()

                    request.BucketName <- bucketName
                    request.Key <- key

                    // TODO add error handling!
                    let! data = client.GetObjectAsync(request) |> Async.AwaitTask

                    return data.ResponseStream |> Ok
                }

            attemptAsync $"Failed to get object `{key}` stream for bucket `{bucketName}`" fn


        let uploadFile (bucketName: string) (client: AmazonS3Client) (key: string) (filePath: string) =
            let fn _ =
                async {
                    let request = PutObjectRequest()

                    request.Key <- key
                    request.BucketName <- bucketName
                    request.FilePath <- filePath

                    let! r = client.PutObjectAsync(request) |> Async.AwaitTask

                    return Ok r.HttpStatusCode
                }

            attemptAsync $"Failed to upload file `{key}` to bucket `{bucketName}`" fn


        let saveStream (bucketName: string) (client: AmazonS3Client) (key: string) (stream: Stream) =
            let fn _ =
                async {
                    let request = PutObjectRequest()

                    request.Key <- key
                    request.BucketName <- bucketName
                    request.InputStream <- stream

                    let! r = client.PutObjectAsync(request) |> Async.AwaitTask

                    return Ok r.HttpStatusCode
                }

            attemptAsync $"Failed to save stream `{key}` to bucket `{bucketName}`" fn


        let createConfig (config: S3Config) =
            let c = AmazonS3Config()

            c.RegionEndpoint <- RegionEndpoint.GetBySystemName(config.RegionalEndpoint)
            c.ServiceURL <- config.ServiceUrl
            c

    type S3Context(config: S3Config) =

        let client =
            new AmazonS3Client(config.AccessKey, config.SecretKey, Internal.createConfig config)
        //let s3

        interface IDisposable with
            member x.Dispose() = client.Dispose()

        static member Create(path: string) =
            match S3Config.Load path with
            | Ok config -> Ok(new S3Context(config))
            | Error e -> Error e

        member _.GetBucketsAsync() =
            async {
                match! Internal.getBucketOverviews client with
                | Ok overviews ->

                    let! results =
                        overviews
                        |> List.map (fun b ->
                            async {
                                match! Internal.getBucketObjects b.Name client with
                                | Ok objs ->
                                    return
                                        (b.Name,
                                         { Name = b.Name
                                           CreatedOn = b.CreatedOn
                                           Objects = objs |> List.map (fun o -> o.Key, o) |> Map.ofList })
                                        |> Ok
                                | Error f -> return Error f

                            })
                        |> Async.Sequential

                    let (ok, errors) = results |> FailureResult.separateResults

                    match errors.IsEmpty with
                    | true -> return Ok ok
                    | false -> return Error <| FailureResult.Aggregate(errors, "Failed to retrieve objects.")

                | Error f -> return Error f

            }

        member ctx.GetBuckets() =
            ctx.GetBucketsAsync() |> Async.RunSynchronously

        member _.GetBucketOverviewsAsync() = Internal.getBucketOverviews client

        member ctx.GetBucketOverviews() =
            ctx.GetBucketOverviewsAsync() |> Async.RunSynchronously

        member _.GetBucketObjectsAsync(bucketName) =
            Internal.getBucketObjects bucketName client

        member ctx.GetBucketObjects(bucketName) =
            ctx.GetBucketObjectsAsync(bucketName) |> Async.RunSynchronously

        member _.DownloadObjectAsync(bucketName, key, filePath, append) =
            Internal.downloadObject bucketName client key filePath append


        member ctx.DownloadObject(bucketName, key, filePath, append) =
            ctx.DownloadObjectAsync(bucketName, key, filePath, append)
            |> Async.RunSynchronously

        member _.UploadObjectAsync(bucketName, key, filePath) =
            Internal.uploadFile bucketName client key filePath

        member ctx.UploadObject(bucketName, key, filePath) =
            ctx.UploadObjectAsync(bucketName, key, filePath) |> Async.RunSynchronously

        member _.ObjectToStreamAsync(bucketName, key) =
            Internal.getObjectStream bucketName client key

        member ctx.ObjectToStream(bucketName, key) =
            ctx.ObjectToStreamAsync(bucketName, key) |> Async.RunSynchronously

        member _.SaveStreamAsync(bucketName, key, stream) =
            Internal.saveStream bucketName client key stream

        member ctx.SaveStream(bucketName, key, stream) =
            ctx.SaveStreamAsync(bucketName, key, stream) |> Async.RunSynchronously

        member _.Configuration = config
