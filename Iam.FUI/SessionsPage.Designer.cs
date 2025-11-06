namespace Uccs.Iam.FUI;

partial class SessionsPage
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
		listView1 = new ListView();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		columnHeader3 = new ColumnHeader();
		button3 = new Button();
		comboBox1 = new ComboBox();
		label1 = new Label();
		comboBox2 = new ComboBox();
		label2 = new Label();
		label3 = new Label();
		SuspendLayout();
		// 
		// listView1
		// 
		listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
		listView1.FullRowSelect = true;
		listView1.Location = new Point(3, 106);
		listView1.Name = "listView1";
		listView1.Size = new Size(642, 491);
		listView1.TabIndex = 0;
		listView1.UseCompatibleStateImageBehavior = false;
		listView1.View = View.Details;
		// 
		// columnHeader1
		// 
		columnHeader1.Text = "Application";
		columnHeader1.Width = 150;
		// 
		// columnHeader2
		// 
		columnHeader2.Text = "Network";
		columnHeader2.Width = 150;
		// 
		// columnHeader3
		// 
		columnHeader3.Text = "Session ID";
		columnHeader3.Width = 300;
		// 
		// button3
		// 
		button3.Location = new Point(666, 106);
		button3.Name = "button3";
		button3.Size = new Size(131, 34);
		button3.TabIndex = 4;
		button3.Text = "Revoke";
		button3.UseVisualStyleBackColor = true;
		// 
		// comboBox1
		// 
		comboBox1.FormattingEnabled = true;
		comboBox1.Location = new Point(49, 56);
		comboBox1.Name = "comboBox1";
		comboBox1.Size = new Size(209, 23);
		comboBox1.TabIndex = 5;
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Location = new Point(3, 59);
		label1.Name = "label1";
		label1.Size = new Size(40, 15);
		label1.TabIndex = 6;
		label1.Text = "Wallet";
		// 
		// comboBox2
		// 
		comboBox2.FormattingEnabled = true;
		comboBox2.Location = new Point(368, 56);
		comboBox2.Name = "comboBox2";
		comboBox2.Size = new Size(429, 23);
		comboBox2.TabIndex = 5;
		// 
		// label2
		// 
		label2.AutoSize = true;
		label2.Location = new Point(310, 59);
		label2.Name = "label2";
		label2.Size = new Size(52, 15);
		label2.TabIndex = 6;
		label2.Text = "Account";
		// 
		// label3
		// 
		label3.BackColor = SystemColors.ControlDarkDark;
		label3.Font = new Font("Segoe UI", 11F);
		label3.ForeColor = SystemColors.Control;
		label3.Location = new Point(0, 0);
		label3.Name = "label3";
		label3.Padding = new Padding(8, 0, 0, 0);
		label3.Size = new Size(800, 32);
		label3.TabIndex = 7;
		label3.Text = "Authentications";
		label3.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// SessionsPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(label3);
		Controls.Add(label2);
		Controls.Add(label1);
		Controls.Add(comboBox2);
		Controls.Add(comboBox1);
		Controls.Add(button3);
		Controls.Add(listView1);
		Name = "SessionsPage";
		Size = new Size(800, 600);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private ListView listView1;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private Button button3;
	private ColumnHeader columnHeader3;
	private ComboBox comboBox1;
	private Label label1;
	private ComboBox comboBox2;
	private Label label2;
	private Label label3;
}
