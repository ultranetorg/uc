
namespace Uccs.Mcv.FUI
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
			Transactions = new ListView();
			columnHeader12 = new ColumnHeader();
			columnHeader1 = new ColumnHeader();
			columnHeader15 = new ColumnHeader();
			columnHeader3 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			columnHeader9 = new ColumnHeader();
			Operations = new ListView();
			columnHeader14 = new ColumnHeader();
			columnHeader8 = new ColumnHeader();
			columnHeader11 = new ColumnHeader();
			label9 = new Label();
			label2 = new Label();
			label3 = new Label();
			Round = new NumericUpDown();
			InfoFields = new Label();
			InfoValues = new Label();
			tabPage4 = new TabPage();
			Violators = new ListView();
			columnHeader13 = new ColumnHeader();
			tabPage2 = new TabPage();
			MemberJoiners = new ListView();
			columnHeader7 = new ColumnHeader();
			tabPage1 = new TabPage();
			Votes = new ListView();
			columnHeader2 = new ColumnHeader();
			columnHeader6 = new ColumnHeader();
			columnHeader5 = new ColumnHeader();
			Tab = new TabControl();
			tabPage3 = new TabPage();
			MemberLeavers = new ListView();
			columnHeader10 = new ColumnHeader();
			((System.ComponentModel.ISupportInitialize)Round).BeginInit();
			tabPage4.SuspendLayout();
			tabPage2.SuspendLayout();
			tabPage1.SuspendLayout();
			Tab.SuspendLayout();
			tabPage3.SuspendLayout();
			SuspendLayout();
			// 
			// Transactions
			// 
			Transactions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Transactions.Columns.AddRange(new ColumnHeader[] { columnHeader12, columnHeader1, columnHeader15, columnHeader3, columnHeader4, columnHeader9 });
			Transactions.FullRowSelect = true;
			Transactions.Location = new Point(584, 42);
			Transactions.Margin = new Padding(7, 6, 7, 6);
			Transactions.Name = "Transactions";
			Transactions.Size = new Size(1282, 737);
			Transactions.TabIndex = 22;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = View.Details;
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
			columnHeader1.TextAlign = HorizontalAlignment.Right;
			columnHeader1.Width = 40;
			// 
			// columnHeader15
			// 
			columnHeader15.Text = "Nid";
			columnHeader15.TextAlign = HorizontalAlignment.Right;
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
			columnHeader4.TextAlign = HorizontalAlignment.Right;
			columnHeader4.Width = 50;
			// 
			// columnHeader9
			// 
			columnHeader9.Text = "Fee";
			columnHeader9.TextAlign = HorizontalAlignment.Right;
			columnHeader9.Width = 120;
			// 
			// Operations
			// 
			Operations.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Operations.Columns.AddRange(new ColumnHeader[] { columnHeader14, columnHeader8, columnHeader11 });
			Operations.FullRowSelect = true;
			Operations.Location = new Point(584, 818);
			Operations.Margin = new Padding(7, 6, 7, 6);
			Operations.Name = "Operations";
			Operations.Size = new Size(1282, 561);
			Operations.TabIndex = 23;
			Operations.UseCompatibleStateImageBehavior = false;
			Operations.View = View.Details;
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
			label9.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label9.Location = new Point(7, 38);
			label9.Margin = new Padding(7, 0, 7, 0);
			label9.Name = "label9";
			label9.Size = new Size(84, 27);
			label9.TabIndex = 24;
			label9.Text = "Round";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label2.Location = new Point(584, 9);
			label2.Margin = new Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new Size(153, 27);
			label2.TabIndex = 24;
			label2.Text = "Transactions";
			// 
			// label3
			// 
			label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			label3.AutoSize = true;
			label3.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label3.Location = new Point(584, 785);
			label3.Margin = new Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new Size(134, 27);
			label3.TabIndex = 24;
			label3.Text = "Operations";
			// 
			// Round
			// 
			Round.Location = new Point(102, 32);
			Round.Margin = new Padding(4, 2, 4, 2);
			Round.Name = "Round";
			Round.Size = new Size(204, 39);
			Round.TabIndex = 25;
			Round.TextAlign = HorizontalAlignment.Right;
			Round.ValueChanged += numericUpDown1_ValueChanged;
			// 
			// InfoFields
			// 
			InfoFields.AutoSize = true;
			InfoFields.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			InfoFields.Location = new Point(386, 32);
			InfoFields.Margin = new Padding(7, 0, 7, 0);
			InfoFields.Name = "InfoFields";
			InfoFields.Size = new Size(70, 81);
			InfoFields.TabIndex = 24;
			InfoFields.Text = "State\r\nTime\r\nHash";
			// 
			// InfoValues
			// 
			InfoValues.AutoSize = true;
			InfoValues.Font = new Font("Tahoma", 8.25F);
			InfoValues.Location = new Point(470, 32);
			InfoValues.Margin = new Padding(7, 0, 7, 0);
			InfoValues.Name = "InfoValues";
			InfoValues.Size = new Size(74, 27);
			InfoValues.TabIndex = 24;
			InfoValues.Text = "Round";
			// 
			// tabPage4
			// 
			tabPage4.Controls.Add(Violators);
			tabPage4.Location = new Point(8, 46);
			tabPage4.Name = "tabPage4";
			tabPage4.Padding = new Padding(3);
			tabPage4.Size = new Size(1876, 1388);
			tabPage4.TabIndex = 3;
			tabPage4.Text = "Violators";
			tabPage4.UseVisualStyleBackColor = true;
			// 
			// Violators
			// 
			Violators.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Violators.Columns.AddRange(new ColumnHeader[] { columnHeader13 });
			Violators.FullRowSelect = true;
			Violators.HeaderStyle = ColumnHeaderStyle.None;
			Violators.Location = new Point(10, 9);
			Violators.Margin = new Padding(7, 6, 7, 6);
			Violators.Name = "Violators";
			Violators.Size = new Size(1846, 1357);
			Violators.TabIndex = 22;
			Violators.UseCompatibleStateImageBehavior = false;
			Violators.View = View.Details;
			// 
			// columnHeader13
			// 
			columnHeader13.Text = "";
			columnHeader13.Width = 300;
			// 
			// tabPage2
			// 
			tabPage2.Controls.Add(MemberJoiners);
			tabPage2.Location = new Point(8, 46);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new Padding(3);
			tabPage2.Size = new Size(1876, 1388);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Member Joiners";
			tabPage2.UseVisualStyleBackColor = true;
			// 
			// MemberJoiners
			// 
			MemberJoiners.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			MemberJoiners.Columns.AddRange(new ColumnHeader[] { columnHeader7 });
			MemberJoiners.FullRowSelect = true;
			MemberJoiners.HeaderStyle = ColumnHeaderStyle.None;
			MemberJoiners.Location = new Point(10, 9);
			MemberJoiners.Margin = new Padding(7, 6, 7, 6);
			MemberJoiners.Name = "MemberJoiners";
			MemberJoiners.Size = new Size(1846, 1357);
			MemberJoiners.TabIndex = 22;
			MemberJoiners.UseCompatibleStateImageBehavior = false;
			MemberJoiners.View = View.Details;
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
			tabPage1.Location = new Point(8, 46);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new Padding(3);
			tabPage1.Size = new Size(1876, 1388);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "Votes";
			tabPage1.UseVisualStyleBackColor = true;
			// 
			// Votes
			// 
			Votes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Votes.Columns.AddRange(new ColumnHeader[] { columnHeader2, columnHeader6, columnHeader5 });
			Votes.FullRowSelect = true;
			Votes.Location = new Point(10, 9);
			Votes.Margin = new Padding(7, 6, 7, 6);
			Votes.Name = "Votes";
			Votes.Size = new Size(560, 1370);
			Votes.TabIndex = 21;
			Votes.UseCompatibleStateImageBehavior = false;
			Votes.View = View.Details;
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
			Tab.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Tab.Controls.Add(tabPage1);
			Tab.Controls.Add(tabPage2);
			Tab.Controls.Add(tabPage3);
			Tab.Controls.Add(tabPage4);
			Tab.Location = new Point(7, 193);
			Tab.Name = "Tab";
			Tab.SelectedIndex = 0;
			Tab.Size = new Size(1892, 1442);
			Tab.TabIndex = 26;
			// 
			// tabPage3
			// 
			tabPage3.Controls.Add(MemberLeavers);
			tabPage3.Location = new Point(8, 46);
			tabPage3.Name = "tabPage3";
			tabPage3.Padding = new Padding(3);
			tabPage3.Size = new Size(1876, 1388);
			tabPage3.TabIndex = 2;
			tabPage3.Text = "Member Leavers";
			tabPage3.UseVisualStyleBackColor = true;
			// 
			// MemberLeavers
			// 
			MemberLeavers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			MemberLeavers.Columns.AddRange(new ColumnHeader[] { columnHeader10 });
			MemberLeavers.FullRowSelect = true;
			MemberLeavers.HeaderStyle = ColumnHeaderStyle.None;
			MemberLeavers.Location = new Point(10, 9);
			MemberLeavers.Margin = new Padding(7, 6, 7, 6);
			MemberLeavers.Name = "MemberLeavers";
			MemberLeavers.Size = new Size(1846, 1357);
			MemberLeavers.TabIndex = 22;
			MemberLeavers.UseCompatibleStateImageBehavior = false;
			MemberLeavers.View = View.Details;
			// 
			// columnHeader10
			// 
			columnHeader10.Text = "";
			columnHeader10.Width = 300;
			// 
			// ChainPanel
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Tab);
			Controls.Add(Round);
			Controls.Add(InfoValues);
			Controls.Add(InfoFields);
			Controls.Add(label9);
			Margin = new Padding(7, 6, 7, 6);
			Name = "ChainPanel";
			Size = new Size(1902, 1638);
			((System.ComponentModel.ISupportInitialize)Round).EndInit();
			tabPage4.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			Tab.ResumeLayout(false);
			tabPage3.ResumeLayout(false);
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
	}
}
