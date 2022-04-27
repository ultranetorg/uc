using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.Zip;

namespace UC.Vwm.Viewer
{
	public partial class MainForm : Form
	{
		string					_path;
		bool					_isLoaded;
		string					_mruFilePath;
		XonControl				_gbMwx;
	
		float					initialImageWidth;
		float					initialImageHeight;
		UC.Image.DevIL.Image	currentImage;

		public MainForm()
		{
			InitializeComponent();

			string vwmviewer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

			if(!Directory.Exists(vwmviewer))
			{
				Directory.CreateDirectory(vwmviewer);
			}

			_mruFilePath = Path.Combine(vwmviewer, "MRU.txt");

			UpdateMruMenu();

			_gbMwx = new XonControl();
			_gbMwx.Size = _splitMain.Panel2.ClientSize;
			_gbMwx.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			_splitMain.Panel2.Controls.Add(_gbMwx);
		}

		public void LoadFile(string path)
		{
			AddToMRU(path);
			Text = "Vwm/Xon Viewer - " + path;

			_path = path;

			_treeFiles.BeforeSelect -= _treeFile_BeforeSelect;

			_treeFiles.Nodes.Clear();
			_gbMwx.Clear();

			var e = Path.GetExtension(path);
			if(e == ".bon")
				LoadXon(path);
			if(e == ".vwm" || e == ".zip")
				LoadVwm(path);
		}

		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			initialImageWidth = _gbImage.Width - 16;
			initialImageHeight = _gbImage.Height - 16;
		}

		void _miFileRecent_DropDownItemClicked(object sender, EventArgs e)
		{
			ToolStripMenuItem i = sender as ToolStripMenuItem;
			if(File.Exists(i.Text))
			{
				LoadFile(i.Text);
			}
			else
			{
				RemoveFromMRU(i.Text);
			}
		}

		private void UpdateMruMenu()
		{
			for(int i=0; i<_miFile.DropDownItems.Count; i++)
			{
				ToolStripItem item = _miFile.DropDownItems[i];
				if(item is ToolStripMenuItem && item.Tag is string && item.Tag.ToString() == "MruItem")
				{
					_miFile.DropDownItems.RemoveAt(i);
					i--;
				}
			}
						
			if(File.Exists(_mruFilePath))
			{
				int i = 1;
				foreach(string f in File.ReadAllLines(_mruFilePath))
				{
					var item = new ToolStripMenuItem(f, null, new EventHandler(_miFileRecent_DropDownItemClicked));
					item.Tag = "MruItem";
					_miFile.DropDownItems.Insert(_miFile.DropDownItems.IndexOf(_misepMRU)+i, item);
					i++;
				}
			}
		}
		
		void AddToMRU(string s)
		{
			List<string> mru = new List<string>();
			if(File.Exists(_mruFilePath))
			{
				foreach(string i in File.ReadAllLines(_mruFilePath))
				{
					mru.Add(i);
				}
			}
			foreach(var i in mru)
			{
				if(i == s)
				{
					mru.Remove(i);
					break;
				}
			}
			mru.Insert(0, s);

			if(mru.Count > 10)
			{
				mru.RemoveRange(10, mru.Count-10);
			}

			File.WriteAllLines(_mruFilePath, mru.ToArray());

			UpdateMruMenu();
		}

		void RemoveFromMRU(string s)
		{
			List<string> mru = new List<string>();
			foreach(string i in File.ReadAllLines(_mruFilePath))
			{
				mru.Add(i);
			}
			foreach(var i in mru)
			{
				if(i == s)
				{
					mru.Remove(i);
					break;
				}
			}

			File.WriteAllLines(_mruFilePath, mru.ToArray());

			UpdateMruMenu();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			_gbMwx.Visible		= false;
			_gbImage.Visible	= false;
		
			if(Program._args.Length > 0)
			{
				LoadFile(Program._args[0]);
			}
		}

		public void LoadXon(string path)
		{
			_gbMwx.Show();

			FileStream s = new FileStream(path, FileMode.Open);
					
			_gbMwx.LoadBon(s, null);

			s.Close();
		}

		void LoadVwm(string vwmFile)
		{
			_isLoaded = false;
		
			_treeFiles.SuspendLayout();
			_treeFiles.TreeViewNodeSorter = new FileExtentionSorter();

			try
			{
				char [] sep = {'\\'};
				_lblLoading.Text	= "Loading...";
				_statusMain.Update();

				ZipFile zip = new ZipFile(_path);
				ZipInputStream s = new ZipInputStream(File.OpenRead(vwmFile));
							
				ZipEntry e;
				while ((e = s.GetNextEntry()) != null)
				{
					TreeNodeCollection nodes;
					TreeNode folder = null;
					string currFolder;
					string[] folders = Path.GetDirectoryName(e.Name).Split(sep, StringSplitOptions.RemoveEmptyEntries);

					for(int i=0; i<folders.Length; i++)
					{	
						currFolder = folders[i];
						
						if(folder != null)
						{
							nodes = folder.Nodes;
						}
						else
						{
							nodes = _treeFiles.Nodes;
						}
						
						folder = nodes[currFolder];
						if(!nodes.ContainsKey(currFolder))
						{
							folder = new TreeNode();
							folder.Text = currFolder;
							folder.Name = currFolder;
							nodes.Add(folder);
						}
					}

					if(!e.IsDirectory)
					{
						nodes = _treeFiles.Nodes;
						for(int i=0; i<folders.Length; i++)
						{
							nodes = nodes[folders[i]].Nodes;
						}
	
						TreeNode fileNode = new TreeNode(Path.GetFileName(e.Name));
						fileNode.Tag = e;
						nodes.Add(fileNode);
					}
				}
				zip.Close();

				_treeFiles.BeforeSelect += _treeFile_BeforeSelect;
			}
			catch(Exception e)
			{
				MessageBox.Show("An error has occured during opening VWM file.\nProbably wrong format or file is corrupted\n\n\n"+e.Message, "Error...");
			}

			_treeFiles.ResumeLayout();

			_lblLoading.Text	= "Ready";
			_isLoaded			= true;
		}

		private void _treeFile_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if(_isLoaded)
			{
				_gbMwx.Clear();
				
				ZipEntry ze = e.Node.Tag as ZipEntry;

				if(ze != null)
				{
					switch(Path.GetExtension(ze.Name).ToLower())
					{
						case ".bon":
							LoadXon(ze, null);
							_gbMwx.Visible		= true;
							_gbImage.Visible	= false;
							break;
						case ".bmp":
						case ".cut":
						case ".dds":
						case ".gif":
						case ".ico":case ".cur":
						case ".jpg":case ".jpe":case ".jpeg":
						case ".lbm":
						case ".lif":
						case ".lmp":
						case ".mdl":
						case ".mng":
						case ".pcd":
						case ".pcx":
						case ".pic":
						case ".png":
						case ".pbm":case ".pgm":case ".ppm":case ".pnm":
						case ".psd":
						case ".sgi":case ".bw":case ".rgb":case ".rgba":
						case ".tga":
						case ".tif":case ".tiff":
						case ".wal":
							LoadImage(ze);
							_gbMwx.Visible		= false;
							_gbImage.Visible	= true;
							break;
						default :
							_gbMwx.Visible		= false;
							_gbImage.Visible	= false;
							break;
					}
				}
			}
		}
		
		void LoadImage(ZipEntry ze)
		{
			if(_isLoaded)
			{
				//try
				{
					ZipFile zip = new ZipFile(_path);
						
					Stream s = zip.GetInputStream(ze);


					byte[] data = new byte[ze.Size];
					int size = s.Read(data, 0, (int)ze.Size);

					currentImage = new Image.DevIL.Image(data);

					_imageBox.Image = new System.Drawing.Bitmap(currentImage.Width, currentImage.Height, currentImage.Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, currentImage.Pointer);

					float kw = 1f;
					if(currentImage.Width > initialImageWidth)
					{
						kw = (float)initialImageWidth / (float)currentImage.Width;
					}

					float kh = 1f;
					if(currentImage.Height > initialImageHeight)
					{
						kh = (float)initialImageHeight / (float)currentImage.Height;
					}

					float k = Math.Min(kw, kh);

					if(k < 1f)
					{
						_imageBox.Width = (int)(currentImage.Width * k);
						_imageBox.Height = (int)(currentImage.Height * k);
					}
					else
					{
						_imageBox.Width = currentImage.Width;
						_imageBox.Height = currentImage.Height;
					}
					
					zip.Close();
				}
				//catch(Exception exc)
				//{
				//	_imageBox.Image = null;
				//	MessageBox.Show("An error has occured during opening Image file.\nProbably wrong format or file is corrupted\n\n"+exc.Message, "Error...");
				//}
			}
		}		

		public void LoadXon(ZipEntry e, TreeNode parent)
		{
			if(_isLoaded)
			{
				//try
				{
					ZipFile zip = new ZipFile(_path);
				
					Stream s = zip.GetInputStream(e);
					
					_gbMwx.LoadBon(s, parent);

					s.Close();
					zip.Close();
				}
				/*catch(Exception exc)
				{
					_txtData.Text  = "An error has occured during opening MWX file\r\nProbably wrong format or file is corrupted\r\n"+exc.Message;
				}*/
			}
		}

		private void _openMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			ofd.Filter = "Supported Files|*.vwm;*.zip;*.bxon";
			ofd.FilterIndex = 0;
			ofd.RestoreDirectory = true;

			if(ofd.ShowDialog() == DialogResult.OK)
			{
				LoadFile(ofd.FileName);
			}
		}

		private void _reloadMenuItem_Click(object sender, EventArgs e)
		{
			LoadFile(_path);
		}
	}

	public class FileExtentionSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			string e1 = Path.GetExtension((x as TreeNode).Text);
			string e2 = Path.GetExtension((y as TreeNode).Text);

			if(string.Compare(e1, e2) != 0)
			{
				return string.Compare(e1, e2);
			}
			else
			{
				return string.Compare((x as TreeNode).Text, (y as TreeNode).Text);
			}
		}
	}
}