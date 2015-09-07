/*
 * -----------A THESAURUS CLIENT & SERVER-----------
 * Submitted as a part of CSE 5306: Lab 1 project
 * The program is coded by:
 * Name: Agrawal, Saurabh V
 * UTA ID: 1000954351
 * Login ID: sva4351
 */
/*
 * Some parts of this program are referred and/or sourced from internet.
 * The links are mentioned here.
 * 1. MSDN .NET Development: http://msdn.microsoft.com/en-us/library/w0x726c2.aspx
 * 2. MSDN Codes, Communication through sockets: http://code.msdn.microsoft.com/Communication-through-91a2582b
 * 3. Several QA threads for common coding problems on: http://stackoverflow.com
 */
/*﻿﻿﻿
 ************* THIS IS THE SERVER PROGRAM ***************
 */
using System;                       // Used for running C# program
using System.Text;                  // Used for ASCII and unicode character encodings
using System.Windows;               // Used for creating and managing the lifetime of dialog boxes
using System.Windows.Controls;      // Used for UI elements for defining appearence
using System.Net;                   // Used for networking
using System.Net.Sockets;           // Used for implementing sockets
using System.Threading;             // Used for enabling multithreaded programming
using System.Windows.Threading;     // Used for supporting WPF multithreading

// Namespace declared for organizing classes
namespace ServerSocketWpfApp
{
    // For managing lifetime of windows we declare windows class
    public partial class MainWindow : Window
    {
        // A socket for listening incoming client connections
        Socket sListener;
        // A socket for handling incoming client connections
        Socket handler;
        // Used for getting IP and port of network endpoint
        IPEndPoint ipEndPoint;
        // String for storing incoming stream of bytes
        private string content = string.Empty;
        // String for storing incoming word as a string
        private string str = string.Empty;
        // String for storing the alternate words for the incoming word, for sending to client
        private string alternate;
        // A standard counter variable
        private int i;
        // Static declaration of the words for which synonyms are to be sent to client, as an array
        string[] word = new string[8] { "perhaps",
                                        "improve",
                                        "effectively",
                                        "expert",
                                        "grasp",
                                        "thought",
                                        "comprehension",
                                        "heightened" };
        // Static declaration of the alternate words associated with each of the words above, as an array
        string[] alt = new string[8]  {  "maybe; perchance; feasibly; reasonably",
                                         "advance, better, correct, develop",
                                         "completely, adequately, finally, definitely",
                                         "adept, experienced, trained, skilled",
                                         "hold, grip, clench, understand",
                                         "attention, logic, thinking, apprehending",
                                         "apprehension, awareness, understanding, grasp",
                                         "profound, fierce, elevated, intensive"  };
        
        // Initialization of new instance of the Window class
        public MainWindow()
        {
            // For initialization of the user interface components in the window
            InitializeComponent();
            // Enabling the 'Start Server' button for starting the server
            Start_Button.IsEnabled = true;
            // Disabling the 'Start Listening' button so that it is not pressed before server is started
            StartListen_Button.IsEnabled = false;
            // Disabling the 'Close Connection' button
            Close_Button.IsEnabled = true;
        }

        /* This method is called whenever the 'Start Server' button is clicked.
         * It takes an object and an event handler as the input.
         * It does all the functions needed to start a server
         */
        private void Start_Click(object sender, RoutedEventArgs e)
        {

            // The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Listening Socket object is set to null in the beginning
                sListener = null;
                // Resolves a host name to an IPHostEntry instance 
                IPHostEntry ipHost = Dns.GetHostEntry("");
                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = ipHost.AddressList[0];
                // Creates a network endpoint 
                ipEndPoint = new IPEndPoint(ipAddr, 4510);

                // Prepare the socket object to listen the incoming connection
                sListener = new Socket(
                    ipAddr.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );

                // Associates a socket with a local endpoint 
                sListener.Bind(ipEndPoint);
                // Displays the status of the server when the server starts, in the window
                status.Text = "Server started.";

                // Makes the 'Start Server' button disabled, as the server is already on
                Start_Button.IsEnabled = false;
                // Enables the 'Start Listening' button
                StartListen_Button.IsEnabled = true;
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        /* This method is invoked when 'Start Listening' button is clicked.
         * It also takes an object and an event handler as the input.
         * Performs necessary job to make the server listen to incoming client requests,
         * After it is started.
         */
        private void Listen_Click(object sender, RoutedEventArgs e)
        {
            //The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Places a socket in a listening state and specifies the maximum 
                // Length of the pending connections queue 
                sListener.Listen(2);

                // Begins an asynchronous operation to accept a connection attempt
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);                
                sListener.BeginAccept(aCallback, sListener);
                
                // Disabling the 'Start Listening' button
                StartListen_Button.IsEnabled = false;
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        /* This method is called whenever an attempt is made by client to connect to server.
         * It starts accepting the client message by storing it in a buffer.
         */
        public void AcceptCallback(IAsyncResult ar)
        {
            // A new socket for listening
            Socket listener = null;
            // A new Socket to handle remote host communication 
            Socket handler = null;
            
            //The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // Receiving byte array
                byte[] buffer = new byte[1024];
                // Get Listening Socket object 
                listener = (Socket)ar.AsyncState;
                // Create a new socket 
                handler = listener.EndAccept(ar);

                // Declaring a IpEndPoint for getting client's IP and port number
                IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

                /* This is used because the UI thread is different in WPF then the socket or
                 * Other threads. Thus, Dispatcher.BeginInvoke() method accesses the UI thread for 
                 * IO purposes. The first argument is the priority, and the second is the method 
                 * that must be processed on UI thread.
                 */
                Application.Current.Dispatcher.BeginInvoke(
                // Normal priority is the typicall application priority
                DispatcherPriority.Normal,
                // Displays the client IP and port on the server textbox and appends when more than one client is connected
                new Action(() => tbStatus.Text = tbStatus.Text + "IP: " + remoteIpEndPoint.Address + " & Port: " + remoteIpEndPoint.Port + "\n"));
                
                // Creates one object array for passing data 
                object[] obj = new object[2];
                // Buffer and handler passed in the array
                obj[0] = buffer;
                obj[1] = handler;

                // Begins to asynchronously receive data 
                handler.BeginReceive(
                    buffer,                 // An array of type Byt for received data 
                    0,                      // The zero-based position in the buffer  
                    buffer.Length,          // The number of bytes to receive 
                    SocketFlags.None,       // Specifies send and receive behaviors 
                    new AsyncCallback(ReceiveCallback),     //An AsyncCallback delegate 
                    obj                     // Specifies infomation for receive operation 
                    );

                // Begins an asynchronous operation to accept a connection attempt,
                // as the server is still running.
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);

            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        
        /* This method is executed after AcceptCallBack for receiving and manipulating the 
         * incoming data from the client.
         */
        public void ReceiveCallback(IAsyncResult ar)
        {
            //The code is written in try block so as to catch any exceptions, if thrown
            try
            {

                // Fetch a user-defined object that contains information 
                object[] obj = new object[2];
                obj = (object[])ar.AsyncState;
                // Received byte array 
                byte[] buffer = (byte[])obj[0];
                // A Socket to handle remote host communication
                handler = (Socket)obj[1];
                // string content is first made empty
                string content = string.Empty;
                // The number of bytes received. 
                int bytesRead = handler.EndReceive(ar);

                // If there is atleast one character in the message, then begin manipulating it
                if (bytesRead > 0)
                {
                    // Content is filled with data from buffer after encoding it
                    content += Encoding.Unicode.GetString(buffer, 0, bytesRead);

                    // Convert byte array to string untill the End Of File <Client Quit> is reached
                    string str = content.Substring(0, content.LastIndexOf("<Client Quit>"));

                    // Start a loop, which will iterate 8 times, as there are 8 words in thesaurus
                    for (i = 0; i < 8;)
                    {
                        // Checking if the received word is in the test data of thesaurus
                        if (str == word[i])
                        {
                            // If a match is found, the string with alternate words of the 
                            // given word is copied in alternate variable, and the loop is exited.
                            alternate = alt [i];
                            break;
                        }
                        else
                        {
                            // If a match is not found, alternate variable is made empty and
                            // the loop counter is incremented by 1.
                            alternate = "";
                            i++;
                        }
                    }
                    // If the alternate variable is empty after checking all the words in the 
                    // dictionary, it is filled with string "Not in Dictionary!" for indicating 
                    // the same to the user.
                    if(alternate == "")
                    {
                           alternate = "Not in Dictionary!";
                    } 

                    // Prepare the reply message 
                    byte[] altreply =  Encoding.Unicode.GetBytes(alternate);

                    // Sends the string of alternate words asynchronously to the client
                    handler.BeginSend(altreply, 0, altreply.Length, 0, new AsyncCallback(SendCallback), handler);

                    // Continues to asynchronously receive data
                    byte[] buffernew = new byte[1024];
                    obj[0] = buffernew;
                    obj[1] = handler;
                    handler.BeginReceive(buffernew, 0, buffernew.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), obj);
                }
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        /* The following method is called in ReceiveCallBack() method when the data with string
         * of alternate words is sent over to the client from the server.
         */
        public void SendCallback(IAsyncResult ar)
        {
            //The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // A Socket which has sent the data to remote host 
                Socket handler = (Socket)ar.AsyncState;
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        /* This method is invoked on clicking the 'Close Connection' button.
         * It closes the sockets, releases the resources and then closes the window
         */
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            //The code is written in try block so as to catch any exceptions, if thrown
            try
            {
                // If the socket is connected, shut it and close it.
                if (sListener.Connected)
                {
                    sListener.Shutdown(SocketShutdown.Receive);
                    sListener.Close();
                }

                // Close the server window
                this.Close();
            }
            // Used for catching any exceptions, if thrown
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }
    }
}
