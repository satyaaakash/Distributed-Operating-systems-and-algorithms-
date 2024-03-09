
# Distributed Systems and Algorithms Projects

This repository contains assignments and projects for the COP5615 course, focusing on advanced concepts in distributed systems and algorithms.

## Project 1: Concurrent Client/Server Socket Communication in F#

### Overview

Project 1 demonstrates the implementation and testing of a concurrent client/server socket communication system using F#. It includes the creation of a server application capable of handling multiple client connections simultaneously and a client application that communicates with the server over a TCP/IP protocol suite.

### Key Features

- **Concurrent Communication**: Supports simultaneous communication between multiple clients and the server.
- **Efficient Socket Handling**: Implements efficient socket management for reliable and scalable communication.
- **Exception Handling**: Incorporates robust exception handling mechanisms for reliable operation.

### Usage

To execute the client/server communication system, follow the setup instructions provided in the project directory. Use the provided commands to compile and run the applications.
### Requirements

- **Server-Side Script**: Capable of handling several client connections simultaneously, improving system performance through concurrent processing.
- **Client-Side Script**: Connects to the server, sends commands, and displays responses asynchronously.
- **Exception Handling**: Robust exception management to ensure reliable system operation.

### Compilation and Execution Instructions

1. Create a project directory.
2. Initialize server and client applications with `dotnet new console --language F# -o server` and `dotnet new console --language F# -o client`, respectively.
3. Rename `Program.fs` to `client.fs` and `server.fs` in their respective directories.
4. Compile the projects using `dotnet build` within each directory.
5. Start the server and client(s) using `dotnet run`.

### Code Structure

- **Server and Client Scripts**: Manage connections, handle client requests, and asynchronous task handling for each client.
- **Exception Handling**: Includes production and processing of error codes for reliable operation.

### Results and Execution

- Demonstrates the server's ability to handle multiple clients, process commands, and manage exceptions.
- Detailed execution results, including test cases and screenshots, are provided.

## Project 2: Chord Protocol Implementation and Network Simulation

### Overview

Project 2 focuses on simulating a Chord peer-to-peer (P2P) network within the Akka.NET framework using F#. The simulation includes implementing a Chord network, node joining, stabilization, efficient routing, and calculating the average hops for message delivery.

### Key Features

- **Chord Ring Formation**: Arranges all nodes into a ring structure, updating whenever nodes join or leave the network.
- **Finger Table Management**: Implements finger tables for each node to optimize routing and lookup times.
- **Node Joining and Stabilization**: Supports node joining and implements stabilization to maintain ring integrity.
- **Efficient Routing**: Utilizes finger tables for efficient routing, significantly reducing the average hops required for message delivery.

### Running the Simulation

Refer to the project directory for instructions on compiling and running the Chord network simulation. Execute the provided commands to initiate the simulation with the desired parameters.

To run the Chord network simulation, execute the following command:

```
dotnet fsi final.fsx <no_of_Nodes> <no_of_Requests>
```

## Project 3: Distributed File System Implementation

### Overview

Project 3 involves designing and implementing a distributed file system (DFS) using F# and Akka.NET. The DFS aims to provide a fault-tolerant, scalable, and efficient file storage and retrieval system across a network of distributed nodes.

### Key Features

- **Fault Tolerance**: Implements fault tolerance mechanisms to ensure system resilience and reliability.
- **Scalability**: Designed to scale seamlessly with the addition of new nodes to the network.
- **File Replication**: Utilizes file replication strategies to enhance data availability and durability.
- **Consistency**: Maintains data consistency across distributed nodes through synchronization protocols.
- **File Operations**: Supports basic file operations such as upload, download, delete, and listing files.

### Setup and Usage

Follow the setup instructions provided in the project directory to compile and run the distributed file system. Use the provided commands to interact with the system and perform file operations.
### Setup Instructions
```
1. Clone the repository to your local machine.
2. Ensure you have the latest version of the .NET SDK and Akka.NET installed.
3. Navigate to the project directory.
4. Compile the project using the `dotnet build` command.
5. Run the DFS using the `dotnet run` command.
```
### Usage

Once the DFS is running, clients can interact with the system using the provided APIs or command-line interface. Sample commands include:
```
- `upload <file>`: Uploads a file to the DFS.
- `download <file>`: Downloads a file from the DFS.
- `delete <file>`: Deletes a file from the DFS.
- `list`: Lists all files stored in the DFS.

```
## License

This project is licensed under the MIT License - see the License file for details.

## Acknowledgments

- Dr. Jonathan for providing guidance and support throughout the course.
- The team members for their contributions to each project.
