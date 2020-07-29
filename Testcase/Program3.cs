using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using CSR;

namespace Testcase
{
	/// <summary>
	/// 进行一个测试命令
	/// </summary>
	public class Program
	{
		static MCCSAPI mapi;
		
		static Dictionary<string, string> nameuuids = new Dictionary<string, string>();
		
		// 获取uuid
		public static string getUUUD(string name)
		{
			string uuid;
			if (nameuuids.TryGetValue(name, out uuid)) {
				return uuid;
			}
			// 重载在线列表
			nameuuids.Clear();
			string ols = mapi.getOnLinePlayers();
			if (!string.IsNullOrEmpty(ols)) {
				var ser = new JavaScriptSerializer();
				ArrayList ol = ser.Deserialize<ArrayList>(ols);
				foreach (Dictionary<string, object> d in ol) {
					object tname, tuuid;
					if (d.TryGetValue("playername", out tname)) {
						if (d.TryGetValue("uuid", out tuuid)) {
							nameuuids[tname.ToString()] = tuuid.ToString();
							if (tname.ToString() == name) {
								uuid = tuuid.ToString();
							}
						}
					}
				}
			}
			return uuid;
		}
		
		// 主入口实现
		public static void init(MCCSAPI api)
		{
			mapi = api;
			
			string[] bts = {
				"后台指令tell",
				"前缀/去前缀名",
				"模拟喊话",
				"模拟执行me指令",
				"后台指令输出hello",
				"查询当前状态至后台",
				"给32个白桦木"
			};
			var a = new ArrayList(bts);
			// 设置指令描述
			api.setCommandDescribeEx("testcase", "开始一个测试用例", MCCSAPI.CommandPermissionLevel.GameMasters,
				(byte)MCCSAPI.CommandCheatFlag.NotCheat, 0);
			// 添加测试指令监听
			api.addBeforeActListener(EventKey.onInputCommand, x => {
				var e = BaseEvent.getFrom(x) as InputCommandEvent;
				if (e.cmd.Trim() == "/testcase") {
					// 此处执行拦截操作
					string uuid = getUUUD(e.playername);
					SimpleGUI gui = new SimpleGUI(api, uuid, "测试用例", "对下列测试用例进行测试：",
						                                         a);
					gui.send(30000, selected => {
						switch (selected) {
							case "0":
								api.runcmd("tell \"" + e.playername + "\" 你好 c# !");
								break;
							case "1":
								{
									string r = e.playername;
									r = (r.IndexOf("[前缀]") >= 0 ? r.Substring(4) : "[前缀]" + r);
									api.reNameByUuid(uuid, r);
								}
								break;
							case "2":
								api.talkAs(uuid, "你好 c# !");
								break;
							case "3":
								api.runcmdAs(uuid, "/me 你好 c# !");
								break;
							case "4":
								api.logout("logout: 你好 c# !");
								break;
							case "5":
								Console.WriteLine(api.selectPlayer(uuid));
								break;
							case "6":
								api.addPlayerItem(uuid, 17, 2, 32);
								break;
							case "null":
								Console.WriteLine("玩家 " + e.playername + " 主动取消了一个表单项。", gui.id);
								break;
						}
					},
						() => Console.WriteLine("[testcase] 玩家 " + e.playername + " 表单已超时，id={0}", gui.id));
					return false;
				}
				return true;
			});
			// 离开监听
			api.addAfterActListener(EventKey.onPlayerLeft, x => {
			                        	var e = BaseEvent.getFrom(x) as PlayerLeftEvent;
			                        	string uuid;
			                        	if (nameuuids.TryGetValue(e.playername, out uuid)) {
			                        		nameuuids.Remove(e.playername);
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
			// TODO 此接口为必要实现
			Testcase.Program.init(api);
			Console.WriteLine("[testcase] 测试命令已装载。用法：/testcase");
		}
	}
}