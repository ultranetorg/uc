
namespace Uccs.Mcv.FUI
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
			label1 = new Label();
			Peers = new ListView();
			columnHeader13 = new ColumnHeader();
			columnHeader14 = new ColumnHeader();
			columnHeader15 = new ColumnHeader();
			columnHeader11 = new ColumnHeader();
			columnHeader16 = new ColumnHeader();
			columnHeader1 = new ColumnHeader();
			BReload = new Button();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label1.Location = new Point(4, 18);
			label1.Margin = new Padding(0, 0, 3, 0);
			label1.Name = "label1";
			label1.Size = new Size(39, 13);
			label1.TabIndex = 2;
			label1.Text = "Peers";
			label1.TextAlign = ContentAlignment.TopRight;
			// 
			// Peers
			// 
			Peers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Peers.Columns.AddRange(new ColumnHeader[] { columnHeader13, columnHeader14, columnHeader15, columnHeader11, columnHeader16, columnHeader1 });
			Peers.FullRowSelect = true;
			Peers.Location = new Point(4, 40);
			Peers.Margin = new Padding(3, 9, 3, 3);
			Peers.Name = "Peers";
			Peers.Size = new Size(1016, 725);
			Peers.TabIndex = 6;
			Peers.UseCompatibleStateImageBehavior = false;
			Peers.View = View.Details;
			// 
			// columnHeader13
			// 
			columnHeader13.Text = "IP";
			columnHeader13.Width = 100;
			// 
			// columnHeader14
			// 
			columnHeader14.Text = "Status";
			columnHeader14.TextAlign = HorizontalAlignment.Center;
			columnHeader14.Width = 150;
			// 
			// columnHeader15
			// 
			columnHeader15.Text = "Retries";
			columnHeader15.TextAlign = HorizontalAlignment.Center;
			columnHeader15.Width = 75;
			// 
			// columnHeader11
			// 
			columnHeader11.Text = "Rank";
			columnHeader11.TextAlign = HorizontalAlignment.Center;
			columnHeader11.Width = 100;
			// 
			// columnHeader16
			// 
			columnHeader16.DisplayIndex = 5;
			columnHeader16.Text = "Last Seen";
			columnHeader16.Width = 150;
			// 
			// columnHeader1
			// 
			columnHeader1.DisplayIndex = 4;
			columnHeader1.Text = "Roles";
			columnHeader1.TextAlign = HorizontalAlignment.Center;
			// 
			// Refresh
			// 
			BReload.Location = new Point(891, 3);
			BReload.Name = "Refresh";
			BReload.Size = new Size(129, 28);
			BReload.TabIndex = 26;
			BReload.Text = "Refresh";
			BReload.UseVisualStyleBackColor = true;
			BReload.Click += Refresh_Click;
			// 
			// NetworkPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(BReload);
			Controls.Add(Peers);
			Controls.Add(label1);
			Margin = new Padding(4, 3, 4, 3);
			Name = "NetworkPanel";
			Size = new Size(1024, 768);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Peers;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private Button BReload;
	}
}
