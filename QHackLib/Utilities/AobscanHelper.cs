﻿using QHackLib.Assemble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QHackLib.Utilities
{
	public class AobscanHelper
	{

		private static byte Ctoh(char hex)
		{
			if (hex >= '0' && hex <= '9')
				return (byte)(hex - '0');
			if (hex >= 'A' && hex <= 'F')
				return (byte)(hex - 'A' + 10);
			if (hex >= 'a' && hex <= 'f')
				return (byte)(hex - 'a' + 10);
			return 0;
		}
		public static byte[] GetHexCodeFromString(string str)
		{
			List<byte> bs = new List<byte>();

			char[] a = str.ToCharArray();
			byte t = 0;
			bool flag = false;
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != ' ')
				{
					if (flag)
					{
						bs.Add((byte)(t * 0x10 + Ctoh(a[i])));
					}
					t = Ctoh(a[i]);
					flag = !flag;
				}
			}
			return bs.ToArray();
		}
		/// <summary>
		/// 搜索Byte数组
		/// </summary>
		/// <param name="a">源数组</param>
		/// <param name="alen">长度</param>
		/// <param name="b">被搜索的数组</param>
		/// <param name="blen">被搜数组的长度</param>
		/// <returns>失败返回-1</returns>
		public static int Memmem(byte[] a, int alen, byte[] b, int blen)
		{
			int i, j;
			for (i = 0; i < alen - blen; ++i)
			{
				for (j = 0; j < blen; ++j)
					if (a[i + j] != b[j])
						break;
				if (j >= blen)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// 失败返回-1
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static int AobscanASM(Context ctx, string asm)
		{
			return Aobscan(ctx, Assembler.Assemble(asm, 0));
		}
		/// <summary>
		/// 失败返回-1
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="hexCode"></param>
		/// <returns></returns>
		public static int Aobscan(Context ctx, string hexCode)
		{
			byte[] bytes = GetHexCodeFromString(hexCode);
			return Aobscan(ctx, bytes);
		}
		/// <summary>
		/// 失败返回-1
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="aob"></param>
		/// <returns></returns>
		public static int Aobscan(Context ctx, byte[] aob)
		{
			int i = 0;
			NativeFunctions.MEMORY_BASIC_INFORMATION mbi;
			while (i < 0x7FFFFFFF)
			{
				int flag = NativeFunctions.VirtualQueryEx(ctx.Handle, i, out mbi, 28);
				if (flag != 28)
					break;
				if ((int)mbi.RegionSize <= 0)
					break;
				if (mbi.Protect != (UInt32)NativeFunctions.AllocationProtect.PAGE_EXECUTE_READWRITE || mbi.State != 0x00001000)
				{
					i += mbi.RegionSize;
					continue;
				}
				byte[] va = new byte[mbi.RegionSize];
				NativeFunctions.ReadProcessMemory(ctx.Handle, i, va, mbi.RegionSize, 0);
				int r = Memmem(va, mbi.RegionSize, aob, aob.Length);
				if (r >= 0)
				{
					return (int)(i + r);
				}
				i += mbi.RegionSize;
			}
			return -1;
		}
	}
}
