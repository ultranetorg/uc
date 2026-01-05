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
		Assets = new ListView();
		columnHeader4 = new ColumnHeader();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		columnHeader3 = new ColumnHeader();
		label2 = new Label();
		Entity = new ComboBox();
		Start = new Button();
		Nets = new ComboBox();
		label4 = new Label();
		Message = new Label();
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
		// Assets
		// 
		Assets.Columns.AddRange(new ColumnHeader[] { columnHeader4, columnHeader1, columnHeader2, columnHeader3 });
		Assets.FullRowSelect = true;
		Assets.Location = new Point(3, 101);
		Assets.Name = "Assets";
		Assets.Size = new Size(794, 496);
		Assets.TabIndex = 8;
		Assets.UseCompatibleStateImageBehavior = false;
		Assets.View = View.Details;
		// 
		// columnHeader4
		// 
		columnHeader4.Text = "Owner";
		columnHeader4.Width = 150;
		// 
		// columnHeader1
		// 
		columnHeader1.Text = "Name";
		columnHeader1.Width = 100;
		// 
		// columnHeader2
		// 
		columnHeader2.Text = "Units";
		columnHeader2.Width = 150;
		// 
		// columnHeader3
		// 
		columnHeader3.Text = "Amount";
		columnHeader3.Width = 150;
		// 
		// label2
		// 
		label2.AutoSize = true;
		label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label2.Location = new Point(273, 55);
		label2.Name = "label2";
		label2.Size = new Size(39, 15);
		label2.TabIndex = 12;
		label2.Text = "Entity";
		// 
		// Entity
		// 
		Entity.FormattingEnabled = true;
		Entity.Location = new Point(318, 52);
		Entity.Margin = new Padding(3, 6, 3, 6);
		Entity.Name = "Entity";
		Entity.Size = new Size(309, 23);
		Entity.TabIndex = 10;
		// 
		// Start
		// 
		Start.Location = new Point(648, 46);
		Start.Name = "Start";
		Start.Size = new Size(149, 32);
		Start.TabIndex = 9;
		Start.Text = "Search";
		Start.UseVisualStyleBackColor = true;
		Start.Click += Start_Click;
		// 
		// Nets
		// 
		Nets.FormattingEnabled = true;
		Nets.Location = new Point(80, 52);
		Nets.Margin = new Padding(3, 6, 3, 6);
		Nets.Name = "Nets";
		Nets.Size = new Size(163, 23);
		Nets.TabIndex = 11;
		// 
		// label4
		// 
		label4.AutoSize = true;
		label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
		label4.Location = new Point(20, 55);
		label4.Name = "label4";
		label4.Size = new Size(54, 15);
		label4.TabIndex = 13;
		label4.Text = "Net/CCP";
		// 
		// Message
		// 
		Message.BackColor = SystemColors.Window;
		Message.Location = new Point(111, 314);
		Message.Name = "Message";
		Message.Size = new Size(578, 71);
		Message.TabIndex = 15;
		Message.Text = "label1";
		Message.TextAlign = ContentAlignment.MiddleCenter;
		// 
		// AssetsPage
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		Controls.Add(Message);
		Controls.Add(label3);
		Controls.Add(Assets);
		Controls.Add(label2);
		Controls.Add(label4);
		Controls.Add(Entity);
		Controls.Add(Nets);
		Controls.Add(Start);
		Name = "AssetsPage";
		Size = new Size(800, 600);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private Label label3;
	private ListView Assets;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private ColumnHeader columnHeader3;
	private Label label2;
	private ComboBox Entity;
	private Button Transfer;
	private ComboBox Nets;
	private Label label4;
	private ColumnHeader columnHeader4;
	private Label Message;
	private ListView Results;
	private Button Start;
}
