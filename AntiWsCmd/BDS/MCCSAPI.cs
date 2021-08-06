/*
 * 由SharpDevelop创建。
 * 用户： BDSNetRunner
 * 日期: 2020/7/17
 * 时间: 16:27
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSR
{
	/// <summary>
	/// API接口定义
	/// </summary>
	public class MCCSAPI
	{
		[DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String path);
        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr lib, String funcName);
        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr lib);

        private readonly string mVersion;
        /// <summary>
        /// 插件版本
        /// </summary>
        public string VERSION {get{return mVersion;}}
        private readonly bool mcommercial;
        /// <summary>
        /// 平台类型
        /// </summary>
        public bool COMMERCIAL {get{return mcommercial;}}

		// 注册事件回调管理，避免回收
		private Dictionary<string, ArrayList> callbks = new Dictionary<string, ArrayList>();
		// 发送tick方法管理，避免回收
		private Dictionary<object, object> ticks = new Dictionary<object, object>();

		private IntPtr hLib;
        public MCCSAPI(String DLLPath, string ver, bool commercial)
        {
        	mVersion = ver;
        	mcommercial = commercial;
            hLib = LoadLibrary(DLLPath);
            if (hLib != IntPtr.Zero) {
            	initApis();
            }
        }
        ~MCCSAPI()
        {
            FreeLibrary(hLib);
        }
        //将要执行的函数转换为委托
        private T Invoke<T>(String APIName)
        {
            IntPtr api = GetProcAddress(hLib, APIName);
            if (api != IntPtr.Zero)
	            //return (T)Marshal.GetDelegateForFunctionPointer(api, typeof(T));
				//若.net framework版本高于4.5.1可用以下替换以上
				return Marshal.GetDelegateForFunctionPointer<T>(api);
            Console.WriteLine("Get Api {0} failed.", APIName);
            return default(T);
        }
        
		private delegate bool CSHOOKFUNC(int rva, IntPtr hook, out IntPtr org);
		private CSHOOKFUNC ccshook;
		private delegate bool CSUNHOOKFUNC(IntPtr hook, out IntPtr org);
		private CSUNHOOKFUNC ccsunhook;
		private delegate IntPtr DLSYMFUNC(int rva);
		private DLSYMFUNC cdlsym;
		private delegate bool READHARDMEMORY(int rva, byte[] odata, int size);
		private READHARDMEMORY creadHardMemory, cwriteHardMemory;
		
		// 初始化所有api函数
		void initApis()
		{
			ccshook = Invoke<CSHOOKFUNC>("cshook");
			ccsunhook = Invoke<CSUNHOOKFUNC>("csunhook");
			cdlsym = Invoke<DLSYMFUNC>("dlsym");
			creadHardMemory = Invoke<READHARDMEMORY>("readHardMemory");
			cwriteHardMemory = Invoke<READHARDMEMORY>("writeHardMemory");
		}

		// 底层相关

		/// <summary>
		/// 设置一个钩子
		/// </summary>
		/// <param name="rva">原型函数相对地址</param>
		/// <param name="hook">新函数</param>
		/// <param name="org">待保存原型函数的指针</param>
		/// <returns></returns>
		public bool cshook(int rva, IntPtr hook, out IntPtr org)
        {
			IntPtr sorg = IntPtr.Zero;
            var ret = ccshook != null && ccshook(rva, hook, out sorg);
            org = sorg;
            return ret;
        }
		/// <summary>
		/// 卸载一个钩子
		/// </summary>
		/// <param name="hook">待卸载的函数</param>
		/// <param name="org">已保存了原型函数的指针</param>
		/// <returns></returns>
		public bool csunhook(IntPtr hook, ref IntPtr org) {
			IntPtr sorg = org;
			var ret = ccsunhook != null && ccsunhook(hook, out sorg);
			org = sorg;
			return ret;
		}
		/// <summary>
		/// 取相对地址对应的实际指针
		/// </summary>
		/// <param name="rva"></param>
		/// <returns></returns>
		public IntPtr dlsym(int rva) {
			return cdlsym != null ? cdlsym(rva) :
				IntPtr.Zero;
		}

		/// <summary>
		/// 读特定段内存硬编码
		/// </summary>
		/// <param name="rva">函数片段起始位置相对地址</param>
		/// <param name="size">内存长度</param>
		/// <returns></returns>
		public byte[] readHardMemory(int rva, int size) {
			byte[] x = new byte[size];
			if (creadHardMemory != null)
				if (creadHardMemory(rva, x, size))
					return x;
			return null;
        }

		/// <summary>
		/// 写特定段内存硬编码
		/// </summary>
		/// <param name="rva">函数片段起始位置相对地址</param>
		/// <param name="data">新数据内容</param>
		/// <param name="size">内存长度</param>
		/// <returns></returns>
		public bool writeHardMemory(int rva, byte[] data, int size)
		{
			return (cwriteHardMemory != null) && cwriteHardMemory(rva, data, size);
		}
	}
}
