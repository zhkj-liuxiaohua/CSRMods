using System;
using System.Collections;
using System.Runtime.InteropServices;
using CSR;

namespace AlwaysNoCheat
{
	/// <summary>
	/// 解锁所有指令为无作弊即可使用状态
	/// </summary>
	public class Hook
	{
		delegate ulong CMD_REG_Func(ulong a1, ulong a2, ulong a3, byte level, byte f1, byte f2);
		private static IntPtr _CS_REGISTERCOMMAND_org = IntPtr.Zero;
		/// <summary>
		/// 新方法，修改注册的命令标识，所有指令全部增加一个无需作弊可用的flag
		/// </summary>
		/// <param name="a1">CommandRegistry句柄</param>
		/// <param name="a2">指令原型</param>
		/// <param name="a3">指令描述</param>
		/// <param name="level">需求op等级</param>
		/// <param name="f1">指令属性flag1</param>
		/// <param name="f2">指令属性flag2</param>
		/// <returns></returns>
		private static ulong _cshook_registerCommand(ulong a1, ulong a2, ulong a3, byte level, byte f1, byte f2) {
			f1 |= (byte)MCCSAPI.CommandCheatFlag.NotCheat;
			var org = Marshal.GetDelegateForFunctionPointer(_CS_REGISTERCOMMAND_org, typeof(CMD_REG_Func)) as CMD_REG_Func;
			return org(a1, a2, a3, level, f1, f2);
		}
		private static readonly CMD_REG_Func cs_reghookptr = _cshook_registerCommand;

		// 注册hook
		public static void init(MCCSAPI api) {
			int rva = 0;
			switch(api.VERSION) {	// 版本适配
				case "1.16.1.2":
					rva = 0x00429850;
					break;
				case "1.16.10.2":
					rva = 0x00429820;
					break;
				case "1.16.20.3":
					rva = 0x0042D250;
					break;
				case "1.16.40.2":
					rva = 0x0042D260;
					break;
				case "1.16.100.4":
					rva = 0x00A1E8E0;
					break;
			}
			if (rva != 0) {
				bool ret = api.cshook(rva,	// IDA CommandRegistry::registerCommand
					           Marshal.GetFunctionPointerForDelegate(cs_reghookptr),
					           out _CS_REGISTERCOMMAND_org);
				if (ret)
					Console.WriteLine("作弊指令已解锁。");
			}
		}
	}
}
