namespace Uccs.Nexus.Windows
{
	partial class AuthorizationForm
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
			if(disposing && (components != null))
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthorizationForm));
			Reject = new Button();
			groupBox1 = new GroupBox();
			Application = new Label();
			Logo = new PictureBox();
			label2 = new Label();
			label3 = new Label();
			label4 = new Label();
			Ask = new Button();
			Operation = new TextBox();
			label1 = new Label();
			Signer = new Label();
			Net = new Label();
			label6 = new Label();
			Shield = new PictureBox();
			label5 = new Label();
			User = new Label();
			((System.ComponentModel.ISupportInitialize)Logo).BeginInit();
			((System.ComponentModel.ISupportInitialize)Shield).BeginInit();
			SuspendLayout();
			// 
			// Reject
			// 
			Reject.DialogResult = DialogResult.Cancel;
			Reject.Location = new Point(311, 373);
			Reject.Margin = new Padding(9);
			Reject.Name = "Reject";
			Reject.Size = new Size(128, 32);
			Reject.TabIndex = 0;
			Reject.Text = "Reject";
			Reject.UseVisualStyleBackColor = true;
			Reject.Click += Reject_Click;
			// 
			// groupBox1
			// 
			groupBox1.Location = new Point(19, 346);
			groupBox1.Margin = new Padding(9);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(420, 9);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			// 
			// Application
			// 
			Application.AutoSize = true;
			Application.ImageAlign = ContentAlignment.TopLeft;
			Application.Location = new Point(133, 130);
			Application.Margin = new Padding(3, 6, 3, 6);
			Application.Name = "Application";
			Application.Size = new Size(108, 15);
			Application.TabIndex = 4;
			Application.Text = "<Allication Name>";
			Application.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Logo
			// 
			Logo.Image = (Image)resources.GetObject("Logo.Image");
			Logo.Location = new Point(19, 12);
			Logo.Name = "Logo";
			Logo.Size = new Size(108, 86);
			Logo.SizeMode = PictureBoxSizeMode.Zoom;
			Logo.TabIndex = 6;
			Logo.TabStop = false;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(84, 211);
			label2.Name = "label2";
			label2.Size = new Size(43, 15);
			label2.TabIndex = 8;
			label2.Text = "Signer";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label3.Location = new Point(58, 130);
			label3.Name = "label3";
			label3.Size = new Size(69, 15);
			label3.TabIndex = 8;
			label3.Text = "Application";
			// 
			// label4
			// 
			label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
			label4.ImageAlign = ContentAlignment.TopLeft;
			label4.Location = new Point(133, 12);
			label4.Name = "label4";
			label4.Size = new Size(306, 86);
			label4.TabIndex = 4;
			label4.Text = "The following website or application requests your approval on the following transaction";
			label4.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Ask
			// 
			Ask.Location = new Point(165, 373);
			Ask.Margin = new Padding(9);
			Ask.Name = "Ask";
			Ask.Size = new Size(128, 32);
			Ask.TabIndex = 3;
			Ask.Text = "Approve";
			Ask.UseVisualStyleBackColor = true;
			Ask.Click += Allow_Click;
			// 
			// Operation
			// 
			Operation.Location = new Point(133, 238);
			Operation.Margin = new Padding(3, 6, 3, 6);
			Operation.Multiline = true;
			Operation.Name = "Operation";
			Operation.ReadOnly = true;
			Operation.Size = new Size(306, 86);
			Operation.TabIndex = 9;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(51, 241);
			label1.Name = "label1";
			label1.Size = new Size(76, 15);
			label1.TabIndex = 8;
			label1.Text = "Operation(s)";
			// 
			// Signer
			// 
			Signer.AutoSize = true;
			Signer.ImageAlign = ContentAlignment.TopLeft;
			Signer.Location = new Point(133, 211);
			Signer.Margin = new Padding(3, 6, 3, 6);
			Signer.Name = "Signer";
			Signer.Size = new Size(270, 15);
			Signer.TabIndex = 4;
			Signer.Text = "0xD061eeCb93844A8cAbea06D80976d5958f48a343";
			Signer.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// Net
			// 
			Net.AutoSize = true;
			Net.ImageAlign = ContentAlignment.TopLeft;
			Net.Location = new Point(133, 157);
			Net.Margin = new Padding(3, 6, 3, 6);
			Net.Name = "Net";
			Net.Size = new Size(40, 15);
			Net.TabIndex = 4;
			Net.Text = "<net>";
			Net.TextAlign = ContentAlignment.MiddleCenter;
			Net.Click += Net_Click;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label6.Location = new Point(99, 157);
			label6.Name = "label6";
			label6.Size = new Size(28, 15);
			label6.TabIndex = 8;
			label6.Text = "Net";
			// 
			// Shield
			// 
			Shield.Image = (Image)resources.GetObject("Shield.Image");
			Shield.Location = new Point(19, 12);
			Shield.Name = "Shield";
			Shield.Size = new Size(108, 86);
			Shield.SizeMode = PictureBoxSizeMode.Zoom;
			Shield.TabIndex = 10;
			Shield.TabStop = false;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label5.Location = new Point(99, 184);
			label5.Name = "label5";
			label5.Size = new Size(33, 15);
			label5.TabIndex = 8;
			label5.Text = "User";
			// 
			// User
			// 
			User.AutoSize = true;
			User.ImageAlign = ContentAlignment.TopLeft;
			User.Location = new Point(133, 184);
			User.Margin = new Padding(3, 6, 3, 6);
			User.Name = "User";
			User.Size = new Size(45, 15);
			User.TabIndex = 4;
			User.Text = "<user>";
			User.TextAlign = ContentAlignment.MiddleCenter;
			User.Click += Net_Click;
			// 
			// AuthorizationForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			CancelButton = Reject;
			ClientSize = new Size(462, 423);
			ControlBox = false;
			Controls.Add(Shield);
			Controls.Add(Operation);
			Controls.Add(label1);
			Controls.Add(label2);
			Controls.Add(label5);
			Controls.Add(label6);
			Controls.Add(label3);
			Controls.Add(Logo);
			Controls.Add(groupBox1);
			Controls.Add(label4);
			Controls.Add(User);
			Controls.Add(Net);
			Controls.Add(Signer);
			Controls.Add(Application);
			Controls.Add(Ask);
			Controls.Add(Reject);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Name = "AuthorizationForm";
			Padding = new Padding(0, 0, 0, 9);
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Authorization Required";
			((System.ComponentModel.ISupportInitialize)Logo).EndInit();
			((System.ComponentModel.ISupportInitialize)Shield).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Button Reject;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label Application;
		private System.Windows.Forms.PictureBox Logo;
		private Label label2;
		private Label label3;
		private Label label4;
		private Button Ask;
		private TextBox Operation;
		private Label label1;
		private Label Signer;
		private Label Net;
		private Label label6;
		private PictureBox Shield;
		private Label label5;
		private Label User;
	}
}