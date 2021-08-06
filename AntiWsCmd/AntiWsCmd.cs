/*
 * 由SharpDevelop创建。
 * 用户： classmates
 * 日期: 2021/8/6
 * 时间: 15:04
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Runtime.InteropServices;
using CSR;

namespace AntiWsCmd
{
	/// <summary>
	/// ws指令显形
	/// </summary>
	public class MyClass
	{
		static MCCSAPI mapi;
		static IntPtr org;
		
		public delegate long ClientAutomationCommandOrigin_getName(long a, long b);
		
		static readonly ClientAutomationCommandOrigin_getName wsf = (a, b) =>  {
			IntPtr func_PlayerCommandOrigin_getName = IntPtr.Zero;
			switch(mapi.VERSION) {
				case "1.17.10.04":	// IDA PlayerCommandOrigin::getName
					func_PlayerCommandOrigin_getName = mapi.dlsym(0x00786150);
					break;
			}
			if (func_PlayerCommandOrigin_getName != IntPtr.Zero) {
				ClientAutomationCommandOrigin_getName pgname = Marshal.GetDelegateForFunctionPointer<ClientAutomationCommandOrigin_getName>(
				func_PlayerCommandOrigin_getName);
				return pgname(a, b);
			}
			return 0;
		};
		
		public static void init(MCCSAPI api) {
			mapi = api;
			switch (api.VERSION) {
				case "1.17.10.04":	// IDA ClientAutomationCommandOrigin::getName
					if (api.cshook(0x00786020, Marshal.GetFunctionPointerForDelegate(wsf), out org)) {
						Console.WriteLine("[AntiWsCmd] ws指令显形已加载。");
					}
					break;
			}
		}
	}
}

namespace CSR {
	partial class Plugin {
		public static void onStart(MCCSAPI api) {
			AntiWsCmd.MyClass.init(api);
		}
	}
}