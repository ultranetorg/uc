
namespace Uccs.Rdn.FUI
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
			this.logbox = new Uccs.Rdn.FUI.Logbox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.fields = new System.Windows.Forms.Label();
			this.values = new System.Windows.Forms.Label();
			this.monitor = new Uccs.Rdn.FUI.ChainMonitor();
			this.sendgroup = new System.Windows.Forms.GroupBox();
			this.amount = new Uccs.Rdn.FUI.CoinEdit();
			this.send = new System.Windows.Forms.Button();
			this.all = new System.Windows.Forms.LinkLabel();
			this.source = new System.Windows.Forms.ComboBox();
			this.destination = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.sendgroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// logbox
			// 
			this.logbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.logbox.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.logbox.Location = new System.Drawing.Point(0, 1210);
			this.logbox.Log = null;
			this.logbox.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.logbox.Multiline = true;
			this.logbox.Name = "logbox";
			this.logbox.ReadOnly = true;
			this.logbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.logbox.ShowSender = false;
			this.logbox.ShowSubject = true;
			this.logbox.Size = new System.Drawing.Size(1121, 422);
			this.logbox.TabIndex = 1;
			this.logbox.WordWrap = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.panel1);
			this.groupBox2.Location = new System.Drawing.Point(0, -13);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(0);
			this.groupBox2.Size = new System.Drawing.Size(1902, 981);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoScroll = true;
			this.panel1.Controls.Add(this.fields);
			this.panel1.Controls.Add(this.values);
			this.panel1.Location = new System.Drawing.Point(30, 47);
			this.panel1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1840, 907);
			this.panel1.TabIndex = 1;
			// 
			// fields
			// 
			this.fields.AutoEllipsis = true;
			this.fields.AutoSize = true;
			this.fields.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.fields.Location = new System.Drawing.Point(0, 0);
			this.fields.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.fields.Name = "fields";
			this.fields.Size = new System.Drawing.Size(276, 22);
			this.fields.TabIndex = 0;
			this.fields.Text = "Waiting for data...\r\n";
			// 
			// values
			// 
			this.values.AutoEllipsis = true;
			this.values.AutoSize = true;
			this.values.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.values.Location = new System.Drawing.Point(474, 0);
			this.values.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.values.Name = "values";
			this.values.Size = new System.Drawing.Size(348, 66);
			this.values.TabIndex = 0;
			this.values.Text = " Waiting for data...      \r\n                 \r\n                    ";
			// 
			// monitor
			// 
			this.monitor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.monitor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.monitor.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.monitor.Location = new System.Drawing.Point(0, 986);
			this.monitor.Margin = new System.Windows.Forms.Padding(9, 6, 9, 6);
			this.monitor.Name = "monitor";
			this.monitor.Size = new System.Drawing.Size(1896, 205);
			this.monitor.TabIndex = 3;
			// 
			// sendgroup
			// 
			this.sendgroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.sendgroup.Controls.Add(this.amount);
			this.sendgroup.Controls.Add(this.send);
			this.sendgroup.Controls.Add(this.all);
			this.sendgroup.Controls.Add(this.source);
			this.sendgroup.Controls.Add(this.destination);
			this.sendgroup.Controls.Add(this.label2);
			this.sendgroup.Controls.Add(this.label4);
			this.sendgroup.Controls.Add(this.label1);
			this.sendgroup.Location = new System.Drawing.Point(1146, 1195);
			this.sendgroup.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.sendgroup.Name = "sendgroup";
			this.sendgroup.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.sendgroup.Size = new System.Drawing.Size(754, 443);
			this.sendgroup.TabIndex = 4;
			this.sendgroup.TabStop = false;
			this.sendgroup.Text = "Send UNT";
			// 
			// amount
			// 
			this.amount.Location = new System.Drawing.Point(197, 234);
			this.amount.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.amount.Name = "amount";
			this.amount.Size = new System.Drawing.Size(240, 39);
			this.amount.TabIndex = 8;
			this.amount.Text = "0.001";
			this.amount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// send
			// 
			this.send.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.send.Location = new System.Drawing.Point(387, 324);
			this.send.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.send.Name = "send";
			this.send.Size = new System.Drawing.Size(308, 58);
			this.send.TabIndex = 2;
			this.send.Text = "Send";
			this.send.UseVisualStyleBackColor = true;
			this.send.Click += new System.EventHandler(this.send_Click);
			// 
			// all
			// 
			this.all.AutoSize = true;
			this.all.Location = new System.Drawing.Point(461, 243);
			this.all.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.all.Name = "all";
			this.all.Size = new System.Drawing.Size(41, 32);
			this.all.TabIndex = 7;
			this.all.TabStop = true;
			this.all.Text = "All";
			this.all.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.all_LinkClicked);
			// 
			// source
			// 
			this.source.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.source.FormattingEnabled = true;
			this.source.Location = new System.Drawing.Point(197, 76);
			this.source.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.source.Name = "source";
			this.source.Size = new System.Drawing.Size(509, 40);
			this.source.TabIndex = 3;
			this.source.SelectionChangeCommitted += new System.EventHandler(this.source_SelectionChangeCommitted);
			// 
			// destination
			// 
			this.destination.FormattingEnabled = true;
			this.destination.Location = new System.Drawing.Point(197, 153);
			this.destination.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.destination.Name = "destination";
			this.destination.Size = new System.Drawing.Size(509, 40);
			this.destination.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(65, 241);
			this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(99, 27);
			this.label2.TabIndex = 1;
			this.label2.Text = "Amount";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label4.Location = new System.Drawing.Point(94, 83);
			this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(70, 27);
			this.label4.TabIndex = 1;
			this.label4.Text = "From";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(115, 160);
			this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 27);
			this.label1.TabIndex = 1;
			this.label1.Text = "To";
			// 
			// DashboardPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.logbox);
			this.Controls.Add(this.sendgroup);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.monitor);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "DashboardPanel";
			this.Size = new System.Drawing.Size(1902, 1638);
			this.groupBox2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.sendgroup.ResumeLayout(false);
			this.sendgroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Logbox logbox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label fields;
		private ChainMonitor monitor;
		private System.Windows.Forms.GroupBox sendgroup;
		private System.Windows.Forms.LinkLabel all;
		private System.Windows.Forms.ComboBox source;
		private System.Windows.Forms.ComboBox destination;
		private System.Windows.Forms.Button send;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label values;
		private FUI.CoinEdit amount;
		private System.Windows.Forms.Panel panel1;
	}
}
