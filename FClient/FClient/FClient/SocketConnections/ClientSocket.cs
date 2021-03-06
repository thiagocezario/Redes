﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Gerenciador.SocketsConnections;
using System.IO;

public class AsynchronousClient
{
    private const int portaDoGerenciador = 23;
    private const string ipAddress = "127.0.0.1";

    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    private static String response = String.Empty;

    public static void StartClient()
    {
        try
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress ipDoGerenciador))
                throw new FormatException(string.Format("{0} is not a valid IP address", ipAddress));
            IPEndPoint remoteEP = new IPEndPoint(ipDoGerenciador, portaDoGerenciador);

            // Create a TCP/IP socket.  
            Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
            string[] myFiles = Array.ConvertAll(new DirectoryInfo("c:\\temp").GetFileSystemInfos(), s => s.Name);
            foreach (var file in myFiles)
            {
                Console.WriteLine(file);
            }
            Receive(client);
            //receiveDone.WaitOne();
            Send(client, remoteEP.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void Stop(Socket client)
    {
        client.Close();
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;

            client.EndConnect(ar);

            connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject();
            state.workSocket = client;

            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Send(Socket client, String data)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;

            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}