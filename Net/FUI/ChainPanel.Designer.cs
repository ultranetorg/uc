
namespace Uccs.Net.FUI
{
	partial class ChainPanel
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
			Transactions = new System.Windows.Forms.ListView();
			columnHeader12 = new System.Windows.Forms.ColumnHeader();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader15 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			columnHeader9 = new System.Windows.Forms.ColumnHeader();
			Operations = new System.Windows.Forms.ListView();
			columnHeader14 = new System.Windows.Forms.ColumnHeader();
			columnHeader8 = new System.Windows.Forms.ColumnHeader();
			columnHeader11 = new System.Windows.Forms.ColumnHeader();
			label9 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			Round = new System.Windows.Forms.NumericUpDown();
			InfoFields = new System.Windows.Forms.Label();
			InfoValues = new System.Windows.Forms.Label();
			tabPage8 = new System.Windows.Forms.TabPage();
			Migrations = new System.Windows.Forms.ListView();
			columnHeader19 = new System.Windows.Forms.ColumnHeader();
			tabPage7 = new System.Windows.Forms.TabPage();
			Emissions = new System.Windows.Forms.ListView();
			columnHeader18 = new System.Windows.Forms.ColumnHeader();
			tabPage6 = new System.Windows.Forms.TabPage();
			AnalyzerLeavers = new System.Windows.Forms.ListView();
			columnHeader17 = new System.Windows.Forms.ColumnHeader();
			tabPage5 = new System.Windows.Forms.TabPage();
			AnalyzerJoiners = new System.Windows.Forms.ListView();
			columnHeader16 = new System.Windows.Forms.ColumnHeader();
			tabPage4 = new System.Windows.Forms.TabPage();
			Violators = new System.Windows.Forms.ListView();
			columnHeader13 = new System.Windows.Forms.ColumnHeader();
			tabPage2 = new System.Windows.Forms.TabPage();
			MemberJoiners = new System.Windows.Forms.ListView();
			columnHeader7 = new System.Windows.Forms.ColumnHeader();
			tabPage1 = new System.Windows.Forms.TabPage();
			Votes = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			Tab = new System.Windows.Forms.TabControl();
			tabPage3 = new System.Windows.Forms.TabPage();
			MemberLeavers = new System.Windows.Forms.ListView();
			columnHeader10 = new System.Windows.Forms.ColumnHeader();
			tabPage9 = new System.Windows.Forms.TabPage();
			FundJoiners = new System.Windows.Forms.ListView();
			columnHeader20 = new System.Windows.Forms.ColumnHeader();
			tabPage10 = new System.Windows.Forms.TabPage();
			FundLeavers = new System.Windows.Forms.ListView();
			columnHeader21 = new System.Windows.Forms.ColumnHeader();
			((System.ComponentModel.ISupportInitialize)Round).BeginInit();
			tabPage8.SuspendLayout();
			tabPage7.SuspendLayout();
			tabPage6.SuspendLayout();
			tabPage5.SuspendLayout();
			tabPage4.SuspendLayout();
			tabPage2.SuspendLayout();
			tabPage1.SuspendLayout();
			Tab.SuspendLayout();
			tabPage3.SuspendLayout();
			tabPage9.SuspendLayout();
			tabPage10.SuspendLayout();
			SuspendLayout();
			// 
			// Transactions
			// 
			Transactions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader12, columnHeader1, columnHeader15, columnHeader3, columnHeader4, columnHeader9 });
			Transactions.FullRowSelect = true;
			Transactions.Location = new System.Drawing.Point(584, 42);
			Transactions.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Transactions.Name = "Transactions";
			Transactions.Size = new System.Drawing.Size(1282, 737);
			Transactions.TabIndex = 22;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = System.Windows.Forms.View.Details;
			Transactions.ItemSelectionChanged += Transactions_ItemSelectionChanged;
			// 
			// columnHeader12
			// 
			columnHeader12.Text = "#";
			columnHeader12.Width = 40;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Id";
			columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader1.Width = 40;
			// 
			// columnHeader15
			// 
			columnHeader15.Text = "Nid";
			columnHeader15.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader15.Width = 40;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Signer";
			columnHeader3.Width = 280;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Ops.(n)";
			columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader4.Width = 50;
			// 
			// columnHeader9
			// 
			columnHeader9.Text = "Fee";
			columnHeader9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader9.Width = 120;
			// 
			// Operations
			// 
			Operations.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Operations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader14, columnHeader8, columnHeader11 });
			Operations.FullRowSelect = true;
			Operations.Location = new System.Drawing.Point(584, 818);
			Operations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Operations.Name = "Operations";
			Operations.Size = new System.Drawing.Size(1282, 561);
			Operations.TabIndex = 23;
			Operations.UseCompatibleStateImageBehavior = false;
			Operations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader14
			// 
			columnHeader14.Text = "#";
			columnHeader14.Width = 40;
			// 
			// columnHeader8
			// 
			columnHeader8.Text = "Id";
			columnHeader8.Width = 40;
			// 
			// columnHeader11
			// 
			columnHeader11.Text = "Description";
			columnHeader11.Width = 500;
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label9.Location = new System.Drawing.Point(7, 38);
			label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(84, 27);
			label9.TabIndex = 24;
			label9.Text = "Round";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label2.Location = new System.Drawing.Point(584, 9);
			label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(153, 27);
			label2.TabIndex = 24;
			label2.Text = "Transactions";
			// 
			// label3
			// 
			label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label3.Location = new System.Drawing.Point(584, 785);
			label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(134, 27);
			label3.TabIndex = 24;
			label3.Text = "Operations";
			// 
			// Round
			// 
			Round.Location = new System.Drawing.Point(102, 32);
			Round.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
			Round.Name = "Round";
			Round.Size = new System.Drawing.Size(204, 39);
			Round.TabIndex = 25;
			Round.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			Round.ValueChanged += numericUpDown1_ValueChanged;
			// 
			// InfoFields
			// 
			InfoFields.AutoSize = true;
			InfoFields.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			InfoFields.Location = new System.Drawing.Point(386, 32);
			InfoFields.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			InfoFields.Name = "InfoFields";
			InfoFields.Size = new System.Drawing.Size(70, 81);
			InfoFields.TabIndex = 24;
			InfoFields.Text = "State\r\nTime\r\nHash";
			// 
			// InfoValues
			// 
			InfoValues.AutoSize = true;
			InfoValues.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			InfoValues.Location = new System.Drawing.Point(470, 32);
			InfoValues.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			InfoValues.Name = "InfoValues";
			InfoValues.Size = new System.Drawing.Size(74, 27);
			InfoValues.TabIndex = 24;
			InfoValues.Text = "Round";
			// 
			// tabPage8
			// 
			tabPage8.Controls.Add(Migrations);
			tabPage8.Location = new System.Drawing.Point(8, 46);
			tabPage8.Name = "tabPage8";
			tabPage8.Padding = new System.Windows.Forms.Padding(3);
			tabPage8.Size = new System.Drawing.Size(1866, 1375);
			tabPage8.TabIndex = 7;
			tabPage8.Text = "DomainBids";
			tabPage8.UseVisualStyleBackColor = true;
			// 
			// DomainBids
			// 
			Migrations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Migrations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader19 });
			Migrations.FullRowSelect = true;
			Migrations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Migrations.Location = new System.Drawing.Point(10, 9);
			Migrations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Migrations.Name = "DomainBids";
			Migrations.Size = new System.Drawing.Size(1846, 1357);
			Migrations.TabIndex = 23;
			Migrations.UseCompatibleStateImageBehavior = false;
			Migrations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader19
			// 
			columnHeader19.Text = "";
			columnHeader19.Width = 300;
			// 
			// tabPage7
			// 
			tabPage7.Controls.Add(Emissions);
			tabPage7.Location = new System.Drawing.Point(8, 46);
			tabPage7.Name = "tabPage7";
			tabPage7.Padding = new System.Windows.Forms.Padding(3);
			tabPage7.Size = new System.Drawing.Size(1866, 1375);
			tabPage7.TabIndex = 6;
			tabPage7.Text = "Emissions";
			tabPage7.UseVisualStyleBackColor = true;
			// 
			// Emissions
			// 
			Emissions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Emissions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader18 });
			Emissions.FullRowSelect = true;
			Emissions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Emissions.Location = new System.Drawing.Point(10, 9);
			Emissions.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Emissions.Name = "Emissions";
			Emissions.Size = new System.Drawing.Size(1846, 1357);
			Emissions.TabIndex = 23;
			Emissions.UseCompatibleStateImageBehavior = false;
			Emissions.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader18
			// 
			columnHeader18.Text = "";
			columnHeader18.Width = 300;
			// 
			// tabPage6
			// 
			tabPage6.Controls.Add(AnalyzerLeavers);
			tabPage6.Location = new System.Drawing.Point(8, 46);
			tabPage6.Name = "tabPage6";
			tabPage6.Padding = new System.Windows.Forms.Padding(3);
			tabPage6.Size = new System.Drawing.Size(1866, 1375);
			tabPage6.TabIndex = 5;
			tabPage6.Text = "Analyzer Leavers";
			tabPage6.UseVisualStyleBackColor = true;
			// 
			// AnalyzerLeavers
			// 
			AnalyzerLeavers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			AnalyzerLeavers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader17 });
			AnalyzerLeavers.FullRowSelect = true;
			AnalyzerLeavers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			AnalyzerLeavers.Location = new System.Drawing.Point(10, 9);
			AnalyzerLeavers.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			AnalyzerLeavers.Name = "AnalyzerLeavers";
			AnalyzerLeavers.Size = new System.Drawing.Size(1846, 1357);
			AnalyzerLeavers.TabIndex = 23;
			AnalyzerLeavers.UseCompatibleStateImageBehavior = false;
			AnalyzerLeavers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader17
			// 
			columnHeader17.Text = "";
			columnHeader17.Width = 300;
			// 
			// tabPage5
			// 
			tabPage5.Controls.Add(AnalyzerJoiners);
			tabPage5.Location = new System.Drawing.Point(8, 46);
			tabPage5.Name = "tabPage5";
			tabPage5.Padding = new System.Windows.Forms.Padding(3);
			tabPage5.Size = new System.Drawing.Size(1866, 1375);
			tabPage5.TabIndex = 4;
			tabPage5.Text = "Analyzer Joiners";
			tabPage5.UseVisualStyleBackColor = true;
			// 
			// AnalyzerJoiners
			// 
			AnalyzerJoiners.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			AnalyzerJoiners.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader16 });
			AnalyzerJoiners.FullRowSelect = true;
			AnalyzerJoiners.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			AnalyzerJoiners.Location = new System.Drawing.Point(10, 9);
			AnalyzerJoiners.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			AnalyzerJoiners.Name = "AnalyzerJoiners";
			AnalyzerJoiners.Size = new System.Drawing.Size(1846, 1357);
			AnalyzerJoiners.TabIndex = 23;
			AnalyzerJoiners.UseCompatibleStateImageBehavior = false;
			AnalyzerJoiners.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader16
			// 
			columnHeader16.Text = "";
			columnHeader16.Width = 300;
			// 
			// tabPage4
			// 
			tabPage4.Controls.Add(Violators);
			tabPage4.Location = new System.Drawing.Point(8, 46);
			tabPage4.Name = "tabPage4";
			tabPage4.Padding = new System.Windows.Forms.Padding(3);
			tabPage4.Size = new System.Drawing.Size(1866, 1375);
			tabPage4.TabIndex = 3;
			tabPage4.Text = "Violators";
			tabPage4.UseVisualStyleBackColor = true;
			// 
			// Violators
			// 
			Violators.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Violators.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader13 });
			Violators.FullRowSelect = true;
			Violators.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Violators.Location = new System.Drawing.Point(10, 9);
			Violators.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Violators.Name = "Violators";
			Violators.Size = new System.Drawing.Size(1846, 1357);
			Violators.TabIndex = 22;
			Violators.UseCompatibleStateImageBehavior = false;
			Violators.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader13
			// 
			columnHeader13.Text = "";
			columnHeader13.Width = 300;
			// 
			// tabPage2
			// 
			tabPage2.Controls.Add(MemberJoiners);
			tabPage2.Location = new System.Drawing.Point(8, 46);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(1866, 1375);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Member Joiners";
			tabPage2.UseVisualStyleBackColor = true;
			// 
			// MemberJoiners
			// 
			MemberJoiners.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			MemberJoiners.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader7 });
			MemberJoiners.FullRowSelect = true;
			MemberJoiners.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			MemberJoiners.Location = new System.Drawing.Point(10, 9);
			MemberJoiners.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			MemberJoiners.Name = "MemberJoiners";
			MemberJoiners.Size = new System.Drawing.Size(1846, 1357);
			MemberJoiners.TabIndex = 22;
			MemberJoiners.UseCompatibleStateImageBehavior = false;
			MemberJoiners.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader7
			// 
			columnHeader7.Text = "";
			columnHeader7.Width = 300;
			// 
			// tabPage1
			// 
			tabPage1.Controls.Add(Votes);
			tabPage1.Controls.Add(Transactions);
			tabPage1.Controls.Add(label2);
			tabPage1.Controls.Add(label3);
			tabPage1.Controls.Add(Operations);
			tabPage1.Location = new System.Drawing.Point(8, 46);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(1876, 1388);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "Votes";
			tabPage1.UseVisualStyleBackColor = true;
			// 
			// Votes
			// 
			Votes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			Votes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2, columnHeader6, columnHeader5 });
			Votes.FullRowSelect = true;
			Votes.Location = new System.Drawing.Point(10, 9);
			Votes.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Votes.Name = "Votes";
			Votes.Size = new System.Drawing.Size(560, 1370);
			Votes.TabIndex = 21;
			Votes.UseCompatibleStateImageBehavior = false;
			Votes.View = System.Windows.Forms.View.Details;
			Votes.ItemSelectionChanged += Blocks_ItemSelectionChanged;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "#";
			columnHeader2.Width = 40;
			// 
			// columnHeader6
			// 
			columnHeader6.Text = "Type";
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "By";
			columnHeader5.Width = 280;
			// 
			// Tab
			// 
			Tab.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Tab.Controls.Add(tabPage1);
			Tab.Controls.Add(tabPage2);
			Tab.Controls.Add(tabPage3);
			Tab.Controls.Add(tabPage4);
			Tab.Controls.Add(tabPage5);
			Tab.Controls.Add(tabPage6);
			Tab.Controls.Add(tabPage7);
			Tab.Controls.Add(tabPage8);
			Tab.Controls.Add(tabPage9);
			Tab.Controls.Add(tabPage10);
			Tab.Location = new System.Drawing.Point(7, 193);
			Tab.Name = "Tab";
			Tab.SelectedIndex = 0;
			Tab.Size = new System.Drawing.Size(1892, 1442);
			Tab.TabIndex = 26;
			// 
			// tabPage3
			// 
			tabPage3.Controls.Add(MemberLeavers);
			tabPage3.Location = new System.Drawing.Point(8, 46);
			tabPage3.Name = "tabPage3";
			tabPage3.Padding = new System.Windows.Forms.Padding(3);
			tabPage3.Size = new System.Drawing.Size(1866, 1375);
			tabPage3.TabIndex = 2;
			tabPage3.Text = "Member Leavers";
			tabPage3.UseVisualStyleBackColor = true;
			// 
			// MemberLeavers
			// 
			MemberLeavers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			MemberLeavers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader10 });
			MemberLeavers.FullRowSelect = true;
			MemberLeavers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			MemberLeavers.Location = new System.Drawing.Point(10, 9);
			MemberLeavers.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			MemberLeavers.Name = "MemberLeavers";
			MemberLeavers.Size = new System.Drawing.Size(1846, 1357);
			MemberLeavers.TabIndex = 22;
			MemberLeavers.UseCompatibleStateImageBehavior = false;
			MemberLeavers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader10
			// 
			columnHeader10.Text = "";
			columnHeader10.Width = 300;
			// 
			// tabPage9
			// 
			tabPage9.Controls.Add(FundJoiners);
			tabPage9.Location = new System.Drawing.Point(8, 46);
			tabPage9.Name = "tabPage9";
			tabPage9.Padding = new System.Windows.Forms.Padding(3);
			tabPage9.Size = new System.Drawing.Size(1866, 1375);
			tabPage9.TabIndex = 8;
			tabPage9.Text = "Fund Joiners";
			tabPage9.UseVisualStyleBackColor = true;
			// 
			// FundJoiners
			// 
			FundJoiners.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			FundJoiners.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader20 });
			FundJoiners.FullRowSelect = true;
			FundJoiners.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			FundJoiners.Location = new System.Drawing.Point(10, 9);
			FundJoiners.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			FundJoiners.Name = "FundJoiners";
			FundJoiners.Size = new System.Drawing.Size(1846, 1357);
			FundJoiners.TabIndex = 23;
			FundJoiners.UseCompatibleStateImageBehavior = false;
			FundJoiners.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader20
			// 
			columnHeader20.Text = "";
			columnHeader20.Width = 300;
			// 
			// tabPage10
			// 
			tabPage10.Controls.Add(FundLeavers);
			tabPage10.Location = new System.Drawing.Point(8, 46);
			tabPage10.Name = "tabPage10";
			tabPage10.Padding = new System.Windows.Forms.Padding(3);
			tabPage10.Size = new System.Drawing.Size(1866, 1375);
			tabPage10.TabIndex = 9;
			tabPage10.Text = "Fund Leavers";
			tabPage10.UseVisualStyleBackColor = true;
			// 
			// FundLeavers
			// 
			FundLeavers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			FundLeavers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader21 });
			FundLeavers.FullRowSelect = true;
			FundLeavers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			FundLeavers.Location = new System.Drawing.Point(10, 9);
			FundLeavers.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			FundLeavers.Name = "FundLeavers";
			FundLeavers.Size = new System.Drawing.Size(1846, 1357);
			FundLeavers.TabIndex = 23;
			FundLeavers.UseCompatibleStateImageBehavior = false;
			FundLeavers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader21
			// 
			columnHeader21.Text = "";
			columnHeader21.Width = 300;
			// 
			// ChainPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Tab);
			Controls.Add(Round);
			Controls.Add(InfoValues);
			Controls.Add(InfoFields);
			Controls.Add(label9);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "ChainPanel";
			Size = new System.Drawing.Size(1902, 1638);
			((System.ComponentModel.ISupportInitialize)Round).EndInit();
			tabPage8.ResumeLayout(false);
			tabPage7.ResumeLayout(false);
			tabPage6.ResumeLayout(false);
			tabPage5.ResumeLayout(false);
			tabPage4.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			Tab.ResumeLayout(false);
			tabPage3.ResumeLayout(false);
			tabPage9.ResumeLayout(false);
			tabPage10.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown Round;
		private System.Windows.Forms.Label InfoFields;
		private System.Windows.Forms.Label InfoValues;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.TabPage tabPage8;
		private System.Windows.Forms.TabPage tabPage7;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.ListView Violators;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ListView MemberJoiners;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.ListView Votes;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.TabControl Tab;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.ListView MemberLeavers;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ListView Migrations;
		private System.Windows.Forms.ColumnHeader columnHeader19;
		private System.Windows.Forms.ListView Emissions;
		private System.Windows.Forms.ColumnHeader columnHeader18;
		private System.Windows.Forms.ListView AnalyzerLeavers;
		private System.Windows.Forms.ColumnHeader columnHeader17;
		private System.Windows.Forms.ListView AnalyzerJoiners;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.TabPage tabPage9;
		private System.Windows.Forms.ListView FundJoiners;
		private System.Windows.Forms.ColumnHeader columnHeader20;
		private System.Windows.Forms.TabPage tabPage10;
		private System.Windows.Forms.ListView FundLeavers;
		private System.Windows.Forms.ColumnHeader columnHeader21;
	}
}
