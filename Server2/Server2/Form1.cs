using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
using System.IO;

namespace Server2
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        bool remoteConnected = false;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket remoteSocket;
        List<Socket> socketList = new List<Socket>();

        string Server1_pub;
        string Server2_pub_priv;
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

                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("MasterServer_pub.txt"))
                {
                    Master_pub = fileReader.ReadLine();
                }

                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server1_pub.txt"))
                {
                    Server1_pub = fileReader.ReadLine();
                }


                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server2_pub_prv.txt"))
                {
                    Server2_pub_priv = fileReader.ReadLine();
                }

                logs.AppendText("------------------------------------------------------------------\n");
                logs.AppendText("Public RSA keys:\n");
                logs.AppendText("Master: " + generateHexStringFromByteArray(Master_pub) + "\n");
                logs.AppendText("server1: " + generateHexStringFromByteArray(Server1_pub) + "\n");
                logs.AppendText("\n\n Private key of Server1\n");
                logs.AppendText("Server2: " + generateHexStringFromByteArray(Server2_pub_priv) + "\n");
                logs.AppendText("------------------------------------------------------------------\n");
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

                    string whoami = "server2";
                    byte[] whoamiByte = string_to_bytes(whoami);
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

        // FUNCTIONS PART START

        private byte[] string_to_bytes(string word)
        {
            return Encoding.Default.GetBytes(word);
        }

        private string bytes_to_string(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes).Trim('\0');
        }

        static string generateHexStringFromByteArray(string input)
        {
            byte[] inputByte = Encoding.Default.GetBytes(input);
            string hexString = BitConverter.ToString(inputByte);
            return hexString.Replace("-", "");
        }
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

        static byte[] decryptWithRSA(byte[] byteInput, int algoLength, string xmlStringKey)
        {
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

        static bool verifyWithRSA(byte[] byteInput, int algoLength, string xmlString, byte[] signature)
        {
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

        static byte[] decryptWithAES128(byte[] byteInput, byte[] key, byte[] IV)
        {
            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            // aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            //aesObject.Padding = PaddingMode.PKCS7;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {
                result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        // FUNCTIONS PART END

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

                    string message = bytes_to_string(buffer);
                    logs.AppendText(message + "\n");
                    if (message == "")
                    {
                        connected = false;
                        logs.AppendText("Disconnected from client. \n");
                    }


                    if (message == "file_upload")
                    {
                        List<byte[]> fileBytesList = new List<byte[]>();

                        string filename = "";

                        Byte[] combinedKeyInput = new Byte[768];
                        s.Receive(combinedKeyInput);

                        byte[] AESKeyEncrypted = new byte[384];
                        Array.Copy(combinedKeyInput, 0, AESKeyEncrypted, 0, 384);
                        byte[] AESIVEncrypted = new byte[384];
                        Array.Copy(combinedKeyInput, 384, AESIVEncrypted, 0, 384);

                        byte[] AESKey = decryptWithRSA(AESKeyEncrypted, 3072, Server2_pub_priv);
                        byte[] AESIV = decryptWithRSA(AESIVEncrypted, 3072, Server2_pub_priv);

                        logs.AppendText("\n\nKey and IV created for aes128 encryption:\n");
                        logs.AppendText("Key: " + generateHexStringFromByteArray(AESKey) + "\n");
                        logs.AppendText("IV: " + generateHexStringFromByteArray(AESIV) + "\n\n\n");

                        Byte[] inputFileSizeBufferEncrypted = new Byte[16];
                        s.Receive(inputFileSizeBufferEncrypted);

                        byte[] inputFileSizeBuffer = decryptWithAES128(inputFileSizeBufferEncrypted, AESKey, AESIV);
                        string inputFileSizeString = bytes_to_string(inputFileSizeBuffer);
                        int inputFileSize = Int32.Parse(inputFileSizeString);

                        Byte[] fileNameInputSizeEncrypted = new Byte[16];
                        s.Receive(fileNameInputSizeEncrypted);
                        byte[] fileNameInputSizeBuffer = decryptWithAES128(fileNameInputSizeEncrypted, AESKey, AESIV);
                        string fileNameInputSizeString = bytes_to_string(fileNameInputSizeBuffer);
                        int fileNameInputSizeInt = Int32.Parse(fileNameInputSizeString);

                        Byte[] fileNameInput = new Byte[fileNameInputSizeInt];
                        s.Receive(fileNameInput);
                        byte[] filenameByte = decryptWithAES128(fileNameInput, AESKey, AESIV);
                        filename = bytes_to_string(filenameByte);

                        logs.AppendText("Started receiving " + filename + " from client with size of " + inputFileSizeString + ".\n");

                        var stream = new FileStream(Directory.GetCurrentDirectory() + "\\" + filename, FileMode.Append);
                        while (true)
                        {
                            byte[] combinedDataInputHeaderEncrypted = new byte[16];
                            s.Receive(combinedDataInputHeaderEncrypted);

                            byte[] fileInputSizeBuffer = decryptWithAES128(combinedDataInputHeaderEncrypted, AESKey, AESIV);
                            string fileInputSizeString = bytes_to_string(fileInputSizeBuffer);
                            int fileInputSizeInt = Int32.Parse(fileInputSizeString);

                            s.ReceiveBufferSize = fileInputSizeInt;
                            byte[] combinedDataInput = new byte[fileInputSizeInt];

                            s.Receive(combinedDataInput);


                            byte[] dataEncrypted = new byte[fileInputSizeInt];
                            Array.Copy(combinedDataInput, 0, dataEncrypted, 0, fileInputSizeInt);

                            byte[] fileContentByte = decryptWithAES128(dataEncrypted, AESKey, AESIV);

                            string fileStringContent = bytes_to_string(fileContentByte);

                            if (bytes_to_string(fileContentByte) == "*--END--*") break;

                            stream.Write(fileContentByte, 0, fileContentByte.Length);
                        }

                        stream.Close();
                        logs.AppendText(filename + " was succesfully received. \n");
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

        private void MasterReceive()
        {
            remoteConnected = true;
            while (remoteConnected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    remoteSocket.Receive(buffer);

                    string message = bytes_to_string(buffer);
                    if (message == "")
                    {
                        remoteConnected = false;
                        logs.AppendText("Disconnected from Master. \n");
                    }
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("Disconnected from Master. \n");
                    }

                    remoteSocket.Close();
                    remoteConnected = false;
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
                    connectButton.Enabled = false;

                    logs.AppendText("Master connection is pending...\n");


                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server2_pub_prv.txt"))
                    {
                        Server2_pub_priv = fileReader.ReadLine();
                    }

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server1_pub.txt"))
                    {
                        Server1_pub = fileReader.ReadLine();
                    }

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("MasterServer_pub.txt"))
                    {
                        Master_pub = fileReader.ReadLine();
                    }


                    Byte[] buffer = new Byte[64];
                    buffer = string_to_bytes("server1");
                    remoteSocket.Send(buffer);


                    Byte[] enryptedSessionKey = new byte[384];
                    remoteSocket.Receive(enryptedSessionKey);

                    Byte[] enryptedSessionKeySigned = new byte[384];
                    remoteSocket.Receive(enryptedSessionKeySigned);

                    // verifying with RSA 3072
                    bool verificationResult = verifyWithRSA(enryptedSessionKey, 3072, Master_pub, enryptedSessionKeySigned);

                    if (!verificationResult)
                    {
                        logs.AppendText("Invalid signature \n");
                    }
                    else
                    {
                        logs.AppendText("Valid signature \n");

                        byte[] decryptedKey = decryptWithRSA(enryptedSessionKey, 3072, Server2_pub_priv);

                        Byte[] aes_key_buffer = new Byte[16];
                        Byte[] aes_iv_buffer = new Byte[16];
                        Byte[] HMAC_buffer = new Byte[16];

                        Array.Copy(decryptedKey, 0, aes_key_buffer, 0, 16);
                        Array.Copy(decryptedKey, 16, aes_iv_buffer, 0, 16);
                        Array.Copy(decryptedKey, 32, HMAC_buffer, 0, 16);
                        //Receive server key objects from master server

                        logs.AppendText("\n\nKeys for connection with server1: ");
                        logs.AppendText("Key: " + generateHexStringFromByteArray(aes_key_buffer) + "\n");
                        logs.AppendText("IV: " + generateHexStringFromByteArray(aes_iv_buffer) + "\n");
                        logs.AppendText("HMAC: " + generateHexStringFromByteArray(HMAC_buffer) + "\n\n\n");
                        logs.AppendText("Sign: " + generateHexStringFromByteArray(bytes_to_string(enryptedSessionKeySigned)) + "\n\n\n");

                        logs.AppendText("Connected to Master \n");

                        Thread masterThread;
                        masterThread = new Thread(new ThreadStart(MasterReceive));
                        masterThread.Start();
                    }

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

            for (int i = 0; i < socketList.Count; i++)
            {
                socketList[i].Close();
            }
            Environment.Exit(0);
        }
    }
}
