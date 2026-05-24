
namespace Uccs.Nexus.Windows
{
	partial class RdnMembersPanel
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
			GraphPpiEndpoints = new ListView();
			columnHeader2 = new ColumnHeader();
			label1 = new Label();
			label3 = new Label();
			SeedhubPpiEndpoints = new ListView();
			columnHeader6 = new ColumnHeader();
			Refresh = new Button();
			SuspendLayout();
			// 
			// Generators
			// 
			Generators.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Generators.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader4 });
			Generators.FullRowSelect = true;
			Generators.Location = new Point(7, 80);
			Generators.Margin = new Padding(6, 12, 6, 6);
			Generators.Name = "Generators";
			Generators.Size = new Size(1888, 1048);
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
			DestLabel.Location = new Point(7, 22);
			DestLabel.Margin = new Padding(7, 12, 7, 12);
			DestLabel.Name = "DestLabel";
			DestLabel.Size = new Size(135, 27);
			DestLabel.TabIndex = 2;
			DestLabel.Text = "Generators";
			DestLabel.TextAlign = ContentAlignment.TopRight;
			// 
			// GraphPpiEndpoints
			// 
			GraphPpiEndpoints.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			GraphPpiEndpoints.Columns.AddRange(new ColumnHeader[] { columnHeader2 });
			GraphPpiEndpoints.FullRowSelect = true;
			GraphPpiEndpoints.HeaderStyle = ColumnHeaderStyle.None;
			GraphPpiEndpoints.Location = new Point(7, 1226);
			GraphPpiEndpoints.Margin = new Padding(6, 12, 6, 6);
			GraphPpiEndpoints.Name = "GraphPpiEndpoints";
			GraphPpiEndpoints.Size = new Size(631, 406);
			GraphPpiEndpoints.TabIndex = 3;
			GraphPpiEndpoints.UseCompatibleStateImageBehavior = false;
			GraphPpiEndpoints.View = View.Details;
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
			label1.Location = new Point(7, 1175);
			label1.Margin = new Padding(7, 12, 7, 12);
			label1.Name = "label1";
			label1.Size = new Size(237, 27);
			label1.TabIndex = 2;
			label1.Text = "Graph Ppi Endpoints";
			label1.TextAlign = ContentAlignment.TopRight;
			// 
			// label3
			// 
			label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			label3.AutoSize = true;
			label3.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label3.Location = new Point(652, 1174);
			label3.Margin = new Padding(7, 13, 7, 13);
			label3.Name = "label3";
			label3.Size = new Size(266, 27);
			label3.TabIndex = 2;
			label3.Text = "Seedhub Ppi Endpoints";
			label3.TextAlign = ContentAlignment.TopRight;
			// 
			// SeedhubPpiEndpoints
			// 
			SeedhubPpiEndpoints.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			SeedhubPpiEndpoints.Columns.AddRange(new ColumnHeader[] { columnHeader6 });
			SeedhubPpiEndpoints.FullRowSelect = true;
			SeedhubPpiEndpoints.HeaderStyle = ColumnHeaderStyle.None;
			SeedhubPpiEndpoints.Location = new Point(652, 1226);
			SeedhubPpiEndpoints.Margin = new Padding(6, 12, 6, 6);
			SeedhubPpiEndpoints.Name = "SeedhubPpiEndpoints";
			SeedhubPpiEndpoints.Size = new Size(631, 406);
			SeedhubPpiEndpoints.TabIndex = 5;
			SeedhubPpiEndpoints.UseCompatibleStateImageBehavior = false;
			SeedhubPpiEndpoints.View = View.Details;
			// 
			// columnHeader6
			// 
			columnHeader6.Text = "IP";
			columnHeader6.Width = 200;
			// 
			// Refresh
			// 
			Refresh.Location = new Point(1655, 6);
			Refresh.Margin = new Padding(6);
			Refresh.Name = "Refresh";
			Refresh.Size = new Size(240, 56);
			Refresh.TabIndex = 24;
			Refresh.Text = "Refresh";
			Refresh.UseVisualStyleBackColor = true;
			Refresh.Click += Refresh_Click;
			// 
			// RdnMembersPanel
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Refresh);
			Controls.Add(SeedhubPpiEndpoints);
			Controls.Add(GraphPpiEndpoints);
			Controls.Add(label3);
			Controls.Add(label1);
			Controls.Add(DestLabel);
			Controls.Add(Generators);
			Margin = new Padding(7, 6, 7, 6);
			Name = "RdnMembersPanel";
			Size = new Size(1902, 1638);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.ListView Generators;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Label DestLabel;
		private System.Windows.Forms.ListView GraphPpiEndpoints;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView SeedhubPpiEndpoints;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private Button Refresh;
	}
}
