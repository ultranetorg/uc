
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
			Blocks = new System.Windows.Forms.ListView();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			columnHeader5 = new System.Windows.Forms.ColumnHeader();
			Transactions = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			Operations = new System.Windows.Forms.ListView();
			label9 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			Round = new System.Windows.Forms.NumericUpDown();
			InfoFields = new System.Windows.Forms.Label();
			InfoValues = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)Round).BeginInit();
			SuspendLayout();
			// 
			// Blocks
			// 
			Blocks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2, columnHeader6, columnHeader5 });
			Blocks.FullRowSelect = true;
			Blocks.Location = new System.Drawing.Point(7, 286);
			Blocks.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Blocks.Name = "Blocks";
			Blocks.Size = new System.Drawing.Size(930, 572);
			Blocks.TabIndex = 21;
			Blocks.UseCompatibleStateImageBehavior = false;
			Blocks.View = System.Windows.Forms.View.Details;
			Blocks.ItemSelectionChanged += Blocks_ItemSelectionChanged;
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
			Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader3, columnHeader4 });
			Transactions.FullRowSelect = true;
			Transactions.Location = new System.Drawing.Point(982, 286);
			Transactions.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Transactions.Name = "Transactions";
			Transactions.Size = new System.Drawing.Size(913, 572);
			Transactions.TabIndex = 22;
			Transactions.UseCompatibleStateImageBehavior = false;
			Transactions.View = System.Windows.Forms.View.Details;
			Transactions.ItemSelectionChanged += Transactions_ItemSelectionChanged;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Id";
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Signer";
			columnHeader3.Width = 280;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Operations(n)";
			columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnHeader4.Width = 100;
			// 
			// Operations
			// 
			Operations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			Operations.FullRowSelect = true;
			Operations.Location = new System.Drawing.Point(7, 956);
			Operations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Operations.Name = "Operations";
			Operations.Size = new System.Drawing.Size(1888, 676);
			Operations.TabIndex = 23;
			Operations.UseCompatibleStateImageBehavior = false;
			Operations.View = System.Windows.Forms.View.List;
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label9.Location = new System.Drawing.Point(35, 38);
			label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(84, 27);
			label9.TabIndex = 24;
			label9.Text = "Round";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label1.Location = new System.Drawing.Point(17, 239);
			label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(84, 27);
			label1.TabIndex = 24;
			label1.Text = "Blocks";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label2.Location = new System.Drawing.Point(1001, 239);
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
			label3.Location = new System.Drawing.Point(17, 897);
			label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(134, 27);
			label3.TabIndex = 24;
			label3.Text = "Operations";
			// 
			// Round
			// 
			Round.Location = new System.Drawing.Point(139, 32);
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
			InfoFields.Location = new System.Drawing.Point(404, 32);
			InfoFields.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			InfoFields.Name = "InfoFields";
			InfoFields.Size = new System.Drawing.Size(112, 189);
			InfoFields.TabIndex = 24;
			InfoFields.Text = "State\r\nTime\r\nHash\r\nPayloads\r\nJoiners\r\nLeavers\r\nViolators";
			// 
			// InfoValues
			// 
			InfoValues.AutoSize = true;
			InfoValues.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			InfoValues.Location = new System.Drawing.Point(530, 32);
			InfoValues.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			InfoValues.Name = "InfoValues";
			InfoValues.Size = new System.Drawing.Size(74, 27);
			InfoValues.TabIndex = 24;
			InfoValues.Text = "Round";
			// 
			// ChainPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(Round);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(InfoValues);
			Controls.Add(InfoFields);
			Controls.Add(label9);
			Controls.Add(Operations);
			Controls.Add(Transactions);
			Controls.Add(Blocks);
			Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			Name = "ChainPanel";
			Size = new System.Drawing.Size(1902, 1638);
			((System.ComponentModel.ISupportInitialize)Round).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.ListView Blocks;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown Round;
		private System.Windows.Forms.Label InfoFields;
		private System.Windows.Forms.Label InfoValues;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
	}
}
