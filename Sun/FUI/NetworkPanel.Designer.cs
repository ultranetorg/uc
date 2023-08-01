
namespace Uccs.Sun.FUI
{
	partial class NetworkPanel
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
			Members = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			DestLabel = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			Funds = new System.Windows.Forms.ListView();
			columnHeader9 = new System.Windows.Forms.ColumnHeader();
			label3 = new System.Windows.Forms.Label();
			Peers = new System.Windows.Forms.ListView();
			columnHeader13 = new System.Windows.Forms.ColumnHeader();
			columnHeader14 = new System.Windows.Forms.ColumnHeader();
			columnHeader15 = new System.Windows.Forms.ColumnHeader();
			columnHeader11 = new System.Windows.Forms.ColumnHeader();
			columnHeaderCR = new System.Windows.Forms.ColumnHeader();
			columnHeaderBR = new System.Windows.Forms.ColumnHeader();
			columnHeader16 = new System.Windows.Forms.ColumnHeader();
			SuspendLayout();
			// 
			// Generators
			// 
			Members.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Members.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader4, columnHeader3, columnHeader2, columnHeader5 });
			Members.FullRowSelect = true;
			Members.Location = new System.Drawing.Point(7, 860);
			Members.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Members.Name = "Generators";
			Members.Size = new System.Drawing.Size(1199, 772);
			Members.TabIndex = 1;
			Members.UseCompatibleStateImageBehavior = false;
			Members.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Account";
			columnHeader1.Width = 300;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Joined At";
			columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader4.Width = 100;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Bail";
			columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader3.Width = 150;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Base IP";
			columnHeader2.Width = 100;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "Hub IP";
			columnHeader5.Width = 100;
			// 
			// DestLabel
			// 
			DestLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			DestLabel.AutoSize = true;
			DestLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			DestLabel.Location = new System.Drawing.Point(7, 819);
			DestLabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			DestLabel.Name = "DestLabel";
			DestLabel.Size = new System.Drawing.Size(114, 27);
			DestLabel.TabIndex = 2;
			DestLabel.Text = "Members";
			DestLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(7, 33);
			label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(73, 27);
			label1.TabIndex = 2;
			label1.Text = "Peers";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Funds
			// 
			Funds.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			Funds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader9 });
			Funds.FullRowSelect = true;
			Funds.Location = new System.Drawing.Point(1239, 856);
			Funds.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Funds.Name = "Funds";
			Funds.Size = new System.Drawing.Size(656, 776);
			Funds.TabIndex = 5;
			Funds.UseCompatibleStateImageBehavior = false;
			Funds.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader9
			// 
			columnHeader9.Text = "Account";
			columnHeader9.Width = 300;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label3.Location = new System.Drawing.Point(1239, 819);
			label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(78, 27);
			label3.TabIndex = 2;
			label3.Text = "Funds";
			label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Peers
			// 
			Peers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Peers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader13, columnHeader14, columnHeader15, columnHeader11, columnHeaderBR, columnHeaderCR, columnHeader16 });
			Peers.FullRowSelect = true;
			Peers.Location = new System.Drawing.Point(7, 83);
			Peers.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Peers.Name = "Peers";
			Peers.Size = new System.Drawing.Size(1888, 697);
			Peers.TabIndex = 6;
			Peers.UseCompatibleStateImageBehavior = false;
			Peers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader13
			// 
			columnHeader13.Text = "IP";
			columnHeader13.Width = 100;
			// 
			// columnHeader14
			// 
			columnHeader14.Text = "Status";
			columnHeader14.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			columnHeader14.Width = 150;
			// 
			// columnHeader15
			// 
			columnHeader15.Text = "Retries";
			columnHeader15.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			columnHeader15.Width = 75;
			// 
			// columnHeader11
			// 
			columnHeader11.Text = "Peer Rank";
			columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			columnHeader11.Width = 100;
			// 
			// columnHeaderCR
			// 
			columnHeaderCR.DisplayIndex = 4;
			columnHeaderCR.Text = "Chain Rank";
			columnHeaderCR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			columnHeaderCR.Width = 100;
			// 
			// columnHeaderBR
			// 
			columnHeaderBR.DisplayIndex = 5;
			columnHeaderBR.Text = "Base Rank";
			columnHeaderBR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			columnHeaderBR.Width = 100;
			// 
			// columnHeader16
			// 
			columnHeader16.Text = "Last Seen";
			columnHeader16.Width = 150;
			// 
			// NetworkPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Peers);
			Controls.Add(Funds);
			Controls.Add(label1);
			Controls.Add(label3);
			Controls.Add(DestLabel);
			Controls.Add(Members);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "NetworkPanel";
			Size = new System.Drawing.Size(1902, 1638);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView Members;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Label DestLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Funds;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView Peers;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.ColumnHeader columnHeaderCR;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeaderBR;
		private System.Windows.Forms.ColumnHeader columnHeader5;
	}
}
