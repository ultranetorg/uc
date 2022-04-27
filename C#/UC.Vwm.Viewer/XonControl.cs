using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace UC.Vwm.Viewer
{
	public partial class XonControl : UserControl
	{
		public XonControl()
		{
			InitializeComponent();

			_treeVwm.BeforeSelect += new TreeViewCancelEventHandler(_treeVwm_BeforeSelect);
		}

		private void _treeVwm_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			VwmEntity ae = e.Node.Tag as VwmEntity;
	
			if(ae != null)
			{
				LoadEntity(ae);
			}
		}

		public void Clear()
		{
			_treeVwm.Nodes.Clear();
		}

		int ReadSize(BinaryReader s)
		{
			uint size = 0;
			var b = s.ReadByte();

			if((b & 0x3) == 0)
			{
				size = b;
			}
			else if((b & 0x3) == 1)
			{
				size = b | (((uint)s.ReadByte()) << 8);
			}
			else if((b & 0x3) == 2)
			{
				size = b | (((uint)s.ReadByte()) << 8) | (((uint)s.ReadByte()) << 16) | (((uint)s.ReadByte()) << 24);
			}
			else
				throw new Exception("64bit Not Supported");
			///else if((size & 0b11) == 0b11)
			///{
			///	s->Read(((unsigned char *)&size) + 7, 7);
			///}

			return (int)(size >> 2);
		}

		public void LoadBon(Stream s, TreeNode parent)
		{
			_txtData.Text  = "";

			BinaryReader br = new BinaryReader(s, Encoding.Unicode);

			string sig = new string(Encoding.ASCII.GetChars(br.ReadBytes(4)));
			uint ntypes = br.ReadUInt32();

			if(ntypes > 0)
			{
				string [] types = new string[ntypes];
	
				for(ulong i=0; i<ntypes; i++)
				{
					string t = "";
					char c = br.ReadChar();
					while(c != '\0')
					{
						t += c;
						c = br.ReadChar();
					}
					types[i] = t;
				}
			
				LoadBon(br, parent, types);
			}
		}

		public void LoadBon(BinaryReader br, TreeNode parent, string [] types)
		{
			VwmEntity ae;
			do
			{
				ae = new VwmEntity();
				
				ae.Flags = br.ReadByte();
				
				char c = br.ReadChar();
				while(c != '\0')
				{
					ae.Name	+= c;
					c = br.ReadChar();
				}

				if((ae.Flags & (byte)EBonHeader.HasType) != 0)
				{
					var index = br.ReadByte();
					ae.Type = types[index];
				}

				if((ae.Flags & (byte)EBonHeader.HasValue) != 0)
				{
					ulong s = 0;

					if((ae.Flags & (byte)EBonHeader.BigValue) == 0)
					{
						s = ((ulong)ae.Flags & 0b0000_0111) + 1;
					}
					else
					{
						var f = ae.Flags & 0b0000_0011;
						if(f == 0b00)	s = br.ReadByte(); else
						if(f == 0b01)	s = br.ReadUInt16(); else
						if(f == 0b10)	s = br.ReadUInt32(); else
						if(f == 0b11)	s = br.ReadUInt64();
					}
				
					ae.Data	= br.ReadBytes((int)s);
				}

/*
				int n;
				switch(ae.Type)
				{
                    case "bool":
                        ae.Data	= br.ReadBytes(1);
                        break;
                    case "int32":
						ae.Data	= br.ReadBytes(4);
						break;
					case "float32":
						ae.Data	= br.ReadBytes(4);
						break;
					case "utf16.array":
						n = ReadSize(br);
						ae.Data	= br.ReadBytes(n * 2);
						break;
					case "float32.vector3":
						ae.Data	= br.ReadBytes(3 * 4);
						break;
					case "int32.array":
						n = ReadSize(br);
						ae.Data	= br.ReadBytes(n * 4);
						break;
					case "float32.vector2.array":
						n = ReadSize(br);
						ae.Data	= br.ReadBytes(n * 2 * 4);
						break;
					case "float32.vector3.array":
						n = ReadSize(br);
						ae.Data	= br.ReadBytes(n * 3 * 4);
						break;
					case "float32.matrix4x4":
						ae.Data	= br.ReadBytes(16 * 4);
						break;
					case "transformation":
						ae.Data	= br.ReadBytes((3 + 4 + 3) * 4);
						break;
				}*/

				TreeNode node = new TreeNode(ae.Name);
				node.Tag = ae;
				if(parent != null)
				{
					parent.Nodes.Add(node);
				}
				else
				{
					_treeVwm.Nodes.Add(node);
				}
				
				if((ae.Flags & (short)EBonHeader.Parent) != 0)
				{
					LoadBon(br, node, types);
				}

				if(parent == null)
				{
					node.Expand();
				}

			}
			while((ae.Flags & (byte)EBonHeader.Last) == 0);
		}

		public void LoadEntity(VwmEntity e)
		{
			StringBuilder s = new StringBuilder(1024);

			if(e.Type != null)
			{
				s.AppendLine("Type = " + e.Type.ToString());
			}

			if(e != null && e.Data != null)
			{
				int n = e.Data.Length;
                s.AppendLine("Size = " + n.ToString());

                s.AppendLine();

                MemoryStream ms = new MemoryStream(e.Data);
				BinaryReader br = new BinaryReader(ms, Encoding.Unicode);
				
				switch(e.Type)
				{
                    case "bool":
                        s.Append(br.ReadBoolean());
                        break;
                    case "int32":
						s.Append(br.ReadInt32());
						break;
					case "float32":
						s.Append(br.ReadSingle());
						break;
					case "utf16.array":
					{
						var c = ReadSize(br);
						s.Append(br.ReadChars(c));
						break;
					}
					case "float32.vector3":
						s.AppendFormat("x = {0}\r\n", br.ReadSingle().ToString());
						s.AppendFormat("y = {0}\r\n", br.ReadSingle().ToString());
						s.AppendFormat("z = {0}\r\n", br.ReadSingle().ToString());
						break;
					case "int32.array":
					{
						var c = ReadSize(br);
						while(c > 0)
						{
							s.Append(br.ReadInt32().ToString() + " ");
							c--;
						}
						break;
					}
					case "float32.vector2.array":
					{
						var c = ReadSize(br);
						while(c > 0)
						{
							s.AppendFormat("{0,15} {1,15}\r\n", br.ReadSingle(), br.ReadSingle());
							c--;
						}
						break;
					}	
					case "float32.vector3.array":
					{
						var c = ReadSize(br);
						while(c > 0)
						{
							s.AppendFormat("{0,15} {1,15} {2,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
							c--;
						}
						break;
					}
					case "float32.matrix4x4":
						s.AppendFormat("{0,15} {1,15} {2,15} {3,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						s.AppendFormat("{0,15} {1,15} {2,15} {3,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						s.AppendFormat("{0,15} {1,15} {2,15} {3,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						s.AppendFormat("{0,15} {1,15} {2,15} {3,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						break;

					case "transformation":
						s.AppendFormat("Posittion: {0,15} {1,15} {2,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						s.AppendFormat("RotationQ: {0,15} {1,15} {2,15} {3,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						s.AppendFormat("Scale:     {0,15} {1,15} {2,15}\r\n", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
						break;
				}
			}
			_txtData.Text = s.ToString();
		}
	}
}
