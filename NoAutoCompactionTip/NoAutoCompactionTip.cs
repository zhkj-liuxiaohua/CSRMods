using System;
using System.Collections;
using System.Runtime.InteropServices;
using CSR;

namespace NoAutoCompactionTip
{
	/// <summary>
	/// 隐藏 “Running AutoCompaction ...” 这个提示
	/// </summary>
	public class Program
	{
		public static MCCSAPI mapi;
		
		public static Hashtable rva_bedlogs = new Hashtable();
		public static Hashtable rva_docalls = new Hashtable();
		
		static IntPtr f_bed_log;
		static IntPtr f_do_call;
		
		public delegate long TASK_DO_CALL(long a, long b);
		public delegate void BED_LOG(int a1, int a2, long a3, int a4, int a5, long a6, int a7, long a8);
		
		// 隐藏输出，直接返回
		static readonly BED_LOG hidelog = (a1, a2, a3, a4, a5, a6, a7, a8) => {};	// Console.WriteLine("[NoAutoCompactionTip] 此处隐藏自动压缩提示。");
		
		// 压缩任务期间，输出前截断LOG，完成任务后恢复log功能
		static readonly TASK_DO_CALL hook_task_do_call = (a, b) => {
			int rva = (int)rva_bedlogs[mapi.VERSION];
			mapi.cshook(rva, Marshal.GetFunctionPointerForDelegate(hidelog), out f_bed_log);
			TASK_DO_CALL org = Marshal.GetDelegateForFunctionPointer<TASK_DO_CALL>(f_do_call);
			long ret = org(a, b);
			mapi.csunhook(Marshal.GetFunctionPointerForDelegate(hidelog), ref f_bed_log);
			return ret;
		};
		
		
		
		public static void init(MCCSAPI api) {
			mapi = api;
			
			rva_bedlogs["1.16.220.02"] = 0x01051430;	// IDA BedrockLog::log
			rva_bedlogs["1.16.221.01"] = 0x01051590;
			rva_bedlogs["1.17.0.03"] = 0x0134F520;
			rva_bedlogs["1.17.2.01"] = 0x0134EFF0;
			rva_bedlogs["1.17.10.04"] = 0x0137BF70;
			rva_docalls["1.16.220.02"] = 0xD50380;		// IDA "Running AutoCompaction..."
			rva_docalls["1.16.221.01"] = 0xD50510;
			rva_docalls["1.17.2.01"] = 0xFC19D0;
			rva_docalls["1.17.10.04"] = 0xFAE630;
			int rva = (int)rva_docalls[api.VERSION];
			if (rva != 0)
				if (api.cshook(rva, Marshal.GetFunctionPointerForDelegate(hook_task_do_call), out f_do_call)) {
					Console.WriteLine("[NoAutoCompactionTip] 隐藏AutoCompaction提示已加载。适配版本：" + api.VERSION);
				}
		}
	}
}

namespace CSR
{
	partial class Plugin
	{
		
		/// <summary>
		/// 通用调用接口，需用户自行实现
		/// </summary>
		/// <param name="api">MC相关调用方法</param>
		public static void onStart(MCCSAPI api) {
			Console.WriteLine("[NoAutoCompactionTip] 已装载。请等待版本适配...");
			// TODO 此接口为必要实现
			NoAutoCompactionTip.Program.init(api);
		}
	}
}