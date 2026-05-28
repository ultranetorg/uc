
namespace Uccs.Mcv.FUI
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
			groupBox1 = new GroupBox();
			IP = new TextBox();
			Declare = new Button();
			Candidates = new ComboBox();
			label3 = new Label();
			label4 = new Label();
			label6 = new Label();
			label1 = new Label();
			Bail = new CoinEdit();
			Declarations = new ListView();
			columnHeader8 = new ColumnHeader();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			Blocks = new ListView();
			columnHeader3 = new ColumnHeader();
			columnHeader7 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			columnHeader5 = new ColumnHeader();
			groupBox2 = new GroupBox();
			NewCandidate = new ComboBox();
			Deactivate = new Button();
			Activate = new Button();
			label7 = new Label();
			label5 = new Label();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			groupBox1.Controls.Add(IP);
			groupBox1.Controls.Add(Declare);
			groupBox1.Controls.Add(Candidates);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(label4);
			groupBox1.Controls.Add(label6);
			groupBox1.Controls.Add(label1);
			groupBox1.Controls.Add(Bail);
			groupBox1.Location = new Point(0, 523);
			groupBox1.Margin = new Padding(4, 3, 4, 3);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new Padding(4, 3, 4, 3);
			groupBox1.Size = new Size(481, 245);
			groupBox1.TabIndex = 3;
			groupBox1.TabStop = false;
			groupBox1.Text = "Declare Candidacy";
			// 
			// IP
			// 
			IP.Location = new Point(161, 138);
			IP.Margin = new Padding(7, 7, 7, 7);
			IP.Name = "IP";
			IP.Size = new Size(91, 23);
			IP.TabIndex = 6;
			// 
			// Declare
			// 
			Declare.Location = new Point(304, 180);
			Declare.Margin = new Padding(9, 9, 9, 9);
			Declare.Name = "Declare";
			Declare.Size = new Size(137, 28);
			Declare.TabIndex = 1;
			Declare.Text = "Declare Candidacy";
			Declare.UseVisualStyleBackColor = true;
			Declare.Click += Declare_Click;
			// 
			// Candidates
			// 
			Candidates.DropDownStyle = ComboBoxStyle.DropDownList;
			Candidates.FormattingEnabled = true;
			Candidates.Location = new Point(161, 39);
			Candidates.Margin = new Padding(7, 7, 7, 7);
			Candidates.Name = "Candidates";
			Candidates.Size = new Size(280, 23);
			Candidates.TabIndex = 5;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label3.Location = new Point(28, 43);
			label3.Margin = new Padding(4, 0, 4, 0);
			label3.Name = "label3";
			label3.Size = new Size(122, 13);
			label3.TabIndex = 4;
			label3.Text = "Candidate's Account";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label4.Location = new Point(131, 142);
			label4.Margin = new Padding(4, 0, 4, 0);
			label4.Name = "label4";
			label4.Size = new Size(19, 13);
			label4.TabIndex = 4;
			label4.Text = "IP";
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Tahoma", 8.25F);
			label6.Location = new Point(161, 107);
			label6.Margin = new Padding(4, 0, 4, 0);
			label6.Name = "label6";
			label6.Size = new Size(209, 13);
			label6.TabIndex = 4;
			label6.Text = "(Set to 0 to withdraw existing declaration)";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label1.Location = new Point(88, 81);
			label1.Margin = new Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new Size(62, 13);
			label1.TabIndex = 4;
			label1.Text = "Bail (UNT)";
			// 
			// Bail
			// 
			Bail.Location = new Point(161, 77);
			Bail.Margin = new Padding(7, 7, 7, 7);
			Bail.Name = "Bail";
			Bail.Size = new Size(91, 23);
			Bail.TabIndex = 3;
			Bail.Text = "0.000000";
			Bail.TextAlign = HorizontalAlignment.Right;
			// 
			// Declarations
			// 
			Declarations.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Declarations.Columns.AddRange(new ColumnHeader[] { columnHeader8, columnHeader1, columnHeader2 });
			Declarations.FullRowSelect = true;
			Declarations.Location = new Point(3, 3);
			Declarations.Name = "Declarations";
			Declarations.Size = new Size(534, 513);
			Declarations.TabIndex = 5;
			Declarations.UseCompatibleStateImageBehavior = false;
			Declarations.View = View.Details;
			// 
			// columnHeader8
			// 
			columnHeader8.Text = "Account";
			columnHeader8.Width = 300;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Round";
			columnHeader1.TextAlign = HorizontalAlignment.Right;
			columnHeader1.Width = 100;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Bail";
			columnHeader2.TextAlign = HorizontalAlignment.Right;
			columnHeader2.Width = 100;
			// 
			// Blocks
			// 
			Blocks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			Blocks.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader7, columnHeader4, columnHeader5 });
			Blocks.FullRowSelect = true;
			Blocks.Location = new Point(554, 3);
			Blocks.Name = "Blocks";
			Blocks.Size = new Size(467, 513);
			Blocks.TabIndex = 6;
			Blocks.UseCompatibleStateImageBehavior = false;
			Blocks.View = View.Details;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Round";
			columnHeader3.Width = 100;
			// 
			// columnHeader7
			// 
			columnHeader7.Text = "Type";
			columnHeader7.TextAlign = HorizontalAlignment.Center;
			columnHeader7.Width = 100;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Transactions";
			columnHeader4.TextAlign = HorizontalAlignment.Right;
			columnHeader4.Width = 100;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "Date";
			columnHeader5.TextAlign = HorizontalAlignment.Right;
			columnHeader5.Width = 150;
			// 
			// groupBox2
			// 
			groupBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			groupBox2.Controls.Add(NewCandidate);
			groupBox2.Controls.Add(Deactivate);
			groupBox2.Controls.Add(Activate);
			groupBox2.Controls.Add(label7);
			groupBox2.Controls.Add(label5);
			groupBox2.Location = new Point(489, 526);
			groupBox2.Margin = new Padding(4, 3, 4, 3);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new Padding(4, 3, 4, 3);
			groupBox2.Size = new Size(535, 242);
			groupBox2.TabIndex = 3;
			groupBox2.TabStop = false;
			groupBox2.Text = "Active Candidate";
			// 
			// NewCandidate
			// 
			NewCandidate.DropDownStyle = ComboBoxStyle.DropDownList;
			NewCandidate.FormattingEnabled = true;
			NewCandidate.Location = new Point(85, 134);
			NewCandidate.Margin = new Padding(7, 7, 7, 7);
			NewCandidate.Name = "NewCandidate";
			NewCandidate.Size = new Size(280, 23);
			NewCandidate.TabIndex = 5;
			// 
			// Deactivate
			// 
			Deactivate.Location = new Point(381, 32);
			Deactivate.Margin = new Padding(9, 9, 9, 9);
			Deactivate.Name = "Deactivate";
			Deactivate.Size = new Size(140, 28);
			Deactivate.TabIndex = 1;
			Deactivate.Text = "Deactivate";
			Deactivate.UseVisualStyleBackColor = true;
			Deactivate.Click += Deactivate_Click;
			// 
			// Activate
			// 
			Activate.Location = new Point(381, 130);
			Activate.Margin = new Padding(9, 9, 9, 9);
			Activate.Name = "Activate";
			Activate.Size = new Size(140, 28);
			Activate.TabIndex = 1;
			Activate.Text = "Activate";
			Activate.UseVisualStyleBackColor = true;
			Activate.Click += Activate_Click;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label7.Location = new Point(24, 40);
			label7.Margin = new Padding(4, 0, 4, 0);
			label7.Name = "label7";
			label7.Size = new Size(50, 13);
			label7.TabIndex = 4;
			label7.Text = "Current";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
			label5.Location = new Point(44, 137);
			label5.Margin = new Padding(4, 0, 4, 0);
			label5.Name = "label5";
			label5.Size = new Size(30, 13);
			label5.TabIndex = 4;
			label5.Text = "New";
			// 
			// MembershipPanel
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(Blocks);
			Controls.Add(Declarations);
			Controls.Add(groupBox2);
			Controls.Add(groupBox1);
			Margin = new Padding(4, 3, 4, 3);
			Name = "MembershipPanel";
			Size = new Size(1024, 768);
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			ResumeLayout(false);

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
