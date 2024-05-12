﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ChatServer
{
   
    private readonly List<ClientHandler> clients = new List<ClientHandler>();

    
    private Socket serverSocket;

    public void Start(int port)
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        serverSocket.Listen(10);

        Console.WriteLine("Server started. Listening on port " + port);

        while (true)
        {
            Socket clientSocket = serverSocket.Accept();
            ClientHandler clientHandler = new ClientHandler(clientSocket, this);
            clients.Add(clientHandler);

            
            Thread clientThread = new Thread(clientHandler.HandleClient);
            clientThread.Start();
        }
    }

   
    public void BroadcastMessage(string message, ClientHandler sender)
    {
        foreach (ClientHandler client in clients)
        {
            if (client != sender)
            {
                client.SendMessage(message);
            }
        }
    }

   
    public void RemoveClient(ClientHandler client)
    {
        clients.Remove(client);
    }
}

class ClientHandler
{
    private readonly Socket clientSocket;
    private readonly ChatServer server;
    private readonly byte[] buffer = new byte[1024];

    public ClientHandler(Socket clientSocket, ChatServer server)
    {
        this.clientSocket = clientSocket;
        this.server = server;
    }

   
    public void HandleClient()
    {
        Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint);

        while (true)
        {
            int bytesReceived = clientSocket.Receive(buffer);
            if (bytesReceived == 0)
            {
                Console.WriteLine("Client disconnected: " + clientSocket.RemoteEndPoint);
                clientSocket.Close();
                server.RemoveClient(this); 
                break;
            }

            string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            Console.WriteLine("Message received from " + clientSocket.RemoteEndPoint + ": " + message);

           
            server.BroadcastMessage(message, this);
        }
    }

   
    public void SendMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        clientSocket.Send(data);
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Введите порт для запуска сервера: ");
        int port = int.Parse(Console.ReadLine());

        ChatServer server = new ChatServer();
        server.Start(port);
    }
}
