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
		ListViewGroup listViewGroup1 = new ListViewGroup("Default", HorizontalAlignment.Left);
		ListViewGroup listViewGroup2 = new ListViewGroup("Found in the Net", HorizontalAlignment.Left);
		ListViewGroup listViewGroup3 = new ListViewGroup("Local Packages", HorizontalAlignment.Left);
		ListViewItem listViewItem1 = new ListViewItem(new string[] { "23432", "apple", "ios/arm64/7.0.0.0", "Ready", "19 days ago" }, -1);
		ListViewItem listViewItem2 = new ListViewItem(new string[] { "345345", "abobe", "photoshop", "win64", "1 Jan 2030" }, -1);
		ListViewItem listViewItem3 = new ListViewItem(new string[] { "6765", "micsrosot", "windows", "x86", "1 Jan 2030" }, -1);
		ListViewItem listViewItem4 = new ListViewItem(new string[] { "567567556", "ultranetorg", "uos", "winx64", "1 Jan 2030" }, -1);
		panel1 = new Panel();
		pictureBox1 = new PictureBox();
		label1 = new Label();
		Results = new ListView();
		columnHeader0 = new ColumnHeader();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		columnHeader5 = new ColumnHeader();
		columnHeader6 = new ColumnHeader();
		comboBox1 = new ComboBox();
		Search = new Button();
		comboBox2 = new ComboBox();
		Query = new ComboBox();
		Info = new TreeView();
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
		panel1.Size = new Size(1915, 137);
		panel1.TabIndex = 5;
		// 
		// pictureBox1
		// 
		pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
		pictureBox1.Location = new Point(32, 32);
		pictureBox1.Margin = new Padding(30, 34, 0, 0);
		pictureBox1.Name = "pictureBox1";
		pictureBox1.Size = new Size(64, 64);
		pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
		pictureBox1.TabIndex = 5;
		pictureBox1.TabStop = false;
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Font = new Font("Verdana", 13F, FontStyle.Bold, GraphicsUnit.Point, 204);
		label1.ForeColor = SystemColors.Control;
		label1.Location = new Point(122, 42);
		label1.Margin = new Padding(6, 0, 6, 0);
		label1.Name = "label1";
		label1.Size = new Size(297, 42);
		label1.TabIndex = 4;
		label1.Text = "UOS Packages";
		// 
		// Results
		// 
		Results.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		Results.Columns.AddRange(new ColumnHeader[] { columnHeader0, columnHeader1, columnHeader2, columnHeader5, columnHeader6 });
		Results.FullRowSelect = true;
		listViewGroup1.Header = "Default";
		listViewGroup1.Name = "listViewGroup1";
		listViewGroup2.Header = "Found in the Net";
		listViewGroup2.Name = "NetPackages";
		listViewGroup3.Header = "Local Packages";
		listViewGroup3.Name = "LocalPackages";
		Results.Groups.AddRange(new ListViewGroup[] { listViewGroup1, listViewGroup2, listViewGroup3 });
		listViewItem1.Group = listViewGroup2;
		listViewItem2.Group = listViewGroup3;
		listViewItem3.Group = listViewGroup3;
		listViewItem4.Group = listViewGroup3;
		Results.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4 });
		Results.LabelEdit = true;
		Results.Location = new Point(22, 341);
		Results.Margin = new Padding(6);
		Results.Name = "Results";
		Results.Size = new Size(986, 1199);
		Results.TabIndex = 20;
		Results.UseCompatibleStateImageBehavior = false;
		Results.View = View.Details;
		// 
		// columnHeader0
		// 
		columnHeader0.Text = "Id";
		columnHeader0.Width = 100;
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
		columnHeader5.Width = 100;
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
		comboBox1.Location = new Point(1034, 269);
		comboBox1.Margin = new Padding(6, 26, 6, 26);
		comboBox1.Name = "comboBox1";
		comboBox1.Size = new Size(221, 40);
		comboBox1.TabIndex = 23;
		// 
		// Search
		// 
		Search.Location = new Point(1556, 181);
		Search.Margin = new Padding(6);
		Search.Name = "Search";
		Search.Size = new Size(334, 60);
		Search.TabIndex = 19;
		Search.Text = "Search";
		Search.UseVisualStyleBackColor = true;
		Search.Click += Search_Click;
		// 
		// comboBox2
		// 
		comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
		comboBox2.FormattingEnabled = true;
		comboBox2.Location = new Point(1298, 269);
		comboBox2.Margin = new Padding(6, 26, 6, 26);
		comboBox2.Name = "comboBox2";
		comboBox2.Size = new Size(221, 40);
		comboBox2.TabIndex = 23;
		// 
		// Query
		// 
		Query.FormattingEnabled = true;
		Query.Location = new Point(22, 188);
		Query.Margin = new Padding(6);
		Query.Name = "Query";
		Query.Size = new Size(1497, 40);
		Query.TabIndex = 25;
		// 
		// Info
		// 
		Info.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
		Info.Location = new Point(1034, 341);
		Info.Name = "Info";
		Info.Size = new Size(856, 1199);
		Info.TabIndex = 26;
		// 
		// PackagesForm
		// 
		AutoScaleDimensions = new SizeF(13F, 32F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(1917, 1555);
		Controls.Add(Info);
		Controls.Add(Results);
		Controls.Add(Query);
		Controls.Add(comboBox2);
		Controls.Add(comboBox1);
		Controls.Add(Search);
		Controls.Add(panel1);
		Margin = new Padding(6);
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
	private ListView Results;
	private ColumnHeader columnHeader0;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private ComboBox comboBox1;
	private Button Search;
	private ColumnHeader columnHeader5;
	private ColumnHeader columnHeader6;
	private ComboBox comboBox2;
	private ComboBox Query;
	private TreeView Info;
}
