open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO

//initializing the variables to use it later in the function

let port = 8080 // Set your desired port number
let mutable clientCounter = 0
let mutable result = 0
let mutable isTerminated = false

let handleClient (clientSocket: TcpClient) =
    async {


        clientCounter <- clientCounter + 1  //keeping the count of number of clients that is being connected to keep track of which client to be communicating with
        let clientId = clientCounter
        //Getting the input from the console input
        let clientStream = clientSocket.GetStream()
        let reader = new StreamReader(clientStream, Encoding.ASCII)
        let writer = new StreamWriter(clientStream, Encoding.ASCII)
        printfn "The client %d is connected..." clientId
        writer.AutoFlush <- true //Flushing out the console output after each input to clear any edge cases or unwanted inputs
        try
            // Greeting a welcome message to the client
            writer.WriteLine("Greeting the first Hello!")

            while true do
                // Read client input
                let input = reader.ReadLine()

                // Handle client commands
                match input with
                | null -> // Client disconnected
                    // printfn "The client %d is connected..." clientId
                    printfn "Client %d disconnected" clientId
                    return ()
                | "bye" -> // Closes the socket for that perticular client
                    writer.WriteLine("-5")
                    return ()
                | "terminate" -> // Close all the live clients and to close the server
                    writer.WriteLine("-5")
                    isTerminated<-true
                    // Closes the perticular envisonment that is running all the clients in the consol
                    Environment.Exit(0)
                    // Exiting the loop to break the while loop
                    return ()
                | _ -> // Taking all input and storing into a array to perform calculations later
                    let parts = input.Split(' ')
                    let op = parts.[0]
                    let nums =  
                        Array.sub parts 1 (parts.Length - 1)
                        |> Array.map (fun num ->
                            match Int32.TryParse(num) with
                            | (true, n) -> n
                            | _ -> -1 // Invalid number format
                        )
                    op, nums
                    
                    try
                        result <-

                        //performing the calculations
                            match op with
                            | "add" -> Array.sum nums
                            | "subtract" -> Array.reduce (-) nums
                            | "multiply" -> Array.fold (*) 1 nums
                            | _ -> -1 // Incorrect operation command
                        if parts.Length < 3 then
                            result <- -2 // Number of inputs is less than two
                        elif parts.Length > 5 then
                            result <- -3 // Number of inputs is more than four
                        elif Array.exists (fun n -> n = -1) nums then
                            result <- -4 // One or more inputs contain non-numbers
                        elif result < 0 then
                            result <- -1 // Incorrect operation command
                        

                        // Print the response with client ID
                        writer.WriteLine(sprintf "%d" result)
                        printfn "Received the command: %s" input
                        printfn "Responding to client %d with result: %d" clientId result
                    with
                    | :? System.FormatException ->
                        writer.WriteLine("-4") // One or more inputs contain non-numbers
        with
        | :? System.IO.IOException ->
            // Handle any IO errors (e.g., client disconnect)
            printfn "Client %d disconnected"clientId
        | ex ->
            printfn "Error: %s" ex.Message
    }

[<EntryPoint>]
let main argv =
    let listener = new TcpListener(IPAddress.Any, port)
    listener.Start()
    //initializing the listener to create a new server to service the clients
    let localEndpoint = listener.LocalEndpoint :?> IPEndPoint
    let ipAddress = localEndpoint.Address.ToString()
    printfn "server is hosted on %s" ipAddress
    printfn "Server is listening on port %d" localEndpoint.Port

    //connecting the clients to the server which is listening
    let rec acceptClients () =
       async {
        let clientSocket = listener.AcceptTcpClientAsync().Result
        Async.Start (handleClient clientSocket)
        if not isTerminated then
                return! acceptClients ()
    }

    // Start accepting clients
    Async.RunSynchronously (acceptClients ()) 
    0
