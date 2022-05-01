using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Server
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        bool remoteConnected = false;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket remoteSocket;
        List<Socket> socketList = new List<Socket>();

        string Server1_pub_priv;
        string Server2_pub;
        string Master_pub;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void listenButton_Click(object sender, EventArgs e)
        {
            int serverPort;
            Thread acceptThread;

            if (Int32.TryParse(clientPort.Text, out serverPort))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                serverSocket.Listen(3);

                listening = true;
                listenButton.Enabled = false;
                acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();

                logs.AppendText("Started listening on port: " + serverPort + "\n");

                
            }
            else
            {
                logs.AppendText("Please check port number \n");
            }
        }

        private void Accept()
        {
            while (listening)
            {
                try
                {
                    
                    Socket newClient = serverSocket.Accept();
                    socketList.Add(newClient);
                    logs.AppendText("A client is connected \n");

                    Byte[] trash = new Byte[64];
                    newClient.Receive(trash);

                    string whoami = "server1";
                    byte[] whoamiByte = Encoding.Default.GetBytes(whoami);
                    newClient.Send(whoamiByte);



                    Thread receiveThread;
                    receiveThread = new Thread(new ThreadStart(Receive));
                    receiveThread.Start();
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        logs.AppendText("The socket stopped working \n");
                    }
                }
            }
        }


        //FUNCTIONS PART START
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        static byte[] decryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(byteInput, "SHA256", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        //FUNCTIONS PART END
        private void Receive()
        {
            Socket s = socketList[socketList.Count - 1];
            bool connected = true;

            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    s.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    logs.AppendText(incomingMessage + "\n");

                    if (remoteConnected)
                    {
                        try
                        {
                            remoteSocket.Send(buffer);

                            buffer = new Byte[64];
                            remoteSocket.Receive(buffer);

                            s.Send(buffer);
                        }
                        catch
                        {
                            remoteConnected = false;
                            remoteSocket = null;
                            connectButton.Enabled = true;
                        }
                    }
                    else
                    {
                        logs.AppendText("Not connected to remote server \n");
                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("A client has disconnected \n");
                    }

                    s.Close();
                    socketList.Remove(s);
                    connected = false;
                }
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = ipAdress.Text;
            int port;
            if (Int32.TryParse(portNum.Text, out port))
            {
                try
                {
                    remoteSocket.Connect(IP, port);
                    remoteConnected = true;
                    connectButton.Enabled = false;

                    //*********Reading From Files ***************//

                    //Read from MasterServer_pub_prv.txt

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server1_pub_prv.txt"))
                    {
                        Server1_pub_priv = fileReader.ReadLine();
                    }
                    //logs.AppendText(Server1_pub_priv + "\n");

                    //Read from MasterServer_pub_prv.txt

                    //Read from Server1_pub.txt

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server2_pub.txt"))
                    {
                        Server2_pub = fileReader.ReadLine();
                    }
                    //logs.AppendText(Server2_pub + "\n");

                    //Read from Server1_pub.txt

                    //Read from Server2_pub.txt

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("MasterServer_pub.txt"))
                    {
                        Master_pub = fileReader.ReadLine();
                    }
                    //logs.AppendText(Master_pub + "\n");

                    //Read from Server2_pub.txt

                    //*********Reading From Files ***************//

                    Byte[] buffer = new Byte[64];
                    buffer = Encoding.Default.GetBytes("server");
                    remoteSocket.Send(buffer);

                    //Receive server key objects from master server;

                    Byte[] enryptedSessionKey = new byte[384];
                    remoteSocket.Receive(enryptedSessionKey);

                    Byte[] enryptedSessionKeySigned = new byte[384];
                    remoteSocket.Receive(enryptedSessionKeySigned);

                    // verifying with RSA 3072
                    string enryptedSessionKeyString = Encoding.Default.GetString(enryptedSessionKey).Trim('\0');
                    bool verificationResult = verifyWithRSA(enryptedSessionKeyString, 3072, Master_pub, enryptedSessionKeySigned);
                    if (verificationResult == true)
                    {
                        logs.AppendText("Valid signature \n");
                    }
                    else
                    {
                        logs.AppendText("Invalid signature \n");
                    }

                    byte[] decryptedKey = decryptWithRSA(enryptedSessionKeyString, 3072, Server1_pub_priv);
                    logs.AppendText("Session key is received from Master \n");
                  
                    
                    Byte[] aes_key_buffer = new Byte[16];
                    Byte[] aes_iv_buffer = new Byte[16];
                    Byte[] HMAC_buffer = new Byte[16];

                    Array.Copy(decryptedKey, 0, aes_key_buffer, 0, 16);
                    Array.Copy(decryptedKey, 16, aes_iv_buffer, 0, 16);
                    Array.Copy(decryptedKey, 32, HMAC_buffer, 0, 16);
                    //Receive server key objects from master server

                    logs.AppendText("Connected to remote server \n");
                    string key_print = generateHexStringFromByteArray(aes_key_buffer);
                    string iv_print = generateHexStringFromByteArray(aes_iv_buffer);
                    string hmac_print = generateHexStringFromByteArray(HMAC_buffer);
                    //logs.AppendText(key_print + "\n");
                    //logs.AppendText(iv_print + "\n");
                    //logs.AppendText(hmac_print + "\n");

                    

                }
                catch
                {
                    logs.AppendText("Could not connect to remote server\n");
                }
            }
            else
            {
                logs.AppendText("Check the port\n");
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}
