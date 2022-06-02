namespace Server2
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
            this.logs = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.clientPort = new System.Windows.Forms.TextBox();
            this.listenButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ipAdress = new System.Windows.Forms.TextBox();
            this.portNum = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ipAdress2 = new System.Windows.Forms.TextBox();
            this.portNum2 = new System.Windows.Forms.TextBox();
            this.connectButton2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(175, 11);
            this.logs.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(247, 234);
            this.logs.TabIndex = 0;
            this.logs.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port:";
            // 
            // clientPort
            // 
            this.clientPort.Location = new System.Drawing.Point(47, 24);
            this.clientPort.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.clientPort.Name = "clientPort";
            this.clientPort.Size = new System.Drawing.Size(112, 20);
            this.clientPort.TabIndex = 2;
            // 
            // listenButton
            // 
            this.listenButton.Location = new System.Drawing.Point(56, 47);
            this.listenButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(56, 19);
            this.listenButton.TabIndex = 3;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            this.listenButton.Click += new System.EventHandler(this.listenButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 95);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "IP:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 123);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Port:";
            // 
            // ipAdress
            // 
            this.ipAdress.Location = new System.Drawing.Point(47, 92);
            this.ipAdress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ipAdress.Name = "ipAdress";
            this.ipAdress.Size = new System.Drawing.Size(112, 20);
            this.ipAdress.TabIndex = 6;
            // 
            // portNum
            // 
            this.portNum.Location = new System.Drawing.Point(47, 123);
            this.portNum.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.portNum.Name = "portNum";
            this.portNum.Size = new System.Drawing.Size(112, 20);
            this.portNum.TabIndex = 7;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(47, 147);
            this.connectButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(56, 19);
            this.connectButton.TabIndex = 8;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 183);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "IP:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 211);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Port:";
            // 
            // ipAdress2
            // 
            this.ipAdress2.Location = new System.Drawing.Point(47, 180);
            this.ipAdress2.Margin = new System.Windows.Forms.Padding(2);
            this.ipAdress2.Name = "ipAdress2";
            this.ipAdress2.Size = new System.Drawing.Size(112, 20);
            this.ipAdress2.TabIndex = 11;
            // 
            // portNum2
            // 
            this.portNum2.Location = new System.Drawing.Point(47, 204);
            this.portNum2.Margin = new System.Windows.Forms.Padding(2);
            this.portNum2.Name = "portNum2";
            this.portNum2.Size = new System.Drawing.Size(112, 20);
            this.portNum2.TabIndex = 12;
            // 
            // connectButton2
            // 
            this.connectButton2.Location = new System.Drawing.Point(47, 225);
            this.connectButton2.Margin = new System.Windows.Forms.Padding(2);
            this.connectButton2.Name = "connectButton2";
            this.connectButton2.Size = new System.Drawing.Size(56, 19);
            this.connectButton2.TabIndex = 13;
            this.connectButton2.Text = "Connect";
            this.connectButton2.UseVisualStyleBackColor = true;
            this.connectButton2.Click += new System.EventHandler(this.connectButton2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 255);
            this.Controls.Add(this.connectButton2);
            this.Controls.Add(this.portNum2);
            this.Controls.Add(this.ipAdress2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.portNum);
            this.Controls.Add(this.ipAdress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listenButton);
            this.Controls.Add(this.clientPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logs);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Server2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox clientPort;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ipAdress;
        private System.Windows.Forms.TextBox portNum;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ipAdress2;
        private System.Windows.Forms.TextBox portNum2;
        private System.Windows.Forms.Button connectButton2;
    }
}

