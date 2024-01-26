open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO

let listenIPAddress = IPAddress.Parse("127.0.0.1")
let listenPort = 12345

let calculate (command:string) (inputs:string array) =
  
    match command with
    | "add" ->
        Array.map int inputs
        |> Array.sum
        |> string
    | "subtract" ->
        Array.map int inputs
        |> Array.reduce (-)
        |> string
    | "multiply" ->
        Array.map int inputs
        |> Array.reduce (*)
        |> string
  

let startServer () =
    try
        let listener = new TcpListener(listenIPAddress, listenPort)
        listener.Start()
        printfn "Server is listening on %s:%d" (listenIPAddress.ToString()) listenPort


        
        while true do
            use client = listener.AcceptTcpClient()
            printfn "Accepted connection from %s" (client.Client.RemoteEndPoint.ToString())
            
            let stream = client.GetStream()
            let reader = new StreamReader(stream)
            let writer = new StreamWriter(stream)
            writer.AutoFlush <- true
            
            let request = reader.ReadLine()
            printfn "Received request: %s" request
            
            let requestParts = request.Split(' ')
            if requestParts.Length >= 2 then
                let command = requestParts.[0]
                let inputs = requestParts.[1..]
                
                let result = calculate command inputs
                writer.WriteLine(result)
                
                printfn "Sent response: %s" result
            
            client.Close()
    with
    | :? Exception as ex ->
        printfn "An error occurred: %s" ex.Message

startServer()     