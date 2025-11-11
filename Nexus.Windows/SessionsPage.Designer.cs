namespace Uccs.Nexus.Windows;

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
		Sessions = new ListView();
		columnHeader1 = new ColumnHeader();
		columnHeader2 = new ColumnHeader();
		columnHeader3 = new ColumnHeader();
		Revoke = new Button();
		Wallets = new ComboBox();
		label1 = new Label();
		Accounts = new ComboBox();
		label2 = new Label();
		label3 = new Label();
		SuspendLayout();
		// 
		// Sessions
		// 
		Sessions.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
		Sessions.FullRowSelect = true;
		Sessions.Location = new Point(3, 106);
		Sessions.Name = "Sessions";
		Sessions.Size = new Size(642, 491);
		Sessions.TabIndex = 0;
		Sessions.UseCompatibleStateImageBehavior = false;
		Sessions.View = View.Details;
		Sessions.ItemSelectionChanged += Sessions_ItemSelectionChanged;
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
		// Revoke
		// 
		Revoke.Location = new Point(666, 106);
		Revoke.Name = "Revoke";
		Revoke.Size = new Size(131, 34);
		Revoke.TabIndex = 4;
		Revoke.Text = "Revoke";
		Revoke.UseVisualStyleBackColor = true;
		Revoke.Click += Revoke_Click;
		// 
		// Wallets
		// 
		Wallets.DropDownStyle = ComboBoxStyle.DropDownList;
		Wallets.FormattingEnabled = true;
		Wallets.Location = new Point(49, 56);
		Wallets.Name = "Wallets";
		Wallets.Size = new Size(240, 23);
		Wallets.TabIndex = 5;
		Wallets.SelectionChangeCommitted += Wallets_SelectionChangeCommitted;
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
		// Accounts
		// 
		Accounts.DropDownStyle = ComboBoxStyle.DropDownList;
		Accounts.FormattingEnabled = true;
		Accounts.Location = new Point(368, 56);
		Accounts.Name = "Accounts";
		Accounts.Size = new Size(429, 23);
		Accounts.TabIndex = 5;
		Accounts.SelectionChangeCommitted += Accounts_SelectionChangeCommitted;
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
		Controls.Add(Accounts);
		Controls.Add(Wallets);
		Controls.Add(Revoke);
		Controls.Add(Sessions);
		Name = "SessionsPage";
		Size = new Size(800, 600);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private ListView Sessions;
	private ColumnHeader columnHeader1;
	private ColumnHeader columnHeader2;
	private Button Revoke;
	private ColumnHeader columnHeader3;
	private ComboBox Wallets;
	private Label label1;
	private ComboBox Accounts;
	private Label label2;
	private Label label3;
}
