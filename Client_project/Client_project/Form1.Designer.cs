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
            this.send_button = new System.Windows.Forms.Button();
            this.IP = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.message_textBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.upload_button = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(53, 102);
            this.connect_button.Margin = new System.Windows.Forms.Padding(4);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(100, 28);
            this.connect_button.TabIndex = 0;
            this.connect_button.Text = "Connect";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.button1_Click);
            // 
            // send_button
            // 
            this.send_button.Location = new System.Drawing.Point(53, 210);
            this.send_button.Margin = new System.Windows.Forms.Padding(4);
            this.send_button.Name = "send_button";
            this.send_button.Size = new System.Drawing.Size(100, 28);
            this.send_button.TabIndex = 1;
            this.send_button.Text = "Send";
            this.send_button.UseVisualStyleBackColor = true;
            this.send_button.Click += new System.EventHandler(this.send_button_Click);
            // 
            // IP
            // 
            this.IP.Location = new System.Drawing.Point(53, 26);
            this.IP.Margin = new System.Windows.Forms.Padding(4);
            this.IP.Name = "IP";
            this.IP.Size = new System.Drawing.Size(132, 22);
            this.IP.TabIndex = 2;
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(53, 58);
            this.port.Margin = new System.Windows.Forms.Padding(4);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(132, 22);
            this.port.TabIndex = 3;
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(195, 26);
            this.logs.Margin = new System.Windows.Forms.Padding(4);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(167, 267);
            this.logs.TabIndex = 4;
            this.logs.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "IP:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 62);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Port:";
            // 
            // message_textBox
            // 
            this.message_textBox.Location = new System.Drawing.Point(77, 178);
            this.message_textBox.Margin = new System.Windows.Forms.Padding(4);
            this.message_textBox.Name = "message_textBox";
            this.message_textBox.Size = new System.Drawing.Size(108, 22);
            this.message_textBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-1, 182);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "Message:";
            // 
            // upload_button
            // 
            this.upload_button.Enabled = false;
            this.upload_button.Location = new System.Drawing.Point(19, 270);
            this.upload_button.Name = "upload_button";
            this.upload_button.Size = new System.Drawing.Size(96, 23);
            this.upload_button.TabIndex = 9;
            this.upload_button.Text = "Send File";
            this.upload_button.UseVisualStyleBackColor = true;
            this.upload_button.Click += new System.EventHandler(this.upload_button_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 321);
            this.Controls.Add(this.upload_button);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.message_textBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.port);
            this.Controls.Add(this.IP);
            this.Controls.Add(this.send_button);
            this.Controls.Add(this.connect_button);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.Button send_button;
        private System.Windows.Forms.TextBox IP;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox message_textBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button upload_button;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

