
namespace Uccs.Sun.FUI
{
	partial class MembersPanel
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
			Generators = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			DestLabel = new System.Windows.Forms.Label();
			BaseRdcIPs = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			label1 = new System.Windows.Forms.Label();
			Proxies = new System.Windows.Forms.ListView();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			SeedHubRdcIPs = new System.Windows.Forms.ListView();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			SuspendLayout();
			// 
			// Generators
			// 
			Generators.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Generators.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader4, columnHeader3 });
			Generators.FullRowSelect = true;
			Generators.Location = new System.Drawing.Point(7, 78);
			Generators.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Generators.Name = "Generators";
			Generators.Size = new System.Drawing.Size(1225, 1554);
			Generators.TabIndex = 1;
			Generators.UseCompatibleStateImageBehavior = false;
			Generators.View = System.Windows.Forms.View.Details;
			Generators.ItemSelectionChanged += Generators_ItemSelectionChanged;
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
			// DestLabel
			// 
			DestLabel.AutoSize = true;
			DestLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			DestLabel.Location = new System.Drawing.Point(0, 32);
			DestLabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			DestLabel.Name = "DestLabel";
			DestLabel.Size = new System.Drawing.Size(135, 27);
			DestLabel.TabIndex = 2;
			DestLabel.Text = "Generators";
			DestLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// BaseRdcIPs
			// 
			BaseRdcIPs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			BaseRdcIPs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2 });
			BaseRdcIPs.FullRowSelect = true;
			BaseRdcIPs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			BaseRdcIPs.Location = new System.Drawing.Point(1264, 78);
			BaseRdcIPs.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			BaseRdcIPs.Name = "BaseRdcIPs";
			BaseRdcIPs.Size = new System.Drawing.Size(631, 301);
			BaseRdcIPs.TabIndex = 3;
			BaseRdcIPs.UseCompatibleStateImageBehavior = false;
			BaseRdcIPs.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "IP";
			columnHeader2.Width = 200;
			// 
			// label1
			// 
			label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(1264, 32);
			label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(230, 27);
			label1.TabIndex = 2;
			label1.Text = "Base Rdc Addresses";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Proxies
			// 
			Proxies.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			Proxies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader5 });
			Proxies.FullRowSelect = true;
			Proxies.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Proxies.Location = new System.Drawing.Point(1264, 1349);
			Proxies.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Proxies.Name = "Proxies";
			Proxies.Size = new System.Drawing.Size(631, 283);
			Proxies.TabIndex = 4;
			Proxies.UseCompatibleStateImageBehavior = false;
			Proxies.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "IP";
			columnHeader5.Width = 200;
			// 
			// label2
			// 
			label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label2.Location = new System.Drawing.Point(1264, 1304);
			label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(94, 27);
			label2.TabIndex = 2;
			label2.Text = "Proxies";
			label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			label3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label3.Location = new System.Drawing.Point(1264, 406);
			label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(283, 27);
			label3.TabIndex = 2;
			label3.Text = "Seed Hub Rdc Addresses";
			label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// SeedHubRdcIPs
			// 
			SeedHubRdcIPs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			SeedHubRdcIPs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader6 });
			SeedHubRdcIPs.FullRowSelect = true;
			SeedHubRdcIPs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			SeedHubRdcIPs.Location = new System.Drawing.Point(1264, 456);
			SeedHubRdcIPs.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			SeedHubRdcIPs.Name = "SeedHubRdcIPs";
			SeedHubRdcIPs.Size = new System.Drawing.Size(631, 301);
			SeedHubRdcIPs.TabIndex = 5;
			SeedHubRdcIPs.UseCompatibleStateImageBehavior = false;
			SeedHubRdcIPs.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader6
			// 
			columnHeader6.Text = "IP";
			columnHeader6.Width = 200;
			// 
			// MembersPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(SeedHubRdcIPs);
			Controls.Add(Proxies);
			Controls.Add(BaseRdcIPs);
			Controls.Add(label2);
			Controls.Add(label3);
			Controls.Add(label1);
			Controls.Add(DestLabel);
			Controls.Add(Generators);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "MembersPanel";
			Size = new System.Drawing.Size(1902, 1638);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView Generators;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Label DestLabel;
		private System.Windows.Forms.ListView BaseRdcIPs;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Proxies;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView SeedHubRdcIPs;
		private System.Windows.Forms.ColumnHeader columnHeader6;
	}
}
