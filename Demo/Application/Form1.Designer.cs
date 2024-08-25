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
			pictureBox1 = new PictureBox();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// userControla1
			// 
			userControla1.BorderStyle = BorderStyle.FixedSingle;
			userControla1.Location = new Point(6, 6);
			userControla1.Margin = new Padding(1, 0, 1, 0);
			userControla1.Name = "userControla1";
			userControla1.Size = new Size(379, 583);
			userControla1.TabIndex = 0;
			// 
			// pictureBox1
			// 
			pictureBox1.Image = Properties.Resources.exolon_a;
			pictureBox1.Location = new Point(398, 6);
			pictureBox1.Margin = new Padding(2, 1, 2, 1);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(437, 583);
			pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
			pictureBox1.TabIndex = 1;
			pictureBox1.TabStop = false;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(849, 598);
			Controls.Add(pictureBox1);
			Controls.Add(userControla1);
			Margin = new Padding(2, 1, 2, 1);
			Name = "Form1";
			Text = "Form1";
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private DemoLibraryA.UserControlA userControla1;
		public PictureBox pictureBox1;
	}
}