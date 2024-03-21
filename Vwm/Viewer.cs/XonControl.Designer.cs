namespace UC.Vwm.Viewer
{
	partial class XonControl
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
			this._gbMwx = new System.Windows.Forms.GroupBox();
			this._treeVwm = new System.Windows.Forms.TreeView();
			this._txtData = new System.Windows.Forms.TextBox();
			this._gbMwx.SuspendLayout();
			this.SuspendLayout();
			// 
			// _gbMwx
			// 
			this._gbMwx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._gbMwx.Controls.Add(this._treeVwm);
			this._gbMwx.Controls.Add(this._txtData);
			this._gbMwx.Location = new System.Drawing.Point(5, 5);
			this._gbMwx.Name = "_gbMwx";
			this._gbMwx.Size = new System.Drawing.Size(1430, 1050);
			this._gbMwx.TabIndex = 14;
			this._gbMwx.TabStop = false;
			this._gbMwx.Text = "Mwx";
			// 
			// _treeVwm
			// 
			this._treeVwm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this._treeVwm.BackColor = System.Drawing.SystemColors.Window;
			this._treeVwm.HideSelection = false;
			this._treeVwm.Indent = 19;
			this._treeVwm.Location = new System.Drawing.Point(8, 16);
			this._treeVwm.Name = "_treeVwm";
			this._treeVwm.Size = new System.Drawing.Size(224, 1026);
			this._treeVwm.TabIndex = 6;
			// 
			// _txtData
			// 
			this._txtData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._txtData.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this._txtData.Location = new System.Drawing.Point(240, 16);
			this._txtData.Multiline = true;
			this._txtData.Name = "_txtData";
			this._txtData.ReadOnly = true;
			this._txtData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this._txtData.Size = new System.Drawing.Size(1182, 1026);
			this._txtData.TabIndex = 10;
			this._txtData.WordWrap = false;
			// 
			// MwxControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._gbMwx);
			this.Name = "MwxControl";
			this.Size = new System.Drawing.Size(1442, 1062);
			this._gbMwx.ResumeLayout(false);
			this._gbMwx.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox _gbMwx;
		private System.Windows.Forms.TreeView _treeVwm;
		private System.Windows.Forms.TextBox _txtData;
	}
}
