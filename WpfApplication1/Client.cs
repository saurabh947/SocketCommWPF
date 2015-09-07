/*
 * -----------A THESAURUS CLIENT & SERVER-----------
 * Submitted as a part of CSE 5306: Lab 1 project
 * The program is coded by:
 * Name: Agrawal, Saurabh V
 * UTA ID: 1000954351
 * Login ID: sva4351
 */
/*
 * Some parts of this program are referred and/or sourced from the internet.
 * The links are mentioned here.
 * 1. MSDN .NET Development: http://msdn.microsoft.com/en-us/library/w0x726c2.aspx
 * 2. MSDN Codes, Communication through sockets: http://code.msdn.microsoft.com/Communication-through-91a2582b
 * 3. Several QA threads for common coding problems on: http://stackoverflow.com
 */
/*﻿﻿﻿
 ************* THIS IS THE CLIENT PROGRAM ***************
 */
using System;                           // Used for running C# program
using System.Text;                      // Used for ASCII and unicode character encodings
using System.Windows;                   // Used for creating and managing the lifetime of dialog boxes
using System.Net;                       // Used for networking
using System.Net.Sockets;               // Used for implementing sockets
using System.Windows.Input;             // Provides different input type support to WPF

// Namespace declared for organizing classes
namespace WpfApplication1
{
    // For managing lifetime of windows we declare windows class
    public partial class MainWindow : Window
    {

        // Receiving byte array  
        byte[] bytes = new byte[1024]; 
        // Declaring a socket
        Socket senderSock;

        // Initialization of new instance of the Window class
        public MainWindow()
        {
            // For initialization of the user interface components in the window
            InitializeComponent();
            // Disabling the 'Send' button as connection is still not made to server
            Send_Button.IsEnabled = false;
            // 'Disconnect' button enabled at all times
            Disconnect_Button.IsEnabled = true;
        }

        /* This method is invoked when the 'Connect' button is clicked.
         * It connects the client to the server asynchronously.
         */
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            // The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,                          // Connection permission
                    TransportType.Tcp,                              // Defines transport types
                    "",                                             // Gets the IP addresses
                    SocketPermission.AllPorts                       // All ports
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();  
                // Resolves a host name to an IPHostEntry instance            
                IPHostEntry ipHost = Dns.GetHostEntry("");
                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = ipHost.AddressList[0];
                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4510);

                // Create one Socket object to setup Tcp connection 
                senderSock = new Socket(
                    ipAddr.AddressFamily,           // Specifies the addressing scheme
                    SocketType.Stream,              // The type of socket
                    ProtocolType.Tcp                // Specifies the protocols
                    );

                // Establishes a connection to a remote host 
                senderSock.Connect(ipEndPoint);

                // Disabling the 'Connect' button
                Connect_Button.IsEnabled = false;
                // Enabling the 'Send' button
                Send_Button.IsEnabled = true;
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }

        }

        /* This method is invoked once the 'Send' button is clicked in the 
         * client window. It asynchronously sends the word to be looked up in thesaurus.
         */
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            // The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Preparing the word for sending
                // <Client Quit> is the sign for end of data
                string theMessageToSend = tbMsg.Text;
                byte[] msg = Encoding.Unicode.GetBytes(theMessageToSend + "<Client Quit>");

                // Sends data to a connected Socket. 
                int bytesSend = senderSock.Send(msg);

                // Method call
                ReceiveDataFromServer();

                // Keeping the 'Send' button enabled if client wishes to send another word
                Send_Button.IsEnabled = true;
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }


        /* This method is called in the 'Send_Click' method, and asynchronously receives the 
         * string of alternate words from the server
         */
        private void ReceiveDataFromServer()
        {
            // The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Receives data from a bound Socket. 
                int bytesRec = senderSock.Receive(bytes);

                // Converts byte array to string 
                String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);

                // Continues to read the data till data isn't available alongwith encodes it
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(bytes);
                    theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
                }

                // Shows the received string in the 'tbReceivedMsg' textbox
                tbReceivedMsg.Text = "" + theMessageToReceive;
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }


        /* Whenever the user presses the 'Disconnect' button, this method is invoked, and 
         * it closes the socket and the window.
         */
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            // The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Closes the Socket connection and releases all resources 
                senderSock.Close();
                // Closes the window
                this.Close();
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        } 
    }
}
