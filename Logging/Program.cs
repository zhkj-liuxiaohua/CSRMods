using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CSR;
namespace Logging
{
	public class Logger {
		private const string AUTOSAVEKEY = "autoSave";
		private const string LOGPATHKEY = "logpath";
		
		private readonly bool mautoSave;
		/// <summary>
		/// 是否自动保存log
		/// </summary>
		public bool autoSave {get{return mautoSave;}}
		private readonly string mpath;
		/// <summary>
		/// log保存所在路径
		/// </summary>
		public string filepath {get{return mpath;}}
		public Logger(string path) {
			var cf = new ConfigReader(path);
			string extsave = cf.getValue(AUTOSAVEKEY), extpath = cf.getValue(LOGPATHKEY);
			if (!string.IsNullOrEmpty(extsave)) {
				mautoSave = (extsave.ToLower() == "true");
			} else
				mautoSave = false;
			if (!string.IsNullOrEmpty(extpath)) {
				mpath = extpath;
			} else
				mpath = "";
			// 写回原数据
			cf.setValue(AUTOSAVEKEY, ("" + mautoSave).ToLower());
			cf.setValue(LOGPATHKEY, mpath);
			cf.save();
			try {
				if (!string.IsNullOrEmpty(mpath)) {
					string mdir = Path.GetDirectoryName(mpath);
					if (!Directory.Exists(mdir))
						Directory.CreateDirectory(mdir);
				}
			}catch(Exception e){Console.WriteLine(e.StackTrace);}
		}
		// 写入一行日志
		public void saveLine(string l) {
			if (autoSave) {
				if (!string.IsNullOrEmpty(filepath))
					try {
						File.AppendAllLines(filepath, new string[]{ l });
					} catch {
					}
			}
		}
	}
	
	class Program {
		private static MCCSAPI mapi = null;
		
		/// <summary>
		/// 固定配置文件所在路径
		/// </summary>
		static readonly string LOGCONFIG = @"CSR\configs\logconfig.txt";
		
		// 返回一个时间前缀
		public static string title(string content) {
			DateTime t = DateTime.Now;
			return "[" + t.ToString("yyyy-MM-dd hh:mm:ss") + " " + content + "]";
		}
		// 打印坐标
		public static string Coordinator(BPos3 b) {
			return "(" + b.x + ", " + b.y + ", " + b.z + ")";
		}
		public static string Coordinator(Vec3 b) {
			return "(" + (int)b.x + ", " + (int)b.y + ", " + (int)b.z + ")";
		}
		// 维度ID转换为中文字符
		public static string toDimenStr(int dimensionId) {
			switch (dimensionId) {
				case 0:
					return "主世界";
				case 1:
					return "地狱";
				case 2:
					return "末地";
				default:
					break;
			}
			return "未知维度";
		}
		
		// 主入口实现
		public static void init(MCCSAPI api) {
			mapi = api;
			Console.OutputEncoding = Encoding.UTF8;
			// 从固定路径读取配置文件
			var logsetting = new Logger(LOGCONFIG);
			
			// 放置方块监听
			api.addAfterActListener(EventKey.onPlacedBlock, x => {
				var e = BaseEvent.getFrom(x) as PlacedBlockEvent;
				if(e == null) return true;
				string str = string.Format("{0} 玩家 {1} {2}在 {3} {4} 放置 {5} 方块。",
					title(EventKey.onPlacedBlock), e.playername,
					!e.isstand ? "悬空地":"",
					e.dimension,
					Coordinator(e.position),
					e.blockname);
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 使用物品监听
			api.addAfterActListener(EventKey.onUseItem, x => {
				var e = BaseEvent.getFrom(x) as UseItemEvent;
				if(e == null) return true;
				if (e.RESULT) {
					string str = string.Format("{0} 玩家 {1} {2}对 {3} {4} 处的 {5} 方块使用 {6} 物品。",
					title(EventKey.onUseItem), e.playername,
					!e.isstand ? "悬空地":"",
					e.dimension,
					Coordinator(e.position),
					e.blockname,
					e.itemname);
					Console.WriteLine("{" + str);
					if (logsetting.autoSave) {
						var t = new Thread(() => logsetting.saveLine(str));
						t.Start();
					}
				}
				return true;
			});
			// 破坏方块监听
			api.addAfterActListener(EventKey.onDestroyBlock, x => {
				var e = BaseEvent.getFrom(x) as DestroyBlockEvent;
				if(e == null) return true;
				string str = string.Format("{0} 玩家 {1} {2}在 {3} {4} 破坏 {5} 方块。",
					title(EventKey.onDestroyBlock), e.playername,
					!e.isstand ? "悬空地":"",
					e.dimension,
					Coordinator(e.position),
					e.blockname);
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 玩家打开箱子
			api.addAfterActListener(EventKey.onStartOpenChest, x => {
					var e = BaseEvent.getFrom(x) as StartOpenChestEvent;
					if(e == null) return true;
					string str = string.Format("{0} 玩家 {1} {2}在 {3} {4} 打开 {5} 箱子。",
					title(EventKey.onDestroyBlock), e.playername,
					!e.isstand ? "悬空地":"",
					e.dimension,
					Coordinator(e.position),
					e.blockname);
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 玩家打开木桶
			api.addAfterActListener(EventKey.onStartOpenBarrel, x => {
				var e = BaseEvent.getFrom(x) as StartOpenBarrelEvent;
				if(e == null) return true;
					string str = string.Format("{0} 玩家 {1} {2}在 {3} {4} 打开 {5} 木桶。",
					title(EventKey.onDestroyBlock), e.playername,
					!e.isstand ? "悬空地":"",
					e.dimension,
					Coordinator(e.position),
					e.blockname);
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 玩家关闭箱子
			api.addAfterActListener(EventKey.onStopOpenChest, x => {
					var e = BaseEvent.getFrom(x) as StopOpenChestEvent;
					if(e == null) return true;
					string str = string.Format("{0} 玩家 {1} {2}在 {3} {4} 关闭 {5} 箱子。",
					title(EventKey.onDestroyBlock), e.playername,
					!e.isstand ? "悬空地":"",
					e.dimension,
					Coordinator(e.position),
					e.blockname);
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 玩家关闭木桶
			api.addAfterActListener(EventKey.onStopOpenBarrel, x => {
				var e = BaseEvent.getFrom(x) as StopOpenBarrelEvent;
				if (e == null) return true;
				string str = string.Format("{0} 玩家 {1} {2}在 {3} {4} 关闭 {5} 木桶。",
					              title(EventKey.onDestroyBlock), e.playername,
					              !e.isstand ? "悬空地" : "",
					              e.dimension,
					              Coordinator(e.position),
					              e.blockname);
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 放入取出物品
			api.addAfterActListener(EventKey.onSetSlot, x => {
				var e = BaseEvent.getFrom(x) as SetSlotEvent;
				if(e == null) return true;
				string str, str1, str2;
				str1 = string.Format("{0} 玩家 {1} {2}在 {3} {4} 的 {5} 里的第 {6} 格",
					title(EventKey.onSetSlot), e.playername,
					!e.isstand ? "悬空地" : "",
					e.dimension,
					Coordinator(e.position),
					e.blockname,
					e.slot);
				str2 = (e.itemcount > 0) ? string.Format(" 放入 {0} 个 {1} 物品。",
					e.itemcount,
					e.itemname) :
					" 取出物品。";
				str = str1 + str2;
				Console.WriteLine("{" + str);
				if (logsetting.autoSave) {
					var t = new Thread(() => logsetting.saveLine(str));
					t.Start();
				}
				return true;
			});
			// 玩家切换维度
			api.addAfterActListener(EventKey.onChangeDimension, x => {
				var e = BaseEvent.getFrom(x) as ChangeDimensionEvent;
				if(e == null) return true;
				if (e.RESULT) {
					string str = string.Format("{0} 玩家 {1} {2}切换维度至 {3} {4}。",
						title(EventKey.onChangeDimension), e.playername,
						!e.isstand ? "悬空地" : "",
						e.dimension,
						Coordinator(e.XYZ));
					Console.WriteLine("{" + str);
					if (logsetting.autoSave) {
						var t = new Thread(() => logsetting.saveLine(str));
						t.Start();
					}
				}
				return true;
			});
			// 命名生物死亡
			api.addAfterActListener(EventKey.onMobDie, x => {
				var e = BaseEvent.getFrom(x) as MobDieEvent;
				if(e == null) return true;
				if (!string.IsNullOrEmpty(e.mobname)) {
					string str = string.Format("{0} {1} {2} 在 {3} {4} 被 {5} 杀死了。",
						title(EventKey.onMobDie),
						string.IsNullOrEmpty(e.playername) ? "实体" : "玩家",
						e.mobname,
						toDimenStr(e.dimensionid),
						Coordinator(e.XYZ),
						e.srcname);
					Console.WriteLine("{" + str);
					if (logsetting.autoSave) {
						var t = new Thread(() => logsetting.saveLine(str));
						t.Start();
					}
				}
				return true;
			});
			// 聊天消息
			api.addAfterActListener(EventKey.onChat, x => {
				var e = BaseEvent.getFrom(x) as ChatEvent;
				if(e == null) return true;
				if (e.chatstyle != "title") {
					string str = string.Format("{0} {1} 说：{2}",
						e.playername,
						string.IsNullOrEmpty(e.target) ? "" : "悄悄地对 " + e.target,
						e.msg);
					Console.WriteLine("{" + str);
					if (logsetting.autoSave) {
						var t = new Thread(() => logsetting.saveLine(str));
						t.Start();
					}
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
			Logging.Program.init(api);
			Console.WriteLine("[Logging] 日志记录插件已装载。");
		}
	}
}