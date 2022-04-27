
namespace UC.Net.Node.FUI
{
	partial class PublishPanel
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Manifest = new System.Windows.Forms.TextBox();
			this.Publish = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.Account = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// Manifest
			// 
			this.Manifest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Manifest.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Manifest.Location = new System.Drawing.Point(98, 51);
			this.Manifest.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
			this.Manifest.Multiline = true;
			this.Manifest.Name = "Manifest";
			this.Manifest.Size = new System.Drawing.Size(925, 716);
			this.Manifest.TabIndex = 11;
			// 
			// Publish
			// 
			this.Publish.Location = new System.Drawing.Point(465, 11);
			this.Publish.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Publish.Name = "Publish";
			this.Publish.Size = new System.Drawing.Size(117, 27);
			this.Publish.TabIndex = 3;
			this.Publish.Text = "Publish";
			this.Publish.UseVisualStyleBackColor = true;
			this.Publish.Click += new System.EventHandler(this.Publish_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(19, 52);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 13);
			this.label3.TabIndex = 13;
			this.label3.Text = "Manifest";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(19, 18);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(53, 13);
			this.label1.TabIndex = 18;
			this.label1.Text = "Account";
			// 
			// Account
			// 
			this.Account.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Account.FormattingEnabled = true;
			this.Account.Location = new System.Drawing.Point(98, 14);
			this.Account.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
			this.Account.Name = "Account";
			this.Account.Size = new System.Drawing.Size(349, 23);
			this.Account.TabIndex = 19;
			// 
			// PublishPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Account);
			this.Controls.Add(this.Manifest);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.Publish);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "PublishPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox Manifest;
		private System.Windows.Forms.Button Publish;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox Account;
	}
}
