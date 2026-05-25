
namespace Uccs.Nexus.Windows
{
	partial class ResourcesPanel
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
			NetworkReleases = new ListView();
			ChId = new ColumnHeader();
			ChAddress = new ColumnHeader();
			ChFlags = new ColumnHeader();
			ChDataControl = new ColumnHeader();
			ChDataType = new ColumnHeader();
			ChDataLength = new ColumnHeader();
			ChInLinks = new ColumnHeader();
			ChOutLinks = new ColumnHeader();
			label5 = new Label();
			OnlineQuery = new ComboBox();
			NetworkSearch = new Button();
			LocalReleases = new ListView();
			columnHeader2 = new ColumnHeader();
			columnHeader1 = new ColumnHeader();
			columnHeader3 = new ColumnHeader();
			columnHeader8 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			label3 = new Label();
			LocalQuery = new ComboBox();
			LocalSearch = new Button();
			SuspendLayout();
			// 
			// NetworkReleases
			// 
			NetworkReleases.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			NetworkReleases.Columns.AddRange(new ColumnHeader[] { ChId, ChAddress, ChFlags, ChDataControl, ChDataType, ChDataLength, ChInLinks, ChOutLinks });
			NetworkReleases.FullRowSelect = true;
			NetworkReleases.Location = new Point(3, 38);
			NetworkReleases.Margin = new Padding(3, 6, 3, 3);
			NetworkReleases.Name = "NetworkReleases";
			NetworkReleases.Size = new Size(1019, 340);
			NetworkReleases.TabIndex = 4;
			NetworkReleases.UseCompatibleStateImageBehavior = false;
			NetworkReleases.View = View.Details;
			// 
			// ChId
			// 
			ChId.Text = "Id";
			ChId.Width = 100;
			// 
			// ChAddress
			// 
			ChAddress.Text = "Address";
			ChAddress.Width = 300;
			// 
			// ChFlags
			// 
			ChFlags.Text = "Flags";
			ChFlags.Width = 150;
			// 
			// ChDataControl
			// 
			ChDataControl.Text = "Control";
			ChDataControl.Width = 75;
			// 
			// ChDataType
			// 
			ChDataType.Text = "Type";
			ChDataType.Width = 150;
			// 
			// ChDataLength
			// 
			ChDataLength.Text = "Length";
			ChDataLength.TextAlign = HorizontalAlignment.Right;
			ChDataLength.Width = 100;
			// 
			// ChInLinks
			// 
			ChInLinks.Text = "In Links";
			ChInLinks.TextAlign = HorizontalAlignment.Right;
			// 
			// ChOutLinks
			// 
			ChOutLinks.Text = "Out Links";
			ChOutLinks.TextAlign = HorizontalAlignment.Right;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label5.Location = new Point(3, 12);
			label5.Name = "label5";
			label5.Size = new Size(109, 13);
			label5.TabIndex = 13;
			label5.Text = "Resource Address";
			// 
			// OnlineQuery
			// 
			OnlineQuery.FormattingEnabled = true;
			OnlineQuery.Location = new Point(127, 7);
			OnlineQuery.Name = "OnlineQuery";
			OnlineQuery.Size = new Size(702, 23);
			OnlineQuery.TabIndex = 0;
			// 
			// NetworkSearch
			// 
			NetworkSearch.Location = new Point(835, 5);
			NetworkSearch.Name = "NetworkSearch";
			NetworkSearch.Size = new Size(186, 26);
			NetworkSearch.TabIndex = 3;
			NetworkSearch.Text = "Search Online";
			NetworkSearch.UseVisualStyleBackColor = true;
			NetworkSearch.Click += OnlineSearch_Click;
			// 
			// LocalReleases
			// 
			LocalReleases.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			LocalReleases.Columns.AddRange(new ColumnHeader[] { columnHeader2, columnHeader1, columnHeader3, columnHeader8, columnHeader4 });
			LocalReleases.FullRowSelect = true;
			LocalReleases.Location = new Point(3, 420);
			LocalReleases.Margin = new Padding(3, 6, 3, 3);
			LocalReleases.Name = "LocalReleases";
			LocalReleases.Size = new Size(1019, 347);
			LocalReleases.TabIndex = 14;
			LocalReleases.UseCompatibleStateImageBehavior = false;
			LocalReleases.View = View.Details;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Id";
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Address";
			columnHeader1.Width = 300;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Control";
			columnHeader3.Width = 150;
			// 
			// columnHeader8
			// 
			columnHeader8.Text = "Content";
			columnHeader8.Width = 150;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Data Length";
			columnHeader4.TextAlign = HorizontalAlignment.Right;
			columnHeader4.Width = 100;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label3.Location = new Point(3, 395);
			label3.Margin = new Padding(4, 0, 4, 0);
			label3.Name = "label3";
			label3.Size = new Size(109, 13);
			label3.TabIndex = 24;
			label3.Text = "Resource Address";
			// 
			// LocalQuery
			// 
			LocalQuery.FormattingEnabled = true;
			LocalQuery.Location = new Point(127, 390);
			LocalQuery.Margin = new Padding(4, 3, 4, 3);
			LocalQuery.Name = "LocalQuery";
			LocalQuery.Size = new Size(702, 23);
			LocalQuery.TabIndex = 27;
			// 
			// LocalSearch
			// 
			LocalSearch.Location = new Point(835, 388);
			LocalSearch.Name = "LocalSearch";
			LocalSearch.Size = new Size(186, 26);
			LocalSearch.TabIndex = 25;
			LocalSearch.Text = " Local Search";
			LocalSearch.UseVisualStyleBackColor = true;
			LocalSearch.Click += LocalSearch_Click;
			// 
			// ResourcesPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(LocalQuery);
			Controls.Add(LocalSearch);
			Controls.Add(label3);
			Controls.Add(LocalReleases);
			Controls.Add(label5);
			Controls.Add(OnlineQuery);
			Controls.Add(NetworkSearch);
			Controls.Add(NetworkReleases);
			Name = "ResourcesPanel";
			Size = new Size(1024, 768);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.ListView NetworkReleases;
		private System.Windows.Forms.ColumnHeader ChAddress;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox OnlineQuery;
		private System.Windows.Forms.Button NetworkSearch;
		private System.Windows.Forms.ColumnHeader ChFlags;
		private System.Windows.Forms.ColumnHeader ChDataType;
		private System.Windows.Forms.ColumnHeader ChInLinks;
		private System.Windows.Forms.ColumnHeader ChId;
		private System.Windows.Forms.ListView LocalReleases;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox LocalQuery;
		private System.Windows.Forms.Button LocalSearch;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private ColumnHeader ChOutLinks;
		private ColumnHeader ChDataControl;
		private ColumnHeader ChDataLength;
	}
}
