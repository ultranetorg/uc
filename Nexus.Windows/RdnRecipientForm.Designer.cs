namespace Uccs.Nexus.Windows
{
	partial class RdnRecipientForm
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
			Cancel = new Button();
			Ok = new Button();
			EntityClass = new ComboBox();
			Entity = new TextBox();
			label1 = new Label();
			label2 = new Label();
			SuspendLayout();
			// 
			// Cancel
			// 
			Cancel.DialogResult = DialogResult.Cancel;
			Cancel.Location = new Point(381, 195);
			Cancel.Margin = new Padding(6);
			Cancel.Name = "Cancel";
			Cancel.Size = new Size(240, 60);
			Cancel.TabIndex = 3;
			Cancel.Text = "Cancel";
			Cancel.UseVisualStyleBackColor = true;
			Cancel.Click += cancel_Click;
			// 
			// Ok
			// 
			Ok.Location = new Point(129, 195);
			Ok.Margin = new Padding(6);
			Ok.Name = "Ok";
			Ok.Size = new Size(240, 60);
			Ok.TabIndex = 2;
			Ok.Text = "OK";
			Ok.UseVisualStyleBackColor = true;
			Ok.Click += ok_Click;
			// 
			// EntityClass
			// 
			EntityClass.DropDownStyle = ComboBoxStyle.DropDownList;
			EntityClass.FormattingEnabled = true;
			EntityClass.Location = new Point(236, 48);
			EntityClass.Margin = new Padding(6, 13, 6, 13);
			EntityClass.Name = "EntityClass";
			EntityClass.Size = new Size(385, 40);
			EntityClass.TabIndex = 16;
			// 
			// Entity
			// 
			Entity.Location = new Point(236, 114);
			Entity.Margin = new Padding(6, 13, 6, 13);
			Entity.Name = "Entity";
			Entity.Size = new Size(385, 39);
			Entity.TabIndex = 21;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(40, 51);
			label1.Margin = new Padding(6, 0, 6, 0);
			label1.Name = "label1";
			label1.Size = new Size(184, 32);
			label1.TabIndex = 19;
			label1.Text = "Recipient Class";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(74, 117);
			label2.Margin = new Padding(6, 0, 6, 0);
			label2.Name = "label2";
			label2.Size = new Size(150, 32);
			label2.TabIndex = 19;
			label2.Text = "Recipient Id";
			// 
			// RdnRecipientForm
			// 
			AcceptButton = Ok;
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = Cancel;
			ClientSize = new Size(668, 306);
			ControlBox = false;
			Controls.Add(Entity);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(EntityClass);
			Controls.Add(Ok);
			Controls.Add(Cancel);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Margin = new Padding(6);
			Name = "RdnRecipientForm";
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Recipient in RDN";
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		public System.Windows.Forms.TextBox gasprice;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.Button Ok;
		private ComboBox EntityClass;
		private TextBox Entity;
		private Label label1;
		private Label label2;
	}
}