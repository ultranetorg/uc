namespace Uccs.Nexus.Windows;

partial class PackagesForm
{
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
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

	#region Windows Form Designer generated code

	/// <summary>
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackagesForm));
		ListViewGroup listViewGroup1 = new ListViewGroup("Found in the Net", HorizontalAlignment.Left);
		ListViewGroup listViewGroup2 = new ListViewGroup("Found in the Net", HorizontalAlignment.Left);
		ListViewGroup listViewGroup3 = new ListViewGroup("Local Packages", HorizontalAlignment.Left);
		ListViewItem listViewItem1 = new ListViewItem(new string[] { "23432", "apple", "ios/arm64/7.0.0.0", "Ready", "19 days ago" }, -1);
		ListViewItem listViewItem2 = new ListViewItem(new string[] { "345345", "abobe", "photoshop", "win64", "5.0.0.0" }, -1);
		ListViewItem listViewItem3 = new ListViewItem(new string[] { "6765", "micsrosot", "windows", "x86", "20.0.0." }, -1);
		ListViewItem listViewItem4 = new ListViewItem(new string[] { "567567556", "ultranetorg", "uos", "winx64", "0.0.1000" }, -1);
		panel1 = new Panel();
		pictureBox1 = new PictureBox();
		label1 = new Label();
		Wallets = new ListView();
		columnHeader0 = new ColumnHeader();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		columnHeader5 = new ColumnHeader();
		columnHeader6 = new ColumnHeader();
		comboBox1 = new ComboBox();
		button2 = new Button();
		treeView1 = new TreeView();
		comboBox2 = new ComboBox();
		comboBox3 = new ComboBox();
		panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
		SuspendLayout();
		// 
		// panel1
		// 
		panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		panel1.BackColor = SystemColors.ControlText;
		panel1.Controls.Add(pictureBox1);
		panel1.Controls.Add(label1);
		panel1.Location = new Point(0, 0);
		panel1.Margin = new Padding(0);
		panel1.Name = "panel1";
		panel1.Size = new Size(1150, 64);
		panel1.TabIndex = 5;
		// 
		// pictureBox1
		// 
		pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
		pictureBox1.Location = new Point(25, 16);
		pictureBox1.Margin = new Padding(16, 16, 0, 0);
		pictureBox1.Name = "pictureBox1";
		pictureBox1.Size = new Size(32, 32);
		pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
		pictureBox1.TabIndex = 5;
		pictureBox1.TabStop = false;
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Font = new Font("Verdana", 13F, FontStyle.Bold, GraphicsUnit.Point, 204);
		label1.ForeColor = SystemColors.Control;
		label1.Location = new Point(64, 20);
		label1.Name = "label1";
		label1.Size = new Size(155, 22);
		label1.TabIndex = 4;
		label1.Text = "UOS Packages";
		// 
		// Wallets
		// 
		Wallets.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		Wallets.Columns.AddRange(new ColumnHeader[] { columnHeader0, columnHeader1, columnHeader2, columnHeader5, columnHeader6 });
		Wallets.FullRowSelect = true;
		listViewGroup1.Header = "Found in the Net";
		listViewGroup1.Name = "listViewGroup1";
		listViewGroup2.Header = "Found in the Net";
		listViewGroup2.Name = "listViewGroup2";
		listViewGroup3.CollapsedState = ListViewGroupCollapsedState.Collapsed;
		listViewGroup3.Header = "Local Packages";
		listViewGroup3.Name = "listViewGroup3";
		Wallets.Groups.AddRange(new ListViewGroup[] { listViewGroup1, listViewGroup2, listViewGroup3 });
		listViewItem1.Group = listViewGroup1;
		listViewItem2.Group = listViewGroup3;
		listViewItem3.Group = listViewGroup3;
		listViewItem4.Group = listViewGroup3;
		Wallets.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4 });
		Wallets.LabelEdit = true;
		Wallets.Location = new Point(12, 164);
		Wallets.Name = "Wallets";
		Wallets.Size = new Size(808, 553);
		Wallets.TabIndex = 20;
		Wallets.UseCompatibleStateImageBehavior = false;
		Wallets.View = View.Details;
		// 
		// columnHeader0
		// 
		columnHeader0.Text = "Id";
		// 
		// columnHeader1
		// 
		columnHeader1.Text = "Author";
		columnHeader1.Width = 150;
		// 
		// columnHeader2
		// 
		columnHeader2.Text = "Name";
		columnHeader2.Width = 400;
		// 
		// columnHeader5
		// 
		columnHeader5.Text = "Status";
		// 
		// columnHeader6
		// 
		columnHeader6.Text = "Last Uplated";
		columnHeader6.Width = 100;
		// 
		// comboBox1
		// 
		comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
		comboBox1.FormattingEnabled = true;
		comboBox1.Location = new Point(557, 126);
		comboBox1.Margin = new Padding(3, 12, 3, 12);
		comboBox1.Name = "comboBox1";
		comboBox1.Size = new Size(121, 23);
		comboBox1.TabIndex = 23;
		// 
		// button2
		// 
		button2.Location = new Point(838, 85);
		button2.Name = "button2";
		button2.Size = new Size(180, 28);
		button2.TabIndex = 19;
		button2.Text = "Search";
		button2.UseVisualStyleBackColor = true;
		// 
		// treeView1
		// 
		treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
		treeView1.Location = new Point(838, 164);
		treeView1.Name = "treeView1";
		treeView1.Size = new Size(299, 553);
		treeView1.TabIndex = 24;
		// 
		// comboBox2
		// 
		comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
		comboBox2.FormattingEnabled = true;
		comboBox2.Location = new Point(699, 126);
		comboBox2.Margin = new Padding(3, 12, 3, 12);
		comboBox2.Name = "comboBox2";
		comboBox2.Size = new Size(121, 23);
		comboBox2.TabIndex = 23;
		// 
		// comboBox3
		// 
		comboBox3.FormattingEnabled = true;
		comboBox3.Location = new Point(12, 88);
		comboBox3.Name = "comboBox3";
		comboBox3.Size = new Size(808, 23);
		comboBox3.TabIndex = 25;
		// 
		// PackagesForm
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(1149, 729);
		Controls.Add(comboBox3);
		Controls.Add(treeView1);
		Controls.Add(comboBox2);
		Controls.Add(comboBox1);
		Controls.Add(Wallets);
		Controls.Add(button2);
		Controls.Add(panel1);
		Name = "PackagesForm";
		StartPosition = FormStartPosition.CenterScreen;
		Text = "UOS Packages";
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
		ResumeLayout(false);
	}

	#endregion
	private Label label1;
	private Panel panel1;
	private PictureBox pictureBox1;
	private ListView Wallets;
	private ColumnHeader columnHeader0;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private ComboBox comboBox1;
	private Button button2;
	private ColumnHeader columnHeader5;
	private TreeView treeView1;
	private ColumnHeader columnHeader6;
	private ComboBox comboBox2;
	private ComboBox comboBox3;
}
