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
        private byte[] string_to_bytes(string word)
        {
            return Encoding.Default.GetBytes(word);
        }

        private string bytes_to_string(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes).Trim('\0');
        }

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
                /*
                try
                {
                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);

                    string message = bytes_to_string(buffer);
                    logs.AppendText("message: " + message + "\n");
                    
                }
                catch {
                    if (!terminating)
                    {
                        logs.AppendText("Connection has lost with the server. \n");
                    }

                    clientSocket.Close();
                    connected = false;
                }
                 * */
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
                buffer = string_to_bytes(message);
                clientSocket.Send(buffer);
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private string get_file_name(string path)
        {
            var filePathList = path.Split('\\');
            return filePathList[filePathList.Count() - 1];
        }

        private byte[] combine_byte_arrays(byte[] arr1, byte[] arr2)
        {
            byte[] combinedEncryptedData = new byte[arr1.Length + arr2.Length];

            System.Buffer.BlockCopy(arr1, 0, combinedEncryptedData, 0, arr1.Length);
            System.Buffer.BlockCopy(arr2, 0, combinedEncryptedData, arr1.Length, arr2.Length);
            //System.Buffer.BlockCopy(arr3, 0, combinedEncryptedData, arr1.Length + arr2.Length, arr3.Length);

            return combinedEncryptedData;
        }

        private byte[] parse_array(byte[] arr, int idx1, int len)
        {
            int lenMin = (arr.Length - idx1) < len ? (arr.Length - idx1) : len;
            byte[] slice = new byte[lenMin];
            System.Buffer.BlockCopy(arr, idx1, slice, 0, lenMin);
            return slice;
        }

        private void upload_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Browse Text Files";
  
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                string filePath = openFileDialog1.FileName;
                string fileName = get_file_name(filePath);
                
                FileStream stream = File.OpenRead(filePath);
                byte[] fileBytes = new byte[stream.Length];
                stream.Read(fileBytes, 0, fileBytes.Length);
                stream.Close();

                byte[] randomAES128Key = GenerateRandomBytes(16);
                byte[] randomAES128IV = GenerateRandomBytes(16);

                string randomKeyString = bytes_to_string(randomAES128Key);
                string randomIVString = bytes_to_string(randomAES128IV);

                byte[] keyRsaEncrypted = encryptWithRSA(randomKeyString, 3072, connected_pub);
                byte[] IVRsaEncrypted = encryptWithRSA(randomIVString, 3072, connected_pub);

                string operation = "file_upload";
                byte[] operationBytes = new byte[11];
                operationBytes = string_to_bytes(operation);

                clientSocket.Send(operationBytes);        

                int mb = 8192;



                byte[] combinedEncryptedKey = new byte[768];
                combinedEncryptedKey = combine_byte_arrays(keyRsaEncrypted, IVRsaEncrypted);
                clientSocket.Send(combinedEncryptedKey);

                byte[] filenameAesEncrypted = encryptWithAES128(fileName, randomAES128Key, randomAES128IV);

                string operationFileByte = (fileBytes.Length).ToString();
                byte[] encryptedFileByteBytes = encryptWithAES128(operationFileByte, randomAES128Key, randomAES128IV);

                clientSocket.Send(encryptedFileByteBytes);


                byte[] FilenameHeader = new byte[32];
                FilenameHeader = string_to_bytes(filenameAesEncrypted.Length.ToString());
                string FileNameHeaderString = bytes_to_string(FilenameHeader);
                byte[] fileNameHeaderEncrypted = encryptWithAES128(FileNameHeaderString, randomAES128Key, randomAES128IV);
                clientSocket.Send(fileNameHeaderEncrypted);

                clientSocket.Send(filenameAesEncrypted);

                int loopCount = (fileBytes.Length / mb) + 1;
                logs.AppendText("loop mb is: " + (loopCount * mb).ToString() + "\n");
                long count = 0;
                for (int i = 0; i < loopCount; i++)
                {
                    byte[] fileBytesSlice = parse_array(fileBytes, i * mb, mb);
                    count += fileBytesSlice.Length;
                    string fileToString = bytes_to_string(fileBytesSlice);

                    byte[] EncryptedData = encryptWithAES128(fileToString, randomAES128Key, randomAES128IV);

                    byte[] DataHeader = new byte[4];
                    DataHeader = string_to_bytes(EncryptedData.Length.ToString());
                    string DataHeaderString = bytes_to_string(DataHeader);
                    byte[] dataHeaderEncrypted = encryptWithAES128(DataHeaderString, randomAES128Key, randomAES128IV);

                    clientSocket.Send(dataHeaderEncrypted);
                    if(EncryptedData.Length != 8208)
                    {
                        logs.AppendText(EncryptedData.Length.ToString() + "\n");
                    }
                    clientSocket.SendBufferSize = EncryptedData.Length;
                    clientSocket.Send(EncryptedData);

                }
                logs.AppendText("LoopCount is: " + (loopCount).ToString() + "\n");
                logs.AppendText("Count is: " + (count).ToString() + "\n");

                
                byte[] endToken = string_to_bytes("*--END--*");
                string endTokenString = "*--END--*";
          
                byte[] endTokenEncrypted = encryptWithAES128(endTokenString, randomAES128Key, randomAES128IV);

                byte[] endTokenLengthEncrypted = new byte[4];
                string endTokenEncryptedLengthString = endTokenEncrypted.Length.ToString();
                endTokenLengthEncrypted = encryptWithAES128(endTokenEncryptedLengthString, randomAES128Key, randomAES128IV);

                clientSocket.Send(endTokenLengthEncrypted);
                clientSocket.Send(endTokenEncrypted);
            }
        }
    }
}
