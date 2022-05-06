using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteServer_project
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<string> Users = new List<String>();
        List<Socket> UserSocketList = new List<Socket>(); // list of client socket connected to server
        List<Socket> ServerSocketList = new List<Socket>(); // list of client socket connected to server
        List<Socket> AllSocketList = new List<Socket>(); // list of all sockets connected to server
        string Master_pub_priv;
        string Server1_pub;
        string Server2_pub;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void listen_button_Click(object sender, EventArgs e)
        {
            string portnum = port.Text;
            int port_num;
            
            if (Int32.TryParse(portnum, out port_num))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port_num));
                serverSocket.Listen(3);

                listening = true;
                listen_button.Enabled = false;
                Thread acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();

                logs.AppendText("Started listening. \n");

                
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("MasterServer_pub_prv.txt"))
                {
                    Master_pub_priv = fileReader.ReadLine();
                }

                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server1_pub.txt"))
                {
                    Server1_pub = fileReader.ReadLine();
                }


                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server2_pub.txt"))
                {
                    Server2_pub = fileReader.ReadLine();
                }


                logs.AppendText("------------------------------------------------------------------");
                logs.AppendText("Public RSA keys:\n");
                logs.AppendText("server1: " + generateHexStringFromByteArray(Server1_pub) + "\n");
                logs.AppendText("server2: " + generateHexStringFromByteArray(Server2_pub) + "\n");
                logs.AppendText("\n\n Private key of Master\n");
                logs.AppendText("Master: " + generateHexStringFromByteArray(Master_pub_priv) + "\n");
                logs.AppendText("------------------------------------------------------------------");


            }
            else
            {
                logs.AppendText("Check the port number. \n");
            }
        }

        //FUNCTIONS PART START
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

        public static byte[] GenerateRandomBytes(int length)
        {
            var rnd = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(rnd);
            return rnd;
        }

        // RSA encryption with varying bit length
        static byte[] encryptWithRSA(byte[] byteInput, int algoLength, string xmlStringKey)
        {
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                //true flag is set to perform direct RSA encryption using OAEP padding
                result = rsaObject.Encrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
        // RSA encryption with varying bit length

        // RSA decryption with varying bit length
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

        static byte[] signWithRSA(byte[] byteInput, int algoLength, string xmlString)
        {
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.SignData(byteInput, "SHA256");
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

        //FUNCTIONS PART END


        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();

                    Byte[] buffer = new Byte[64];
                    newClient.Receive(buffer);
                    string which_incomer = bytes_to_string(buffer);

                    if(which_incomer == "client"){
                        UserSocketList.Add(newClient);
                        AllSocketList.Add(newClient);
                        logs.AppendText("A client is connected. \n");
                        string whoami = "master";
                        byte[] whoamiByte = string_to_bytes(whoami);
                        newClient.Send(whoamiByte);
                        Users.Add("Client");
                    }

                    else if (which_incomer == "server1")
                    {
                        ServerSocketList.Add(newClient);
                        AllSocketList.Add(newClient);
                        Users.Add("Server");
                        logs.AppendText("A server is connected. \n");
 
                        byte[] randomBytes = new byte[48];
                        randomBytes = GenerateRandomBytes(48);

                        byte[] encryptedRSA = encryptWithRSA(randomBytes, 3072, Server1_pub);
                        byte[] sessionKeySigned = signWithRSA(encryptedRSA, 3072, Master_pub_priv);

                        Byte[] aes_key_buffer = new Byte[16];
                        Byte[] aes_iv_buffer = new Byte[16];
                        Byte[] HMAC_buffer = new Byte[16];

                        Array.Copy(randomBytes, 0, aes_key_buffer, 0, 16);
                        Array.Copy(randomBytes, 16, aes_iv_buffer, 0, 16);
                        Array.Copy(randomBytes, 32, HMAC_buffer, 0, 16);

                        logs.AppendText("\n\nKeys for connection with server1: ");
                        logs.AppendText("Key: " + generateHexStringFromByteArray(aes_key_buffer) + "\n");
                        logs.AppendText("IV: " + generateHexStringFromByteArray(aes_iv_buffer) + "\n");
                        logs.AppendText("HMAC: " + generateHexStringFromByteArray(HMAC_buffer) + "\n\n\n");
                        logs.AppendText("Sign: " + generateHexStringFromByteArray(sessionKeySigned) + "\n\n\n");

                        
                        newClient.Send(encryptedRSA);
                        newClient.Send(sessionKeySigned);

                        logs.AppendText("New session key provided to Server \n");
                    }
                   
                    Thread receiveThread = new Thread(new ThreadStart(Receive));
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
                        logs.AppendText("Socket has stopped working. \n");
                    }
                }
            }
        }

        private void Receive()
        {
            Socket s = AllSocketList[AllSocketList.Count - 1]; //client that is newly added
            string user = Users[AllSocketList.Count - 1];
            bool connected = true;

            while (!terminating && connected)
            {
                try
                {
                    Byte[] buffer = new Byte[11];
                    s.Receive(buffer);

                    string message = bytes_to_string(buffer);
                    if (message == "")
                    {
                        connected = false;
                        logs.AppendText("Disconnected from " + user + ". \n");
                    }

                    if(message == "file_upload")
                    {
                        List<byte[]> fileBytesList= new List<byte[]>();

                        string filename = "";

                        Byte[] combinedKeyInput = new Byte[768];
                        s.Receive(combinedKeyInput);

                        byte[] AESKeyEncrypted = new byte[384];
                        Array.Copy(combinedKeyInput, 0, AESKeyEncrypted, 0, 384);
                        byte[] AESIVEncrypted = new byte[384];
                        Array.Copy(combinedKeyInput, 384, AESIVEncrypted, 0, 384);

                        byte[] AESKey = decryptWithRSA(AESKeyEncrypted, 3072, Master_pub_priv);
                        byte[] AESIV = decryptWithRSA(AESIVEncrypted, 3072, Master_pub_priv);

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

                        logs.AppendText("Started receiving " + filename + " from " + user + " with size of " + inputFileSizeString + ".\n");

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
                        logs.AppendText("A " + user + " has disconnected. \n");
                    }

                    s.Close();

                    if (user == "Client") UserSocketList.Remove(s);
                    else ServerSocketList.Remove(s);

                    AllSocketList.Remove(s);
                    connected = false;
                }
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;

            for (int i = 0; i < AllSocketList.Count; i++)
            {
                AllSocketList[i].Close();
            }

            Environment.Exit(0);
        }
    }
}
