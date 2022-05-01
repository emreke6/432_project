using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Data.SqlClient;

namespace Client_project
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        string Master_pub;
        string Server1_pub;
        string Server2_pub;
        string connected_pub;


        public class Document
        {
            public int DocId { get; set; }
            public string DocName { get; set; }
            public byte[] DocContent { get; set; }
        }

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ip = IP.Text;
            string portNum = port.Text;
            int port_num;
            if (Int32.TryParse(portNum, out port_num))
            {
                try
                {
                    clientSocket.Connect(ip, port_num);
                    connect_button.Enabled = false;
                    connected = true;
                    upload_button.Enabled = true;
                    logs.AppendText("Connected to the server. \n");

                    //Read from MasterServer_pub_prv.txt

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("MasterServer_pub.txt"))
                    {
                        Master_pub = fileReader.ReadLine();
                    }

                    //Read from MasterServer_pub_prv.txt

                    //Read from Server1_pub.txt

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server1_pub.txt"))
                    {
                        Server1_pub = fileReader.ReadLine();
                    }

                    //Read from Server1_pub.txt

                    //Read from Server2_pub.txt

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server2_pub.txt"))
                    {
                        Server2_pub = fileReader.ReadLine();
                    }

                    //Read from Server2_pub.txt

                    Byte[] buffer = new Byte[64];
                    buffer = Encoding.Default.GetBytes("client");
                    clientSocket.Send(buffer);

                    byte[] incomingWho = new byte[20];
                    clientSocket.Receive(incomingWho);
                    string incomingWhoString = Encoding.Default.GetString(incomingWho).Trim('\0');
                    logs.AppendText("Connected to " + incomingWhoString + "\n");

                    if(incomingWhoString == "master"){
                        connected_pub = Master_pub;
                    }
                    else if (incomingWhoString == "server1")
                    {
                        connected_pub = Server1_pub;
                    }
                    else if (incomingWhoString == "server2")
                    {
                        connected_pub = Server2_pub;
                    }
                    Thread receiveThread = new Thread(new ThreadStart(Receive));
                    receiveThread.Start();

                }
                catch {
                    logs.AppendText("Could not connect to the server. \n");
                }

            }
            else {
                logs.AppendText("Check the port number. \n");
            }

        }

        // FUNCTIONS START //
        public static byte[] GenerateRandomBytes(int length)
        {
            var rnd = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(rnd);
            return rnd;
        }

        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        static byte[] encryptWithAES128(string input, byte[] key, byte[] IV)
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
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform encryptor = aesObject.CreateEncryptor();
            byte[] result = null;

            try
            {
                result = encryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
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

        // FUNCTIONS END //
        private void Receive()
        {
            while (connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));

                    logs.AppendText(message + "\n");
                }
                catch {
                    if (!terminating)
                    {
                        logs.AppendText("Connection has lost with the server. \n");
                    }

                    clientSocket.Close();
                    connected = false;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void send_button_Click(object sender, EventArgs e)
        {
            String message = message_textBox.Text;

            if (message != "" && message.Length < 63)
            {
                Byte[] buffer = new Byte[64];
                buffer = Encoding.Default.GetBytes(message);
                clientSocket.Send(buffer);
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

         

        private void upload_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Browse Text Files";
            //openFileDialog1.ShowDialog();
  
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Read file to byte array
                string filepath = openFileDialog1.FileName;
                var filepathList = filepath.Split('\\');
                string filename = filepathList[filepathList.Count() - 1];
                FileStream stream = File.OpenRead(filepath);
                byte[] fileBytes = new byte[stream.Length];

                stream.Read(fileBytes, 0, fileBytes.Length);
                stream.Close();

                byte[] randomAES128Key = GenerateRandomBytes(16);
                byte[] randomAES128IV = GenerateRandomBytes(16);

                string randomKeyString = Encoding.Default.GetString(randomAES128Key).Trim('\0');
                string randomIVString = Encoding.Default.GetString(randomAES128IV).Trim('\0');

                byte[] keyEncryptedRSA = encryptWithRSA(randomKeyString, 3072, connected_pub);
                byte[] IVEncryptedRSA = encryptWithRSA(randomIVString, 3072, connected_pub);
                string encryptFileString = Encoding.Default.GetString(fileBytes).Trim('\0');
                byte[] fileEncryptedAES = encryptWithAES128(encryptFileString, randomAES128Key, randomAES128IV );
                byte[] filenameEncryptedAES = encryptWithAES128(filename + "---", randomAES128Key, randomAES128IV);

                byte[] combinedEncryptedData = new byte[keyEncryptedRSA.Length + IVEncryptedRSA.Length + filenameEncryptedAES.Length + fileEncryptedAES.Length];
                System.Buffer.BlockCopy(keyEncryptedRSA, 0, combinedEncryptedData, 0, keyEncryptedRSA.Length);
                System.Buffer.BlockCopy(IVEncryptedRSA, 0, combinedEncryptedData, keyEncryptedRSA.Length, IVEncryptedRSA.Length);
                System.Buffer.BlockCopy(filenameEncryptedAES, 0, combinedEncryptedData, keyEncryptedRSA.Length + IVEncryptedRSA.Length, filenameEncryptedAES.Length);
                System.Buffer.BlockCopy(fileEncryptedAES, 0, combinedEncryptedData, keyEncryptedRSA.Length + IVEncryptedRSA.Length + filenameEncryptedAES.Length, fileEncryptedAES.Length);

                string whatToDo = "file_upload";
                byte[] whatToDoByte = Encoding.Default.GetBytes(whatToDo);
                clientSocket.Send(whatToDoByte);

                clientSocket.Send(combinedEncryptedData);
                logs.AppendText(combinedEncryptedData.Length.ToString() + "\n");
                //logs.AppendText(combinedEncryptedData.Length.ToString() + "\n");
                string deneme = generateHexStringFromByteArray(randomAES128Key);
                logs.AppendText(deneme + "\n");

                string deneme2 = Encoding.Default.GetString(fileEncryptedAES).Trim('\0');
                logs.AppendText(deneme2 + "\n");
            }

        }
    }
}
