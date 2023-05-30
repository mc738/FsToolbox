namespace FsToolbox.S3

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
        { [<JsonPropertyName("accessKey")>] AccessKey: string
          [<JsonPropertyName("secretKey")>] SecretKey: string
          [<JsonPropertyName("regionalEndpoint")>] RegionalEndpoint: string
          [<JsonPropertyName("serviceUrl")>] ServiceUrl: string }

        static member Load(path: string) = 
            match File.Exists path with
            | true ->
                File.ReadAllText path |> JsonSerializer.Deserialize<S3Config> |> Ok
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

    module Internal =

        let getBucketOverviews (client: AmazonS3Client) =
            async {
                let! r = client.ListBucketsAsync() |> Async.AwaitTask

                return
                    r.Buckets
                    |> List.ofSeq
                    |> List.map
                        (fun b ->
                            { Name = b.BucketName
                              CreatedOn = b.CreationDate })
            }

        let getBucketObjects (bucketName: string) (client: AmazonS3Client) =
            async {
                let! r =
                    client.ListObjectsAsync(bucketName)
                    |> Async.AwaitTask

                return
                    r.S3Objects
                    |> List.ofSeq
                    |> List.map
                        (fun o ->
                            { Key = o.Key
                              Size = o.Size
                              Owner = o.Owner.Id
                              LastModified = o.LastModified
                              ETag = o.ETag })
            }

        let downloadObject (bucketName: string) (client: AmazonS3Client) (key: string) filePath append =
            async {
                let request = GetObjectRequest()

                request.BucketName <- bucketName
                request.Key <- key

                let! data = client.GetObjectAsync(request) |> Async.AwaitTask

                data.WriteResponseStreamToFileAsync(filePath, append, CancellationToken.None)
                |> Async.AwaitTask
                |> ignore
            }

        let writeObjectToStream (bucketName: string) (client: AmazonS3Client) (key: string) (stream: Stream) =
            async {
                let request = GetObjectRequest()

                request.BucketName <- bucketName
                request.Key <- key

                // TODO add error handling!
                let! data = client.GetObjectAsync(request) |> Async.AwaitTask

                data.ResponseStream.CopyToAsync stream |> Async.AwaitTask |> ignore
            }
     
        let getObjectStream (bucketName: string) (client: AmazonS3Client) (key: string) =
            async {
                let request = GetObjectRequest()

                request.BucketName <- bucketName
                request.Key <- key

                // TODO add error handling!
                let! data = client.GetObjectAsync(request) |> Async.AwaitTask

                return data.ResponseStream
            }
            
        let uploadFile (bucketName: string) (client: AmazonS3Client) (key: string) (filePath: string) =
            async {
                let request = PutObjectRequest()

                request.Key <- key
                request.BucketName <- bucketName
                request.FilePath <- filePath
                
                let! r = client.PutObjectAsync(request) |> Async.AwaitTask

                printfn $"{r.HttpStatusCode}"
            }

        let saveStream (bucketName: string) (client: AmazonS3Client) (key: string) (stream: Stream) =
            async {
                let request = PutObjectRequest()

                request.Key <- key
                request.BucketName <- bucketName
                request.InputStream <- stream
                
                let! r = client.PutObjectAsync(request) |> Async.AwaitTask

                printfn $"{r.HttpStatusCode}"
            }
            
        let createConfig (config: S3Config) =
            let c = AmazonS3Config()
            
            c.RegionEndpoint <- RegionEndpoint.GetBySystemName(config.RegionalEndpoint)
            c.ServiceURL <- config.ServiceUrl
            c
    
    type S3Context(config: S3Config) =
           
        let client = new AmazonS3Client(config.AccessKey, config.SecretKey, Internal.createConfig config)
        //let s3
             
        interface IDisposable with
          member x.Dispose() =
              client.Dispose()
        
        static member Create(path: string) =
            match S3Config.Load path with
            | Ok config -> Ok (new S3Context(config))
            | Error e -> Error e
         
        member _.GetBuckets() =
            async {
                let! overviews = Internal.getBucketOverviews client

                return!
                    overviews
                    |> List.map
                        (fun b ->
                            async {
                                let! objs = Internal.getBucketObjects b.Name client
                                return
                                    b.Name,
                                    { Name = b.Name
                                      CreatedOn = b.CreatedOn
                                      Objects = objs |> List.map (fun o -> o.Key, o) |> Map.ofList }
                            }) 
                    |> Async.Sequential
                    
            }

        member _.GetBucketOverviews() = Internal.getBucketOverviews client
        
        member _.GetBucketObjects(bucketName) = Internal.getBucketObjects bucketName client
        
        member _.DownloadObject(bucketName, key, filePath, append) =
            Internal.downloadObject bucketName client key filePath append
                    
        member _.UploadObject(bucketName, key, filePath) =
            Internal.uploadFile bucketName client key filePath
             
        member _.ObjectToStream(bucketName, key) =
            Internal.getObjectStream bucketName client key
            
        member _.SaveStream(bucketName, key, stream) =
            Internal.saveStream bucketName client key stream
            
        member _.Configuration = config