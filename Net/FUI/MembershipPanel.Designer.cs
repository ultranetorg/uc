
namespace Uccs.Net.FUI
{
	partial class MembershipPanel
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.IP = new System.Windows.Forms.TextBox();
			this.Declare = new System.Windows.Forms.Button();
			this.Candidates = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.Bail = new Uccs.Net.FUI.CoinEdit();
			this.Declarations = new System.Windows.Forms.ListView();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.Blocks = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.NewCandidate = new System.Windows.Forms.ComboBox();
			this.Deactivate = new System.Windows.Forms.Button();
			this.Activate = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.IP);
			this.groupBox1.Controls.Add(this.Declare);
			this.groupBox1.Controls.Add(this.Candidates);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.Bail);
			this.groupBox1.Location = new System.Drawing.Point(0, 1116);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox1.Size = new System.Drawing.Size(893, 523);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Declare Candidacy";
			// 
			// IP
			// 
			this.IP.Location = new System.Drawing.Point(299, 294);
			this.IP.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.IP.Name = "IP";
			this.IP.Size = new System.Drawing.Size(166, 39);
			this.IP.TabIndex = 6;
			// 
			// Declare
			// 
			this.Declare.Location = new System.Drawing.Point(565, 384);
			this.Declare.Margin = new System.Windows.Forms.Padding(17, 19, 17, 19);
			this.Declare.Name = "Declare";
			this.Declare.Size = new System.Drawing.Size(254, 60);
			this.Declare.TabIndex = 1;
			this.Declare.Text = "Declare Candidacy";
			this.Declare.UseVisualStyleBackColor = true;
			this.Declare.Click += new System.EventHandler(this.Declare_Click);
			// 
			// Candidates
			// 
			this.Candidates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Candidates.FormattingEnabled = true;
			this.Candidates.Location = new System.Drawing.Point(299, 83);
			this.Candidates.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Candidates.Name = "Candidates";
			this.Candidates.Size = new System.Drawing.Size(517, 40);
			this.Candidates.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(52, 92);
			this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(237, 27);
			this.label3.TabIndex = 4;
			this.label3.Text = "Candidate\'s Account";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label4.Location = new System.Drawing.Point(243, 303);
			this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(37, 27);
			this.label4.TabIndex = 4;
			this.label4.Text = "IP";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label6.Location = new System.Drawing.Point(299, 228);
			this.label6.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(420, 27);
			this.label6.TabIndex = 4;
			this.label6.Text = "(Set to 0 to withdraw existing declaration)";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(163, 173);
			this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(126, 27);
			this.label1.TabIndex = 4;
			this.label1.Text = "Bail (UNT)";
			// 
			// Bail
			// 
			this.Bail.Location = new System.Drawing.Point(299, 164);
			this.Bail.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Bail.Name = "Bail";
			this.Bail.Size = new System.Drawing.Size(166, 39);
			this.Bail.TabIndex = 3;
			this.Bail.Text = "0.000000";
			this.Bail.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Declarations
			// 
			this.Declarations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.Declarations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader1,
            this.columnHeader2});
			this.Declarations.FullRowSelect = true;
			this.Declarations.Location = new System.Drawing.Point(0, 0);
			this.Declarations.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Declarations.Name = "Declarations";
			this.Declarations.Size = new System.Drawing.Size(994, 1096);
			this.Declarations.TabIndex = 5;
			this.Declarations.UseCompatibleStateImageBehavior = false;
			this.Declarations.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Account";
			this.columnHeader8.Width = 300;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Round";
			this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader1.Width = 100;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Bail";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader2.Width = 100;
			// 
			// Blocks
			// 
			this.Blocks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Blocks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader7,
            this.columnHeader4,
            this.columnHeader5});
			this.Blocks.FullRowSelect = true;
			this.Blocks.Location = new System.Drawing.Point(1023, 0);
			this.Blocks.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.Blocks.Name = "Blocks";
			this.Blocks.Size = new System.Drawing.Size(875, 1096);
			this.Blocks.TabIndex = 6;
			this.Blocks.UseCompatibleStateImageBehavior = false;
			this.Blocks.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Round";
			this.columnHeader3.Width = 100;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Type";
			this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.columnHeader7.Width = 100;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Transactions";
			this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader4.Width = 100;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Date";
			this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader5.Width = 150;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this.NewCandidate);
			this.groupBox2.Controls.Add(this.Deactivate);
			this.groupBox2.Controls.Add(this.Activate);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Location = new System.Drawing.Point(908, 1122);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.groupBox2.Size = new System.Drawing.Size(994, 516);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Active Candidate";
			// 
			// NewCandidate
			// 
			this.NewCandidate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.NewCandidate.FormattingEnabled = true;
			this.NewCandidate.Location = new System.Drawing.Point(158, 286);
			this.NewCandidate.Margin = new System.Windows.Forms.Padding(13, 15, 13, 15);
			this.NewCandidate.Name = "NewCandidate";
			this.NewCandidate.Size = new System.Drawing.Size(517, 40);
			this.NewCandidate.TabIndex = 5;
			// 
			// Deactivate
			// 
			this.Deactivate.Location = new System.Drawing.Point(708, 68);
			this.Deactivate.Margin = new System.Windows.Forms.Padding(17, 19, 17, 19);
			this.Deactivate.Name = "Deactivate";
			this.Deactivate.Size = new System.Drawing.Size(260, 60);
			this.Deactivate.TabIndex = 1;
			this.Deactivate.Text = "Deactivate";
			this.Deactivate.UseVisualStyleBackColor = true;
			this.Deactivate.Click += new System.EventHandler(this.Deactivate_Click);
			// 
			// Activate
			// 
			this.Activate.Location = new System.Drawing.Point(708, 277);
			this.Activate.Margin = new System.Windows.Forms.Padding(17, 19, 17, 19);
			this.Activate.Name = "Activate";
			this.Activate.Size = new System.Drawing.Size(260, 60);
			this.Activate.TabIndex = 1;
			this.Activate.Text = "Activate";
			this.Activate.UseVisualStyleBackColor = true;
			this.Activate.Click += new System.EventHandler(this.Activate_Click);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label7.Location = new System.Drawing.Point(45, 85);
			this.label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(97, 27);
			this.label7.TabIndex = 4;
			this.label7.Text = "Current";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(82, 292);
			this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(62, 27);
			this.label5.TabIndex = 4;
			this.label5.Text = "New";
			// 
			// MembershipPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Blocks);
			this.Controls.Add(this.Declarations);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Name = "MembershipPanel";
			this.Size = new System.Drawing.Size(1902, 1638);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ListView Declarations;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ComboBox Candidates;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button Declare;
		private System.Windows.Forms.Label label1;
		private CoinEdit Bail;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox IP;
		private System.Windows.Forms.ListView Blocks;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox NewCandidate;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button Activate;
		private System.Windows.Forms.Button Deactivate;
		private System.Windows.Forms.Label label7;
	}
}
