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

namespace Server
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        bool remoteConnected = false;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket remoteSocket, server2Socket;
        List<Socket> socketList = new List<Socket>();

        List<string> server_replicates = new List<string>();
        List<string> server1_replicates = new List<string>();
        List<string> server2_replicates = new List<string>();
        List<string> connected_servers = new List<string>();

        string Server1_pub_priv;
        string Server2_pub;
        string Master_pub;

        Byte[] aes_key_buffer = new Byte[16];
        Byte[] aes_iv_buffer = new Byte[16];
        Byte[] HMAC_buffer = new Byte[16];

        List<string> Users = new List<String>();
        List<Socket> UserSocketList = new List<Socket>(); // list of client socket connected to server
        List<Socket> ServerSocketList = new List<Socket>(); // list of client socket connected to server
        List<Socket> AllSocketList = new List<Socket>(); // list of all sockets connected to server

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
                new System.IO.StreamReader("Server1_pub_prv.txt"))
                {
                    Server1_pub_priv = fileReader.ReadLine();
                }


                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server2_pub.txt"))
                {
                    Server2_pub = fileReader.ReadLine();
                }

                logs.AppendText("------------------------------------------------------------------");
                logs.AppendText("Public RSA keys:\n");
                logs.AppendText("Master: " + generateHexStringFromByteArray(Master_pub) + "\n");
                logs.AppendText("server2: " + generateHexStringFromByteArray(Server2_pub) + "\n");
                logs.AppendText("\n\n Private key of Server1\n");
                logs.AppendText("Master: " + generateHexStringFromByteArray(Server1_pub_priv) + "\n");
                logs.AppendText("------------------------------------------------------------------");
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


                    Byte[] incomer = new Byte[64];
                    newClient.Receive(incomer);
                    string which_incomer = bytes_to_string(incomer);

                    string whoami = "server1";
                    byte[] whoamiByte = new byte[64];
                    whoamiByte = string_to_bytes(whoami);

                    if (which_incomer == "client")
                    {
                        UserSocketList.Add(newClient);
                        AllSocketList.Add(newClient);
                        logs.AppendText("A client is connected. \n");

                        string directory1 = Directory.GetCurrentDirectory() + "\\ReceivedFiles";
                        if (!Directory.Exists(directory1))
                        {
                            Directory.CreateDirectory(directory1);
                        }
                        newClient.Send(whoamiByte);

                        Users.Add("Client");

                        Thread receiveThread = new Thread(new ThreadStart(Receive));
                        receiveThread.Start();
                    }

                    else if (which_incomer == "server2")
                    {
                        ServerSocketList.Add(newClient);
                        AllSocketList.Add(newClient);
                        server2Socket = newClient;
                        Users.Add("Server2");
                        logs.AppendText("Server2 is connected. \n");

                        newClient.Send(whoamiByte);

                        connected_servers.Add("Server2");

                        List<string> sent_files = new List<string>();
                        List<string> sent_files_1 = new List<string>();
                        try
                        {
                            if (connected_servers.Count == 2)
                            {
                                for (int i = 0; i < server_replicates.Count; i++)
                                {
                                    send_replicate(remoteSocket, server_replicates[i], Master_pub);
                                    send_replicate(server2Socket, server_replicates[i], Server2_pub);
                                    sent_files.Add(server_replicates[i]);
                                }
                            }

                            /*
                            for (int i = 0; i < server1_replicates.Count; i++)
                            {                                    
                                send_replicate(server1Socket, server1_replicates[i], Server1_pub);
                                sent_files_1.Add(server_replicates[i]);                                
                            } */
                        }
                        finally
                        {
                            for (int i = 0; i < sent_files.Count; i++)
                            {
                                server_replicates.Remove(sent_files[i]);
                            }

                            /*
                            for (int i = 0; i < sent_files_1.Count; i++)
                            {
                                server1_replicates.Remove(sent_files_1[i]);
                            }*/
                        }

                        Thread Server2ReceiveThread = new Thread(new ThreadStart(server2Receive));
                        Server2ReceiveThread.Start();
                    }

                    else if (which_incomer == "master")
                    {
                        

                        Thread masterReceiveThread = new Thread(new ThreadStart(MasterReceive));
                        masterReceiveThread.Start();
                    }




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
        static byte[] applyHMACwithSHA256(byte[] byteInput, byte[] key)
        {
            HMACSHA256 hmacSHA256 = new HMACSHA256(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA256.ComputeHash(byteInput);

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

        //FUNCTIONS PART END

        //NICE FUNCTIONS START

        void save_Queue()
        {
            using (StreamWriter writer = new StreamWriter("Queue.txt"))
            {
                logs.AppendText("BURDA. \n");
                for (int i = 0; i < server_replicates.Count; i++)
                {
                    writer.WriteLine(server_replicates[i]);
                }
            }
        }

        void send_replicate(Socket inputSocket, string filename, string connected_what)
        {
            //FOR PROJECT PART 2 //
            Socket whatSocket = inputSocket;
            string connected_pub = connected_what;
            string filenamex = Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename;
            //string path = Path.GetDirectoryName(filenamex);
            FileStream stream2 = File.OpenRead(filenamex);
            byte[] fileBytes = new byte[stream2.Length];
            stream2.Read(fileBytes, 0, fileBytes.Length);
            stream2.Close();

            logs.AppendText(filename + " with size of " + fileBytes.Length.ToString() + " is being sent. \n");


            string operation = "file_replic";
            byte[] operationBytes = new byte[11];
            operationBytes = string_to_bytes(operation);

            whatSocket.Send(operationBytes);

            int mb = 8192;


            byte[] fileNameByte = string_to_bytes(filename);
            byte[] filenameAesEncrypted = encryptWithAES128(fileNameByte, aes_key_buffer, aes_iv_buffer);

            byte[] operationFileByte = string_to_bytes((fileBytes.Length).ToString());
            byte[] encryptedFileByteBytes = encryptWithAES128(operationFileByte, aes_key_buffer, aes_iv_buffer);

            whatSocket.Send(encryptedFileByteBytes);

            byte[] FilenameHeader = new byte[32];
            FilenameHeader = string_to_bytes(filenameAesEncrypted.Length.ToString());
            byte[] fileNameHeaderEncrypted = encryptWithAES128(FilenameHeader, aes_key_buffer, aes_iv_buffer);
            whatSocket.Send(fileNameHeaderEncrypted);
            whatSocket.Send(filenameAesEncrypted);

            int loopCount = (fileBytes.Length / mb) + 1;
            long count = 0;
            bool totalVerify = true;
            for (int i = 0; i < loopCount; i++)
            {
                byte[] fileBytesSlice = parse_array(fileBytes, i * mb, mb);
                count += fileBytesSlice.Length;

                byte[] EncryptedData = encryptWithAES128(fileBytesSlice, aes_key_buffer, aes_iv_buffer);

                byte[] DataHeader = new byte[4];
                DataHeader = string_to_bytes(EncryptedData.Length.ToString());
                byte[] dataHeaderEncrypted = encryptWithAES128(DataHeader, aes_key_buffer, aes_iv_buffer);

                whatSocket.Send(dataHeaderEncrypted);
                //remoteSocket.SendBufferSize = EncryptedData.Length;

                whatSocket.Send(EncryptedData);

                //HmacPart
                try
                {
                    byte[] HmacValue = new byte[32];
                    HmacValue = applyHMACwithSHA256(fileBytesSlice, HMAC_buffer);
                    whatSocket.Send(HmacValue);
                }
                catch
                {
                    byte[] HmacValue = new byte[32];
                    HmacValue = string_to_bytes("HMAC_CATCH_ERRORHMAC_CATCH_ERROR");
                    whatSocket.Send(HmacValue);
                    break;
                }

                
            }


            if (totalVerify == false)
            {
                logs.AppendText("There is an error in verification for the file:" + filename + "\n");
                byte[] errorToken = string_to_bytes("*--ERR--*");

                byte[] errorTokenEncrypted = encryptWithAES128(errorToken, aes_key_buffer, aes_iv_buffer);

                byte[] errorTokenLengthEncrypted = new byte[4];
                byte[] errorTokenEncryptedLengthByte = string_to_bytes(errorTokenEncrypted.Length.ToString());
                errorTokenLengthEncrypted = encryptWithAES128(errorTokenEncryptedLengthByte, aes_key_buffer, aes_iv_buffer);

                whatSocket.Send(errorTokenLengthEncrypted);
                whatSocket.Send(errorTokenEncrypted);
            }


            else
            {
                byte[] endToken = string_to_bytes("*--END--*");

                byte[] endTokenEncrypted = encryptWithAES128(endToken, aes_key_buffer, aes_iv_buffer);

                byte[] endTokenLengthEncrypted = new byte[4];
                byte[] endTokenEncryptedLengthByte = string_to_bytes(endTokenEncrypted.Length.ToString());
                endTokenLengthEncrypted = encryptWithAES128(endTokenEncryptedLengthByte, aes_key_buffer, aes_iv_buffer);

                whatSocket.Send(endTokenLengthEncrypted);
                whatSocket.Send(endTokenEncrypted);

                logs.AppendText(filename + " was succesfully sent. \n");

            }

            //FOR PROJECT PART 2 //

        }
        //NICE FUNCTIONS END

        private void Receive()
        {
            Socket s = socketList[socketList.Count - 1];
            bool connected = true;

            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[11];
                    s.Receive(buffer);

                    string message = bytes_to_string(buffer);
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

                        try
                        {
                            byte[] AESKey = decryptWithRSA(AESKeyEncrypted, 3072, Server1_pub_priv);
                            byte[] AESIV = decryptWithRSA(AESIVEncrypted, 3072, Server1_pub_priv);

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

                            if (File.Exists(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename))
                            {
                                File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                            }

                            var stream = new FileStream(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename, FileMode.Append);
                            bool totalVerify = true;
                            while (true)
                            {
                                byte[] combinedDataInputHeaderEncrypted = new byte[16];
                                s.Receive(combinedDataInputHeaderEncrypted);

                                byte[] fileInputSizeBuffer = decryptWithAES128(combinedDataInputHeaderEncrypted, AESKey, AESIV);
                                string fileInputSizeString = bytes_to_string(fileInputSizeBuffer);
                                int fileInputSizeInt = Int32.Parse(fileInputSizeString);

                                byte[] combinedDataInput = new byte[fileInputSizeInt];

                                s.Receive(combinedDataInput);


                                byte[] dataEncrypted = new byte[fileInputSizeInt];
                                Array.Copy(combinedDataInput, 0, dataEncrypted, 0, fileInputSizeInt);

                                byte[] fileContentByte = decryptWithAES128(dataEncrypted, AESKey, AESIV);

                                string fileStringContent = bytes_to_string(fileContentByte);

                                if (bytes_to_string(fileContentByte) == "*--ERR--*")
                                {
                                    totalVerify = false;
                                    break;
                                }

                                if (bytes_to_string(fileContentByte) == "*--END--*") break;

                                byte[] fileSigned = signWithRSA(fileContentByte, 3072, Server1_pub_priv);

                                byte[] fileSignedLength = new byte[4];
                                fileSignedLength = string_to_bytes(fileSigned.Length.ToString());
                                byte[] fileSignedLengthEncrypted = encryptWithAES128(fileSignedLength, AESKey, AESIV);

                                s.Send(fileSignedLengthEncrypted);

                                s.Send(fileSigned);

                                Console.WriteLine(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                                stream.Write(fileContentByte, 0, fileContentByte.Length);
                            }

                            stream.Close();

                            if (totalVerify == false)
                            {
                                logs.AppendText("There is an error in verification for the file:" + filename + "\n");
                                File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                            }

                            else
                            {

                                logs.AppendText("All file packets for the " + filename + " file was succesfully verified. \n");
                                logs.AppendText(filename + " was succesfully received and stored in the File Sytem. \n");

                                //FOR PROJECT PART 2 //

                                server_replicates.Add(filename);

                                if (connected_servers.Count == 2)
                                {
                                    try
                                    {
                                        send_replicate(remoteSocket, filename, Master_pub);
                                        send_replicate(server2Socket, filename, Server2_pub);
                                        server_replicates.Remove(filename);
                                    }
                                    catch
                                    {
                                        logs.AppendText("There is an error in replication process \n");
                                        server_replicates.Remove(filename);
                                    }
                                }
                                
                              
                                //FOR PROJECT PART 2 //
                            }
                        }
                        catch 
                        {
                            logs.AppendText("There is an error in decryption process for the file \n");
                            if (File.Exists(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename))
                            {
                                File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                            }
                            string messageError = "Decryption_Error";
                            byte[] messageByte = new byte[16];
                            messageByte = string_to_bytes(messageError);
                            s.Send(messageByte);
                        }
                        



                    }
                    else if (message == "file_downlo")
                    {
                        byte[] size_byteEncrypted = new byte[384];

                        s.Receive(size_byteEncrypted);
                        byte[] size_byte = decryptWithRSA(size_byteEncrypted, 3072, Server1_pub_priv);
                        int sizeInt = Int32.Parse(bytes_to_string(size_byte));
                        logs.AppendText("size is " + sizeInt.ToString() + "\n");
                        byte[] filenameByte = new byte[sizeInt];
                        s.Receive(filenameByte);
                        string filenameDownload = bytes_to_string(filenameByte);
                        logs.AppendText("Requested file has a name of " + filenameDownload + "\n");

                        string downlaodFileDirectory = Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filenameDownload;

                        if (File.Exists(downlaodFileDirectory))
                        {
                            logs.AppendText("File with a name of " + filenameDownload + " exists! \n");
                            string ack = "positive";
                            byte[] ackByte = new byte[8];
                            ackByte = string_to_bytes(ack);
                            s.Send(ackByte);

                            byte[] ackByteSigned = new byte[384];
                            ackByteSigned = signWithRSA(ackByte, 3072, Server1_pub_priv);
                            s.Send(ackByteSigned);

                            FileStream stream = File.OpenRead(downlaodFileDirectory);
                            byte[] fileBytes = new byte[stream.Length];
                            stream.Read(fileBytes, 0, fileBytes.Length);
                            stream.Close();

                            logs.AppendText(filenameDownload + " with size of " + fileBytes.Length.ToString() + " is being sent. \n");


                            int mb = 8192;
                            int loopCount = (fileBytes.Length / mb) + 1;
                            long count = 0;
                            bool totalVerify = true;
                            for (int i = 0; i < loopCount; i++)
                            {
                                byte[] fileBytesSlice = parse_array(fileBytes, i * mb, mb);
                                count += fileBytesSlice.Length;

                                byte[] DataHeader = new byte[4];
                                string fileLength = fileBytesSlice.Length.ToString();

                                if (0 < fileBytesSlice.Length && fileBytesSlice.Length < 10)
                                {
                                    fileLength = fileLength + "---";
                                }
                                else if (9 < fileBytesSlice.Length && fileBytesSlice.Length < 100)
                                {
                                    fileLength = fileLength + "--";
                                }
                                else if (99 < fileBytesSlice.Length && fileBytesSlice.Length < 1000)
                                {
                                    fileLength = fileLength + "-";
                                }

                                DataHeader = string_to_bytes(fileLength);

                                s.Send(DataHeader);

                                s.SendBufferSize = fileBytesSlice.Length;
                                s.Send(fileBytesSlice);

                                byte[] signedFile = new byte[384];
                                signedFile = signWithRSA(fileBytesSlice, 3072, Server1_pub_priv);
                                s.Send(signedFile);

                            }

                            if (totalVerify == false)
                            {
                                logs.AppendText("There is an error in verification for the file:" + filenameDownload + "\n");
                                byte[] errorToken = string_to_bytes("*--ERR--*");

                                byte[] length = new byte[4];
                                length = string_to_bytes(errorToken.Length.ToString());

                                s.Send(length);

                                s.Send(errorToken);
                            }


                            else
                            {
                                byte[] endToken = string_to_bytes("*--END--*");
                                byte[] length = new byte[4];
                                length = string_to_bytes(endToken.Length.ToString());

                                s.Send(length);
                                s.Send(endToken);

                                logs.AppendText(filenameDownload + " was succesfully sent. \n");

                            }


                        }

                        else
                        {
                            logs.AppendText("File with a name of " + filenameDownload + " doesn't exists! \n");
                            string ack = "negative";
                            byte[] ackByte = new byte[8];
                            ackByte = string_to_bytes(ack);
                            s.Send(ackByte);

                            byte[] ackByteSigned = new byte[384];
                            ackByteSigned = signWithRSA(ackByte, 3072, Server1_pub_priv);
                            s.Send(ackByteSigned);
                        }


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
            string user = "master";
            remoteConnected = true;
            while (remoteConnected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[11];
                    remoteSocket.Receive(buffer);

                    string message = bytes_to_string(buffer);
                    if (message == "HmacErrSer1")
                    {

                        Byte[] buffer2 = new Byte[384];
                        remoteSocket.Receive(buffer2);

                        bool value = verifyWithRSA(buffer, 3072, Master_pub, buffer2);

                        if (value == true)
                        {
                            logs.AppendText("File Replication Hmac Error from Master \n");
                            logs.AppendText("file was unsuccesfully sent and Message verified with RSA 3072. \n");
                        }
                        else
                        {
                            logs.AppendText("Signature Verification was failed \n");
                        }
                    }
                    if (message == "HmacVerSer1")
                    {
                        Byte[] buffer2 = new Byte[384];
                        remoteSocket.Receive(buffer2);

                        bool value = verifyWithRSA(buffer, 3072, Master_pub, buffer2);

                        if (value == true)
                        {
                            logs.AppendText("File Replication Hmac Verified from Master \n");
                            logs.AppendText("file was succesfully sent and Message verified with RSA 3072. \n");
                        }
                        else
                        {
                            logs.AppendText("Signature Verification was failed \n");
                        }

                    }
                    if (message == "")
                    {
                        remoteConnected = false;
                        logs.AppendText("Disconnected from " + user + ". \n");
                    }

                    if (message == "file_replic")
                    {
                        List<byte[]> fileBytesList = new List<byte[]>();
                        logs.AppendText("REPLICKE \n");
                        string filename = "";

                        byte[] AESKey = new byte[16];
                        Array.Copy(aes_key_buffer, 0, AESKey, 0, 16);
                        byte[] AESIV = new byte[16];
                        Array.Copy(aes_iv_buffer, 0, AESIV, 0, 16);

                        logs.AppendText("\n\nKey and IV created for aes128 encryption:\n");
                        logs.AppendText("Key: " + generateHexStringFromByteArray(AESKey) + "\n");
                        logs.AppendText("IV: " + generateHexStringFromByteArray(AESIV) + "\n\n\n");

                        Byte[] inputFileSizeBufferEncrypted = new Byte[16];
                        remoteSocket.Receive(inputFileSizeBufferEncrypted);

                        byte[] inputFileSizeBuffer = decryptWithAES128(inputFileSizeBufferEncrypted, AESKey, AESIV);
                        string inputFileSizeString = bytes_to_string(inputFileSizeBuffer);
                        int inputFileSize = Int32.Parse(inputFileSizeString);

                        Byte[] fileNameInputSizeEncrypted = new Byte[16];
                        remoteSocket.Receive(fileNameInputSizeEncrypted);
                        byte[] fileNameInputSizeBuffer = decryptWithAES128(fileNameInputSizeEncrypted, AESKey, AESIV);
                        string fileNameInputSizeString = bytes_to_string(fileNameInputSizeBuffer);
                        int fileNameInputSizeInt = Int32.Parse(fileNameInputSizeString);

                        Byte[] fileNameInput = new Byte[fileNameInputSizeInt];
                        remoteSocket.Receive(fileNameInput);
                        byte[] filenameByte = decryptWithAES128(fileNameInput, AESKey, AESIV);
                        filename = bytes_to_string(filenameByte);

                        logs.AppendText("Started receiving " + filename + " from " + user + " with size of " + inputFileSizeString + ".\n");

                        if (File.Exists(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename))
                        {
                            File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                        }

                        var stream = new FileStream(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename, FileMode.Append);

                        bool totalVerify = true;
                        bool hmacValue = true;
                        while (true)
                        {
                            byte[] combinedDataInputHeaderEncrypted = new byte[16];
                            remoteSocket.Receive(combinedDataInputHeaderEncrypted);
                            byte[] fileInputSizeBuffer = decryptWithAES128(combinedDataInputHeaderEncrypted, AESKey, AESIV);
                            string fileInputSizeString = bytes_to_string(fileInputSizeBuffer);
                            int fileInputSizeInt = Int32.Parse(fileInputSizeString);

                            byte[] combinedDataInput = new byte[fileInputSizeInt];

                            remoteSocket.Receive(combinedDataInput);

                            byte[] dataEncrypted = new byte[fileInputSizeInt];
                            Array.Copy(combinedDataInput, 0, dataEncrypted, 0, fileInputSizeInt);

                            byte[] fileContentByte = decryptWithAES128(dataEncrypted, AESKey, AESIV);

                            string fileStringContent = bytes_to_string(fileContentByte);

                            if (bytes_to_string(fileContentByte) == "*--ERR--*")
                            {
                                totalVerify = false;
                                break;
                            }
                            if (bytes_to_string(fileContentByte) == "*--END--*") break;

                            logs.AppendText("Server2 Hmackey from master: " + generateHexStringFromByteArray(HMAC_buffer) + "\n");
                            byte[] HmacValue = new byte[32];
                            remoteSocket.Receive(HmacValue);

                            byte[] HmacServer1 = new byte[32];

                            if (bytes_to_string(HmacValue) == "HMAC_CATCH_ERRORHMAC_CATCH_ERROR")
                            {
                                hmacValue = false;
                                break;
                            }
                            //Byte[] HMAC_buffer2 = string_to_bytes("eeee");

                            try
                            {
                                HmacServer1 = applyHMACwithSHA256(fileContentByte, HMAC_buffer);
                            }
                            catch
                            {
                                hmacValue = false;
                                break;
                            }

                            if (generateHexStringFromByteArray(HmacServer1) != generateHexStringFromByteArray(HmacValue))
                            {
                                hmacValue = false;
                            }
                            
                            stream.Write(fileContentByte, 0, fileContentByte.Length);
                        }

                        stream.Close();

                        if (totalVerify == false)
                        {
                            logs.AppendText("There is an error in verification for the file:" + filename + "\n");
                            File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                        }
                        else if (hmacValue == false)
                        {
                            logs.AppendText("There is an ERROR IN HMAC: \n");

                            string path = Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename;
                            logs.AppendText("Path: " + path + "\n");
                            if (File.Exists(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename))
                            {
                                File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                            }

                            string messageError = "HmacErrMast";
                            byte[] messageByte = new byte[11];
                            messageByte = string_to_bytes(messageError);
                            remoteSocket.Send(messageByte);

                            byte[] signMessageError = signWithRSA(messageByte, 3072, Server1_pub_priv);
                            remoteSocket.Send(signMessageError);
                        }
                        else 
                        {
                            logs.AppendText("All file packets for the " + filename + " file was succesfully verified. \n");
                            logs.AppendText(filename + " was succesfully received and stored in the File Sytem. \n");

                            string messageError = "HmacVerMast";
                            byte[] messageByte = new byte[11];
                            messageByte = string_to_bytes(messageError);
                            remoteSocket.Send(messageByte);

                            byte[] signMessageError = signWithRSA(messageByte, 3072, Server1_pub_priv);
                            remoteSocket.Send(signMessageError);
                        }

                    }                  
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("Disconnected from Master. \n");
                    }

                    connectButton.Enabled = true;
                    remoteSocket.Close();
                    connected_servers.Remove("master");
                    remoteConnected = false;
                }
            }
        }

        private void server2Receive()
        {
            string user = "server2";
            bool server2Connected = true;
            while (server2Connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[11];
                    server2Socket.Receive(buffer);

                    string message = bytes_to_string(buffer);

                    if (message == "HmacErrSer1")
                    {

                        Byte[] buffer2 = new Byte[384];
                        server2Socket.Receive(buffer2);

                        bool value = verifyWithRSA(buffer, 3072, Server2_pub, buffer2);

                        if (value == true)
                        {
                            logs.AppendText("File Replication Hmac Error from Server2 \n");
                            logs.AppendText("file was unsuccesfully sent and Message verified with RSA 3072. \n");
                        }
                        else
                        {
                            logs.AppendText("Signature Verification was failed \n");
                        }
                    }
                    if (message == "HmacVerSer1")
                    {
                        Byte[] buffer2 = new Byte[384];
                        server2Socket.Receive(buffer2);

                        bool value = verifyWithRSA(buffer, 3072, Server2_pub, buffer2);

                        if (value == true)
                        {
                            logs.AppendText("File Replication Hmac Verified from Server2 \n");
                            logs.AppendText("file was succesfully sent and Message verified with RSA 3072. \n");
                        }
                        else
                        {
                            logs.AppendText("Signature Verification was failed \n");
                        }

                    }

                    if (message == "")
                    {
                        server2Connected = false;
                        logs.AppendText("Disconnected from " + user + ". \n");
                    }

                    if (message == "file_replic")
                    {
                        List<byte[]> fileBytesList = new List<byte[]>();
                        logs.AppendText("REPLICKE \n");
                        string filename = "";

                        byte[] AESKey = new byte[16];
                        Array.Copy(aes_key_buffer, 0, AESKey, 0, 16);
                        byte[] AESIV = new byte[16];
                        Array.Copy(aes_iv_buffer, 0, AESIV, 0, 16);

                        logs.AppendText("\n\nKey and IV created for aes128 encryption:\n");
                        logs.AppendText("Key: " + generateHexStringFromByteArray(AESKey) + "\n");
                        logs.AppendText("IV: " + generateHexStringFromByteArray(AESIV) + "\n\n\n");

                        Byte[] inputFileSizeBufferEncrypted = new Byte[16];
                        server2Socket.Receive(inputFileSizeBufferEncrypted);

                        byte[] inputFileSizeBuffer = decryptWithAES128(inputFileSizeBufferEncrypted, AESKey, AESIV);
                        string inputFileSizeString = bytes_to_string(inputFileSizeBuffer);
                        int inputFileSize = Int32.Parse(inputFileSizeString);

                        Byte[] fileNameInputSizeEncrypted = new Byte[16];
                        server2Socket.Receive(fileNameInputSizeEncrypted);
                        byte[] fileNameInputSizeBuffer = decryptWithAES128(fileNameInputSizeEncrypted, AESKey, AESIV);
                        string fileNameInputSizeString = bytes_to_string(fileNameInputSizeBuffer);
                        int fileNameInputSizeInt = Int32.Parse(fileNameInputSizeString);

                        Byte[] fileNameInput = new Byte[fileNameInputSizeInt];
                        server2Socket.Receive(fileNameInput);
                        byte[] filenameByte = decryptWithAES128(fileNameInput, AESKey, AESIV);
                        filename = bytes_to_string(filenameByte);

                        logs.AppendText("Started receiving " + filename + " from " + user + " with size of " + inputFileSizeString + ".\n");

                        if (File.Exists(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename))
                        {
                            File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                        }

                        var stream = new FileStream(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename, FileMode.Append);

                        bool totalVerify = true;
                        bool hmacValue = true;
                        while (true)
                        {
                            byte[] combinedDataInputHeaderEncrypted = new byte[16];
                            server2Socket.Receive(combinedDataInputHeaderEncrypted);
                            byte[] fileInputSizeBuffer = decryptWithAES128(combinedDataInputHeaderEncrypted, AESKey, AESIV);
                            string fileInputSizeString = bytes_to_string(fileInputSizeBuffer);
                            int fileInputSizeInt = Int32.Parse(fileInputSizeString);

                            byte[] combinedDataInput = new byte[fileInputSizeInt];

                            server2Socket.Receive(combinedDataInput);

                            byte[] dataEncrypted = new byte[fileInputSizeInt];
                            Array.Copy(combinedDataInput, 0, dataEncrypted, 0, fileInputSizeInt);

                            byte[] fileContentByte = decryptWithAES128(dataEncrypted, AESKey, AESIV);

                            string fileStringContent = bytes_to_string(fileContentByte);

                            if (bytes_to_string(fileContentByte) == "*--ERR--*")
                            {
                                totalVerify = false;
                                break;
                            }
                            if (bytes_to_string(fileContentByte) == "*--END--*") break;

                            logs.AppendText("Server1 Hmackey from server2: " + generateHexStringFromByteArray(HMAC_buffer) + "\n");

                            byte[] HmacValue = new byte[32];
                            server2Socket.Receive(HmacValue);

                            byte[] HmacServer1 = new byte[32];

                            if (bytes_to_string(HmacValue) == "HMAC_CATCH_ERRORHMAC_CATCH_ERROR")
                            {
                                hmacValue = false;
                                break;
                            }
                            //Byte[] HMAC_buffer2 = string_to_bytes("eeee");

                            try
                            {
                                HmacServer1 = applyHMACwithSHA256(fileContentByte, HMAC_buffer);
                            }
                            catch
                            {
                                hmacValue = false;
                                break;
                            }

                            if (generateHexStringFromByteArray(HmacServer1) != generateHexStringFromByteArray(HmacValue))
                            {
                                hmacValue = false;
                            }

                            

                            stream.Write(fileContentByte, 0, fileContentByte.Length);
                        }

                        stream.Close();

                        if (totalVerify == false)
                        {
                            logs.AppendText("There is an error in verification for the file:" + filename + "\n");
                            File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                        }
                        else if (hmacValue == false)
                        {
                            logs.AppendText("There is an ERROR IN HMAC: \n");
                            if (File.Exists(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename))
                            {
                                File.Delete(Directory.GetCurrentDirectory() + "\\ReceivedFiles\\" + filename);
                            }

                            string messageError = "HmacErrSer2";
                            byte[] messageByte = new byte[11];
                            messageByte = string_to_bytes(messageError);
                            server2Socket.Send(messageByte);

                            byte[] signMessageError = signWithRSA(messageByte, 3072, Server1_pub_priv);
                            server2Socket.Send(signMessageError);
                        }
                        else
                        {
                            logs.AppendText("All file packets for the " + filename + " file was succesfully verified. \n");
                            logs.AppendText(filename + " was succesfully received and stored in the File Sytem. \n");

                            string messageError = "HmacVerSer2";
                            byte[] messageByte = new byte[11];
                            messageByte = string_to_bytes(messageError);
                            server2Socket.Send(messageByte);

                            byte[] signMessageError = signWithRSA(messageByte, 3072, Server1_pub_priv);
                            server2Socket.Send(signMessageError);
                        }

                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("Disconnected from Server2. \n");
                    }

                    connectButton2.Enabled = true;
                    server2Socket.Close();
                    connected_servers.Remove("Server2");
                    server2Connected = false;
                }
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = ipAdress.Text;
            int port;
            if (Int32.TryParse(portNum.Text, out port))
            {
                try
                {
                    socket1.Connect(IP, port);
                    connectButton.Enabled = false;

                    logs.AppendText("Connection is pending...\n");


                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server1_pub_prv.txt"))
                    {
                        Server1_pub_priv = fileReader.ReadLine();
                    }

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server2_pub.txt"))
                    {
                        Server2_pub = fileReader.ReadLine();
                    }

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("MasterServer_pub.txt"))
                    {
                        Master_pub = fileReader.ReadLine();
                    }
                    

                    Byte[] buffer = new Byte[64];
                    buffer = string_to_bytes("server1");
                    socket1.Send(buffer);

                    Byte[] who = new Byte[64];
                    socket1.Receive(who);
                    string whoString = bytes_to_string(who);


                    if(whoString == "server2")
                    {
                        server2Socket = socket1;
                        logs.AppendText("Connected to Server2 \n");

                        Thread serverThread;
                        serverThread = new Thread(new ThreadStart(server2Receive));
                        serverThread.Start();
                    }

                    else
                    {
                        remoteSocket = socket1;
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

                            byte[] decryptedKey = decryptWithRSA(enryptedSessionKey, 3072, Server1_pub_priv);

                        

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

                            ServerSocketList.Add(remoteSocket);
                            AllSocketList.Add(remoteSocket);
                            Users.Add("Master");
                            logs.AppendText("Master Server is connected. \n");

                            connected_servers.Add("master");

                            List<string> sent_files = new List<string>();
                            List<string> sent_files_1 = new List<string>();
                            try
                            {
                                if (connected_servers.Count == 2)
                                {
                                    for (int i = 0; i < server_replicates.Count; i++)
                                    {
                                        send_replicate(remoteSocket, server_replicates[i], Master_pub);
                                        send_replicate(server2Socket, server_replicates[i], Server2_pub);
                                        sent_files.Add(server_replicates[i]);
                                    }
                                }

                                /*
                                for (int i = 0; i < server1_replicates.Count; i++)
                                {                                    
                                    send_replicate(server1Socket, server1_replicates[i], Server1_pub);
                                    sent_files_1.Add(server_replicates[i]);                                
                                } */
                            }
                            finally
                            {
                                for (int i = 0; i < sent_files.Count; i++)
                                {
                                    server_replicates.Remove(sent_files[i]);
                                }

                                /*
                                for (int i = 0; i < sent_files_1.Count; i++)
                                {
                                    server1_replicates.Remove(sent_files_1[i]);
                                }*/
                            }

                            Thread masterThread;
                            masterThread = new Thread(new ThreadStart(MasterReceive));
                            masterThread.Start();
                        }
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

            save_Queue();

            for (int i = 0; i < socketList.Count; i++)
            {
                socketList[i].Close();
            }
            Environment.Exit(0);
        }

        private void connectButton2_Click(object sender, EventArgs e)
        {
            Socket socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = ipAdress2.Text;
            int port;
            if (Int32.TryParse(portNum2.Text, out port))
            {
                try
                {
                    socket2.Connect(IP, port);
                    connectButton2.Enabled = false;

                    logs.AppendText("Connection is pending...\n");


                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server1_pub_prv.txt"))
                    {
                        Server1_pub_priv = fileReader.ReadLine();
                    }

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server2_pub.txt"))
                    {
                        Server2_pub = fileReader.ReadLine();
                    }

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("MasterServer_pub.txt"))
                    {
                        Master_pub = fileReader.ReadLine();
                    }
                    

                    Byte[] buffer = new Byte[64];
                    buffer = string_to_bytes("server1");
                    socket2.Send(buffer);

                    Byte[] who = new Byte[64];
                    socket2.Receive(who);
                    string whoString = bytes_to_string(who);


                    if(whoString == "server2")
                    {
                        server2Socket = socket2;
                        logs.AppendText("Connected to Server2 \n");

                        ServerSocketList.Add(server2Socket);
                        AllSocketList.Add(server2Socket);
                        Users.Add("Server2");
                        logs.AppendText("Server2 is connected. \n");

                        connected_servers.Add("Server2");

                        List<string> sent_files = new List<string>();
                        List<string> sent_files_1 = new List<string>();
                        try
                        {
                            if (connected_servers.Count == 2)
                            {
                                for (int i = 0; i < server_replicates.Count; i++)
                                {
                                    send_replicate(remoteSocket, server_replicates[i], Master_pub);
                                    send_replicate(server2Socket, server_replicates[i], Server2_pub);
                                    sent_files.Add(server_replicates[i]);
                                }
                            }

                            /*
                            for (int i = 0; i < server1_replicates.Count; i++)
                            {                                    
                                send_replicate(server1Socket, server1_replicates[i], Server1_pub);
                                sent_files_1.Add(server_replicates[i]);                                
                            } */
                        }
                        finally
                        {
                            for (int i = 0; i < sent_files.Count; i++)
                            {
                                server_replicates.Remove(sent_files[i]);
                            }

                            /*
                            for (int i = 0; i < sent_files_1.Count; i++)
                            {
                                server1_replicates.Remove(sent_files_1[i]);
                            }*/
                        }


                        Thread serverThread;
                        serverThread = new Thread(new ThreadStart(server2Receive));
                        serverThread.Start();
                    }

                    else
                    {
                        remoteSocket = socket2;
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

                            byte[] decryptedKey = decryptWithRSA(enryptedSessionKey, 3072, Server1_pub_priv);

                        

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
    }
}
