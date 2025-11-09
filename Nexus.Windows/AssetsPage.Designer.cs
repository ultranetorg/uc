namespace Uccs.Nexus.Windows;

partial class AssetsPage
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
		label3 = new Label();
		listView1 = new ListView();
		columnHeader4 = new ColumnHeader();
		columnHeader5 = new ColumnHeader();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		columnHeader3 = new ColumnHeader();
		label2 = new Label();
		label1 = new Label();
		comboBox2 = new ComboBox();
		comboBox1 = new ComboBox();
		button3 = new Button();
		comboBox3 = new ComboBox();
		label4 = new Label();
		SuspendLayout();
		// 
		// label3
		// 
		label3.BackColor = SystemColors.ControlDarkDark;
		label3.Font = new Font("Segoe UI", 12F);
		label3.ForeColor = SystemColors.Control;
		label3.Location = new Point(0, 0);
		label3.Name = "label3";
		label3.Padding = new Padding(8, 0, 0, 0);
		label3.Size = new Size(800, 32);
		label3.TabIndex = 14;
		label3.Text = "Assets";
		label3.TextAlign = ContentAlignment.MiddleLeft;
		// 
		// listView1
		// 
		listView1.Columns.AddRange(new ColumnHeader[] { columnHeader4, columnHeader5, columnHeader1, columnHeader2, columnHeader3 });
		listView1.FullRowSelect = true;
		listView1.Location = new Point(3, 178);
		listView1.Name = "listView1";
		listView1.Size = new Size(577, 419);
		listView1.TabIndex = 8;
		listView1.UseCompatibleStateImageBehavior = false;
		listView1.View = View.Details;
		// 
		// columnHeader4
		// 
		columnHeader4.Text = "Owner Class";
		columnHeader4.Width = 100;
		// 
		// columnHeader5
		// 
		columnHeader5.Text = "Owner Id";
		columnHeader5.Width = 100;
		// 
		// columnHeader1
		// 
		columnHeader1.Text = "Name";
		columnHeader1.Width = 100;
		// 
		// columnHeader2
		// 
		columnHeader2.Text = "Units";
		columnHeader2.Width = 100;
		// 
		// columnHeader3
		// 
		columnHeader3.Text = "Amount";
		columnHeader3.Width = 100;
		// 
		// label2
		// 
		label2.AutoSize = true;
		label2.Location = new Point(3, 136);
		label2.Name = "label2";
		label2.Size = new Size(52, 15);
		label2.TabIndex = 12;
		label2.Text = "Account";
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Location = new Point(3, 97);
		label1.Name = "label1";
		label1.Size = new Size(40, 15);
		label1.TabIndex = 13;
		label1.Text = "Wallet";
		label1.Click += label1_Click;
		// 
		// comboBox2
		// 
		comboBox2.FormattingEnabled = true;
		comboBox2.Location = new Point(80, 133);
		comboBox2.Name = "comboBox2";
		comboBox2.Size = new Size(429, 23);
		comboBox2.TabIndex = 10;
		// 
		// comboBox1
		// 
		comboBox1.FormattingEnabled = true;
		comboBox1.Location = new Point(80, 94);
		comboBox1.Name = "comboBox1";
		comboBox1.Size = new Size(209, 23);
		comboBox1.TabIndex = 11;
		// 
		// button3
		// 
		button3.Location = new Point(600, 178);
		button3.Name = "button3";
		button3.Size = new Size(197, 34);
		button3.TabIndex = 9;
		button3.Text = "Transfer";
		button3.UseVisualStyleBackColor = true;
		// 
		// comboBox3
		// 
		comboBox3.FormattingEnabled = true;
		comboBox3.Location = new Point(80, 52);
		comboBox3.Name = "comboBox3";
		comboBox3.Size = new Size(209, 23);
		comboBox3.TabIndex = 11;
		// 
		// label4
		// 
		label4.AutoSize = true;
		label4.Location = new Point(3, 55);
		label4.Name = "label4";
		label4.Size = new Size(54, 15);
		label4.TabIndex = 13;
		label4.Text = "Net/CCP";
		label4.Click += label1_Click;
		// 
		// AssetsPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(label3);
		Controls.Add(listView1);
		Controls.Add(label2);
		Controls.Add(label4);
		Controls.Add(label1);
		Controls.Add(comboBox2);
		Controls.Add(comboBox3);
		Controls.Add(comboBox1);
		Controls.Add(button3);
		Name = "AssetsPage";
		Size = new Size(800, 600);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private Label label3;
	private ListView listView1;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private ColumnHeader columnHeader3;
	private Label label2;
	private Label label1;
	private ComboBox comboBox2;
	private ComboBox comboBox1;
	private Button button3;
	private ComboBox comboBox3;
	private Label label4;
	private ColumnHeader columnHeader4;
	private ColumnHeader columnHeader5;
}
