using System;
using CSR;

namespace AlwaysNoCheat
{
	/// <summary>
	/// 实现解锁全部命令为无需作弊状态可使用
	/// </summary>
	public class Program
	{
		public static readonly string LOGCONFIG = @"CSR\configs\logconfig.txt";
		
		// 主入口实现
		public static void init(MCCSAPI api) {
			// 读取配置 根据配置设置hook
			ConfigReader cf = new ConfigReader(LOGCONFIG);
			
			bool nocheatable = cf.getValue("AlwaysNoCheat").ToLower() == "true";
			string nocheatablestr = ("" + nocheatable).ToLower();
			cf.setValue("AlwaysNoCheat", nocheatablestr);
			cf.save();
			if (nocheatable) {
				Hook.init(api);
			}
		}
	}
}

namespace CSR {
	partial class Plugin {
		// 用户主实现
		public static void onStart(MCCSAPI api) {
			AlwaysNoCheat.Program.init(api);
		}
		
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
	}
}