namespace FsToolbox.Core

[<RequireQualifiedAccess>]
module Http =

    open System.IO
    open System.Net.Http.Headers
    open System.Net.Http.Json
    open System.Text.Json
    open FsToolbox.Core
    open System.Net.Http

    type HttpFailure = { StatusCode: string; Message: string }

    type RequestResult<'T> =
        | Success of 'T
        | Failure of HttpFailure
        | DeserializationError of Failure
        | Exception of Failure

    let setBearerToken (token: string) (client: HttpClient) =
        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)
        client

    let deserializeJsonAsync<'T> (body: Stream) =
        task {
            try
                let! result = JsonSerializer.DeserializeAsync<'T> body
                return (RequestResult.Success result)
            with exn ->
                return
                    { Message = exn.Message
                      Exception = Some exn }
                    |> RequestResult.DeserializationError
        }

    let getAsync<'T> (url: string) (client: HttpClient) =
        task {
            try
                let! response = client.GetAsync(url)

                return!
                    task {
                        match response.IsSuccessStatusCode with
                        | true ->
                            let! body = response.Content.ReadAsStreamAsync()
                            let! result = deserializeJsonAsync<'T> body
                            return result
                        | false ->
                            let! body = response.Content.ReadAsStringAsync()

                            return
                                { StatusCode = response.StatusCode.ToString()
                                  Message = body }
                                |> RequestResult.Failure
                    }
            with exn ->
                return
                    { Message = exn.Message
                      Exception = Some exn }
                    |> RequestResult.Exception
        }

    let postJsonAsync<'T> (url: string) (client: HttpClient) (data: 'T) =
        task {
            try
                let! response = client.PostAsJsonAsync(url, data)

                let! body = response.Content.ReadAsStringAsync()

                return
                    match response.IsSuccessStatusCode with
                    | true ->

                        Success body
                    | false ->
                        { StatusCode = response.StatusCode.ToString()
                          Message = body }
                        |> RequestResult.Failure

            with exn ->
                return
                    ({ Message = exn.Message
                       Exception = Some exn }
                     |> RequestResult.Exception)

        }

    let postAndReplyJsonAsync<'T, 'R> (url: string) (client: HttpClient) (data: 'T) =
        task {
            let! r = postJsonAsync url client data

            match r with
            | RequestResult.Success json ->
                try
                    return JsonSerializer.Deserialize<'R> json |> RequestResult.Success
                with exn ->
                    return
                        { Message = exn.Message
                          Exception = Some exn }
                        |> RequestResult.DeserializationError
            | RequestResult.Failure f -> return RequestResult.Failure f
            | RequestResult.DeserializationError f -> return RequestResult.DeserializationError f
            | RequestResult.Exception f -> return RequestResult.Exception f
        }
