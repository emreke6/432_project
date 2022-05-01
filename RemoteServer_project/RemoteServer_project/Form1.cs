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

                //Read from MasterServer_pub_prv.txt
                
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("MasterServer_pub_prv.txt"))
                {
                    Master_pub_priv = fileReader.ReadLine();
                }
                //logs.AppendText(Master_pub_priv + "\n");

                //Read from MasterServer_pub_prv.txt

                //Read from Server1_pub.txt
                
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server1_pub.txt"))
                {
                    Server1_pub = fileReader.ReadLine();
                }
                //logs.AppendText(Server1_pub + "\n");

                //Read from Server1_pub.txt

                //Read from Server2_pub.txt

                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server2_pub.txt"))
                {
                    Server2_pub = fileReader.ReadLine();
                }
                //logs.AppendText(Server2_pub + "\n");

                //Read from Server2_pub.txt




            }
            else
            {
                logs.AppendText("Check the port number. \n");
            }
        }

        //FUNCTIONS PART START
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
        static byte[] encryptWithRSA(string input, int algoLength, string xmlStringKey)
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

        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
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

        static byte[] decryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

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
                    string which_incomer = Encoding.Default.GetString(buffer);
                    which_incomer = which_incomer.Substring(0, which_incomer.IndexOf("\0"));

                    if(which_incomer == "client"){
                        UserSocketList.Add(newClient);
                        AllSocketList.Add(newClient);
                        logs.AppendText("A client is connected. \n");
                        string whoami = "master";
                        byte[] whoamiByte = Encoding.Default.GetBytes(whoami);
                        newClient.Send(whoamiByte);

                    }

                    else if (which_incomer == "server")
                    {

                        ServerSocketList.Add(newClient);
                        AllSocketList.Add(newClient);
                        logs.AppendText("A server is connected. \n");
                        //send server key objects
                        //Random byte generation
                        byte[] randomBytes = new byte[48];
                        randomBytes = GenerateRandomBytes(48);
                        //Random byte generation
                        //newClient.Send(random_bytes);
                        string encryptMessage = Encoding.Default.GetString(randomBytes).Trim('\0');
                        byte[] encryptedRSA = encryptWithRSA(encryptMessage, 3072, Server1_pub);
                        string encryptedRSAstring = Encoding.Default.GetString(encryptedRSA).Trim('\0');
                        byte[] sessionKeySigned = signWithRSA(encryptedRSAstring, 3072, Master_pub_priv);
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
            bool connected = true;

            

            while (!terminating && connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    s.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));
                    logs.AppendText(message + "\n");

                    if(message == "file_upload"){

                        Byte[] combinedDataInput  = new Byte[896]; //16 MB input
                        s.Receive(combinedDataInput);
                        logs.AppendText(combinedDataInput.Length.ToString() + "\n");
                        byte[] AESKeyEncrypted = new byte[384];
                        Array.Copy(combinedDataInput, 0, AESKeyEncrypted, 0, 384);
                        byte[] AESIVEncrypted = new byte[384];
                        Array.Copy(combinedDataInput, 384, AESIVEncrypted, 0, 384);
                        byte[] dataEncrypted = new byte[combinedDataInput.Count() - 768];
                        Array.Copy(combinedDataInput, 768, dataEncrypted, 0, combinedDataInput.Count() - 768);

                        //RSA Decryption
                        string AESKeyEncryptedString = Encoding.Default.GetString(AESKeyEncrypted).Trim('\0');
                        byte[] AESKey = decryptWithRSA(AESKeyEncryptedString, 3072, Master_pub_priv);
                        string AESIVEncryptedString = Encoding.Default.GetString(AESIVEncrypted).Trim('\0');
                        byte[] AESIV = decryptWithRSA(AESIVEncryptedString, 3072, Master_pub_priv);
                        //RSA Decryption
                        //AES Decryption
                        string dataEncryptedString = Encoding.Default.GetString(dataEncrypted).Trim('\0');
                        byte[] Data = decryptWithAES128(dataEncryptedString, AESKey, AESIV);
                        string DataString = Encoding.Default.GetString(Data).Trim('\0');
                        //logs.AppendText(DataString);

                        string[] fileArray = DataString.Split(new string[] { "---" }, StringSplitOptions.None);
                        
                        string filename = fileArray[0];
                        string fileContent = fileArray[1];
                        logs.AppendText(DataString + "\n");
                        byte[] fileBytes = Encoding.Default.GetBytes(fileContent);
                        using (Stream file = File.OpenWrite(Directory.GetCurrentDirectory() + "\\" + filename))
                        {
                            file.Write(fileBytes, 0, fileBytes.Length);
                        }

                        //AES Decryption

                        //string hexdeneme = generateHexStringFromByteArray(AESKey);
                        //logs.AppendText(hexdeneme + "\n");
                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("A client is disconnected. \n");
                    }

                    s.Close();
                    //UserSocketList.Remove(s);
                    AllSocketList.Remove(s);
                    connected = false;
                }
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
