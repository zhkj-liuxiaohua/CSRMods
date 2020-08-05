using System;
using CSR;

namespace HideSeed
{
	/// <summary>
	/// 隐藏种子
	/// </summary>
	public class Program
	{
		static MCCSAPI mapi;
		
		/// <summary>
		/// 固定配置文件所在路径
		/// </summary>
		static readonly string LOGCONFIG = @"CSR\configs\logconfig.txt";
		
		public static void init(MCCSAPI api) {
			mapi = api;
			
			var cf = new ConfigReader(LOGCONFIG);
			
			var seedstr = cf.getValue("fakeseed");
			if (!string.IsNullOrEmpty(seedstr)) {
				// 此处隐藏种子
				try {
					int fakeseed = Convert.ToInt32(seedstr);
					Hook.init(api, fakeseed);
				} catch(Exception e) {Console.WriteLine(e.StackTrace);}
			} else {
				Console.WriteLine("[HideSeed] 默认值为空，不予隐藏。");
				cf.setValue("fakeseed", null);
				cf.save();
			}
		}
	}
}

namespace CSR
{
	partial class Plugin
	{
		public static int onServerStart(string pathandversion) {
			string path = null, version = null;
			bool commercial = false;
			string [] pav = pathandversion.Split(',');
			if (pav.Length > 1) {
				path = pav[0];
				version = pav[1];
				commercial = (pav[pav.Length - 1] == "1");
				var api = new MCCSAPI(path, version, commercial);
				if (api != null) {
					onStart(api);
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
			Console.WriteLine("[HideSeed] 已装载。请等待版本适配...");
			// TODO 此接口为必要实现
			HideSeed.Program.init(api);
		}
	}
}