/*
 * 由SharpDevelop创建。
 * 用户： BDSNetRunner
 * 日期: 2020/7/27
 * 时间: 23:47
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections;
using System.IO;

namespace CSR
{
	/// <summary>
	/// 等号分割符用的配置文件
	/// </summary>
	public class ConfigReader
	{
		public string configpath;
		private Hashtable mconfigs;
		
		public ConfigReader(string filepath)
		{
			configpath = filepath;
			reload();
		}
		
		/// <summary>
		/// 保存配置
		/// </summary>
		public void save() {
			var al = new ArrayList();
			foreach(string k in mconfigs.Keys) {
				string pair = k + "=" + mconfigs[k];
				al.Add(pair);
			}
			try {
				string dir = Path.GetDirectoryName(configpath);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				File.WriteAllLines(configpath, (string[])al.ToArray(typeof(string)));
			}catch(Exception e){Console.WriteLine(e.StackTrace);}
		}
		
		/// <summary>
		/// 重载配置文件
		/// </summary>
		public void reload() {
			mconfigs = new Hashtable();
			string [] datas = null;
			try {
				datas = File.ReadAllLines(configpath);
			} catch {}
			if (datas != null && datas.Length > 0) {
				// 读取配置文件
				foreach(string s in datas) {
					var sec = s.Split('=');
					if (sec.Length == 2) {
						string k = sec[0].Trim();
						string v = sec[1].Trim();
						mconfigs[k] = v;
					}
				}
			} else {
				Console.WriteLine("[Base] 读取配置失败，使用默认配置。配置文件位于 {0}", configpath);
			}
		}
		
		/// <summary>
		/// 添加一个配置项
		/// </summary>
		/// <param name="k"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public bool setValue(string k, string v) {
			if (mconfigs == null || string.IsNullOrEmpty(configpath))
				return false;
			mconfigs[k] = v;
			return true;
		}
		
		/// <summary>
		/// 获取一个配置项
		/// </summary>
		/// <param name="k"></param>
		/// <returns></returns>
		public string getValue(string k) {
			string ret = string.Empty;
			if (mconfigs != null) {
				object o = mconfigs[k];
				if (o != null)
					ret = o.ToString();
			}
			return  ret;
		}
	}
}
