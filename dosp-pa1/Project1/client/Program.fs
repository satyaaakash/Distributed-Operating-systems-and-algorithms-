// Importing the libraries
open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO

module TcpClientUtils =
    let private printServerMessage message = printfn "Server: %s" message
    let private readCommandFromUser prompt =
        printf "%s" prompt
        Console.ReadLine()

    let communicateWithServer serverIP port =
        async {
            try
                let port =8080
                let tcpClient = new TcpClient()
                tcpClient.Connect("192.168.0.223", port)
                //Getting the input from the console input
                use netStream = tcpClient.GetStream()
                use streamReader = new StreamReader(netStream, Encoding.ASCII)
                use streamWriter = new StreamWriter(netStream, Encoding.ASCII)
                streamWriter.AutoFlush <- true

                // Greeting the server with Hello at first
                let initialMessage = streamReader.ReadLine()
                printServerMessage initialMessage

                let rec loop () =
                    async {
                        
                        //taking the statement to be computed from the user

                        let userCommand = readCommandFromUser "Calculations will be performed like (e.g., add 1 2 or substract 3 1 or multiply 6 3):"
                        streamWriter.WriteLine(userCommand)
                        let serverReply = streamReader.ReadLine()
                        match serverReply with
                        | "-5" ->
                            // terminating when the serverReply is -5
                            printfn "Terminating connection."
                            Environment.Exit(0)
                        | _ ->
                            //Giving out the reply from the server if any
                            printServerMessage serverReply
                            return! loop()
                    }
                return! loop()

            with
            
            //Giving the exception out if any

            | :? SocketException as socketEx ->
                printfn "Encountered socket issue: %s" socketEx.Message
            | :? IOException as ioEx ->
                printfn "IO issue: %s" ioEx.Message
        }

[<EntryPoint>]
let main _ =

    //setting the server to be heard on form the client and fixating on the port number to be similar

    let serverIPAddress = IPAddress.Parse("192.168.0.223")
    let serverPortNum = 8080
    TcpClientUtils.communicateWithServer serverIPAddress serverPortNum |> Async.RunSynchronously
    0