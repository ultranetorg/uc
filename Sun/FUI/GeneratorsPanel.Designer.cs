
namespace Uccs.Sun.FUI
{
	partial class GeneratorsPanel
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
			IPs = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			label1 = new System.Windows.Forms.Label();
			Proxies = new System.Windows.Forms.ListView();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			label2 = new System.Windows.Forms.Label();
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
			// IPs
			// 
			IPs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			IPs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2 });
			IPs.FullRowSelect = true;
			IPs.Location = new System.Drawing.Point(1264, 78);
			IPs.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			IPs.Name = "IPs";
			IPs.Size = new System.Drawing.Size(631, 695);
			IPs.TabIndex = 3;
			IPs.UseCompatibleStateImageBehavior = false;
			IPs.View = System.Windows.Forms.View.Details;
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
			label1.Size = new System.Drawing.Size(155, 27);
			label1.TabIndex = 2;
			label1.Text = "IP Addresses";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Proxies
			// 
			Proxies.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			Proxies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader5 });
			Proxies.FullRowSelect = true;
			Proxies.Location = new System.Drawing.Point(1264, 854);
			Proxies.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Proxies.Name = "Proxies";
			Proxies.Size = new System.Drawing.Size(631, 778);
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
			label2.Location = new System.Drawing.Point(1264, 801);
			label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(94, 27);
			label2.TabIndex = 2;
			label2.Text = "Proxies";
			label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// GeneratorsPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Proxies);
			Controls.Add(IPs);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(DestLabel);
			Controls.Add(Generators);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "GeneratorsPanel";
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
		private System.Windows.Forms.ListView IPs;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Proxies;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader5;
	}
}
