
namespace Uccs.Sun.FUI
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
			NetworkReleases = new System.Windows.Forms.ListView();
			CHId = new System.Windows.Forms.ColumnHeader();
			CHAddress = new System.Windows.Forms.ColumnHeader();
			CHExpiration = new System.Windows.Forms.ColumnHeader();
			CHLastProlongation = new System.Windows.Forms.ColumnHeader();
			CHFlags = new System.Windows.Forms.ColumnHeader();
			CHType = new System.Windows.Forms.ColumnHeader();
			CHReserved = new System.Windows.Forms.ColumnHeader();
			CHData = new System.Windows.Forms.ColumnHeader();
			CHAnalysisStage = new System.Windows.Forms.ColumnHeader();
			CHAnalysisFee = new System.Windows.Forms.ColumnHeader();
			CHAnalysisHVR = new System.Windows.Forms.ColumnHeader();
			CHGood = new System.Windows.Forms.ColumnHeader();
			CHBad = new System.Windows.Forms.ColumnHeader();
			CHResources = new System.Windows.Forms.ColumnHeader();
			label5 = new System.Windows.Forms.Label();
			NetworkQuery = new System.Windows.Forms.ComboBox();
			NetworkSearch = new System.Windows.Forms.Button();
			LocalReleases = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			columnHeader8 = new System.Windows.Forms.ColumnHeader();
			label3 = new System.Windows.Forms.Label();
			LocalQuery = new System.Windows.Forms.ComboBox();
			LocalSearch = new System.Windows.Forms.Button();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			SuspendLayout();
			// 
			// NetworkReleases
			// 
			NetworkReleases.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			NetworkReleases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { CHId, CHAddress, CHExpiration, CHLastProlongation, CHFlags, CHType, CHReserved, CHData, CHAnalysisStage, CHAnalysisFee, CHAnalysisHVR, CHGood, CHBad, CHResources });
			NetworkReleases.FullRowSelect = true;
			NetworkReleases.Location = new System.Drawing.Point(12, 112);
			NetworkReleases.Margin = new System.Windows.Forms.Padding(6);
			NetworkReleases.Name = "NetworkReleases";
			NetworkReleases.Size = new System.Drawing.Size(1884, 689);
			NetworkReleases.TabIndex = 4;
			NetworkReleases.UseCompatibleStateImageBehavior = false;
			NetworkReleases.View = System.Windows.Forms.View.Details;
			// 
			// CHId
			// 
			CHId.Text = "Id";
			// 
			// CHAddress
			// 
			CHAddress.Text = "Address";
			CHAddress.Width = 300;
			// 
			// CHExpiration
			// 
			CHExpiration.Text = "Expiration";
			CHExpiration.Width = 100;
			// 
			// CHLastProlongation
			// 
			CHLastProlongation.Text = "Last Prolongation";
			CHLastProlongation.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CHFlags
			// 
			CHFlags.Text = "Flags";
			// 
			// CHType
			// 
			CHType.Text = "Type";
			// 
			// CHReserved
			// 
			CHReserved.Text = "Reserved";
			CHReserved.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CHData
			// 
			CHData.Text = "Data";
			CHData.Width = 200;
			// 
			// CHAnalysisStage
			// 
			CHAnalysisStage.Text = "Analysis Stage";
			// 
			// CHAnalysisFee
			// 
			CHAnalysisFee.Text = "Analysis Fee";
			CHAnalysisFee.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CHAnalysisHVR
			// 
			CHAnalysisHVR.Text = "Analysis HVR";
			CHAnalysisHVR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CHGood
			// 
			CHGood.Text = "Good";
			CHGood.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CHBad
			// 
			CHBad.Text = "Bad";
			CHBad.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CHResources
			// 
			CHResources.Text = "Resources";
			CHResources.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label5.Location = new System.Drawing.Point(12, 39);
			label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(115, 27);
			label5.TabIndex = 13;
			label5.Text = "Resource";
			// 
			// NetworkQuery
			// 
			NetworkQuery.FormattingEnabled = true;
			NetworkQuery.Location = new System.Drawing.Point(139, 32);
			NetworkQuery.Margin = new System.Windows.Forms.Padding(6);
			NetworkQuery.Name = "NetworkQuery";
			NetworkQuery.Size = new System.Drawing.Size(687, 40);
			NetworkQuery.TabIndex = 0;
			// 
			// NetworkSearch
			// 
			NetworkSearch.Location = new System.Drawing.Point(856, 24);
			NetworkSearch.Margin = new System.Windows.Forms.Padding(6);
			NetworkSearch.Name = "NetworkSearch";
			NetworkSearch.Size = new System.Drawing.Size(345, 55);
			NetworkSearch.TabIndex = 3;
			NetworkSearch.Text = "Search on the Network";
			NetworkSearch.UseVisualStyleBackColor = true;
			NetworkSearch.Click += NetworkSearch_Click;
			// 
			// LocalReleases
			// 
			LocalReleases.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			LocalReleases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2, columnHeader1, columnHeader6, columnHeader8 });
			LocalReleases.FullRowSelect = true;
			LocalReleases.Location = new System.Drawing.Point(12, 908);
			LocalReleases.Margin = new System.Windows.Forms.Padding(6);
			LocalReleases.Name = "LocalReleases";
			LocalReleases.Size = new System.Drawing.Size(1884, 724);
			LocalReleases.TabIndex = 14;
			LocalReleases.UseCompatibleStateImageBehavior = false;
			LocalReleases.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Address";
			columnHeader2.Width = 300;
			// 
			// columnHeader6
			// 
			columnHeader6.Text = "Type";
			// 
			// columnHeader8
			// 
			columnHeader8.Text = "Data";
			columnHeader8.Width = 500;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label3.Location = new System.Drawing.Point(12, 838);
			label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(115, 27);
			label3.TabIndex = 24;
			label3.Text = "Resource";
			// 
			// LocalQuery
			// 
			LocalQuery.FormattingEnabled = true;
			LocalQuery.Location = new System.Drawing.Point(139, 831);
			LocalQuery.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			LocalQuery.Name = "LocalQuery";
			LocalQuery.Size = new System.Drawing.Size(687, 40);
			LocalQuery.TabIndex = 27;
			// 
			// LocalSearch
			// 
			LocalSearch.Location = new System.Drawing.Point(856, 823);
			LocalSearch.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			LocalSearch.Name = "LocalSearch";
			LocalSearch.Size = new System.Drawing.Size(240, 55);
			LocalSearch.TabIndex = 25;
			LocalSearch.Text = " Local Search";
			LocalSearch.UseVisualStyleBackColor = true;
			LocalSearch.Click += LocalSearch_Click;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Status";
			columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// ResourcesPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(LocalQuery);
			Controls.Add(LocalSearch);
			Controls.Add(label3);
			Controls.Add(LocalReleases);
			Controls.Add(label5);
			Controls.Add(NetworkQuery);
			Controls.Add(NetworkSearch);
			Controls.Add(NetworkReleases);
			Margin = new System.Windows.Forms.Padding(6);
			Name = "ResourcesPanel";
			Size = new System.Drawing.Size(1902, 1638);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.ListView NetworkReleases;
		private System.Windows.Forms.ColumnHeader CHAddress;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox NetworkQuery;
		private System.Windows.Forms.Button NetworkSearch;
		private System.Windows.Forms.ColumnHeader CHExpiration;
		private System.Windows.Forms.ColumnHeader CHLastProlongation;
		private System.Windows.Forms.ColumnHeader CHFlags;
		private System.Windows.Forms.ColumnHeader CHType;
		private System.Windows.Forms.ColumnHeader CHReserved;
		private System.Windows.Forms.ColumnHeader CHData;
		private System.Windows.Forms.ColumnHeader CHAnalysisStage;
		private System.Windows.Forms.ColumnHeader CHAnalysisFee;
		private System.Windows.Forms.ColumnHeader CHAnalysisHVR;
		private System.Windows.Forms.ColumnHeader CHGood;
		private System.Windows.Forms.ColumnHeader CHBad;
		private System.Windows.Forms.ColumnHeader CHResources;
		private System.Windows.Forms.ColumnHeader CHId;
		private System.Windows.Forms.ListView LocalReleases;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox LocalQuery;
		private System.Windows.Forms.Button LocalSearch;
		private System.Windows.Forms.ColumnHeader columnHeader1;
	}
}
