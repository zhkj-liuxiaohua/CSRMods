using System;
using System.Runtime.InteropServices;
using CSR;

namespace HideSeed
{
	/// <summary>
	/// 隐藏种子
	/// </summary>
	public class Hook
	{
		static int fakeseed;
		
		delegate ulong HIDESEEDPACKET(IntPtr a1, ulong a2);
		static IntPtr hideorg = IntPtr.Zero;
		/// <summary>
		/// 新方法，hook方式修改数据包，隐藏种子
		/// </summary>
		/// <param name="a1">StartGamePacket指针</param>
		/// <param name="a2">BinaryStream引用</param>
		/// <returns></returns>
		private static readonly HIDESEEDPACKET hide = (a1, a2) => {
			Marshal.WriteInt32(a1 + 40, fakeseed);
			var org = Marshal.GetDelegateForFunctionPointer<HIDESEEDPACKET>(hideorg);
			return org(a1, a2);
		};

		// 注册hook
		public static void init(MCCSAPI api, int seed) {
			fakeseed = seed;
			int rva = 0;
			switch(api.VERSION) {	// 版本适配
				case "1.16.1.2":
					rva = 0x00380010;
					break;
				case "1.16.10.2":
					rva = 0x0037FFE0;
					break;
			}
			if (rva != 0) {
				bool ret = api.cshook(rva,	// IDA StartGamePacket::write
					           Marshal.GetFunctionPointerForDelegate(hide),
					           out hideorg);
				if (ret)
					Console.WriteLine("[HideSeed] 种子已隐藏。");
			}
		}
	}
}
