
namespace Uccs.Rdn.FUI
{
	partial class FlowControlForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Logview = new Uccs.Rdn.FUI.Logbox();
			this.Abort = new System.Windows.Forms.Button();
			this.Exit = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// Log
			// 
			this.Logview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Logview.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Logview.Location = new System.Drawing.Point(12, 12);
			this.Logview.Log = null;
			this.Logview.Margin = new System.Windows.Forms.Padding(6);
			this.Logview.Multiline = true;
			this.Logview.Name = "Log";
			this.Logview.ReadOnly = true;
			this.Logview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.Logview.Size = new System.Drawing.Size(775, 134);
			this.Logview.TabIndex = 0;
			this.Logview.WordWrap = false;
			// 
			// Abort
			// 
			this.Abort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Abort.Location = new System.Drawing.Point(12, 158);
			this.Abort.Margin = new System.Windows.Forms.Padding(6);
			this.Abort.Name = "Abort";
			this.Abort.Size = new System.Drawing.Size(120, 24);
			this.Abort.TabIndex = 1;
			this.Abort.Text = "Abort";
			this.Abort.UseVisualStyleBackColor = true;
			this.Abort.Click += new System.EventHandler(this.Abort_Click);
			// 
			// Exit
			// 
			this.Exit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Exit.Location = new System.Drawing.Point(664, 158);
			this.Exit.Margin = new System.Windows.Forms.Padding(6);
			this.Exit.Name = "Exit";
			this.Exit.Size = new System.Drawing.Size(120, 24);
			this.Exit.TabIndex = 1;
			this.Exit.Text = "Close";
			this.Exit.UseVisualStyleBackColor = true;
			this.Exit.Click += new System.EventHandler(this.Exit_Click);
			// 
			// FlowControlForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(799, 197);
			this.Controls.Add(this.Exit);
			this.Controls.Add(this.Abort);
			this.Controls.Add(this.Logview);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FlowControlForm";
			this.Text = "FlowControlForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Logbox Logview;
		private System.Windows.Forms.Button Abort;
		private System.Windows.Forms.Button Exit;
	}
}