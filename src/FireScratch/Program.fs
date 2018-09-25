// Learn more about F# at http://fsharp.org

open System
open Firebase.Auth
open Firebase.Database.Query
open Firebase.Database
open Firebase.Database.Streaming
open System.Threading.Tasks
open System
open System.Linq
open System.Threading.Tasks

let apikey = "<firebase api key>"
let databaseURL = "<firebase databaseURL>"
let DatabasePath = "scratch/firecat"
let email = "<e-mail>"
let passwd = "<password>"

let mutable authLink : FirebaseAuthLink = null
let mutable realtimeDatabaseWatcher : IDisposable = null

type Data() =
    let mutable _from : string = ""
    let mutable _to : string = ""
    let mutable _x : int = 0
    let mutable _y : int = 0
    let mutable _text = ""

    member x.From with get() = _from and set(v) = _from <- v
    member x.To with get() = _to and set(v) = _to <- v
    member x.X with get() = _x and set(v) = _x <- v
    member x.Y with get() = _y and set(v) = _y <- v
    member x.Text with get() = _text and set(v) = _text <- v

let mutable fireData : Data = new Data ()


// ログイン
let singIn() =
    let auth = new FirebaseAuthProvider( new FirebaseConfig( apikey ))
    authLink <- auth.SignInWithEmailAndPasswordAsync( email, passwd ).Result
    printfn "サインインに成功しました"

// クエリを取得
let GetDatabaseQuery( path ) =
    let opt = new FirebaseOptions()
    opt.AuthTokenAsyncFactory <- fun () -> Task.FromResult( authLink.FirebaseToken )
    let client = new FirebaseClient( databaseURL, opt )
    client.Child( path )

// データをアップロード
let upload( data : Data ) =
    let query = GetDatabaseQuery( DatabasePath )
    query.PostAsync( data ) |> ignore
    ()
// テキストを保存
let uploadText( text: string ) = upload( new Data( Text = text ))


// テキストを取得
let downloadText() =
    let query = GetDatabaseQuery( DatabasePath )
    let results = query.OnceAsync<Data>().Result
    let items = results.Select( fun o -> o.Object )
    items.First().Text

// リアルタイムデータの監視
let startWatchingRealtime() =
    realtimeDatabaseWatcher <- 
        GetDatabaseQuery(DatabasePath)
            .AsObservable<Data>()
            .Subscribe( fun ev -> 
                if ev <> null then
                    let text = ev.Object.Text
                    match ev.EventType with
                        | FirebaseEventType.InsertOrUpdate -> fireData <- ev.Object
                        | FirebaseEventType.Delete -> ()
                        | _ -> ()
            )
    ()

// Scratchから受信するためのHTTPサーバー
let Server( port ) =

    // firebase にログイン
    singIn()
    startWatchingRealtime()

    let listener = new System.Net.HttpListener()
    listener.Prefixes.Add("http://127.0.0.1:"+(port |> string)+"/" )
    listener.Start()
    while true do
        let context = listener.GetContext()
        let res = context.Response
        let mutable data = ""
        let path = context.Request.Url.PathAndQuery
        match path with 
            | "/poll" -> 
                data <- data + String.Format("text {0}\n", fireData.Text )
                data <- data + String.Format("from {0}\n", fireData.From )
                data <- data + String.Format("to {0}\n", fireData.To )
                data <- data + String.Format("x {0}\n", fireData.X )
                data <- data + String.Format("y {0}\n", fireData.Y )
                // printfn "%s" path
                // printfn "%s" debug
            | "/reset_all" ->
                printfn "/reset_all"
                data <- "ok"
            | _ ->
                let pa = path.Split([|'/'|])
                match pa.[1] with   
                | "say" ->
                    let me = pa.[2]
                    let you = pa.[3]
                    let text = pa.[4]
                    printfn "say %s %s %s" me you text
                    upload( Data( From = me, To = you, Text = text ))
                | "sayall" ->
                    let text = pa.[2]
                    printfn "sayall %s" text
                    uploadText( text )
                | "movex" ->
                    let me = pa.[2]
                    let x = pa.[3] |> int
                    printfn "movex %s %d" me x
                    upload( Data( From = me, X = x ))
                | "movey" ->
                    let me = pa.[2]
                    let y = pa.[3] |> int
                    printfn "movey %s %d" me y
                    upload( Data( From = me, Y = y ))
                | "movexy" ->
                    let me = pa.[2]
                    let x = pa.[3] |> int
                    let y = pa.[4] |> int
                    printfn "movexy %s %d %d" me x y 
                    upload( Data( From = me, X = x, Y = y ))
                | _ ->
                    data <- ""
                printfn "%s" path
        res.StatusCode <- 200
        let sw = new System.IO.StreamWriter( res.OutputStream )
        sw.Write( data )
        sw.Close()
    ()

[<EntryPoint>]
let main argv =
    printfn "Start Firebase Scratch Server"
    Server(5411)
    Console.ReadKey() |> ignore   
    0 
