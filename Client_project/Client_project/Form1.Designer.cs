namespace Client_project
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.connect_button = new System.Windows.Forms.Button();
            this.IP = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.upload_button = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button_disconnect = new System.Windows.Forms.Button();
            this.download_button = new System.Windows.Forms.Button();
            this.downloadFile = new System.Windows.Forms.TextBox();
            this.textBox_username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(40, 106);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(75, 23);
            this.connect_button.TabIndex = 0;
            this.connect_button.Text = "Connect";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.button1_Click);
            // 
            // IP
            // 
            this.IP.Location = new System.Drawing.Point(40, 21);
            this.IP.Name = "IP";
            this.IP.Size = new System.Drawing.Size(100, 20);
            this.IP.TabIndex = 2;
            this.IP.TextChanged += new System.EventHandler(this.IP_TextChanged);
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(40, 47);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(100, 20);
            this.port.TabIndex = 3;
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(146, 21);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(209, 218);
            this.logs.TabIndex = 4;
            this.logs.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "IP:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Port:";
            // 
            // upload_button
            // 
            this.upload_button.Enabled = false;
            this.upload_button.Location = new System.Drawing.Point(15, 173);
            this.upload_button.Margin = new System.Windows.Forms.Padding(2);
            this.upload_button.Name = "upload_button";
            this.upload_button.Size = new System.Drawing.Size(72, 19);
            this.upload_button.TabIndex = 9;
            this.upload_button.Text = "Send File";
            this.upload_button.UseVisualStyleBackColor = true;
            this.upload_button.Click += new System.EventHandler(this.upload_button_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button_disconnect
            // 
            this.button_disconnect.Enabled = false;
            this.button_disconnect.Location = new System.Drawing.Point(40, 134);
            this.button_disconnect.Margin = new System.Windows.Forms.Padding(2);
            this.button_disconnect.Name = "button_disconnect";
            this.button_disconnect.Size = new System.Drawing.Size(75, 23);
            this.button_disconnect.TabIndex = 10;
            this.button_disconnect.Text = "Disconnect";
            this.button_disconnect.UseVisualStyleBackColor = true;
            this.button_disconnect.Click += new System.EventHandler(this.button_disconnect_Click);
            // 
            // download_button
            // 
            this.download_button.Location = new System.Drawing.Point(15, 237);
            this.download_button.Margin = new System.Windows.Forms.Padding(2);
            this.download_button.Name = "download_button";
            this.download_button.Size = new System.Drawing.Size(72, 19);
            this.download_button.TabIndex = 11;
            this.download_button.Text = "Download";
            this.download_button.UseVisualStyleBackColor = true;
            this.download_button.Click += new System.EventHandler(this.download_button_Click);
            // 
            // downloadFile
            // 
            this.downloadFile.Location = new System.Drawing.Point(15, 205);
            this.downloadFile.Margin = new System.Windows.Forms.Padding(2);
            this.downloadFile.Name = "downloadFile";
            this.downloadFile.Size = new System.Drawing.Size(76, 20);
            this.downloadFile.TabIndex = 12;
            // 
            // textBox_username
            // 
            this.textBox_username.Location = new System.Drawing.Point(58, 80);
            this.textBox_username.Name = "textBox_username";
            this.textBox_username.Size = new System.Drawing.Size(82, 20);
            this.textBox_username.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-1, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Username";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 261);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_username);
            this.Controls.Add(this.downloadFile);
            this.Controls.Add(this.download_button);
            this.Controls.Add(this.button_disconnect);
            this.Controls.Add(this.upload_button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.port);
            this.Controls.Add(this.IP);
            this.Controls.Add(this.connect_button);
            this.Name = "Form1";
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.TextBox IP;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button upload_button;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button_disconnect;
        private System.Windows.Forms.Button download_button;
        private System.Windows.Forms.TextBox downloadFile;
        private System.Windows.Forms.TextBox textBox_username;
        private System.Windows.Forms.Label label3;
    }
}

