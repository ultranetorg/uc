namespace Uccs.Demo.Application
{
	partial class Form1
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
			userControla1 = new DemoLibraryA.UserControlA();
			SuspendLayout();
			// 
			// userControla1
			// 
			userControla1.BorderStyle = BorderStyle.FixedSingle;
			userControla1.Location = new Point(12, 12);
			userControla1.Name = "userControla1";
			userControla1.Size = new Size(705, 1242);
			userControla1.TabIndex = 0;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1576, 1275);
			Controls.Add(userControla1);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
		}

		#endregion

		private DemoLibraryA.UserControlA userControla1;
	}
}