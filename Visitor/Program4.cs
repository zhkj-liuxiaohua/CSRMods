using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using CSR;

namespace Visitor
{
	/// <summary>
	/// 实现后台访客
	/// </summary>
	public class Program
	{
		static MCCSAPI mapi;
		
		static Dictionary<string, string> namexuids = new Dictionary<string, string>();
		static Dictionary<string, string> nameuuids = new Dictionary<string, string>();
		
		// 从在线列表中获取xuid
		public static string getXUID(string name)
		{
			string xuid = null;
			if (namexuids.TryGetValue(name, out xuid)) {
				return xuid;
			}
			// 重载在线列表
			namexuids.Clear();
			nameuuids.Clear();
			string ols = mapi.getOnLinePlayers();
			if (!string.IsNullOrEmpty(ols)) {
				var ser = new JavaScriptSerializer();
				ArrayList ol = ser.Deserialize<ArrayList>(ols);
				foreach (Dictionary<string, object> d in ol) {
					object tname, txuid, tuuid;
					if (d.TryGetValue("playername", out tname)) {
						if (d.TryGetValue("xuid", out txuid)) {
							namexuids[tname.ToString()] = txuid.ToString();
							if (tname.ToString() == name) {
								xuid = txuid.ToString();
							}
						}
						if (d.TryGetValue("uuid", out tuuid)) {
							nameuuids[tname.ToString()] = tuuid.ToString();
						}
					}
				}
			}
			return xuid;
		}
		// 获取uuid
		public static string getUUUD(string name)
		{
			string uuid;
			if (nameuuids.TryGetValue(name, out uuid)) {
				return uuid;
			}
			// 重载在线列表
			nameuuids.Clear();
			namexuids.Clear();
			string ols = mapi.getOnLinePlayers();
			if (!string.IsNullOrEmpty(ols)) {
				try {
					var ser = new JavaScriptSerializer();
					ArrayList ol = ser.Deserialize<ArrayList>(ols);
					foreach (Dictionary<string, object> d in ol) {
						object tname, tuuid, txuid;
						if (d.TryGetValue("playername", out tname)) {
							if (d.TryGetValue("uuid", out tuuid)) {
								nameuuids[tname.ToString()] = tuuid.ToString();
								if (tname.ToString() == name) {
									uuid = tuuid.ToString();
								}
							}
							if (d.TryGetValue("xuid", out txuid)) {
								namexuids[tname.ToString()] = txuid.ToString();
							}
						}
					}
				} catch(Exception e){Console.WriteLine(e.StackTrace);}
			}
			return uuid;
		}
		
		// 从白名单中获取xuid
		public static string getLeftXUID(string name) {
			string wl = string.Empty;
			try {
				wl = File.ReadAllText("whitelist.json");
			} catch{}
			var ser = new JavaScriptSerializer();
			if (!string.IsNullOrEmpty(wl)) {
				try {
					var wlal = ser.Deserialize<ArrayList>(wl);
					if (wlal != null && wlal.Count > 0) {
						foreach (Dictionary<string, object> d in wlal) {
							object dname;
							if (d.TryGetValue("name", out dname)) {
								if (dname.ToString() == name) {	// 找到
									object dxuid;
									if (d.TryGetValue("xuid", out dxuid)) {
										return dxuid.ToString();
									}
								}
							}
						}
					}
				}catch(Exception e){Console.WriteLine(e.StackTrace);}
			}
			return null;
		}
		
		// 将玩家加入权限列表
		public static bool visitorPlayer(string xuid) {
			if (!string.IsNullOrEmpty(xuid)) {
				var ser = new JavaScriptSerializer();
				string sops = string.Empty;
				try {
					sops = File.ReadAllText("permissions.json");
				} catch{}
				var opl = new ArrayList();
				bool finded = false;
				if (!string.IsNullOrEmpty(sops)) {
					var vis = ser.Deserialize<ArrayList>(sops);
					vis = vis ?? new ArrayList();
					opl = vis;
					foreach (Dictionary<string, object> d in opl) {
						object dxuid;
						if (d.TryGetValue("xuid", out dxuid)) {
							if (dxuid.ToString() == xuid) {	// 找到
								d["permission"] = "visitor";
								finded = true;
								break;
							}
						}
					}
				}
				if (!finded) {	// 装入新权限
					var nd = new Dictionary<string, object>();
					nd["permission"] = "visitor";
					nd["xuid"] = xuid;
					opl.Add(nd);
				}
				try {
					File.WriteAllText("permissions.json", ser.Serialize(opl));
					return true;
				} catch{}
			}
			return false;
		}
		// 发送一个tellraw
		public static void tellraw(string pname, string msg) {
			var ser = new JavaScriptSerializer();
			var rawtxt = new Dictionary<string, object>();
			var rawtext = new ArrayList();
			var text = new Dictionary<string, object>();
			text["text"] = msg;
			rawtext.Add(text);
			rawtxt["rawtext"] = rawtext;
			mapi.runcmd("tellraw \"" + pname + "\" " + ser.Serialize(rawtxt));
		}
		
		// 主入口实现
		public static void init(MCCSAPI api) {
			mapi = api;
			
			// 监听后台指令
			api.addBeforeActListener(EventKey.onServerCmd, x => {
				var e = BaseEvent.getFrom(x) as ServerCmdEvent;
				string scmd = e.cmd.Trim();
				if (scmd.ToLower().IndexOf("visitor") == 0) {	// 可能找到
					string[] cmds = scmd.Split(' ');
					if (cmds[0].ToLower() == "visitor") {	// 找到
						if (cmds.Length > 1) {
							string pname = scmd.Substring(7).Trim().Trim('"');
							string xuid = null;
							if (!string.IsNullOrEmpty(xuid = getXUID(pname))) {
								// 在线降权
								if (visitorPlayer(xuid)) {
									tellraw(pname, "您已被降级权限为访客。");
									api.logout("Visited : " + pname);
									return false;
								}
							} else if (!string.IsNullOrEmpty(xuid = getLeftXUID(pname))) {
								// 离线降权
								if (visitorPlayer(xuid)) {
									api.logout("玩家 " + pname + " 已被降级权限为访客。");
									return false;
								}
							} else {
								api.logout("未能找到对应玩家。");
							}
						} else {
							api.logout("[vistor] 参数过少。用法：visitor [playername]");
						}
						return false;
					}
				}
				return true;
			});
			
			// 离开监听
			api.addAfterActListener(EventKey.onPlayerLeft, x => {
				var e = BaseEvent.getFrom(x) as PlayerLeftEvent;
				string uuid, xuid;
				if (nameuuids.TryGetValue(e.playername, out uuid)) {
					nameuuids.Remove(e.playername);
				}
				if (namexuids.TryGetValue(e.playername, out xuid)) {
					namexuids.Remove(e.playername);
				}
				return true;
			});
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
			// TODO 此接口为必要实现
			Visitor.Program.init(api);
			Console.WriteLine("[visitor] 访客命令已装载（仅限后台）。用法：visitor [playername]");
		}
	}
}