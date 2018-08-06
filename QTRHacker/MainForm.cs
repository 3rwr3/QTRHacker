﻿/*
 * Created by SharpDevelop.
 * User: jianqiu
 * Date: 2015/9/12
 * Time: 15:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Resources;
using System.IO;
using System.Linq;
using System.Reflection;
using QTRHacker.Functions;

namespace QTRHacker
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private bool flag = false;
		private Label status;
		private int hProcess;
		public const int PROCESS_ALL_ACCESS = 0x1F0FFF;

		public delegate void HackFunc(GameContext c);
		private Button Extra;
		private ExtraForm ExtraHack = null;
		private TabControl mainTab;
		private TabPage buttonTabPage1;
		private TabPage buttonTabPage2;
		private TabPage buttonTabPage3;
		private TabPage buttonTabPage4;
		private TabPage buttonTabPage5;
		private TabPage buttonTabPage6;

		internal Dictionary<string, TabPage> tabs = new Dictionary<string, TabPage>();
		internal Dictionary<string, int> indexes = new Dictionary<string, int>();

		private List<Plugin> Plugins = new List<Plugin>();

		public static ImageList item_images = new ImageList();
		private string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString() + "---" + gameVersion;
		public int processID;
		public static MainForm mainWindow;
		private Image cross;
		public static Resources resource = new Resources();
		public static string gameVersion = "1.3.5.3";
		public const int MAX_ITEMS_1353 = 3930;
		private const int row = 12;
		public static GameContext Context;
		public static bool CanHack => !(Context == null || Context.MyPlayer.BaseAddress <= 0);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;
			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}
		[DllImport("User32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);
		[DllImport("User32.dll")]
		public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
		[DllImport("User32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
		[DllImport("kernel32.dll")]
		public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);


		public TabPage RegisterTab(string Text)
		{
			TabPage tp = new TabPage(Text);
			tp.BackColor = Color.LightGray;
			tabs[Text] = tp;
			indexes[Text] = 0;
			mainTab.Controls.Add(tp);
			return tp;
		}

		public void InitControls()
		{
			MemoryStream ms = new MemoryStream(resource.ItemImage);
			BinaryReader br = new BinaryReader(ms);
			while (true)
			{
				if (br.PeekChar() == -1) break;
				string name = br.ReadString();
				long length = br.ReadInt64();
				byte[] data = br.ReadBytes((int)length);
				MemoryStream imgms = new MemoryStream(data);
				Image img = Image.FromStream(imgms);
				item_images.Images.Add(name, img);
			}
			br.Close();
			ms.Close();
			item_images.ColorDepth = ColorDepth.Depth32Bit;
			item_images.ImageSize = new Size(20, 20);

			{
				var none = item_images.Images[0];//just to load all images
			}

			status = new Label()
			{
				Text = Lang.none,
				Location = new Point(0, 50),
				Size = new Size(190, 25),
				Font = new Font("Arial", 10)
			};
			this.Controls.Add(status);

			Extra = new Button()
			{
				Location = new Point(245, 50),
				Size = new Size(50, 25),
				Text = Lang.extra
			};
			Extra.Click += delegate (object sender, EventArgs e)
			 {
				 if (!CanHack)
				 {
					 MessageBox.Show(Lang.nonePlayerBase);
					 return;
				 }
				 if (ExtraHack == null)
				 {
					 ExtraHack = new ExtraForm(Context);
					 ExtraHack.Show(this);
					 ExtraHack.Location = new Point(Location.X + Width, Location.Y);
					 Extra.Font = new Font("Arial", 10, FontStyle.Bold);
				 }
				 else
				 {
					 ExtraHack.Dispose();
					 ExtraHack = null;
					 Extra.Font = new Font("Arial", 10);
				 }
			 };
			this.Controls.Add(Extra);



			mainTab = new MTabControl()
			{
				Location = new Point(0, 75),
				Size = new Size(300, 500 - 105),
				SelectedIndex = 0
			};
			buttonTabPage1 = RegisterTab("1");

			buttonTabPage2 = RegisterTab("2");

			buttonTabPage6 = RegisterTab("3");

			buttonTabPage3 = RegisterTab(Lang.Event);

			buttonTabPage4 = RegisterTab(Lang.builder);

			buttonTabPage5 = RegisterTab(Lang.eff);


			this.Controls.Add(mainTab);



			AddButton(buttonTabPage1, Lang.infLife, 0, Utils.InfiniteLife_E, Utils.InfiniteLife_D);
			AddButton(buttonTabPage1, Lang.infOxygen, 1, Utils.InfiniteOxygen_E, Utils.InfiniteOxygen_D);
			AddButton(buttonTabPage1, Lang.infSummon, 2, Utils.InfiniteMinion_E, Utils.InfiniteMinion_D);
			AddButton(buttonTabPage1, Lang.infMana, 3, Utils.InfiniteMana_E, Utils.InfiniteMana_D);
			AddButton(buttonTabPage1, Lang.infItemAndAmmo, 4, (ctx) => { Utils.InfiniteItem_E(ctx); Utils.InfiniteAmmo_E(ctx); }, (ctx)=> { Utils.InfiniteItem_D(ctx); Utils.InfiniteAmmo_D(ctx); });
			AddButton(buttonTabPage1, Lang.infFly, 5, Utils.InfiniteFly_E, Utils.InfiniteFly_D);
			//AddButton(buttonTabPage1, Lang.immuneStoned, 6, HackFunctions.immuneBuff, HackFunctions.De_immuneBuff);
			AddButton(buttonTabPage1, Lang.highLight, 7, Utils.HighLight_E, Utils.HighLight_D);
			AddButton(buttonTabPage1, Lang.ghostMode, 8, Utils.GhostMode_E, Utils.GhostMode_D);
			/*AddButton(buttonTabPage1, Lang.respawnAtOnce, 9, HackFunctions.NoRespawnTime, HackFunctions.De_NoRespawnTime);
			AddButton(buttonTabPage1, Lang.attackThroughWalls, 10, HackFunctions.AttackThroughWalls, HackFunctions.De_AttackThroughWalls);
			AddButton(buttonTabPage1, Lang.noPotionDelay, 11, HackFunctions.NoPotionDelay, HackFunctions.De_NoPotionDelay);*/

			AddButton(buttonTabPage2, Lang.decreaseGravity, 0, Utils.LowGravity_E, Utils.LowGravity_D);
			AddButton(buttonTabPage2, Lang.increaseSpeed, 1, Utils.FastSpeed_E, Utils.FastSpeed_D);
			//AddButton(buttonTabPage2, Lang.killAllNPC, 2, HackFunctions.KillAllNPC, HackFunctions.De_KillAllNPC);
			AddButton(buttonTabPage2, Lang.projectileThroughWalls, 3, Utils.ProjectileIgnoreTile_E, Utils.ProjectileIgnoreTile_D);
			AddButton(buttonTabPage2, Lang.superPick, 4, Utils.GrabItemFarAway_E, Utils.GrabItemFarAway_D);
			AddButton(buttonTabPage2, Lang.extraTwoSlots, 5, Utils.BonusTwoSlots_E, Utils.BonusTwoSlots_D);
			AddButton(buttonTabPage2, Lang.goldHoleDropBag, 6, Utils.GoldHoleDropsBag_E, Utils.GoldHoleDropsBag_D);
			AddButton(buttonTabPage2, Lang.slimeGunBurn, 7, Utils.SlimeGunBurn_E, Utils.SlimeGunBurn_D);
			AddButton(buttonTabPage2, Lang.fishOnlyCrates, 8, Utils.FishOnlyCrates_E, Utils.FishOnlyCrates_D);
			/*AddButton(buttonTabPage2, Lang.killAllScreen, 9, HackFunctions.KillAllScreen, HackFunctions.De_KillAllScreen);
			AddButton(buttonTabPage2, Lang.allRecipe, 10, HackFunctions.EnableAllRecipes, HackFunctions.De_EnableAllRecipes);
			AddButton(buttonTabPage2, Lang.strengthen_Vampire_Knives, 11, HackFunctions.StrengthenVampireKnives, HackFunctions.De_StrengthenVampireKnives);


			AddButton(buttonTabPage6, Lang.blockAttacking, 0, HackFunctions.BlockAttacking, HackFunctions.De_BlockAttacking, true);
			AddButton(buttonTabPage6, Lang.burnAllNPC, 1, HackFunctions.BurnAllNPC, null, false);
			AddButton(buttonTabPage6, Lang.burnAllPlayer, 2, HackFunctions.BurnAllPlayer, null, false);
			AddButton(buttonTabPage6, Lang.dropLava, 3, () =>
			{
				for (int i = 0; i < 50; i++)
				{
					if (!HackFunctions.getPlayerActive(i)) continue;
					if (i == HackFunctions.getMyPlayer()) continue;
					int X = (int)HackFunctions.getPlayerX(i) / 16;
					int Y = (int)HackFunctions.getPlayerY(i) / 16;
					HackFunctions.DropLiquid(X, Y, 32);
					if (HackFunctions.GetNetMode() == 1)
						HackFunctions.SendNetWater(X, Y);
				}
				return 1;
			}, null, false);
			unsafe
			{
				Button u = null;
				u = AddButton(buttonTabPage6, Lang.randomUUID, 4, () =>
				  {
					  if (HackFunctions.RandomUUID() == 0) return 0;
					  StringBuilder UUIDString = new StringBuilder(32 + 4);
					  Int16* UUID = HackFunctions.ReadUUID();
					  for (int i = 0; i < 32 + 4; i++)
					  {
						  UUIDString.Append((char)UUID[i]);
					  }
					  u.Text = Lang.randomUUID + ":" + UUIDString.ToString();
					  FreeMemory((int)UUID);
					  //Marshal.FreeHGlobal((IntPtr)UUID);
					  return 1;
				  }, null, false);
				u.Font = new Font("SimSun", 8);
			}




			AddButton(buttonTabPage3, Lang.toggleDay, 0, HackFunctions.ToggleTime, null, false);
			AddButton(buttonTabPage3, Lang.toggleSunDial, 1, HackFunctions.SunDial, null, false);
			AddButton(buttonTabPage3, Lang.toggleBloodMoon, 2, HackFunctions.BloodMoon, null, false);
			AddButton(buttonTabPage3, Lang.toggleEclipse, 3, HackFunctions.Eclipse, null, false);
			AddButton(buttonTabPage3, Lang.snowMoon, 4, HackFunctions.SnowMoon, null, false);
			AddButton(buttonTabPage3, Lang.pumpkinMoon, 5, HackFunctions.PumpkinMoon, null, false);


			AddButton(buttonTabPage4, Lang.superRange, 0, HackFunctions.IgnoreRange, HackFunctions.De_IgnoreRange);
			AddButton(buttonTabPage4, Lang.fastTileSpeed, 1, HackFunctions.FastTileSpeed, HackFunctions.De_FastTileSpeed);
			AddButton(buttonTabPage4, Lang.rulerEffect, 2, HackFunctions.RulerEffect, HackFunctions.De_RulerEffect);
			AddButton(buttonTabPage4, Lang.machinicalRulerEffect, 3, HackFunctions.MachinicalRulerEffect, HackFunctions.De_MachinicalRulerEffect);
			AddButton(buttonTabPage4, Lang.showCircuit, 4, HackFunctions.ShowCircuit, HackFunctions.De_ShowCircuit);

			AddButton(buttonTabPage5, Lang.infernoEffect, 0, HackFunctions.InfernoEffect, HackFunctions.De_InfernoEffect);
			AddButton(buttonTabPage5, Lang.shadowDodge, 1, HackFunctions.ShadowDodge, HackFunctions.De_ShadowDodge);*/

			LoadPlugins();
		}

		private void LoadPlugins()
		{
			if (!Directory.Exists(".\\plugins"))
			{
				Directory.CreateDirectory(".\\plugins");
			}
			Directory.EnumerateFiles(".\\plugins").ToList().ForEach(s =>
			{
				if (s.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
				{
					FileStream fs = File.Open(s, FileMode.Open);
					byte[] b = new byte[fs.Length];
					fs.Read(b, 0, (int)fs.Length);
					Assembly a = Assembly.Load(b);
					foreach (var type in a.GetTypes())
					{
						object[] ca = type.GetCustomAttributes(typeof(PluginInfo), false);
						if (ca.Length != 0)
						{
							var o = (Plugin)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
							Plugins.Add(o);
							o.Loaded();
							break;
						}
					}
				}
			});

		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public Button AddButton(Panel p, string name, int rank, HackFunc hfunc, HackFunc cancel, bool Closeable = true)
		{
			int height = 30;
			Button b = new Button()
			{
				Text = name,
				Size = new Size(240 + (Closeable ? 0 : 50), height),
				Location = new Point(0, height * rank)
			};
			b.Click += delegate (object sender, EventArgs e)
			 {
				 if (CanHack)
				 {
					 hfunc(Context);
					 status.Text = Lang.sucToHack;
				 }
				 else
				 {
					 status.Text = Lang.faiToHack;
				 }
			 };
			p.Controls.Add(b);
			if (Closeable)
			{
				Button c = new Button();
				c.Text = Lang.off;
				c.Size = new Size(45, height);
				c.Location = new Point(245, height * rank);
				c.Click += delegate (object sender, EventArgs e)
				 {
					 if (CanHack)
					 {
						 cancel(Context);
						 status.Text = Lang.sucToCancel;
					 }
					 else
					 {
						 status.Text = Lang.faiToCancel;
					 }
				 };
				p.Controls.Add(c);
			}
			return b;
		}


		public MainForm()
		{

			BackColor = Color.LightGray;
			cross = (Image)resource.res.GetObject("cross");
			mainWindow = this;
			InitializeComponent();
			InitControls();
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (!flag)
				e.Graphics.DrawImage(cross, 20, 15, 25, 25);
			e.Graphics.DrawString(Lang.dragTip, new Font("Arial", 10), new SolidBrush(Color.Black), 20 + 25 + 20, 20);


		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.X >= 20 && e.Y >= 15 && e.X <= 20 + 25 && e.Y <= 15 + 25)
			{
				this.Cursor = Cursors.Cross;
			}
			else
			{
				if (!flag)
					this.Cursor = Cursors.Default;
			}
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.X >= 20 && e.Y >= 15 && e.X <= 20 + 25 && e.Y <= 15 + 25)
			{
				flag = true;
				this.Refresh();
			}
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (flag)
			{
				POINT p = new POINT();
				GetCursorPos(out p);
				IntPtr wnd;
				wnd = WindowFromPoint(p.X, p.Y);
				GetWindowThreadProcessId(wnd, out processID);
				hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processID);
				Context = GameContext.OpenGame(processID);
				if (CanHack)
					status.Text = string.Format(Lang.baseaddr + ":{0:x8}", Context.MyPlayer.BaseAddress);
				else
					status.Text = Lang.faiToGetBase;
				flag = false;
				this.Refresh();
			}
		}
	}
}