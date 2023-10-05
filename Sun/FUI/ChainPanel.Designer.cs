
namespace Uccs.Sun.FUI
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
			Votes = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			Transactions = new System.Windows.Forms.ListView();
			columnHeader12 = new System.Windows.Forms.ColumnHeader();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
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
			tabLeavers = new System.Windows.Forms.TabControl();
			tabPage1 = new System.Windows.Forms.TabPage();
			tabPage2 = new System.Windows.Forms.TabPage();
			Joiners = new System.Windows.Forms.ListView();
			columnHeader7 = new System.Windows.Forms.ColumnHeader();
			tabPage3 = new System.Windows.Forms.TabPage();
			Leavers = new System.Windows.Forms.ListView();
			columnHeader10 = new System.Windows.Forms.ColumnHeader();
			tabPage4 = new System.Windows.Forms.TabPage();
			Violators = new System.Windows.Forms.ListView();
			columnHeader13 = new System.Windows.Forms.ColumnHeader();
			columnHeader15 = new System.Windows.Forms.ColumnHeader();
			((System.ComponentModel.ISupportInitialize)Round).BeginInit();
			tabLeavers.SuspendLayout();
			tabPage1.SuspendLayout();
			tabPage2.SuspendLayout();
			tabPage3.SuspendLayout();
			tabPage4.SuspendLayout();
			SuspendLayout();
			// 
			// Votes
			// 
			Votes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Votes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2, columnHeader6, columnHeader5 });
			Votes.FullRowSelect = true;
			Votes.Location = new System.Drawing.Point(10, 9);
			Votes.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Votes.Name = "Votes";
			Votes.Size = new System.Drawing.Size(796, 1367);
			Votes.TabIndex = 21;
			Votes.UseCompatibleStateImageBehavior = false;
			Votes.View = System.Windows.Forms.View.Details;
			Votes.ItemSelectionChanged += Blocks_ItemSelectionChanged;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "#";
			columnHeader2.Width = 20;
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
			// Transactions
			// 
			Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader12, columnHeader1, columnHeader15, columnHeader3, columnHeader4, columnHeader9 });
			Transactions.FullRowSelect = true;
			Transactions.Location = new System.Drawing.Point(7, 193);
			Transactions.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Transactions.Name = "Transactions";
			Transactions.Size = new System.Drawing.Size(1038, 662);
			Transactions.TabIndex = 22;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = System.Windows.Forms.View.Details;
			Transactions.ItemSelectionChanged += Transactions_ItemSelectionChanged;
			// 
			// columnHeader12
			// 
			columnHeader12.Text = "#";
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Id";
			columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
			Operations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			Operations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader14, columnHeader8, columnHeader11 });
			Operations.FullRowSelect = true;
			Operations.Location = new System.Drawing.Point(7, 915);
			Operations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Operations.Name = "Operations";
			Operations.Size = new System.Drawing.Size(1038, 717);
			Operations.TabIndex = 23;
			Operations.UseCompatibleStateImageBehavior = false;
			Operations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader14
			// 
			columnHeader14.Text = "#";
			// 
			// columnHeader8
			// 
			columnHeader8.Text = "Id";
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
			label2.Location = new System.Drawing.Point(7, 160);
			label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(153, 27);
			label2.TabIndex = 24;
			label2.Text = "Transactions";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label3.Location = new System.Drawing.Point(7, 871);
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
			// tabLeavers
			// 
			tabLeavers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabLeavers.Controls.Add(tabPage1);
			tabLeavers.Controls.Add(tabPage2);
			tabLeavers.Controls.Add(tabPage3);
			tabLeavers.Controls.Add(tabPage4);
			tabLeavers.Location = new System.Drawing.Point(1067, 193);
			tabLeavers.Name = "tabLeavers";
			tabLeavers.SelectedIndex = 0;
			tabLeavers.Size = new System.Drawing.Size(832, 1439);
			tabLeavers.TabIndex = 26;
			// 
			// tabPage1
			// 
			tabPage1.Controls.Add(Votes);
			tabPage1.Location = new System.Drawing.Point(8, 46);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(816, 1385);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "Votes";
			tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			tabPage2.Controls.Add(Joiners);
			tabPage2.Location = new System.Drawing.Point(8, 46);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(816, 1385);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Member Joiners";
			tabPage2.UseVisualStyleBackColor = true;
			// 
			// Joiners
			// 
			Joiners.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Joiners.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader7 });
			Joiners.FullRowSelect = true;
			Joiners.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Joiners.Location = new System.Drawing.Point(10, 9);
			Joiners.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Joiners.Name = "Joiners";
			Joiners.Size = new System.Drawing.Size(796, 1367);
			Joiners.TabIndex = 22;
			Joiners.UseCompatibleStateImageBehavior = false;
			Joiners.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader7
			// 
			columnHeader7.Text = "";
			columnHeader7.Width = 300;
			// 
			// tabPage3
			// 
			tabPage3.Controls.Add(Leavers);
			tabPage3.Location = new System.Drawing.Point(8, 46);
			tabPage3.Name = "tabPage3";
			tabPage3.Padding = new System.Windows.Forms.Padding(3);
			tabPage3.Size = new System.Drawing.Size(816, 1385);
			tabPage3.TabIndex = 2;
			tabPage3.Text = "Member Leavers";
			tabPage3.UseVisualStyleBackColor = true;
			// 
			// Leavers
			// 
			Leavers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Leavers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader10 });
			Leavers.FullRowSelect = true;
			Leavers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Leavers.Location = new System.Drawing.Point(10, 9);
			Leavers.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Leavers.Name = "Leavers";
			Leavers.Size = new System.Drawing.Size(796, 1367);
			Leavers.TabIndex = 22;
			Leavers.UseCompatibleStateImageBehavior = false;
			Leavers.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader10
			// 
			columnHeader10.Text = "";
			columnHeader10.Width = 300;
			// 
			// tabPage4
			// 
			tabPage4.Controls.Add(Violators);
			tabPage4.Location = new System.Drawing.Point(8, 46);
			tabPage4.Name = "tabPage4";
			tabPage4.Padding = new System.Windows.Forms.Padding(3);
			tabPage4.Size = new System.Drawing.Size(816, 1385);
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
			Violators.Size = new System.Drawing.Size(796, 1367);
			Violators.TabIndex = 22;
			Violators.UseCompatibleStateImageBehavior = false;
			Violators.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader13
			// 
			columnHeader13.Text = "";
			columnHeader13.Width = 300;
			// 
			// columnHeader15
			// 
			columnHeader15.Text = "Nid";
			columnHeader15.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChainPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(tabLeavers);
			Controls.Add(Round);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(InfoValues);
			Controls.Add(InfoFields);
			Controls.Add(label9);
			Controls.Add(Operations);
			Controls.Add(Transactions);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "ChainPanel";
			Size = new System.Drawing.Size(1902, 1638);
			((System.ComponentModel.ISupportInitialize)Round).EndInit();
			tabLeavers.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			tabPage3.ResumeLayout(false);
			tabPage4.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.ListView Votes;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown Round;
		private System.Windows.Forms.Label InfoFields;
		private System.Windows.Forms.Label InfoValues;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.TabControl tabLeavers;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.ListView Joiners;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ListView Leavers;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ListView Violators;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
	}
}
