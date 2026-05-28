
namespace Uccs.Mcv.FUI
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
			Generators = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			DestLabel = new Label();
			BaseRdcIPs = new ListView();
			columnHeader2 = new ColumnHeader();
			label1 = new Label();
			Refresh = new Button();
			SuspendLayout();
			// 
			// Generators
			// 
			Generators.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Generators.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader4 });
			Generators.FullRowSelect = true;
			Generators.Location = new Point(4, 43);
			Generators.Margin = new Padding(3, 9, 3, 3);
			Generators.Name = "Generators";
			Generators.Size = new Size(1016, 490);
			Generators.TabIndex = 1;
			Generators.UseCompatibleStateImageBehavior = false;
			Generators.View = View.Details;
			Generators.ItemSelectionChanged += Generators_ItemSelectionChanged;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "User";
			columnHeader1.Width = 300;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Joined At";
			columnHeader4.TextAlign = HorizontalAlignment.Right;
			columnHeader4.Width = 100;
			// 
			// DestLabel
			// 
			DestLabel.AutoSize = true;
			DestLabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			DestLabel.Location = new Point(4, 21);
			DestLabel.Margin = new Padding(0, 0, 3, 0);
			DestLabel.Name = "DestLabel";
			DestLabel.Size = new Size(71, 13);
			DestLabel.TabIndex = 2;
			DestLabel.Text = "Generators";
			DestLabel.TextAlign = ContentAlignment.TopRight;
			// 
			// BaseRdcIPs
			// 
			BaseRdcIPs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			BaseRdcIPs.Columns.AddRange(new ColumnHeader[] { columnHeader2 });
			BaseRdcIPs.FullRowSelect = true;
			BaseRdcIPs.HeaderStyle = ColumnHeaderStyle.None;
			BaseRdcIPs.Location = new Point(4, 587);
			BaseRdcIPs.Margin = new Padding(3, 9, 3, 3);
			BaseRdcIPs.Name = "BaseRdcIPs";
			BaseRdcIPs.Size = new Size(401, 177);
			BaseRdcIPs.TabIndex = 3;
			BaseRdcIPs.UseCompatibleStateImageBehavior = false;
			BaseRdcIPs.View = View.Details;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "IP";
			columnHeader2.Width = 200;
			// 
			// label1
			// 
			label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			label1.AutoSize = true;
			label1.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label1.Location = new Point(4, 565);
			label1.Margin = new Padding(0, 0, 3, 0);
			label1.Name = "label1";
			label1.Size = new Size(119, 13);
			label1.TabIndex = 2;
			label1.Text = "Graph Ppi Endpoints";
			label1.TextAlign = ContentAlignment.TopRight;
			// 
			// Refresh
			// 
			Refresh.Location = new Point(891, 3);
			Refresh.Name = "Refresh";
			Refresh.Size = new Size(129, 28);
			Refresh.TabIndex = 25;
			Refresh.Text = "Refresh";
			Refresh.UseVisualStyleBackColor = true;
			// 
			// MembersPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Refresh);
			Controls.Add(BaseRdcIPs);
			Controls.Add(label1);
			Controls.Add(DestLabel);
			Controls.Add(Generators);
			Margin = new Padding(4, 3, 4, 3);
			Name = "MembersPanel";
			Size = new Size(1024, 768);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView Generators;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Label DestLabel;
		private System.Windows.Forms.ListView BaseRdcIPs;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private Button Refresh;
	}
}
