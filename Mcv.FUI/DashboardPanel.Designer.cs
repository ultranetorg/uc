
namespace Uccs.Mcv.FUI
{
	partial class DashboardPanel
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
			Logbox = new Logbox();
			panel1 = new Panel();
			fields = new Label();
			values = new Label();
			Monitor = new ChainMonitor();
			panel1.SuspendLayout();
			SuspendLayout();
			// 
			// Logbox
			// 
			Logbox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Logbox.Font = new Font("Lucida Console", 8F);
			Logbox.Location = new Point(4, 567);
			Logbox.Log = null;
			Logbox.Multiline = true;
			Logbox.Name = "Logbox";
			Logbox.ReadOnly = true;
			Logbox.ScrollBars = ScrollBars.Vertical;
			Logbox.ShowSender = false;
			Logbox.ShowSubject = true;
			Logbox.Size = new Size(1015, 198);
			Logbox.TabIndex = 1;
			Logbox.WordWrap = false;
			// 
			// panel1
			// 
			panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			panel1.AutoScroll = true;
			panel1.Controls.Add(fields);
			panel1.Controls.Add(values);
			panel1.Location = new Point(4, 3);
			panel1.Name = "panel1";
			panel1.Size = new Size(1015, 453);
			panel1.TabIndex = 1;
			// 
			// fields
			// 
			fields.AutoEllipsis = true;
			fields.AutoSize = true;
			fields.Font = new Font("Lucida Console", 8.25F, FontStyle.Bold);
			fields.Location = new Point(4, 5);
			fields.Margin = new Padding(4, 0, 4, 0);
			fields.Name = "fields";
			fields.Size = new Size(157, 11);
			fields.TabIndex = 0;
			fields.Text = "Waiting for data...\r\n";
			// 
			// values
			// 
			values.AutoEllipsis = true;
			values.AutoSize = true;
			values.Font = new Font("Lucida Console", 8.25F);
			values.Location = new Point(256, 5);
			values.Margin = new Padding(4, 0, 4, 0);
			values.Name = "values";
			values.Size = new Size(187, 33);
			values.TabIndex = 0;
			values.Text = " Waiting for data...      \r\n                 \r\n                    ";
			// 
			// Monitor
			// 
			Monitor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Monitor.BorderStyle = BorderStyle.Fixed3D;
			Monitor.Font = new Font("Lucida Console", 8.25F);
			Monitor.Location = new Point(4, 462);
			Monitor.Name = "Monitor";
			Monitor.Size = new Size(1015, 98);
			Monitor.TabIndex = 3;
			// 
			// DashboardPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(panel1);
			Controls.Add(Logbox);
			Controls.Add(Monitor);
			Margin = new Padding(4, 3, 4, 3);
			Name = "DashboardPanel";
			Size = new Size(1024, 768);
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Logbox Logbox;
		private System.Windows.Forms.Label fields;
		public ChainMonitor Monitor;
		private System.Windows.Forms.Label values;
		private System.Windows.Forms.Panel panel1;
	}
}
