﻿using QTRHacker.Functions;
using QTRHacker.Functions.GameObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QTRHacker.PlayerEditor
{
	public class InvEditor : TabPage
	{
		private Panel AltPanel;
		private ItemPropertiesPanel ItemPropertiesPanel;
		private ItemIcon[] ItemSlots;
		private AltItemIcon[] AltSlots;
		private const int AltPanelWidth = 90, AltPanelHeight = 270, AltGap = 3, AltWidth = 30 - AltGap, HackPanelHeight = 360;
		private const int SlotsWidth = 50;
		private const int SlotsGap = 5;
		private Panel SlotsPanel;
		private Timer timer;
		private int Selected = 0, LastSelectedID = 0;
		private AltItemIcon AltSelected;
		private GameContext Context;
		private int Clip_ItemType;
		private int Clip_ItemStack;
		private byte Clip_ItemPrefix;
		private Form ParentForm;
		public InvEditor(GameContext Context, Form ParentForm)
		{
			this.Context = Context;
			this.ParentForm = ParentForm;
			Text = "背包";
			ItemPropertiesPanel = new ItemPropertiesPanel();
			ItemSlots = new ItemIcon[Player.ITEM_MAX_COUNT - 9];
			AltSlots = new AltItemIcon[AltPanelWidth * AltPanelHeight];

			SlotsPanel = new Panel();

			SlotsPanel.Size = new Size(10 * (SlotsWidth + SlotsGap), 300);
			SlotsPanel.Location = new Point(5, 5);
			this.Controls.Add(SlotsPanel);

			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Items.Add("Copy");
			cms.Items.Add("Paste");
			cms.ItemClicked += (sender, e) =>
			{
				var item = Context.MyPlayer.Inventory[Selected];
				switch (e.ClickedItem.Text)
				{
					case "Copy":

						Clip_ItemType = item.Type;
						Clip_ItemStack = item.Stack;
						Clip_ItemPrefix = item.Prefix;
						RefreshSelected();
						break;
					case "Paste":
						if (Clip_ItemType != 0)
						{
							item.SetDefaultsAndPrefix(Clip_ItemType, Clip_ItemPrefix);
							item.Stack = Clip_ItemStack;
						}
						RefreshSelected();
						break;
				}
			};
			for (int i = 0; i < ItemSlots.Length; i++)
			{
				int row = (int)Math.Floor((double)(i / 10));
				int off = i % 10;
				ItemSlots[i] = new ItemIcon(Context, Context.MyPlayer.Inventory, i, i)
				{
					Size = new Size(SlotsWidth, SlotsWidth),
					Location = new Point(off * (SlotsWidth + SlotsGap), row * (SlotsWidth + SlotsGap)),


					BackColor = Color.CadetBlue,
					SizeMode = PictureBoxSizeMode.CenterImage
				};
				ItemSlots[i].Click += (sender, e) =>
				{
					MouseEventArgs mea = (MouseEventArgs)e;
					ItemIcon ii = (ItemIcon)sender;

					foreach (var s in ItemSlots)
					{
						s.Selected = false;
					}
						((ItemIcon)sender).Selected = true;
					SlotsPanel.Refresh();
					Selected = ((ItemIcon)sender).Number;
					InitData(Selected);
					if (mea.Button == MouseButtons.Right)
					{
						cms.Show(ii, mea.Location.X, mea.Location.Y);
					}
				};
				this.SlotsPanel.Controls.Add(ItemSlots[i]);
			}

			ContextMenuStrip altCms = new ContextMenuStrip();
			altCms.Items.Add("Edit");

			altCms.ItemClicked += (sender, e) =>
			{
				switch (e.ClickedItem.Text)
				{
					case "Edit":
						{
							Form f = new Form();
							TextBox ItemID = new TextBox();
							TextBox ItemCount = new TextBox();
							ComboBox prefix = new ComboBox();
							Button et = new Button();

							f.Text = "Edit";
							f.StartPosition = FormStartPosition.CenterParent;
							f.FormBorderStyle = FormBorderStyle.FixedSingle;
							f.MaximizeBox = false;
							f.MinimizeBox = false;
							f.Size = new Size(265, 105);

							Label tip1 = new Label()
							{
								Text = "ItemID",
								Location = new Point(0, 5),
								Size = new Size(80, 20)
							};
							f.Controls.Add(tip1);

							ItemID.Location = new Point(85, 0);
							ItemID.Size = new Size(95, 20);
							ItemID.Text = AltSelected.ID.ToString();
							ItemID.KeyPress += delegate (object sender1, KeyPressEventArgs e1)
							{
								if (!Char.IsNumber(e1.KeyChar) && e1.KeyChar != 8)
								{
									e1.Handled = true;
								}
							};
							f.Controls.Add(ItemID);


							Label tip2 = new Label()
							{
								Text = "ItemStack",
								Location = new Point(0, 25),
								Size = new Size(80, 20)
							};
							f.Controls.Add(tip2);

							ItemCount.Location = new Point(85, 20);
							ItemCount.Size = new Size(95, 20);
							ItemCount.Text = AltSelected.Stack.ToString();
							ItemCount.KeyPress += delegate (object sender1, KeyPressEventArgs e1)
							{
								if (!Char.IsNumber(e1.KeyChar) && e1.KeyChar != 8)
								{
									e1.Handled = true;
								}
							};
							f.Controls.Add(ItemCount);

							prefix.Location = new Point(85, 40);
							prefix.Size = new Size(95, 20);
							prefix.DropDownStyle = ComboBoxStyle.DropDownList;
							prefix.DropDownHeight = 150;
							foreach (var o in MainForm.resource.Prefix)
							{
								string[] t = o.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
								string v = t[0];
								prefix.Items.Add(v);
							}
							prefix.SelectedIndex = GetIndexFromPrefix(AltSelected.Prefix);

							f.Controls.Add(prefix);

							Label tip3 = new Label()
							{
								Text = "ItemPrefix",
								Location = new Point(0, 45),
								Size = new Size(80, 20)
							};
							f.Controls.Add(tip3);


							et.Text = Lang.confirm;
							et.Size = new Size(65, 60);
							et.Location = new Point(180, 0);
							et.Click += delegate (object sender1, EventArgs e1)
							{
								AltSelected.ID = Convert.ToInt32(ItemID.Text);
								AltSelected.Stack = Convert.ToInt32(ItemCount.Text);
								AltSelected.Prefix = GetPrefixFromIndex(prefix.SelectedIndex);
								f.Dispose();
								var img = MainForm.item_images.Images["Item_" + AltSelected.ID];
								if (img != null)
									AltSelected.Image = img;
								SaveAltItems();
							};
							f.Controls.Add(et);
							f.StartPosition = FormStartPosition.CenterParent;
							f.ShowDialog(this);
						}
						break;
				}
			};
			AltPanel = new Panel() { Location = new Point(560, 5), Size = new Size(AltPanelWidth, AltPanelHeight) };
			Controls.Add(AltPanel);
			if (!File.Exists("AlternativeItem"))
			{
				BinaryWriter bw = new BinaryWriter(File.Open("AlternativeItem", FileMode.OpenOrCreate));//ID Stack Prefix
				for (int i = 0; i < (AltPanelHeight / AltWidth) * (AltPanelWidth / AltWidth); i++)
				{
					bw.Write(0);        //ID
					bw.Write(0);        //Stack
					bw.Write((byte)0);  //Prefix
				}
				bw.Close();
			}
			BinaryReader br = new BinaryReader(File.Open("AlternativeItem", FileMode.Open));//ID Stack Prefix
			for (int i = 0; i < AltPanelHeight / AltWidth; i++)
			{
				for (int j = 0; j < AltPanelWidth / AltWidth; j++)
				{
					int n = i * AltPanelWidth / AltWidth + j;
					AltSlots[n] = new AltItemIcon()
					{
						Size = new Size(AltWidth - AltGap, AltWidth - AltGap),
						Location = new Point(j * (AltWidth + AltGap), i * (AltWidth + AltGap) + 2),
						SizeMode = PictureBoxSizeMode.CenterImage,
						ID = br.ReadInt32(),
						Stack = br.ReadInt32(),
						Prefix = br.ReadByte(),
					};
					Image img;
					if ((img = MainForm.item_images.Images["Item_" + AltSlots[n].ID]) != null)
					{
						AltSlots[n].Image = img;
					}
					AltSlots[n].BackColor = Color.CadetBlue;
					AltSlots[n].Click += (sender, e) =>
					{

						MouseEventArgs mea = (MouseEventArgs)e;
						AltItemIcon aii = (AltItemIcon)sender;
						this.AltSelected = aii;
						if (mea.Button == MouseButtons.Right)
						{
							altCms.Show(aii, mea.Location.X, mea.Location.Y);
						}
					};
					AltSlots[n].DoubleClick += (sender, e) =>
					{
						var p = Context.MyPlayer;
						for (int h = 0; h < Player.ITEM_MAX_COUNT - 9; h++)
						{
							var item = p.Inventory[h];
							if (item.Type == 0)
							{
								item.SetDefaults(AltSelected.ID);
								item.SetPrefix(AltSelected.Prefix);
								item.Stack = AltSelected.Stack;
								break;
							}
						}
					};
					AltPanel.Controls.Add(AltSlots[n]);
				}
			}
			br.Close();

			ItemPropertiesPanel.Location = new Point(ItemSlots.Length / 5 * (SlotsWidth + SlotsGap) + 105, 5);
			ItemPropertiesPanel.Size = new Size(350, HackPanelHeight);
			this.Controls.Add(ItemPropertiesPanel);


			Button OK = new Button();
			OK.Click += (sender, e) =>
			{
				ApplyData(Selected);
				InitData(Selected);
				RefreshSelected();
			};
			OK.Text = Lang.confirmHack;
			OK.Size = new Size(80, 30);
			OK.Location = new Point(260, 0);
			ItemPropertiesPanel.Controls.Add(OK);


			Button Refresh = new Button();
			Refresh.Click += (sender, e) =>
			{
				InitData(Selected);
				SlotsPanel.Refresh();
			};
			Refresh.Text = Lang.refresh;
			Refresh.Size = new Size(80, 30);
			Refresh.Location = new Point(260, 30);
			ItemPropertiesPanel.Controls.Add(Refresh);


			Button SaveInv = new Button();
			SaveInv.Click += (sender, e) =>
			{
				SaveFileDialog sfd = new SaveFileDialog()
				{
					Filter = "inv files (*.inv)|*.inv",
				};
				if (!Directory.Exists("./invs"))
					Directory.CreateDirectory("./invs");
				sfd.InitialDirectory = Path.GetFullPath("./invs");
				if (sfd.ShowDialog() == DialogResult.OK)
				{
					SaveInventory(sfd.FileName);
					SlotsPanel.Refresh();
				}
			};
			SaveInv.Text = Lang.save;
			SaveInv.Size = new Size(80, 30);
			SaveInv.Location = new Point(260, 60);
			ItemPropertiesPanel.Controls.Add(SaveInv);

			Button LoadInv = new Button();
			LoadInv.Click += (sender, e) =>
			{
				OpenFileDialog ofd = new OpenFileDialog()
				{
					Filter = "inv files (*.inv)|*.inv"
				};
				if (!Directory.Exists("./invs"))
					Directory.CreateDirectory("./invs");
				ofd.InitialDirectory = Path.GetFullPath("./invs");
				if (ofd.ShowDialog() == DialogResult.OK)
				{
					LoadInventory(ofd.FileName);
					SlotsPanel.Refresh();
					InitData(Selected);
				}
			};
			LoadInv.Text = Lang.load;
			LoadInv.Size = new Size(80, 30);
			LoadInv.Location = new Point(260, 90);
			ItemPropertiesPanel.Controls.Add(LoadInv);

			Button SaveInvPItem = new Button();
			SaveInvPItem.Click += (sender, e) =>
			{
				SaveFileDialog sfd = new SaveFileDialog()
				{
					Filter = "inv files (*.invp)|*.invp"
				};
				if (!Directory.Exists("./invs"))
					Directory.CreateDirectory("./invs");
				sfd.InitialDirectory = Path.GetFullPath("./invs");
				if (sfd.ShowDialog() == DialogResult.OK)
				{


					File.WriteAllText(sfd.FileName, Context.MyPlayer.SerializeInventoryWithProperties());
					SlotsPanel.Refresh();

				}
			};
			SaveInvPItem.Text = Lang.save + "(P)";
			SaveInvPItem.Size = new Size(80, 30);
			SaveInvPItem.Location = new Point(260, 120);
			ItemPropertiesPanel.Controls.Add(SaveInvPItem);

			Button LoadInvPItem = new Button();
			LoadInvPItem.Click += (sender, e) =>
			{
				OpenFileDialog ofd = new OpenFileDialog()
				{
					Filter = "inv files (*.invp)|*.invp"
				};
				if (!Directory.Exists("./invs"))
					Directory.CreateDirectory("./invs");
				ofd.InitialDirectory = Path.GetFullPath("./invs");
				if (ofd.ShowDialog() == DialogResult.OK)
				{
					int j = 0;
					Form p = new Form();
					ProgressBar pb = new ProgressBar();
					Label tip = new Label(), percent = new Label();
					tip.Text = "Loading inventory...";
					tip.Location = new Point(0, 0);
					tip.Size = new Size(150, 30);
					tip.TextAlign = ContentAlignment.MiddleCenter;
					percent.Location = new Point(150, 0);
					percent.Size = new Size(30, 30);
					percent.TextAlign = ContentAlignment.MiddleCenter;
					System.Timers.Timer timer = new System.Timers.Timer(1);
					p.FormBorderStyle = FormBorderStyle.FixedSingle;
					p.ClientSize = new Size(300, 60);
					p.ControlBox = false;
					pb.Location = new Point(0, 30);
					pb.Size = new Size(300, 30);
					pb.Maximum = 2;
					pb.Minimum = 0;
					pb.Value = 0;
					p.Controls.Add(tip);
					p.Controls.Add(percent);
					p.Controls.Add(pb);
					timer.Elapsed += (sender1, e1) =>
					{
						pb.Value = j;
						percent.Text = pb.Value + "/" + pb.Maximum;
						if (j >= pb.Maximum) p.Dispose();
					};
					timer.Start();
					p.Show();
					p.Location = new System.Drawing.Point(ParentForm.Location.X + ParentForm.Width / 2 - p.ClientSize.Width / 2, ParentForm.Location.Y + ParentForm.Height / 2 - p.ClientSize.Height / 2);
					new System.Threading.Thread((s) =>
					{
						j++;
						MainForm.mainWindow.Enabled = false;
						if (ExtForm.Window != null)
							ExtForm.Window.Enabled = false;
						this.Parent.Enabled = false;

						Context.MyPlayer.DeserializeInventoryWithProperties(File.ReadAllText(ofd.FileName));
						InitData(Selected);
						j++;

						this.Parent.Enabled = true;
						if (ExtForm.Window != null)
							ExtForm.Window.Enabled = true;
						MainForm.mainWindow.Enabled = true;
					}
					).Start();
				}
			};
			LoadInvPItem.Text = Lang.load + "(P)";
			LoadInvPItem.Size = new Size(80, 30);
			LoadInvPItem.Location = new Point(260, 150);
			ItemPropertiesPanel.Controls.Add(LoadInvPItem);

			Button InitItem = new Button();
			InitItem.Click += (sender, e) =>
			{
				Item item = Context.MyPlayer.Inventory[Selected];
				item.SetDefaults(Convert.ToInt32(((TextBox)ItemPropertiesPanel.Hack["Type"]).Text));
				item.SetPrefix(GetPrefixFromIndex(ItemPropertiesPanel.SelectedPrefix));
				int stack = Convert.ToInt32(((TextBox)ItemPropertiesPanel.Hack["Stack"]).Text);
				item.Stack = stack == 0 ? 1 : stack;
				RefreshSelected();
				InitData(Selected);
			};
			InitItem.Text = Lang.init;
			InitItem.Size = new Size(80, 30);
			InitItem.Location = new Point(260, 180);
			ItemPropertiesPanel.Controls.Add(InitItem);

			ItemSlots[0].Selected = true;
			InitData(0);
			timer = new Timer()
			{
				Interval = 500
			};
			timer.Tick += (sender, e) =>
			{
				if (this.Enabled)
				{
					SlotsPanel.Refresh();
					Item item = Context.MyPlayer.Inventory[Selected];
					if (LastSelectedID != item.Type)
					{
						InitData(Selected);
						LastSelectedID = item.Type;
					}
				}
			};
			timer.Start();
		}
		private void RefreshSelected()
		{
			ItemSlots[Selected].Refresh();
		}
		private void InitData(int slot)
		{
			Item item = Context.MyPlayer.Inventory[slot];
			Type t = typeof(Item);
			foreach (DictionaryEntry de in ItemPropertiesPanel.Hack)
			{
				object[] args = new object[1];
				args[0] = slot;
				var pi = t.GetProperty((string)de.Key);
				if (pi == null)
					return;
				((TextBox)de.Value).Text = Convert.ToString(pi.GetValue(item));

			}
			{
				ItemPropertiesPanel.SelectedPrefix = GetIndexFromPrefix(item.Prefix);
			}
			{
				ItemPropertiesPanel.AutoReuse = item.AutoReuse ? CheckState.Checked : CheckState.Unchecked;
			}
			{
				ItemPropertiesPanel.Equippable = item.Accessory ? CheckState.Checked : CheckState.Unchecked;
			}
		}
		private void ApplyData(int slot)
		{
			Item item = Context.MyPlayer.Inventory[slot];
			Type t = typeof(Item);
			foreach (DictionaryEntry de in ItemPropertiesPanel.Hack)
			{
				object[] args = new object[1];
				args[0] = slot;
				var pi = t.GetProperty((string)de.Key);
				if (pi == null)
					return;
				if (pi.PropertyType == typeof(long) || pi.PropertyType == typeof(ulong))
					pi.SetValue(item, Convert.ToInt64(((TextBox)de.Value).Text));
				else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(uint))
					pi.SetValue(item, Convert.ToInt32(((TextBox)de.Value).Text));
				else if (pi.PropertyType == typeof(short) || pi.PropertyType == typeof(ushort))
					pi.SetValue(item, Convert.ToInt16(((TextBox)de.Value).Text));
				else if (pi.PropertyType == typeof(float))
					pi.SetValue(item, Convert.ToSingle(((TextBox)de.Value).Text));
				else if (pi.PropertyType == typeof(double))
					pi.SetValue(item, Convert.ToDouble(((TextBox)de.Value).Text));
				else if (pi.PropertyType == typeof(bool))
					pi.SetValue(item, Convert.ToBoolean(((TextBox)de.Value).Text));
				else if (pi.PropertyType == typeof(byte))
					pi.SetValue(item, Convert.ToByte(((TextBox)de.Value).Text));

			}
			{
				item.Prefix = GetPrefixFromIndex(ItemPropertiesPanel.SelectedPrefix);
			}
			{
				item.AutoReuse = ItemPropertiesPanel.AutoReuse == CheckState.Checked;
			}
			{
				item.Accessory = ItemPropertiesPanel.Equippable == CheckState.Checked;
			}
		}
		public void SaveAltItems()
		{
			BinaryWriter bw = new BinaryWriter(File.Open("AlternativeItem", FileMode.OpenOrCreate));//ID Stack Prefix
			for (int i = 0; i < AltPanelHeight / AltWidth; i++)
			{
				for (int j = 0; j < AltPanelWidth / AltWidth; j++)
				{
					int n = i * AltPanelWidth / AltWidth + j;
					bw.Write(AltSlots[n].ID);
					bw.Write(AltSlots[n].Stack);
					bw.Write(AltSlots[n].Prefix);
				}
			}
			bw.Close();
		}
		public static byte GetPrefixFromIndex(int id)
		{
			return Convert.ToByte(MainForm.resource.Prefix[id].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1]);
		}
		private int GetIndexFromPrefix(byte id)
		{
			int j = 0;
			foreach (var o in MainForm.resource.Prefix)
			{
				string[] t = o.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
				byte i = Convert.ToByte(t[1]);
				if (i == id)
					return j;
				j++;
			}
			return 0;
		}



		public void SaveInventory(string name)
		{
			if (File.Exists(name)) File.Delete(name);
			BinaryWriter bw = new BinaryWriter(new FileStream(name, FileMode.OpenOrCreate));
			var player = Context.MyPlayer;
			for (int i = 0; i < Player.ITEM_MAX_COUNT; i++)
			{
				var item = player.Inventory[i];
				bw.Write(item.Type);
				bw.Write(item.Stack);
				bw.Write(item.Prefix);
			}
			for (int i = 0; i < Player.ARMOR_MAX_COUNT; i++)
			{
				var item = player.Armor[i];
				bw.Write(item.Type);
				bw.Write(item.Stack);
				bw.Write(item.Prefix);
			}
			for (int i = 0; i < Player.DYE_MAX_COUNT; i++)
			{
				var item = player.Dye[i];
				bw.Write(item.Type);
				bw.Write(item.Stack);
				bw.Write(item.Prefix);
			}
			for (int i = 0; i < Player.MISC_MAX_COUNT; i++)
			{
				var item = player.Misc[i];
				bw.Write(item.Type);
				bw.Write(item.Stack);
				bw.Write(item.Prefix);
			}
			for (int i = 0; i < Player.MISCDYE_MAX_COUNT; i++)
			{
				var item = player.MiscDye[i];
				bw.Write(item.Type);
				bw.Write(item.Stack);
				bw.Write(item.Prefix);
			}
			bw.Close();
		}
		public void LoadInventory(string name)
		{
			int j = 0;
			Form p = new Form();
			ProgressBar pb = new ProgressBar();
			Label tip = new Label(), percent = new Label();
			tip.Text = "Loading inventory...";
			tip.Location = new Point(0, 0);
			tip.Size = new Size(150, 30);
			tip.TextAlign = ContentAlignment.MiddleCenter;
			percent.Location = new Point(150, 0);
			percent.Size = new Size(30, 30);
			percent.TextAlign = ContentAlignment.MiddleCenter;
			System.Timers.Timer timer = new System.Timers.Timer(1);
			p.FormBorderStyle = FormBorderStyle.FixedSingle;
			p.ClientSize = new Size(300, 60);
			p.ControlBox = false;
			pb.Location = new Point(0, 30);
			pb.Size = new Size(300, 30);
			pb.Maximum = Player.ITEM_MAX_COUNT + Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT + Player.MISC_MAX_COUNT + Player.MISCDYE_MAX_COUNT;
			pb.Minimum = 0;
			pb.Value = 0;
			p.Controls.Add(tip);
			p.Controls.Add(percent);
			p.Controls.Add(pb);
			timer.Elapsed += (sender, e) =>
			{
				pb.Value = j;
				percent.Text = pb.Value + "/" + pb.Maximum;
				if (j >= pb.Maximum) p.Dispose();
			};
			timer.Start();
			p.Show();
			p.Location = new System.Drawing.Point(ParentForm.Location.X + ParentForm.Width / 2 - p.ClientSize.Width / 2, ParentForm.Location.Y + ParentForm.Height / 2 - p.ClientSize.Height / 2);
			new System.Threading.Thread((s) =>
			{
				MainForm.mainWindow.Enabled = false;
				if (ExtForm.Window != null)
					ExtForm.Window.Enabled = false;
				this.Parent.Enabled = false;
				var player = Context.MyPlayer;
				BinaryReader br = new BinaryReader(new FileStream(name, FileMode.Open));
				for (int i = 0; i < Player.ITEM_MAX_COUNT; i++)
				{
					j++;
					var item = player.Inventory[i];
					int type = br.ReadInt32();
					int stack = br.ReadInt32();
					byte prefix = br.ReadByte();
					if (type <= 0 && item.Type <= 0) continue;
					item.SetDefaultsAndPrefix(type, prefix);
					item.Stack = stack;
				}
				for (int i = 0; i < Player.ARMOR_MAX_COUNT; i++)
				{
					j++;
					var item = player.Armor[i];
					int type = br.ReadInt32();
					int stack = br.ReadInt32();
					byte prefix = br.ReadByte();
					if (type <= 0 && item.Type <= 0) continue;
					item.SetDefaultsAndPrefix(type, prefix);
					item.Stack = stack;
				}
				for (int i = 0; i < Player.DYE_MAX_COUNT; i++)
				{
					j++;
					var item = player.Dye[i];
					int type = br.ReadInt32();
					int stack = br.ReadInt32();
					byte prefix = br.ReadByte();
					if (type <= 0 && item.Type <= 0) continue;
					item.SetDefaultsAndPrefix(type, prefix);
					item.Stack = stack;
				}
				for (int i = 0; i < Player.MISC_MAX_COUNT; i++)
				{
					j++;
					var item = player.Misc[i];
					int type = br.ReadInt32();
					int stack = br.ReadInt32();
					byte prefix = br.ReadByte();
					if (type <= 0 && item.Type <= 0) continue;
					item.SetDefaultsAndPrefix(type, prefix);
					item.Stack = stack;
				}
				for (int i = 0; i < Player.MISCDYE_MAX_COUNT; i++)
				{
					j++;
					var item = player.MiscDye[i];
					int type = br.ReadInt32();
					int stack = br.ReadInt32();
					byte prefix = br.ReadByte();
					if (type <= 0 && item.Type <= 0) continue;
					item.SetDefaultsAndPrefix(type, prefix);
					item.Stack = stack;
				}
				br.Close();
				this.Parent.Enabled = true;
				if (ExtForm.Window != null)
					ExtForm.Window.Enabled = true;
				MainForm.mainWindow.Enabled = true;
			}
			).Start();
		}
	}
}
