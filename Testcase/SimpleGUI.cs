/*
 * 由SharpDevelop创建。
 * 用户： classmates
 * 日期: 2020/7/29
 * 时间: 1:45
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections;
using System.Threading;
using System.Web.Script.Serialization;
using CSR;

namespace Testcase
{
	/// <summary>
	/// 简易表单处理程序
	/// </summary>
	public class SimpleGUI {
		private MCCSAPI mapi;
		
		public delegate void ONSELECT(string selected);
		public delegate void ONTIMEOUT();
		/// <summary>
		/// 表单id
		/// </summary>
		public uint id;
		/// <summary>
		/// 玩家uuid
		/// </summary>
		public string uuid;
		/// <summary>
		/// 返回的选项
		/// </summary>
		public string selected;
		/// <summary>
		/// 表单标题
		/// </summary>
		public string title;
		/// <summary>
		/// 内容
		/// </summary>
		public string content;
		/// <summary>
		/// 按钮内容
		/// </summary>
		public ArrayList buttons;
		/// <summary>
		/// 处理选择程序
		/// </summary>
		public ONSELECT onselected;
		/// <summary>
		/// 超时处理程序
		/// </summary>
		public ONTIMEOUT ontimeout;
		/// <summary>
		/// 超时毫秒数
		/// </summary>
		public int timeout;
		/// <summary>
		/// 注册回调
		/// </summary>
		public MCCSAPI.EventCab fmcb;
		/// <summary>
		/// 是否取消
		/// </summary>
		public bool canceled;
		/// <summary>
		/// 创建一个简单表单
		/// </summary>
		/// <param name = "api"></param>
		/// <param name = "u">玩家uuid</param>
		/// <param name="t">标题</param>
		/// <param name="c">内容</param>
		/// <param name="b">按钮列表</param>
		public SimpleGUI(MCCSAPI api, string u, string t, string c, ArrayList b) {
			mapi = api;
			uuid = u;
			title = t;
			content = c;
			buttons = (b ?? new ArrayList());
		}
		/// <summary>
		/// 开始超时监听
		/// </summary>
		public void startTimeout() {
			Thread t = new Thread(()=>{
			                      	Thread.Sleep(timeout);
			                      	if (!canceled) {
			                      		mapi.releaseForm(id);
			                      		mapi.removeBeforeActListener(EventKey.onFormSelect, fmcb);
			                      		if (ontimeout != null)
			                      			ontimeout();
			                      	}
			                      });
			t.Start();
		}
		/// <summary>
		/// 取消一个超时监听
		/// </summary>
		public void cancelTimeout() {
			canceled = true;
		}
		/// <summary>
		/// 发送一个简易表单
		/// </summary>
		/// <param name="tout">超时时间</param>
		/// <param name="func">主选择处理函数</param>
		/// <param name="tofunc">超时处理函数</param>
		/// <returns></returns>
		public bool send(int tout, ONSELECT func, ONTIMEOUT tofunc) {
			timeout = tout;
			onselected = func;
			ontimeout = tofunc;
			fmcb = (x) => {
				var e = BaseEvent.getFrom(x) as FormSelectEvent;
				if (e.formid == id) {	// 确定是当前表单
					mapi.removeBeforeActListener(EventKey.onFormSelect, fmcb);
					cancelTimeout();
					onselected(e.selected);
				}
				return true;
			};
			mapi.addBeforeActListener(EventKey.onFormSelect, fmcb);
			string bts = "[]";
			if (buttons != null && buttons.Count > 0) {
				var ser = new JavaScriptSerializer();
				bts = ser.Serialize(buttons);
			}
			id = mapi.sendSimpleForm(uuid, title, content, bts);
			bool ret = (id != 0);
			if (timeout > 0) {
				startTimeout();
			}
			return ret;
		}
	}
}
