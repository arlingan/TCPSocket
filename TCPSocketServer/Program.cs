using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousSocketListener
{
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com". 
        var hostName = Dns.GetHostName(); //"crossfireplus.jci-io.com";
        Console.WriteLine($"HostName: {hostName}");
        IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName); //Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2323);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            Console.WriteLine($"IPEndPoint IPAddress is '{localEndPoint.Address}' and Port is '{localEndPoint.Port}'");
            listener.Bind(localEndPoint);
            listener.Listen(100);
            Console.WriteLine("Server is running. Listening for connections ...");
            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);
        Console.WriteLine("Connection established...");

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the
                // client. Display it on the console.  
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                // Echo the data back to the client.  
                Send(handler, content);

                // Reregister for reading data...  
                StateObject sObject = new StateObject();
                sObject.workSocket = handler;
                handler.BeginReceive(sObject.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), sObject);
            }
            else
            {
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}




//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;

//namespace TCPSocketServer
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            IPHostEntry ipHost = Dns.GetHostEntry("10.211.55.3"); //Dns.GetHostEntry(Dns.GetHostName());
//            IPAddress ipAddr = ipHost.AddressList[0];
//            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 2323);

//            // Create TCP/IP Socket
//            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//            try
//            {

//                listener.Bind(localEndPoint);
//                listener.Listen(10);

//                // Data buffer 
//                byte[] bytes = new Byte[1024];


//                while (true)
//                {
//                    Console.WriteLine("Server listening for connections....");
//                    Socket clientSocket = listener.Accept();
//                    string data = null;

//                    //while (true)
//                    //{
//                        int bytesRec = clientSocket.Receive(bytes);
//                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
//                        if (data.IndexOf("<EOF>") > -1)
//                        {
//                            // Show the data on the console.  
//                            Console.WriteLine("Text received : {0}", data);

//                            // Echo the data back to the client.  
//                            byte[] msg = Encoding.ASCII.GetBytes(data);
//                            clientSocket.Send(msg);
//                        }
//                    //}

//                    //while (true)
//                    //{

//                    //    Console.WriteLine("Waiting connection ... ");

//                    //    // Suspend while waiting for 
//                    //    // incoming connection Using  
//                    //    // Accept() method the server  
//                    //    // will accept connection of client 
//                    //    Socket clientSocket = listener.Accept();

//                    //    // Data buffer 
//                    //    byte[] bytes = new Byte[1024];
//                    //    string data = null;

//                    //    while (true)
//                    //    {

//                    //        int numByte = clientSocket.Receive(bytes);

//                    //        data += Encoding.ASCII.GetString(bytes,
//                    //                                   0, numByte);

//                    //        if (data.IndexOf("<EOF>") > -1)
//                    //            break;
//                    //    }

//                    //    Console.WriteLine("Message received -> {0} ", data);
//                    //    byte[] message = Encoding.ASCII.GetBytes("Test Server");

//                    //    // Send a message to Client  
//                    //    // using Send() method 
//                    //    clientSocket.Send(message);

//                    //    // Close client Socket using the 
//                    //    // Close() method. After closing, 
//                    //    // we can use the closed Socket  
//                    //    // for a new Client Connection 
//                    //    clientSocket.Shutdown(SocketShutdown.Both);
//                    //    clientSocket.Close();
//                    //}
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }
//    }
//}
