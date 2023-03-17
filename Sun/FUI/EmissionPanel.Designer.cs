
namespace UC.Sun.FUI
{
	partial class EmissionPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmissionPanel));
			this.transfergroup = new System.Windows.Forms.GroupBox();
			this.eth = new UC.Sun.FUI.CoinEdit();
			this.privatekeyChoice = new System.Windows.Forms.RadioButton();
			this.walletChoice = new System.Windows.Forms.RadioButton();
			this.label8 = new System.Windows.Forms.Label();
			this.transfer = new System.Windows.Forms.Button();
			this.privatekey = new System.Windows.Forms.TextBox();
			this.source = new System.Windows.Forms.TextBox();
			this.browse = new System.Windows.Forms.Button();
			this.destination = new System.Windows.Forms.ComboBox();
			this.DestLabel = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.estimated = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.finishgroup = new System.Windows.Forms.GroupBox();
			this.Unfinished = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.RefreshUnfinished = new System.Windows.Forms.Button();
			this.finish = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.transfergroup.SuspendLayout();
			this.finishgroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// transfergroup
			// 
			this.transfergroup.Controls.Add(this.eth);
			this.transfergroup.Controls.Add(this.privatekeyChoice);
			this.transfergroup.Controls.Add(this.walletChoice);
			this.transfergroup.Controls.Add(this.label8);
			this.transfergroup.Controls.Add(this.transfer);
			this.transfergroup.Controls.Add(this.privatekey);
			this.transfergroup.Controls.Add(this.source);
			this.transfergroup.Controls.Add(this.browse);
			this.transfergroup.Controls.Add(this.destination);
			this.transfergroup.Controls.Add(this.DestLabel);
			this.transfergroup.Controls.Add(this.label5);
			this.transfergroup.Controls.Add(this.label2);
			this.transfergroup.Controls.Add(this.label11);
			this.transfergroup.Controls.Add(this.estimated);
			this.transfergroup.Controls.Add(this.label6);
			this.transfergroup.Location = new System.Drawing.Point(0, 0);
			this.transfergroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.transfergroup.Name = "transfergroup";
			this.transfergroup.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.transfergroup.Size = new System.Drawing.Size(623, 325);
			this.transfergroup.TabIndex = 6;
			this.transfergroup.TabStop = false;
			this.transfergroup.Text = "ETH to UNT Transfer ";
			// 
			// eth
			// 
			this.eth.Location = new System.Drawing.Point(237, 196);
			this.eth.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.eth.Name = "eth";
			this.eth.Size = new System.Drawing.Size(124, 23);
			this.eth.TabIndex = 11;
			this.eth.Text = "0.000123";
			this.eth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.eth.TextChanged += new System.EventHandler(this.eth_TextChanged);
			// 
			// privatekeyChoice
			// 
			this.privatekeyChoice.AutoSize = true;
			this.privatekeyChoice.Location = new System.Drawing.Point(237, 119);
			this.privatekeyChoice.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.privatekeyChoice.Name = "privatekeyChoice";
			this.privatekeyChoice.Size = new System.Drawing.Size(114, 19);
			this.privatekeyChoice.TabIndex = 10;
			this.privatekeyChoice.TabStop = true;
			this.privatekeyChoice.Text = "From Private Key";
			this.privatekeyChoice.UseVisualStyleBackColor = true;
			this.privatekeyChoice.CheckedChanged += new System.EventHandler(this.sourceChoice_CheckedChanged);
			// 
			// walletChoice
			// 
			this.walletChoice.AutoSize = true;
			this.walletChoice.Location = new System.Drawing.Point(237, 81);
			this.walletChoice.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.walletChoice.Name = "walletChoice";
			this.walletChoice.Size = new System.Drawing.Size(110, 19);
			this.walletChoice.TabIndex = 10;
			this.walletChoice.TabStop = true;
			this.walletChoice.Text = "From Wallet File";
			this.walletChoice.UseVisualStyleBackColor = true;
			this.walletChoice.CheckedChanged += new System.EventHandler(this.sourceChoice_CheckedChanged);
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label8.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label8.Location = new System.Drawing.Point(24, -618);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(818, 95);
			this.label8.TabIndex = 8;
			this.label8.Text = resources.GetString("label8.Text");
			// 
			// transfer
			// 
			this.transfer.Location = new System.Drawing.Point(421, 252);
			this.transfer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.transfer.Name = "transfer";
			this.transfer.Size = new System.Drawing.Size(166, 27);
			this.transfer.TabIndex = 2;
			this.transfer.Text = "Transfer";
			this.transfer.UseVisualStyleBackColor = true;
			this.transfer.Click += new System.EventHandler(this.transfer_Click);
			// 
			// privatekey
			// 
			this.privatekey.Location = new System.Drawing.Point(370, 118);
			this.privatekey.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.privatekey.Name = "privatekey";
			this.privatekey.Size = new System.Drawing.Size(216, 23);
			this.privatekey.TabIndex = 7;
			this.privatekey.TextChanged += new System.EventHandler(this.walletORprivatekey_TextChanged);
			// 
			// source
			// 
			this.source.Location = new System.Drawing.Point(237, 39);
			this.source.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.source.Name = "source";
			this.source.ReadOnly = true;
			this.source.Size = new System.Drawing.Size(349, 23);
			this.source.TabIndex = 7;
			this.source.TextChanged += new System.EventHandler(this.source_TextChanged);
			// 
			// browse
			// 
			this.browse.Location = new System.Drawing.Point(370, 77);
			this.browse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.browse.Name = "browse";
			this.browse.Size = new System.Drawing.Size(217, 27);
			this.browse.TabIndex = 6;
			this.browse.Text = "Browse ...";
			this.browse.UseVisualStyleBackColor = true;
			this.browse.Click += new System.EventHandler(this.transferBrowse_Click);
			// 
			// destination
			// 
			this.destination.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.destination.FormattingEnabled = true;
			this.destination.Location = new System.Drawing.Point(237, 158);
			this.destination.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.destination.Name = "destination";
			this.destination.Size = new System.Drawing.Size(280, 23);
			this.destination.TabIndex = 3;
			// 
			// DestLabel
			// 
			this.DestLabel.AutoSize = true;
			this.DestLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.DestLabel.Location = new System.Drawing.Point(47, 43);
			this.DestLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.DestLabel.Name = "DestLabel";
			this.DestLabel.Size = new System.Drawing.Size(153, 13);
			this.DestLabel.TabIndex = 1;
			this.DestLabel.Text = "Source Ethereum Account";
			this.DestLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(26, 162);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(171, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "Destination Ultranet Account";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(49, 234);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(152, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Estimated UNT to Receive";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label11.Location = new System.Drawing.Point(372, 200);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(26, 13);
			this.label11.TabIndex = 1;
			this.label11.Text = "ETH";
			// 
			// estimated
			// 
			this.estimated.AutoSize = true;
			this.estimated.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.estimated.Location = new System.Drawing.Point(237, 234);
			this.estimated.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.estimated.Name = "estimated";
			this.estimated.Size = new System.Drawing.Size(68, 13);
			this.estimated.TabIndex = 1;
			this.estimated.Text = "-.------   UNT";
			this.estimated.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label6.Location = new System.Drawing.Point(164, 200);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(52, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "Amount";
			// 
			// label9
			// 
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(649, 39);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(265, 240);
			this.label9.TabIndex = 9;
			this.label9.Text = "WARNING\r\n\r\nETH coins you transfer will not be stored on any accounts and will be " +
    "burned forever and there is no way to recover them since UNT tokens are not back" +
    "ed by ETH but replace them.";
			// 
			// finishgroup
			// 
			this.finishgroup.Controls.Add(this.Unfinished);
			this.finishgroup.Controls.Add(this.RefreshUnfinished);
			this.finishgroup.Controls.Add(this.finish);
			this.finishgroup.Controls.Add(this.label1);
			this.finishgroup.Location = new System.Drawing.Point(0, 332);
			this.finishgroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.finishgroup.Name = "finishgroup";
			this.finishgroup.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.finishgroup.Size = new System.Drawing.Size(623, 436);
			this.finishgroup.TabIndex = 10;
			this.finishgroup.TabStop = false;
			this.finishgroup.Text = "Finish Incomleted Transfer";
			// 
			// Unfinished
			// 
			this.Unfinished.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.Unfinished.HideSelection = false;
			this.Unfinished.Location = new System.Drawing.Point(26, 60);
			this.Unfinished.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Unfinished.Name = "Unfinished";
			this.Unfinished.Size = new System.Drawing.Size(560, 164);
			this.Unfinished.TabIndex = 4;
			this.Unfinished.UseCompatibleStateImageBehavior = false;
			this.Unfinished.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Account";
			this.columnHeader1.Width = 300;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Amount (ETH)";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader2.Width = 150;
			// 
			// RefreshUnfinished
			// 
			this.RefreshUnfinished.Location = new System.Drawing.Point(26, 249);
			this.RefreshUnfinished.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.RefreshUnfinished.Name = "RefreshUnfinished";
			this.RefreshUnfinished.Size = new System.Drawing.Size(166, 27);
			this.RefreshUnfinished.TabIndex = 2;
			this.RefreshUnfinished.Text = "Refresh";
			this.RefreshUnfinished.UseVisualStyleBackColor = true;
			// 
			// finish
			// 
			this.finish.Location = new System.Drawing.Point(421, 249);
			this.finish.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.finish.Name = "finish";
			this.finish.Size = new System.Drawing.Size(166, 27);
			this.finish.TabIndex = 2;
			this.finish.Text = "Finish";
			this.finish.UseVisualStyleBackColor = true;
			this.finish.Click += new System.EventHandler(this.finish_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(26, 31);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(123, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Unfinished Transfers";
			// 
			// EmissionPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.finishgroup);
			this.Controls.Add(this.transfergroup);
			this.Controls.Add(this.label9);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "EmissionPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.transfergroup.ResumeLayout(false);
			this.transfergroup.PerformLayout();
			this.finishgroup.ResumeLayout(false);
			this.finishgroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox transfergroup;
		private System.Windows.Forms.Button transfer;
		private System.Windows.Forms.TextBox source;
		private System.Windows.Forms.Button browse;
		private System.Windows.Forms.ComboBox destination;
		private System.Windows.Forms.Label DestLabel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label estimated;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.RadioButton privatekeyChoice;
		private System.Windows.Forms.RadioButton walletChoice;
		private System.Windows.Forms.TextBox privatekey;
		private System.Windows.Forms.GroupBox finishgroup;
		private System.Windows.Forms.Button finish;
		private FUI.CoinEdit eth;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Unfinished;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button RefreshUnfinished;
	}
}
