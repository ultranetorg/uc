
namespace UC.Net.Node.FUI
{
	partial class InitialsPanel
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
			this.manage = new System.Windows.Forms.GroupBox();
			this.unregister = new System.Windows.Forms.Button();
			this.save = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.nodes = new System.Windows.Forms.TextBox();
			this.zone = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.manage.SuspendLayout();
			this.SuspendLayout();
			// 
			// manage
			// 
			this.manage.Controls.Add(this.unregister);
			this.manage.Controls.Add(this.save);
			this.manage.Location = new System.Drawing.Point(430, 67);
			this.manage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.manage.Name = "manage";
			this.manage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.manage.Size = new System.Drawing.Size(401, 117);
			this.manage.TabIndex = 6;
			this.manage.TabStop = false;
			this.manage.Text = "Manage";
			this.manage.Visible = false;
			// 
			// unregister
			// 
			this.unregister.Enabled = false;
			this.unregister.Location = new System.Drawing.Point(219, 50);
			this.unregister.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.unregister.Name = "unregister";
			this.unregister.Size = new System.Drawing.Size(105, 28);
			this.unregister.TabIndex = 1;
			this.unregister.Text = "Remove";
			this.unregister.UseVisualStyleBackColor = true;
			this.unregister.Click += new System.EventHandler(this.unregister_Click);
			// 
			// save
			// 
			this.save.Location = new System.Drawing.Point(83, 50);
			this.save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.save.Name = "save";
			this.save.Size = new System.Drawing.Size(105, 28);
			this.save.TabIndex = 1;
			this.save.Text = "Save";
			this.save.UseVisualStyleBackColor = true;
			this.save.Click += new System.EventHandler(this.register_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(430, 21);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(369, 30);
			this.label2.TabIndex = 5;
			this.label2.Text = "These nodes are used as a first peers on the first run\r\nIt\'s stored in a decentra" +
    "lized manner using special Ethereum contract\r\n";
			// 
			// nodes
			// 
			this.nodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.nodes.Location = new System.Drawing.Point(0, 67);
			this.nodes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.nodes.Multiline = true;
			this.nodes.Name = "nodes";
			this.nodes.ReadOnly = true;
			this.nodes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.nodes.Size = new System.Drawing.Size(401, 700);
			this.nodes.TabIndex = 7;
			// 
			// zone
			// 
			this.zone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.zone.FormattingEnabled = true;
			this.zone.Location = new System.Drawing.Point(70, 21);
			this.zone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.zone.Name = "zone";
			this.zone.Size = new System.Drawing.Size(181, 23);
			this.zone.TabIndex = 8;
			this.zone.SelectedIndexChanged += new System.EventHandler(this.zone_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(0, 24);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Network";
			// 
			// InitialsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.zone);
			this.Controls.Add(this.nodes);
			this.Controls.Add(this.manage);
			this.Controls.Add(this.label2);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "InitialsPanel";
			this.Size = new System.Drawing.Size(1024, 768);
			this.manage.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button unregister;
		private System.Windows.Forms.Button save;
		private System.Windows.Forms.GroupBox manage;
		private System.Windows.Forms.TextBox nodes;
		private System.Windows.Forms.ComboBox zone;
		private System.Windows.Forms.Label label1;
	}
}
