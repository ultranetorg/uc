
namespace UC.Net.Node.FUI
{
	partial class ExplorerPanel
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
			this.Blocks = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.Transactions = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.Operations = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.label9 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.Round = new System.Windows.Forms.NumericUpDown();
			this.InfoFields = new System.Windows.Forms.Label();
			this.InfoValues = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.Round)).BeginInit();
			this.SuspendLayout();
			// 
			// Blocks
			// 
			this.Blocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.Blocks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader5});
			this.Blocks.FullRowSelect = true;
			this.Blocks.HideSelection = false;
			this.Blocks.Location = new System.Drawing.Point(0, 183);
			this.Blocks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Blocks.Name = "Blocks";
			this.Blocks.Size = new System.Drawing.Size(242, 585);
			this.Blocks.TabIndex = 21;
			this.Blocks.UseCompatibleStateImageBehavior = false;
			this.Blocks.View = System.Windows.Forms.View.Details;
			this.Blocks.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.Blocks_ItemSelectionChanged);
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "#";
			this.columnHeader2.Width = 20;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "By";
			this.columnHeader5.Width = 150;
			// 
			// Transactions
			// 
			this.Transactions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader7});
			this.Transactions.FullRowSelect = true;
			this.Transactions.HideSelection = false;
			this.Transactions.Location = new System.Drawing.Point(254, 183);
			this.Transactions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Transactions.Name = "Transactions";
			this.Transactions.Size = new System.Drawing.Size(326, 585);
			this.Transactions.TabIndex = 22;
			this.Transactions.UseCompatibleStateImageBehavior = false;
			this.Transactions.View = System.Windows.Forms.View.Details;
			this.Transactions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.Transactions_ItemSelectionChanged);
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Signer";
			this.columnHeader3.Width = 150;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Id";
			this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader4.Width = 30;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Status";
			this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader7.Width = 100;
			// 
			// Operations
			// 
			this.Operations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Operations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader6});
			this.Operations.FullRowSelect = true;
			this.Operations.HideSelection = false;
			this.Operations.Location = new System.Drawing.Point(593, 183);
			this.Operations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Operations.Name = "Operations";
			this.Operations.Size = new System.Drawing.Size(431, 585);
			this.Operations.TabIndex = 23;
			this.Operations.UseCompatibleStateImageBehavior = false;
			this.Operations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Operation";
			this.columnHeader1.Width = 280;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Status";
			this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader6.Width = 100;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(19, 18);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(43, 13);
			this.label9.TabIndex = 24;
			this.label9.Text = "Round";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(4, 155);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 13);
			this.label1.TabIndex = 24;
			this.label1.Text = "Blocks";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(254, 155);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 13);
			this.label2.TabIndex = 24;
			this.label2.Text = "Transactions";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(593, 155);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 13);
			this.label3.TabIndex = 24;
			this.label3.Text = "Operations";
			// 
			// Round
			// 
			this.Round.Location = new System.Drawing.Point(75, 15);
			this.Round.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.Round.Name = "Round";
			this.Round.Size = new System.Drawing.Size(110, 23);
			this.Round.TabIndex = 25;
			this.Round.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.Round.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			// 
			// InfoFields
			// 
			this.InfoFields.AutoSize = true;
			this.InfoFields.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.InfoFields.Location = new System.Drawing.Point(18, 53);
			this.InfoFields.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.InfoFields.Name = "InfoFields";
			this.InfoFields.Size = new System.Drawing.Size(63, 78);
			this.InfoFields.TabIndex = 24;
			this.InfoFields.Text = "State :\r\nTime :\r\nHash :\r\nJoiners :\r\nLeavers :\r\nViolators :";
			// 
			// InfoValues
			// 
			this.InfoValues.AutoSize = true;
			this.InfoValues.Location = new System.Drawing.Point(98, 53);
			this.InfoValues.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.InfoValues.Name = "InfoValues";
			this.InfoValues.Size = new System.Drawing.Size(42, 15);
			this.InfoValues.TabIndex = 24;
			this.InfoValues.Text = "Round";
			// 
			// ExplorerPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Round);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.InfoValues);
			this.Controls.Add(this.InfoFields);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.Operations);
			this.Controls.Add(this.Transactions);
			this.Controls.Add(this.Blocks);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "ExplorerPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			((System.ComponentModel.ISupportInitialize)(this.Round)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ListView Blocks;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ListView Transactions;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.NumericUpDown Round;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.Label InfoFields;
		private System.Windows.Forms.Label InfoValues;
	}
}
