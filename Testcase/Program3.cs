using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
		
		// 临时效果
		static string tmpeff;
		// 记录侧边栏打开次数
		static Dictionary<string, int> sidecount = new Dictionary<string, int>();
		
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
			
			string[] bts = null;
			if (!api.COMMERCIAL) {
				bts = new string[] {
					"后台指令tell",
					"前缀/去前缀名",
					"模拟喊话",
					"模拟执行me指令",
					"后台指令输出hello",
					"查询当前状态至后台",
					"给32个白桦木"
				};
			} else {
				bts = new string[] {
					"后台指令tell",
					"前缀/去前缀名",
					"模拟喊话",
					"模拟执行me指令",
					"后台指令输出hello",
					"查询当前状态至后台",
					"给32个白桦木",			// 以下是非社区版内容
					"穿墙能力开/关",
					"传送至梦之故里大厅",
					"跨维传送至末地祭坛",
					"读当前属性值至后台",
					"临时攻击+10,生命+2,附命+2,抗飞+1,等级+5,食物+2",
					"读属性上限值至后台",
					"攻限+10,命限+10,食限+10",
					"读当前选中物品至后台",
					"给1个附魔叉",
					"替换副手为一个叉子",
					"保存玩家所有物品列表至pit.json并清空",
					"读pit.json到当前玩家",
					"读玩家当前效果列表到后台",
					"设置玩家临时存储的效果列表",
					"显示欢迎一个血条",
					"清除欢迎血条",
					"显示一个带统计的自定义侧边栏",
					"移除自定义侧边栏",
					"读当前权限与游戏模式至后台",
					"切换op/visitor，生存/生存观察者模式",
					"导出当前位置+长宽高x10的结构到st1.json",
					"读结构st1.json到当前位置"
				};
			}
			
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
					var gui = new SimpleGUI(api, uuid, "测试用例", "请于30秒内选择测试用例进行测试：",
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
								
								#region 以下部分为非社区版内容
								
							case "7":
								{
									string sa = api.getPlayerAbilities(uuid);
									if (sa != null) {
										var ser = new JavaScriptSerializer();
										var ja = ser.Deserialize<Dictionary<string, object>>(sa);
										var cja = new Dictionary<string, object>();
										object nnoclip;
										if (ja.TryGetValue("noclip", out nnoclip)) {
											cja["noclip"] = !((bool)nnoclip);
										}
										api.setPlayerAbilities(uuid, ser.Serialize(cja));
									}
								}
								break;
							case "8":
								api.transferserver(uuid, "www.xiafox.com", 19132);
								break;
							case "9":
								api.teleport(uuid, 10, 99, 10, 2);
								break;
							case "10":
								api.logout(api.getPlayerAttributes(uuid));
								break;
							case "11":
								{
									var sa = api.getPlayerAttributes(uuid);
									if (sa != null) {
										var ser = new JavaScriptSerializer();
										var ja = ser.Deserialize<Dictionary<string, object>>(sa);
										var cja = new Dictionary<string, object>();
										object atdmg;
										if (ja.TryGetValue("attack_damage", out atdmg)) {
											cja["attack_damage"] = Convert.ToSingle(atdmg) + 10;
										}
										if (ja.TryGetValue("absorption", out atdmg)) {
											cja["absorption"] = Convert.ToSingle(atdmg) + 2;
										}
										if (ja.TryGetValue("health", out atdmg)) {
											cja["health"] = Convert.ToSingle(atdmg) + 2;
										}
										if (ja.TryGetValue("knockback_resistance", out atdmg)) {
											cja["knockback_resistance"] = Convert.ToSingle(atdmg) + 1;
										}
										if (ja.TryGetValue("level", out atdmg)) {
											cja["level"] = Convert.ToSingle(atdmg) + 5;
										}
										if (ja.TryGetValue("hunger", out atdmg)) {
											cja["hunger"] = Convert.ToSingle(atdmg) + 2;
										}
										api.setPlayerTempAttributes(uuid, ser.Serialize(cja));
									}
								}
								break;
							case "12":
								api.logout(api.getPlayerMaxAttributes(uuid));
								break;
							case "13":
								{
									var sa = api.getPlayerMaxAttributes(uuid);
									if (sa != null) {
										var ser = new JavaScriptSerializer();
										var ja = ser.Deserialize<Dictionary<string, object>>(sa);
										var cja = new Dictionary<string, object>();
										object dmg;
										if (ja.TryGetValue("maxattack_damage", out dmg)) {
											cja["maxattack_damage"] = Convert.ToSingle(dmg) + 10;
										}
										if (ja.TryGetValue("maxhealth", out dmg)) {
											cja["maxhealth"] = Convert.ToSingle(dmg) + 10;
										}
										if (ja.TryGetValue("maxhunger", out dmg)) {
											cja["maxhunger"] = Convert.ToSingle(dmg) + 10;
										}
										api.setPlayerMaxAttributes(uuid, ser.Serialize(cja));
									}
								}
								break;
							case "14":
								api.logout(api.getPlayerSelectedItem(uuid));
								break;
							case "15":
								{
									// tt - TAG_TYPE		标签数据类型，总计11种类型
									// tv - TAG_VALUE		标签值，由类型决定
									// ck - Compound_KEY	nbt关键字，字符串类型
									// cv - Compound_VALUE	nbt值，总是为一个TAG
									var jitem = "{" +
									           "\"tt\": 10, \"tv\": [" +
									           "{ \"ck\": \"Count\", \"cv\": { \"tt\": 1, \"tv\": 1 } }," +
									           "{ \"ck\": \"Damage\", \"cv\": { \"tt\": 2, \"tv\": 0 } }," +
									           "{ \"ck\": \"Name\", \"cv\": { \"tt\": 8, \"tv\": \"minecraft:trident\" } }," +
									           "{ \"ck\": \"tag\", \"cv\": { \"tt\": 10, \"tv\": [" +
									           "{ \"ck\": \"ench\", \"cv\": { \"tt\": 9, \"tv\": [" +
									           "{ \"tt\": 10, \"tv\": [" +
									           "{ \"ck\": \"id\", \"cv\": { \"tt\": 2, \"tv\": 10 } }," +
									           "{ \"ck\": \"lvl\", \"cv\": { \"tt\": 2, \"tv\": 9999 } }]" +
									           "}]}" +
									           "}]}" +
									           "}]" +
									           "}";
									api.addPlayerItemEx(uuid, jitem);
								}
								break;
							case "16":
								{
									var jtem = "{" +
									           "\"Offhand\": { \"tt\": 9, \"tv\": [" +
									           "{ \"tt\": 10, \"tv\": [" +
									           "{ \"ck\": \"Count\", \"cv\": { \"tt\": 1, \"tv\": 1 } }," +
									           "{ \"ck\": \"Damage\", \"cv\": { \"tt\": 2, \"tv\": 0 } }," +
									           "{ \"ck\": \"Name\", \"cv\": { \"tt\": 8, \"tv\": \"minecraft:trident\" } }," +
									           "{ \"ck\": \"tag\", \"cv\": { \"tt\": 10, \"tv\": [" +
									           "{ \"ck\": \"ench\", \"cv\": {" +
									           "\"tt\": 9, \"tv\": [" +
									           "{ \"tt\": 10, \"tv\": [" +
									           "{ \"ck\": \"id\", \"cv\": { \"tt\": 2, \"tv\": 7 } }," +
									           "{ \"ck\": \"lvl\", \"cv\": { \"tt\": 2, \"tv\": 8888 } }]" +
									           "}]}" +
									           "}]}" +
									           "}]" +
									           "}]}" +
									           "}";
									api.setPlayerItems(uuid, jtem);
								}
								break;
							case "17":
								{
									var its = api.getPlayerItems(uuid);
									File.WriteAllText("pit.json", its);
									// 此处使用空气填充末影箱
									var sair = "{\"tt\":10,\"tv\":[{\"ck\":\"Count\",\"cv\":{\"tt\":1,\"tv\":0}},{\"ck\":\"Damage\",\"cv\":{\"tt\":2,\"tv\":0}},{\"ck\":\"Name\"" + ",\"cv\":{\"tt\":8,\"tv\":\"\"}},{\"ck\":\"Slot\",\"cv\":{\"tt\":1,\"tv\":0}}]}";
									//let air = { "tt": 10, "tv": [{ "ck": "Count", "cv": { "tt": 1, "tv": 0 } }, { "ck": "Damage", "cv": { "tt": 2, "tv": 0 } }, { "ck": "Name", "cv": { "tt": 8, "tv": "" } }, { "ck": "Slot", "cv": { "tt": 1, "tv": 0 } }] };
						
									var endc = new Dictionary<string, object>();
									var eci = new Dictionary<string, object>();
									var edclist = new ArrayList();
									eci["tt"] = 9;
									var ser = new JavaScriptSerializer();
									for (int i = 0; i < 27; i++) {
										var mair = ser.Deserialize<Dictionary<string, object>>(sair);
										if (((Dictionary<string, object>)(((ArrayList)(mair["tv"]))[3]))["ck"].ToString() == "Slot") {		// 此处需修改并应用
											((Dictionary<string, object>)(((Dictionary<string, object>)(((ArrayList)(mair["tv"]))[3]))["cv"]))["tv"] = i;
										}
										edclist.Add(mair);
									}
									eci["tv"] = edclist;
									endc["EnderChestInventory"] = eci;
									api.setPlayerItems(uuid, ser.Serialize(endc));
									api.runcmd("clear \"" + e.playername + "\"");
								}
								break;
							case "18":
								{
									try {
										var its = File.ReadAllText("pit.json");
										if (its != null) {
											api.setPlayerItems(uuid, its);
										}
									} catch {
									}
								}
								break;
							case "19":
								{
									var efs = api.getPlayerEffects(uuid);
									tmpeff = efs;
									Console.WriteLine(efs);
								}
								break;
							case "20":
								api.setPlayerEffects(uuid, tmpeff);
								break;
							case "21":
								api.setPlayerBossBar(uuid, "欢迎使用NetRunner自定义血条！", ((float)(new Random().Next() % 1001)) / 1000.0f);
								break;
							case "22":
								api.removePlayerBossBar(uuid);
								break;
							case "23":
								{
									int count = 0;
									if (!sidecount.TryGetValue(uuid, out count)) {
										sidecount[uuid] = 0;
									}
									++sidecount[uuid];
									var ser = new JavaScriptSerializer();
									var list = new List<object>();
									list.Add("第" + sidecount[uuid] + "次打开侧边栏    ");
									list.Add("这是第二行 ");
									list.Add("这是下一行 ");
									list.Add("属性：[待填写] ");
									list.Add("§e颜色自拟 ");
									api.setPlayerSidebar(uuid, e.playername + "的侧边栏", ser.Serialize(list));
								}
								break;
							case "24":
								api.removePlayerSidebar(uuid);
								break;
							case "25":
								Console.WriteLine(api.getPlayerPermissionAndGametype(uuid));
								break;
							case "26":
								{
									var st = api.getPlayerPermissionAndGametype(uuid);
									var ser = new JavaScriptSerializer();
									var t = ser.Deserialize<Dictionary<string, object>>(st);
									if (t != null) {
										t["gametype"] = (int)(t["gametype"]) == 0 ? 3 : 0;
										t["permission"] = (int)(t["permission"]) == 0 ? 2 : 0;
										api.setPlayerPermissionAndGametype(uuid, ser.Serialize(t));
									}
								}
								break;
							case "27":
								{
									var posa = e.XYZ;
									var ser = new JavaScriptSerializer();
									var posb = new Vec3();
									posb.x = posa.x + 10;
									posb.y = posa.y + 10;
									posb.z = posa.z + 10;
									var data = api.getStructure(e.dimensionid, ser.Serialize(posa), ser.Serialize(posb), false, true);
									File.WriteAllText("st1.json", data);
								}
								break;
							case "28":
								try {
									var data = File.ReadAllText("st1.json");
									var ser = new JavaScriptSerializer();
									api.setStructure(data, e.dimensionid, ser.Serialize(e.XYZ), 0, true, true);
								} catch {
								}
								break;
								
								#endregion
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