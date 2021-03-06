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
	public class ArmorEditor : TabPage
	{
		private Panel AltPanel;
		private ItemPropertiesPanel ItemPropertiesPanel;
		private ItemIcon[] ArmorSlots;
		private AltItemIcon[] AltSlots;
		private const int AltPanelWidth = 90, AltPanelHeight = 270, AltGap = 3, AltWidth = 30 - AltGap, HackPanelHeight = 360;
		private const int SlotsWidth = 50;
		private const int SlotsGap = 5;
		private Panel SlotsPanel;
		private Timer timer;
		private int Selected = 0, LastSelectedID = 0;
		private GameContext Context;
		private int Clip_ItemType;
		private int Clip_ItemStack;
		private byte Clip_ItemPrefix;
		private Form ParentForm;
		private ContextMenuStrip SlotRightClickStrip;
		public ArmorEditor(GameContext Context, Form ParentForm)
		{
			this.Context = Context;
			this.ParentForm = ParentForm;
#if ENG
			Text = "Armor";
#else
			Text = "装备";
#endif

			ItemPropertiesPanel = new ItemPropertiesPanel();
			ArmorSlots = new ItemIcon[Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT + Player.MISC_MAX_COUNT + Player.MISCDYE_MAX_COUNT];
			AltSlots = new AltItemIcon[AltPanelWidth * AltPanelHeight];

			SlotsPanel = new Panel();
			
			Label Members = new Label();
			Members.Bounds = new Rectangle(275, 165, 270, 105);
			Members.BackColor = Color.FromArgb(201, 200, 170);
#if ENG
#else
			Members.Text = "鸣谢(同一批排名不分先后)：\n" +
				"第一批：Qiu(作者),帅哭,白鼠,魄沫,子不语,\n小兲,鱼,子落,永夜,欧酋,子程,恋恋\n" +
	"第二批：司机熊,呆呆,子渔,凤凰蛇,提灯,南风南,Ronin,男人,天然大豆,清辞,晨曦(御龙),袁瑞,mz,dio,锐,firmament,面条,心醉\n" +
 "感谢他们这一路的支持，修改器才有今天，在此列下名单，以作纪念";
#endif
			SlotsPanel.Controls.Add(Members);
			SlotsPanel.Size = new Size(10 * (SlotsWidth + SlotsGap), 300);
			SlotsPanel.Location = new Point(5, 5);
			this.Controls.Add(SlotsPanel);

			SlotRightClickStrip = new ContextMenuStrip();
			SlotRightClickStrip.Items.Add("Copy");
			SlotRightClickStrip.Items.Add("Paste");
			SlotRightClickStrip.ItemClicked += (sender, e) =>
			{
				var item = GetTargetItem(Selected);
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
			for (int i = 0; i < Player.ARMOR_MAX_COUNT; i++)
			{
				int row = (int)Math.Floor((double)(i / 10));
				int off = i % 10;
				ArmorSlots[i] = new ItemIcon(Context, Context.MyPlayer.Armor, i, i)
				{
					Size = new Size(SlotsWidth, SlotsWidth),
					Location = new Point(off * (SlotsWidth + SlotsGap), row * (SlotsWidth + SlotsGap)),


					BackColor = Color.CadetBlue,
					SizeMode = PictureBoxSizeMode.CenterImage
				};
				ArmorSlots[i].Click += SlotClick;
				this.SlotsPanel.Controls.Add(ArmorSlots[i]);
			}
			for (int i = Player.ARMOR_MAX_COUNT; i < Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT; i++)
			{
				int row = (int)Math.Floor((double)(i / 10));
				int off = i % 10;
				ArmorSlots[i] = new ItemIcon(Context, Context.MyPlayer.Dye, i, i - Player.ARMOR_MAX_COUNT)
				{
					Size = new Size(SlotsWidth, SlotsWidth),
					Location = new Point(off * (SlotsWidth + SlotsGap), row * (SlotsWidth + SlotsGap)),


					BackColor = Color.FromArgb(96, 160, 128),
					SizeMode = PictureBoxSizeMode.CenterImage
				};
				ArmorSlots[i].Click += SlotClick;
				this.SlotsPanel.Controls.Add(ArmorSlots[i]);
			}
			for (int i = Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT; i < Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT + Player.MISC_MAX_COUNT; i++)
			{
				int row = (int)Math.Floor((double)(i / 10));
				int off = i % 10;
				ArmorSlots[i] = new ItemIcon(Context, Context.MyPlayer.Misc, i, i - (Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT))
				{
					Size = new Size(SlotsWidth, SlotsWidth),
					Location = new Point(off * (SlotsWidth + SlotsGap), row * (SlotsWidth + SlotsGap)),


					BackColor = Color.FromArgb(169, 112, 144),
					SizeMode = PictureBoxSizeMode.CenterImage
				};
				ArmorSlots[i].Click += SlotClick;
				this.SlotsPanel.Controls.Add(ArmorSlots[i]);
			}
			for (int i = Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT + Player.MISC_MAX_COUNT; i < Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT + Player.MISC_MAX_COUNT + Player.MISCDYE_MAX_COUNT; i++)
			{
				int row = (int)Math.Floor((double)((i + 5) / 10));
				int off = (i + 5) % 10;
				ArmorSlots[i] = new ItemIcon(Context, Context.MyPlayer.MiscDye, i, i - (Player.ARMOR_MAX_COUNT + Player.DYE_MAX_COUNT + Player.MISC_MAX_COUNT))
				{
					Size = new Size(SlotsWidth, SlotsWidth),
					Location = new Point(off * (SlotsWidth + SlotsGap), row * (SlotsWidth + SlotsGap)),


					BackColor = Color.FromArgb(160, 128, 96),
					SizeMode = PictureBoxSizeMode.CenterImage
				};
				ArmorSlots[i].Click += SlotClick;
				this.SlotsPanel.Controls.Add(ArmorSlots[i]);
			}



			AltPanel = new Panel() { Location = new Point(560, 5), Size = new Size(AltPanelWidth, AltPanelHeight) };
			Controls.Add(AltPanel);
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
						Enabled = false
					};
					AltSlots[n].BackColor = Color.Gray;
					AltPanel.Controls.Add(AltSlots[n]);
				}
			}

			ItemPropertiesPanel.Location = new Point(10 * (SlotsWidth + SlotsGap) + 105, 5);
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




			Button InitItem = new Button();
			InitItem.Click += (sender, e) =>
			{
				Item item = GetTargetItem(Selected);
				item.SetDefaults(Convert.ToInt32(((TextBox)ItemPropertiesPanel.Hack["Type"]).Text));
				item.SetPrefix(GetPrefixFromIndex(ItemPropertiesPanel.SelectedPrefix));
				int stack = Convert.ToInt32(((TextBox)ItemPropertiesPanel.Hack["Stack"]).Text);
				item.Stack = stack == 0 ? 1 : stack;
				RefreshSelected();
				InitData(Selected);
			};
			InitItem.Text = Lang.init;
			InitItem.Size = new Size(80, 30);
			InitItem.Location = new Point(260, 60);
			ItemPropertiesPanel.Controls.Add(InitItem);

			ArmorSlots[0].Selected = true;
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
					Item item = GetTargetItem(Selected);
					if (LastSelectedID != item.Type)
					{
						InitData(Selected);
						LastSelectedID = item.Type;
					}
				}
			};
			timer.Start();
		}
		public void SlotClick(object sender, EventArgs e)
		{
			MouseEventArgs mea = (MouseEventArgs)e;
			ItemIcon ii = (ItemIcon)sender;

			foreach (var s in ArmorSlots)
			{
				s.Selected = false;
			}
			((ItemIcon)sender).Selected = true;
			SlotsPanel.Refresh();
			Selected = ((ItemIcon)sender).Number;
			InitData(Selected);
			if (mea.Button == MouseButtons.Right)
			{
				SlotRightClickStrip.Show(ii, mea.Location.X, mea.Location.Y);
			}
		}
		private void RefreshSelected()
		{
			ArmorSlots[Selected].Refresh();
		}
		private Item GetTargetItem(int slot)
		{
			var a = ArmorSlots[Selected];
			return a.Slots[a.ID];
		}
		private void InitData(int slot)
		{
			Item item = GetTargetItem(slot);

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
			Item item = GetTargetItem(slot);
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




	}
}
