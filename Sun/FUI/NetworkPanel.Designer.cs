
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
			this.Generators = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.DestLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.Hubs = new System.Windows.Forms.ListView();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.label2 = new System.Windows.Forms.Label();
			this.Funds = new System.Windows.Forms.ListView();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.label3 = new System.Windows.Forms.Label();
			this.Peers = new System.Windows.Forms.ListView();
			this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderCR = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderHR = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderSR = new System.Windows.Forms.ColumnHeader();
			this.columnHeader16 = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderBR = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// Generators
			// 
			this.Generators.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Generators.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader3,
            this.columnHeader2});
			this.Generators.FullRowSelect = true;
			this.Generators.Location = new System.Drawing.Point(0, 860);
			this.Generators.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Generators.Name = "Generators";
			this.Generators.Size = new System.Drawing.Size(933, 778);
			this.Generators.TabIndex = 1;
			this.Generators.UseCompatibleStateImageBehavior = false;
			this.Generators.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Account";
			this.columnHeader1.Width = 300;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Joined At";
			this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader4.Width = 100;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Bail";
			this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader3.Width = 150;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "IP";
			this.columnHeader2.Width = 100;
			// 
			// DestLabel
			// 
			this.DestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DestLabel.AutoSize = true;
			this.DestLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.DestLabel.Location = new System.Drawing.Point(0, 819);
			this.DestLabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.DestLabel.Name = "DestLabel";
			this.DestLabel.Size = new System.Drawing.Size(135, 27);
			this.DestLabel.TabIndex = 2;
			this.DestLabel.Text = "Generators";
			this.DestLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(0, 32);
			this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 27);
			this.label1.TabIndex = 2;
			this.label1.Text = "Peers";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Hubs
			// 
			this.Hubs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Hubs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
			this.Hubs.FullRowSelect = true;
			this.Hubs.Location = new System.Drawing.Point(984, 860);
			this.Hubs.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Hubs.Name = "Hubs";
			this.Hubs.Size = new System.Drawing.Size(918, 356);
			this.Hubs.TabIndex = 4;
			this.Hubs.UseCompatibleStateImageBehavior = false;
			this.Hubs.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "IP";
			this.columnHeader5.Width = 100;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Rank";
			this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(984, 819);
			this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(68, 27);
			this.label2.TabIndex = 2;
			this.label2.Text = "Hubs";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Funds
			// 
			this.Funds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Funds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9});
			this.Funds.FullRowSelect = true;
			this.Funds.Location = new System.Drawing.Point(984, 1280);
			this.Funds.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Funds.Name = "Funds";
			this.Funds.Size = new System.Drawing.Size(918, 356);
			this.Funds.TabIndex = 5;
			this.Funds.UseCompatibleStateImageBehavior = false;
			this.Funds.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "Account";
			this.columnHeader9.Width = 300;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(984, 1235);
			this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(78, 27);
			this.label3.TabIndex = 2;
			this.label3.Text = "Funds";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Peers
			// 
			this.Peers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Peers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader11,
            this.columnHeaderCR,
            this.columnHeaderBR,
            this.columnHeaderHR,
            this.columnHeaderSR,
            this.columnHeader16});
			this.Peers.FullRowSelect = true;
			this.Peers.Location = new System.Drawing.Point(0, 83);
			this.Peers.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Peers.Name = "Peers";
			this.Peers.Size = new System.Drawing.Size(1902, 697);
			this.Peers.TabIndex = 6;
			this.Peers.UseCompatibleStateImageBehavior = false;
			this.Peers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader13
			// 
			this.columnHeader13.Text = "IP";
			this.columnHeader13.Width = 100;
			// 
			// columnHeader14
			// 
			this.columnHeader14.Text = "Status";
			this.columnHeader14.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader14.Width = 150;
			// 
			// columnHeader15
			// 
			this.columnHeader15.Text = "Retries";
			this.columnHeader15.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader15.Width = 75;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "Peer Rank";
			this.columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader11.Width = 100;
			// 
			// columnHeaderCR
			// 
			this.columnHeaderCR.Text = "Chain Rank";
			this.columnHeaderCR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeaderCR.Width = 100;
			// 
			// columnHeaderHR
			// 
			this.columnHeaderHR.Text = "Hub Rank";
			this.columnHeaderHR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeaderHR.Width = 100;
			// 
			// columnHeaderSR
			// 
			this.columnHeaderSR.Text = "Seed Rank";
			this.columnHeaderSR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeaderSR.Width = 100;
			// 
			// columnHeader16
			// 
			this.columnHeader16.Text = "Last Seen";
			this.columnHeader16.Width = 150;
			// 
			// columnHeaderBR
			// 
			this.columnHeaderBR.Text = "Base Rank";
			this.columnHeaderBR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeaderBR.Width = 100;
			// 
			// NetworkPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Peers);
			this.Controls.Add(this.Funds);
			this.Controls.Add(this.Hubs);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.DestLabel);
			this.Controls.Add(this.Generators);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "NetworkPanel";
			this.Size = new System.Drawing.Size(1902, 1638);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView Generators;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Label DestLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Hubs;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListView Funds;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView Peers;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.ColumnHeader columnHeaderCR;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeaderHR;
		private System.Windows.Forms.ColumnHeader columnHeaderSR;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeaderBR;
	}
}
