namespace Uccs.Mcv.FUI
{
	partial class McvForm
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
			Navigator = new TreeView();
			place = new Panel();
			SuspendLayout();
			// 
			// Navigator
			// 
			Navigator.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			Navigator.HideSelection = false;
			Navigator.Location = new Point(10, 10);
			Navigator.Margin = new Padding(4, 3, 4, 3);
			Navigator.Name = "Navigator";
			Navigator.Size = new Size(174, 768);
			Navigator.TabIndex = 9;
			Navigator.AfterSelect += navigator_AfterSelect;
			// 
			// place
			// 
			place.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			place.Location = new Point(197, 9);
			place.Margin = new Padding(4, 3, 4, 3);
			place.Name = "place";
			place.Size = new Size(1024, 768);
			place.TabIndex = 10;
			// 
			// McvForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1234, 788);
			Controls.Add(place);
			Controls.Add(Navigator);
			Margin = new Padding(4, 3, 4, 3);
			Name = "McvForm";
			Padding = new Padding(7);
			Text = "Node";
			FormClosing += MainForm_FormClosing;
			ResumeLayout(false);

		}

		#endregion
		protected System.Windows.Forms.TreeView Navigator;
		private System.Windows.Forms.Panel place;
	}
}

