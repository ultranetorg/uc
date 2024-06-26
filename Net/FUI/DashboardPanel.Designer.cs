
namespace Uccs.Net.FUI
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
			groupBox2 = new System.Windows.Forms.GroupBox();
			panel1 = new System.Windows.Forms.Panel();
			fields = new System.Windows.Forms.Label();
			values = new System.Windows.Forms.Label();
			Monitor = new ChainMonitor();
			groupBox2.SuspendLayout();
			panel1.SuspendLayout();
			SuspendLayout();
			// 
			// logbox
			// 
			Logbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Logbox.Font = new System.Drawing.Font("Lucida Console", 8F);
			Logbox.Location = new System.Drawing.Point(0, 567);
			Logbox.Log = null;
			Logbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Logbox.Multiline = true;
			Logbox.Name = "logbox";
			Logbox.ReadOnly = true;
			Logbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			Logbox.ShowSender = false;
			Logbox.ShowSubject = true;
			Logbox.Size = new System.Drawing.Size(1020, 200);
			Logbox.TabIndex = 1;
			Logbox.WordWrap = false;
			// 
			// groupBox2
			// 
			groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox2.Controls.Add(panel1);
			groupBox2.Location = new System.Drawing.Point(0, -6);
			groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new System.Windows.Forms.Padding(0);
			groupBox2.Size = new System.Drawing.Size(1024, 460);
			groupBox2.TabIndex = 2;
			groupBox2.TabStop = false;
			// 
			// panel1
			// 
			panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panel1.AutoScroll = true;
			panel1.Controls.Add(fields);
			panel1.Controls.Add(values);
			panel1.Location = new System.Drawing.Point(16, 22);
			panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(991, 425);
			panel1.TabIndex = 1;
			// 
			// fields
			// 
			fields.AutoEllipsis = true;
			fields.AutoSize = true;
			fields.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Bold);
			fields.Location = new System.Drawing.Point(0, 0);
			fields.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			fields.Name = "fields";
			fields.Size = new System.Drawing.Size(157, 11);
			fields.TabIndex = 0;
			fields.Text = "Waiting for data...\r\n";
			// 
			// values
			// 
			values.AutoEllipsis = true;
			values.AutoSize = true;
			values.Font = new System.Drawing.Font("Lucida Console", 8.25F);
			values.Location = new System.Drawing.Point(255, 0);
			values.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			values.Name = "values";
			values.Size = new System.Drawing.Size(187, 33);
			values.TabIndex = 0;
			values.Text = " Waiting for data...      \r\n                 \r\n                    ";
			// 
			// Monitor
			// 
			Monitor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Monitor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			Monitor.Font = new System.Drawing.Font("Lucida Console", 8.25F);
			Monitor.Location = new System.Drawing.Point(0, 462);
			Monitor.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			Monitor.Name = "Monitor";
			Monitor.Size = new System.Drawing.Size(1019, 98);
			Monitor.TabIndex = 3;
			// 
			// DashboardPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Logbox);
			Controls.Add(groupBox2);
			Controls.Add(Monitor);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "DashboardPanel";
			Size = new System.Drawing.Size(1024, 768);
			groupBox2.ResumeLayout(false);
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Logbox Logbox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label fields;
		public ChainMonitor Monitor;
		private System.Windows.Forms.Label values;
		private System.Windows.Forms.Panel panel1;
	}
}
