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
        string connected_server;


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
                    terminating = false;
                    upload_button.Enabled = true;
                    IP.Enabled = false;
                    port.Enabled = false;
                    button_disconnect.Enabled = true;

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

                    byte[] incomingWho = new byte[64];
                    clientSocket.Receive(incomingWho);
                    string incomingWhoString = Encoding.Default.GetString(incomingWho).Trim('\0');
                    logs.AppendText("Connected to " + incomingWhoString + "\n");

                    logs.AppendText("--------------------------------------------------------------\n");
                    logs.AppendText("Public RSA keys:\n");
                    logs.AppendText("server1: " + generateHexStringFromByteArray(Server1_pub) + "\n");
                    logs.AppendText("server2: " + generateHexStringFromByteArray(Server2_pub) + "\n");
                    logs.AppendText("Master: " + generateHexStringFromByteArray(Master_pub) + "\n");
                    logs.AppendText("--------------------------------------------------------------\n");

                    string directory1 = Directory.GetCurrentDirectory() + "\\DownloadFiles";
                    if (!Directory.Exists(directory1))
                    {
                        Directory.CreateDirectory(directory1);
                    }

                    if (incomingWhoString == "master"){
                        connected_pub = Master_pub;
                        connected_server = "master"; 
                    }
                    else if (incomingWhoString == "server1")
                    {
                        connected_pub = Server1_pub;
                        connected_server = "server1";
                    }
                    else if (incomingWhoString == "server2")
                    {
                        connected_pub = Server2_pub;
                        connected_server = "server2";
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

        static byte[] encryptWithAES128(byte[] byteInput, byte[] key, byte[] IV)
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

        

        // FUNCTIONS END //
        private void Receive()
        {
            while (connected)
            {
                //Socket client = clientSocket;
                try
                {
                    Byte[] buffer = new Byte[0];
                    clientSocket.Receive(buffer);
                   
                }
                catch {

                    if (!terminating)
                    {
                        logs.AppendText("Connection has lost with the server. \n");
                    }

                    clientSocket.Close();
                    connected = false;

                    IP.Enabled = true;
                    port.Enabled = true;
                    connect_button.Enabled = true;
                    upload_button.Enabled = false;
                    button_disconnect.Enabled = false;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Master_pub = "";
            Server1_pub = "";
            Server2_pub = "";
            connected_pub = "";
            connected_server = "";

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

                logs.AppendText(fileName + " with size of " + fileBytes.Length.ToString() + " is being sent. \n");

                byte[] randomAES128Key = GenerateRandomBytes(16);
                byte[] randomAES128IV = GenerateRandomBytes(16);


                logs.AppendText("\n\nKey and IV created for aes128 encryption:\n");
                logs.AppendText("Key: " + generateHexStringFromByteArray(randomAES128Key) + "\n");
                logs.AppendText("IV: " + generateHexStringFromByteArray(randomAES128IV) + "\n\n\n");

                byte[] keyRsaEncrypted = encryptWithRSA(randomAES128Key, 3072, connected_pub);
                byte[] IVRsaEncrypted = encryptWithRSA(randomAES128IV, 3072, connected_pub);

                string operation = "file_upload";
                byte[] operationBytes = new byte[11];
                operationBytes = string_to_bytes(operation);

                clientSocket.Send(operationBytes);        

                int mb = 8192;

                byte[] combinedEncryptedKey = new byte[768];
                combinedEncryptedKey = combine_byte_arrays(keyRsaEncrypted, IVRsaEncrypted);
                clientSocket.Send(combinedEncryptedKey);

                byte[] fileNameByte = string_to_bytes(fileName);
                byte[] filenameAesEncrypted = encryptWithAES128(fileNameByte, randomAES128Key, randomAES128IV);

                byte[] operationFileByte = string_to_bytes((fileBytes.Length).ToString());
                byte[] encryptedFileByteBytes = encryptWithAES128(operationFileByte, randomAES128Key, randomAES128IV);

                clientSocket.Send(encryptedFileByteBytes);


                byte[] FilenameHeader = new byte[32];
                FilenameHeader = string_to_bytes(filenameAesEncrypted.Length.ToString());
                byte[] fileNameHeaderEncrypted = encryptWithAES128(FilenameHeader, randomAES128Key, randomAES128IV);
                clientSocket.Send(fileNameHeaderEncrypted);

                clientSocket.Send(filenameAesEncrypted);

                int loopCount = (fileBytes.Length / mb) + 1;
                long count = 0;
                bool totalVerify = true;
                bool decryptionerror = false;
                for (int i = 0; i < loopCount; i++)
                {
                    byte[] fileBytesSlice = parse_array(fileBytes, i * mb, mb);
                    count += fileBytesSlice.Length;

                    byte[] EncryptedData = encryptWithAES128(fileBytesSlice, randomAES128Key, randomAES128IV);

                    byte[] DataHeader = new byte[4];
                    DataHeader = string_to_bytes(EncryptedData.Length.ToString());
                    byte[] dataHeaderEncrypted = encryptWithAES128(DataHeader, randomAES128Key, randomAES128IV);

                    clientSocket.Send(dataHeaderEncrypted);
                    clientSocket.SendBufferSize = EncryptedData.Length;
                    clientSocket.Send(EncryptedData);

                    
                    byte[] fileSignedInputSizeEncrypted = new byte[16];
                    clientSocket.Receive(fileSignedInputSizeEncrypted);

                    if (bytes_to_string(fileSignedInputSizeEncrypted) == "Decryption_Error")
                    {
                        logs.AppendText("There is decryption error in server");
                        decryptionerror = true;
                        break;
                    }

                    byte[] fileSignedInputSizeByte = decryptWithAES128(fileSignedInputSizeEncrypted, randomAES128Key, randomAES128IV);
                    string fileSignedInputSizeString = bytes_to_string(fileSignedInputSizeByte);
                    int fileSignedInputSizeInt = Int32.Parse(fileSignedInputSizeString);
                    
                    byte[] fileSignedInput = new byte[fileSignedInputSizeInt];
                   
                    clientSocket.Receive(fileSignedInput);


                    if (bytes_to_string(fileSignedInput) == "Decryption_Error")
                    {
                        logs.AppendText("There is decryption error in server");
                        decryptionerror = true;
                        break;
                    }

                    bool verifyresult = verifyWithRSA(fileBytesSlice, 3072, connected_pub, fileSignedInput);

                    if (!verifyresult)
                    {
                        totalVerify = false;
                        break;
                    }
                    
                }


                if (totalVerify == false)
                {
                    logs.AppendText("There is an error in verification for the file:" + fileName + "\n");
                    byte[] errorToken = string_to_bytes("*--ERR--*");

                    byte[] errorTokenEncrypted = encryptWithAES128(errorToken, randomAES128Key, randomAES128IV);

                    byte[] errorTokenLengthEncrypted = new byte[4];
                    byte[] errorTokenEncryptedLengthByte = string_to_bytes(errorTokenEncrypted.Length.ToString());
                    errorTokenLengthEncrypted = encryptWithAES128(errorTokenEncryptedLengthByte, randomAES128Key, randomAES128IV);

                    clientSocket.Send(errorTokenLengthEncrypted);
                    clientSocket.Send(errorTokenEncrypted);
                }
                else if (decryptionerror == true)
                {
                    logs.AppendText("File could not sent succesfully:" + fileName + "\n");
                }

                else 
                {
                    byte[] endToken = string_to_bytes("*--END--*");

                    byte[] endTokenEncrypted = encryptWithAES128(endToken, randomAES128Key, randomAES128IV);

                    byte[] endTokenLengthEncrypted = new byte[4];
                    byte[] endTokenEncryptedLengthByte = string_to_bytes(endTokenEncrypted.Length.ToString());
                    endTokenLengthEncrypted = encryptWithAES128(endTokenEncryptedLengthByte, randomAES128Key, randomAES128IV);

                    clientSocket.Send(endTokenLengthEncrypted);
                    clientSocket.Send(endTokenEncrypted);

                    logs.AppendText(fileName + " was succesfully sent. \n");
                  
                }
                
            }
        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            connected = false;
            IP.Enabled = true;
            port.Enabled = true;
            connect_button.Enabled = true;
            upload_button.Enabled = false;
            button_disconnect.Enabled = false;

            terminating = true;

            clientSocket.Close();
            logs.AppendText("Disconnected from " + connected_server + ". \n");

            Master_pub = "";
            Server1_pub = "";
            Server2_pub = "";
            connected_pub = "";
            connected_server = "";
        }

        private void IP_TextChanged(object sender, EventArgs e)
        {

        }

        private void download_button_Click(object sender, EventArgs e)
        {
            string downloadFilename = downloadFile.Text;
            byte[] filenameByte = string_to_bytes(downloadFilename);

            string operation = "file_downlo";
            byte[] op_bytes = new byte[11];
            op_bytes = string_to_bytes(operation);
            clientSocket.Send(op_bytes);

            int size = filenameByte.Length;
            string sizeString = size.ToString();
            
            byte[] sizeByteEncrypted = new byte[384];

            byte[] size_byte = string_to_bytes(sizeString);
            sizeByteEncrypted = encryptWithRSA(size_byte, 3072, connected_pub);

            clientSocket.Send(sizeByteEncrypted);

            clientSocket.Send(filenameByte);

            byte[] ackByte = new byte[8];

            clientSocket.Receive(ackByte);

            byte[] ackByteSigned = new byte[384];

            clientSocket.Receive(ackByteSigned);
            bool verify2 = verifyWithRSA(ackByte, 3072, Master_pub, ackByteSigned);

            string ack = bytes_to_string(ackByte);

            if(ack == "positive" && verify2 == true)
            {
                logs.AppendText("Positive acknowlegement received and it is verified \n");

                var stream = new FileStream(Directory.GetCurrentDirectory() + "\\DownloadFiles\\" + downloadFilename, FileMode.Append);

                bool totalVerify = true;
                while (true)
                {
                    byte[] combinedDataInputHeader = new byte[4];
                    clientSocket.Receive(combinedDataInputHeader);

                    string fileInputSizeString = bytes_to_string(combinedDataInputHeader);
                    string temp_size = "";

                    if (fileInputSizeString == "9*--")
                    {
                        fileInputSizeString = "6";
                    }

                    else
                    {
                        for (int i = 0; i < fileInputSizeString.Length; i++)
                        {
                            if (fileInputSizeString[i] != '-')
                            {
                                temp_size = temp_size + fileInputSizeString[i];
                            }
                            else
                            {
                                break;
                            }
                        }
                        fileInputSizeString = temp_size;
                    }


                    

                    int fileInputSizeInt = Int32.Parse(fileInputSizeString);

                    byte[] combinedDataInput = new byte[fileInputSizeInt];

                    clientSocket.Receive(combinedDataInput);

                    

                    byte[] data = new byte[fileInputSizeInt];
                    Array.Copy(combinedDataInput, 0, data, 0, fileInputSizeInt);

                    byte[] fileContentByte = new byte[fileInputSizeInt];

                    Array.Copy(data, 0, fileContentByte, 0, fileInputSizeInt);

                    string fileStringContent = bytes_to_string(fileContentByte);
                    string res = bytes_to_string(fileContentByte);
                    if (res == "ERR--*" || res == "*--ERR--*")
                    {
                        totalVerify = false;
                        break;
                    }
                    if (res == "END--*" || res == "*--END--*") break;


                    byte[] combinedDataInputSigned = new byte[384];

                    clientSocket.Receive(combinedDataInputSigned);

                    bool verify1 = verifyWithRSA(fileContentByte, 3072, connected_pub, combinedDataInputSigned);
                    if(!verify1)
                    {
                        totalVerify = false;
                        break;
                    }


                    stream.Write(fileContentByte, 0, fileContentByte.Length);
                }

                stream.Close();

                if (totalVerify == false)
                {
                    logs.AppendText("There is an error in verification for the file:" + downloadFilename + "\n");
                    File.Delete(Directory.GetCurrentDirectory() + "\\DownloadFiles\\" + downloadFilename);
                }

                else
                {
                    logs.AppendText("All file packets for the " + downloadFilename + " file was succesfully verified. \n");
                    logs.AppendText(downloadFilename + " was succesfully received and stored in the File Sytem. \n");

                }
            }

            else if (ack == "positive" && verify2 == false) 
            {
                logs.AppendText("Positive acknowlegement received but it is NOT verified \n");
            }


            else if (ack == "negative" && verify2 == true)
            {
                logs.AppendText("Negative acknowlegement received and it is verified \n");
            }

            else if (ack == "negative" && verify2 == false)
            {
                logs.AppendText("Negative acknowlegement received but it is NOT verified \n");
            }


            
        }
    }
}
