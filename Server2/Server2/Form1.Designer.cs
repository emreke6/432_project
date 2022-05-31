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
            this.SuspendLayout();
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(237, 12);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(328, 260);
            this.logs.TabIndex = 0;
            this.logs.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port:";
            // 
            // clientPort
            // 
            this.clientPort.Location = new System.Drawing.Point(63, 29);
            this.clientPort.Name = "clientPort";
            this.clientPort.Size = new System.Drawing.Size(148, 22);
            this.clientPort.TabIndex = 2;
            // 
            // listenButton
            // 
            this.listenButton.Location = new System.Drawing.Point(74, 58);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(75, 23);
            this.listenButton.TabIndex = 3;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            this.listenButton.Click += new System.EventHandler(this.listenButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "IP:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 169);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Port:";
            // 
            // ipAdress
            // 
            this.ipAdress.Location = new System.Drawing.Point(63, 132);
            this.ipAdress.Name = "ipAdress";
            this.ipAdress.Size = new System.Drawing.Size(148, 22);
            this.ipAdress.TabIndex = 6;
            this.ipAdress.TextChanged += new System.EventHandler(this.ipAdress_TextChanged);
            // 
            // portNum
            // 
            this.portNum.Location = new System.Drawing.Point(63, 163);
            this.portNum.Name = "portNum";
            this.portNum.Size = new System.Drawing.Size(148, 22);
            this.portNum.TabIndex = 7;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(29, 205);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 8;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 284);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.portNum);
            this.Controls.Add(this.ipAdress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listenButton);
            this.Controls.Add(this.clientPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logs);
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
    }
}

