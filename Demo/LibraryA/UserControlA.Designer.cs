﻿namespace DemoLibraryA
{
	partial class UserControlA
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
			controlaa1 = new DemoLibraryAA.ControlAA();
			label1 = new Label();
			SuspendLayout();
			// 
			// controlaa1
			// 
			controlaa1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			controlaa1.BackgroundImageLayout = ImageLayout.None;
			controlaa1.BorderStyle = BorderStyle.FixedSingle;
			controlaa1.Location = new Point(104, 149);
			controlaa1.Name = "controlaa1";
			controlaa1.Size = new Size(578, 368);
			controlaa1.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(-1, 0);
			label1.Name = "label1";
			label1.Size = new Size(193, 32);
			label1.TabIndex = 1;
			label1.Text = "Library A Control";
			// 
			// UserControlA
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			BorderStyle = BorderStyle.FixedSingle;
			Controls.Add(label1);
			Controls.Add(controlaa1);
			Name = "UserControlA";
			Size = new Size(993, 1176);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DemoLibraryAA.ControlAA controlaa1;
		private Label label1;
	}
}