
namespace UC.Sun.FUI
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
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.Transactions = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.Operations = new System.Windows.Forms.ListView();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.label9 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.Round = new System.Windows.Forms.NumericUpDown();
			this.InfoFields = new System.Windows.Forms.Label();
			this.InfoValues = new System.Windows.Forms.Label();
			this.Operation = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.Round)).BeginInit();
			this.SuspendLayout();
			// 
			// Blocks
			// 
			this.Blocks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader6,
            this.columnHeader5});
			this.Blocks.FullRowSelect = true;
			this.Blocks.HideSelection = false;
			this.Blocks.Location = new System.Drawing.Point(0, 286);
			this.Blocks.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Blocks.Name = "Blocks";
			this.Blocks.Size = new System.Drawing.Size(446, 572);
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
			// columnHeader6
			// 
			this.columnHeader6.Text = "Type";
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "By";
			this.columnHeader5.Width = 150;
			// 
			// Transactions
			// 
			this.Transactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader7});
			this.Transactions.FullRowSelect = true;
			this.Transactions.HideSelection = false;
			this.Transactions.Location = new System.Drawing.Point(472, 286);
			this.Transactions.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Transactions.Name = "Transactions";
			this.Transactions.Size = new System.Drawing.Size(602, 572);
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
			// columnHeader7
			// 
			this.columnHeader7.Text = "Successful Ops.";
			this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader7.Width = 100;
			// 
			// Operations
			// 
			this.Operations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Operations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader1,
            this.columnHeader4});
			this.Operations.FullRowSelect = true;
			this.Operations.HideSelection = false;
			this.Operations.Location = new System.Drawing.Point(1105, 286);
			this.Operations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Operations.Name = "Operations";
			this.Operations.Size = new System.Drawing.Size(797, 572);
			this.Operations.TabIndex = 23;
			this.Operations.UseCompatibleStateImageBehavior = false;
			this.Operations.View = System.Windows.Forms.View.Details;
			this.Operations.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.Operations_ItemSelectionChanged);
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Id";
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Operation";
			this.columnHeader1.Width = 280;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Status";
			this.columnHeader4.Width = 100;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(35, 38);
			this.label9.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(84, 27);
			this.label9.TabIndex = 24;
			this.label9.Text = "Round";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(0, 239);
			this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 27);
			this.label1.TabIndex = 24;
			this.label1.Text = "Blocks";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(472, 239);
			this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(153, 27);
			this.label2.TabIndex = 24;
			this.label2.Text = "Transactions";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(1101, 239);
			this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(134, 27);
			this.label3.TabIndex = 24;
			this.label3.Text = "Operations";
			// 
			// Round
			// 
			this.Round.Location = new System.Drawing.Point(139, 32);
			this.Round.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
			this.Round.Name = "Round";
			this.Round.Size = new System.Drawing.Size(204, 39);
			this.Round.TabIndex = 25;
			this.Round.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.Round.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			// 
			// InfoFields
			// 
			this.InfoFields.AutoSize = true;
			this.InfoFields.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.InfoFields.Location = new System.Drawing.Point(404, 32);
			this.InfoFields.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.InfoFields.Name = "InfoFields";
			this.InfoFields.Size = new System.Drawing.Size(126, 162);
			this.InfoFields.TabIndex = 24;
			this.InfoFields.Text = "State :\r\nTime :\r\nHash :\r\nJoiners :\r\nLeavers :\r\nViolators :";
			// 
			// InfoValues
			// 
			this.InfoValues.AutoSize = true;
			this.InfoValues.Location = new System.Drawing.Point(544, 28);
			this.InfoValues.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.InfoValues.Name = "InfoValues";
			this.InfoValues.Size = new System.Drawing.Size(83, 32);
			this.InfoValues.TabIndex = 24;
			this.InfoValues.Text = "Round";
			// 
			// Operation
			// 
			this.Operation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Operation.Location = new System.Drawing.Point(0, 940);
			this.Operation.Multiline = true;
			this.Operation.Name = "Operation";
			this.Operation.ReadOnly = true;
			this.Operation.Size = new System.Drawing.Size(1902, 698);
			this.Operation.TabIndex = 27;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(0, 891);
			this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(206, 27);
			this.label5.TabIndex = 24;
			this.label5.Text = "Operation Details";
			// 
			// ExplorerPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Operation);
			this.Controls.Add(this.Round);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.InfoValues);
			this.Controls.Add(this.InfoFields);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.Operations);
			this.Controls.Add(this.Transactions);
			this.Controls.Add(this.Blocks);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "ExplorerPanel";
			this.Size = new System.Drawing.Size(1902, 1638);
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
		private System.Windows.Forms.ListView Operations;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.NumericUpDown Round;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.Label InfoFields;
		private System.Windows.Forms.Label InfoValues;
		private System.Windows.Forms.TextBox Operation;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader6;
	}
}
