
namespace Uccs.Net.FUI
{
	partial class NexusNetworkPanel
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
			label1 = new System.Windows.Forms.Label();
			Peers = new System.Windows.Forms.ListView();
			columnHeader13 = new System.Windows.Forms.ColumnHeader();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			label1.Location = new System.Drawing.Point(4, 15);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(39, 13);
			label1.TabIndex = 2;
			label1.Text = "Peers";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Peers
			// 
			Peers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Peers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader13 });
			Peers.FullRowSelect = true;
			Peers.Location = new System.Drawing.Point(4, 39);
			Peers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Peers.Name = "Peers";
			Peers.Size = new System.Drawing.Size(1016, 726);
			Peers.TabIndex = 6;
			Peers.UseCompatibleStateImageBehavior = false;
			Peers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader13
			// 
			columnHeader13.Text = "IP";
			columnHeader13.Width = 100;
			// 
			// InterzoneNetworkPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Peers);
			Controls.Add(label1);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "NexusNetworkPanel";
			Size = new System.Drawing.Size(1024, 768);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView Peers;
		private System.Windows.Forms.ColumnHeader columnHeader13;
	}
}
