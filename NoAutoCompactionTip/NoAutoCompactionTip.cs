/*
 * 由SharpDevelop创建。
 * 用户： Admin
 * 日期: 2021/5/17
 * 时间: 20:37
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections;
using System.Collections.Generic;
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
		static BED_LOG hidelog = (a1, a2, a3, a4, a5, a6, a7, a8) => {};	// Console.WriteLine("[NoAutoCompactionTip] 此处隐藏自动压缩提示。");
		
		// 压缩任务期间，输出前截断LOG，完成任务后恢复log功能
		static TASK_DO_CALL hook_task_do_call = (a, b) => {
			int rva = (int)rva_bedlogs[mapi.VERSION];
			mapi.cshook(rva, Marshal.GetFunctionPointerForDelegate(hidelog), out f_bed_log);
			TASK_DO_CALL org = Marshal.GetDelegateForFunctionPointer<TASK_DO_CALL>(f_do_call);
			long ret = org(a, b);
			mapi.csunhook(Marshal.GetFunctionPointerForDelegate(hidelog), ref f_bed_log);
			return ret;
		};
		
		
		
		public static void init(MCCSAPI api) {
			mapi = api;
			
			rva_bedlogs["1.16.221.01"] = 0x01051590;
			rva_docalls["1.16.221.01"] = 0xD50510;
			
			int rva = (int)rva_docalls[api.VERSION];
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
		private static MCCSAPI mapi = null;
		/// <summary>
		/// 静态api对象
		/// </summary>
		public static MCCSAPI api { get { return mapi; } }
		public static int onServerStart(string pathandversion) {
			string path = null, version = null;
			bool commercial = false;
			string [] pav = pathandversion.Split(',');
			if (pav.Length > 1) {
				path = pav[0];
				version = pav[1];
				commercial = (pav[pav.Length - 1] == "1");
				mapi = new MCCSAPI(path, version, commercial);
				if (mapi != null) {
					onStart(mapi);
					GC.KeepAlive(mapi);
					return 0;
				}
			}
			Console.WriteLine("Load failed.");
			return -1;
		}
		
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