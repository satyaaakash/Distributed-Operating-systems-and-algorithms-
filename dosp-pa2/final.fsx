//Satya Aakash Obellaneni
//Guna Teja Athota
//Kondreddy Rohith Sai Reddy (65682267)
//Sushmanth Karnam







#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"



// Import required namespaces and libraries
open System
open Akka.Actor
open Akka.FSharp
open System.Security.Cryptography

// Define a record type to store information about the finger table
type FingerTableInfo =
    { mutable Start: int
      mutable NodeID: int
      mutable NodeRef: IActorRef }

// Define an enumeration for different types of actions in the Chord network
type ActionType =
    | JoinChord of IActorRef
    | UpdateChordRing of IActorRef
    | Stabilize
    | FindSuccessor of int * string * IActorRef
    | UpdateSuccessor of IActorRef * string * bool
    | FindPredecessor
    | GetPredecessor of IActorRef * bool
    | UpdateFingerTable
    | ShowFingerTable
    | SendMessage
    | MessageHop of int * int * IActorRef

// Define an enumeration for different stages in finding a node in the Chord network
type FindNode =
    | Initialize
    | NodeFound
    | MessageDelivered of int
    | PrintAndStart

// Create an ActorSystem
let system = ActorSystem.Create("ChordSimulation")

// Function to generate a random string of a given length
let generateRandomString n =
    let random = Random()
    let chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
    new string (Array.init n (fun _ -> chars.[random.Next(chars.Length)]))

// Function to generate a random node ID based on a random string
let generateRandomNodeID m =
    let randStr = generateRandomString 32
    // Hash the random string and compute the absolute value
    let nodeID = Math.Abs(BitConverter.ToInt32((new SHA256Managed()).ComputeHash(System.Text.Encoding.ASCII.GetBytes("rveerannagowda;" + randStr)), 0))
    nodeID

// Define a ChordCounter actor responsible for managing Chord nodes and counting hops
type ChordCounter(nodes: IActorRef[], numNodes: int, numRequests: int) =
    inherit Actor()
    let mutable joinCounter = 0
    let mutable peerCounter = 0
    let mutable totalHops = 0
    let mutable totalMessagesDelivered = 0

    override x.OnReceive message =
        match message  :?> FindNode with
        | Initialize ->
            // Initiate the Chord network by having the first node join itself
            nodes.[0] <! JoinChord(nodes.[0])
        | MessageDelivered hops ->
            // Count the hops and check if all messages have been delivered
            totalMessagesDelivered <- totalMessagesDelivered + 1
            totalHops <- hops + totalHops
            if totalMessagesDelivered = (numNodes * numRequests) then
                printfn "Chords have total hops in : %d" totalHops
                let averageHops = float totalHops / float (numNodes * numRequests)
                printfn "Average hops are : %A" averageHops
                Environment.Exit(0)
        | PrintAndStart ->
            // Trigger displaying finger tables and sending messages
            peerCounter <- peerCounter + 1
            if peerCounter < nodes.Length then
                nodes.[peerCounter] <! ShowFingerTable
            if peerCounter = nodes.Length then
                for i in 1 .. nodes.Length do
                    nodes.[i - 1] <! SendMessage
        | NodeFound ->
            // Handle node found in the Chord network
            joinCounter <- joinCounter + 1
            if joinCounter < nodes.Length then
                nodes.[joinCounter] <! JoinChord(nodes.[joinCounter - 1])
            if joinCounter = nodes.Length then
                // Schedule showing finger tables after a delay
                system.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(100.0), nodes.[0], ShowFingerTable, x.Self)

// Define a ChordNode actor representing a node in the Chord network
type ChordNode(nodeID: int, m: int) =
    inherit Actor()
    let mutable nodeRef: IActorRef = null
    let mutable pred: IActorRef = null
    let mutable succ: IActorRef = null

    // Create the initial finger table
    let createFingerTable =
        Array.init m (fun i -> { Start = (nodeID + pown 2 i - 1) % (pown 2 m); NodeID = Int32.MaxValue; NodeRef = null })
    
    let mutable fingerTable = createFingerTable
    
    // Function to check if an ID falls within a specified range
    let rec checkRange id start endId =
        id > start && id < endId || (start > endId && not (checkRange id endId start) && (id <> start) && (id <> endId))

    // Function to check if an ID belongs to a specified range
    let rec belongs id start endId =
        id > start && id <= endId || (start > endId && not (belongs id endId start)) || (start = endId)

    // Function to find the immediate preceding node
    let immediatePrecedingNode id =
        let mutable result = null
        let mutable flag = false
        let mutable count = m
        while not flag && count >= 1 do
            let isTrue = checkRange fingerTable.[count - 1].NodeID id nodeID
            if isTrue then
                result <- fingerTable.[count - 1].NodeRef
                flag <- true
            count <- count - 1
        result
    
    override x.OnReceive message =
        match message  :?> ActionType with
        | SendMessage ->
            // Send random messages with a specified number of hops
            let msgID = generateRandomNodeID m
            let initialHop = 0
            for _ in 1 .. 10 do
                system.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(100.0), x.Self, MessageHop(msgID, initialHop, x.Self), x.Self)
        | JoinChord ref ->
            // Handle a node joining the Chord network
            nodeRef <- x.Sender
            if ref.Path.Name.Equals(x.Self.Path.Name) then
                // Initialize a new Chord network with this node as the only member
                pred <- null
                succ <- x.Self
                for i in 1 .. m do
                    fingerTable.[i - 1].NodeID <- nodeID
                    fingerTable.[i - 1].NodeRef <- x.Self
                nodeRef <! NodeFound
                // Schedule stabilization and finger table updates
                system.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(50.0), TimeSpan.FromMilliseconds(80.0), x.Self, Stabilize, x.Self)
                system.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(60.0), TimeSpan.FromMilliseconds(90.0), x.Self, UpdateFingerTable, x.Self)
            else
                pred <- null
                ref <! FindSuccessor(nodeID, "join", x.Self)
        | Stabilize ->
            // Perform stabilization of the Chord network
            succ <! FindPredecessor
        | FindPredecessor ->
            // Find the predecessor node
            let (pre, valid) =
                if isNull pred then (x.Self, false) else (pred, true)
            pre <! GetPredecessor(pre, valid)
        | GetPredecessor(p, valid) ->
            if valid then
                let preID = int p.Path.Name
                let sucID = int succ.Path.Name
                if checkRange preID sucID nodeID then
                    // Update the successor
                    succ <- p
                succ <! UpdateChordRing(x.Self)
        | UpdateSuccessor(s, reason, valid) ->
            if (reason = "join") then
                if valid then succ <- s
                nodeRef <! NodeFound
                // Schedule repeated stabilizations
                system.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(50.0), TimeSpan.FromMilliseconds(80.0), x.Self, Stabilize, x.Self)
                system.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(60.0), TimeSpan.FromMilliseconds(90.0), x.Self, Stabilize, x.Self)
            else
                let index = int (reason.Split(".")).[2]
                if valid then
                    // Update the finger table
                    fingerTable.[index].NodeRef <- s
                    fingerTable.[index].NodeID <- int s.Path.Name
        | FindSuccessor(id, taskType, ref) ->
            // Find the successor node for a given ID
            let succID = int succ.Path.Name
            if belongs id nodeID succID then
                let (successorType, valid) =
                    if isNull succ then (x.Self, false) else (succ, true)
                ref <! UpdateSuccessor(successorType, taskType, valid)
            else
                let nextHop = immediatePrecedingNode id
                if not (isNull nextHop) then
                    nextHop <! FindSuccessor(id, taskType, ref)
                else
                    succ <! FindSuccessor(id, taskType, ref)
        | ShowFingerTable ->
            // Display the finger table and start sending messages
            let mutable predName = null
            
            if not (isNull pred) then
                predName <- pred.Path.Name
            x.Sender <! PrintAndStart
        | UpdateFingerTable ->
            // Periodically update a random entry in the finger table
            let i = Random().Next(0, m)
            x.Self <! FindSuccessor(fingerTable.[i].Start, "finger.update." + string i, x.Self)
            
        | UpdateChordRing(ref) ->
            if isNull pred then
                pred <- ref
            else
                let preID = int pred.Path.Name
                let refID = int ref.Path.Name
                if checkRange refID preID nodeID then
                    pred <- ref

        | MessageHop(msgID, hopCount, ref) ->
            // Handle message hops within the Chord network
            let visitCount = hopCount + 1
            let maximum = int (pown 2 m)
            if visitCount = maximum then
                // Notify the sender when the message has reached its destination
                nodeRef <! MessageDelivered(visitCount)
            else
                let sucID = int succ.Path.Name
                if belongs msgID nodeID sucID then
                    // Notify the sender when the message has reached its destination
                    nodeRef <! MessageDelivered(visitCount)
                else
                    let closestHop = immediatePrecedingNode msgID
                    if not (isNull closestHop) then
                        closestHop <! MessageHop(msgID, visitCount, ref)
                    else
                        succ <! MessageHop(msgID, visitCount, ref)

// Main function to initialize and run the Chord simulation
let main() =
    // Parse command-line arguments to determine the number of nodes and requests
    let arguments : string array = fsi.CommandLineArgs |> Array.tail
    let numNodes = int arguments.[0]
    let numRequests = int arguments.[1]
    // Calculate the value of 'm' based on the number of nodes
    let m = int(ceil (Math.Log(float numNodes) / Math.Log(2.0)))
    
    // Generate random node IDs
    let nodeIDs = [for _ in 1 .. numNodes -> generateRandomNodeID m]
    
    // Create Chord node actors and a reference node
    let nodeActors = [|for id in nodeIDs -> system.ActorOf(Props.Create(typeof<ChordNode>, id, m), string id)|]
    let refNode = system.ActorOf(Props.Create(typeof<ChordCounter>, nodeActors, numNodes, numRequests), "refNode")
    
    // Initialize the Chord simulation
    refNode <! Initialize
    // Wait for user input (for demonstration purposes)
    Console.ReadLine() |> ignore

main()