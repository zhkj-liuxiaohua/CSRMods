'
' 由SharpDevelop创建。
' 用户： BDSNetRunner
' 日期: 2020/8/4
' 时间: 17:34
' 
' 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
'
Imports CSR

REM 本测试程序提供一个VB.Net调用范例，监听一次破坏方块
Public Class MyVBClass

    Shared cb As MCCSAPI.EventCab

    Public Shared Sub init(api As MCCSAPI)
        cb = Function(x As Events)
                 Dim e As DestroyBlockEvent = CType(BaseEvent.getFrom(x), DestroyBlockEvent)
                 If (Not IsNothing(e)) Then
                     Console.WriteLine("[vbtest] 玩家" + e.playername + " 在 " + e.dimension + " (" + e.XYZ.x.ToString() +
                                       "," + e.XYZ.y.ToString() + "," + e.XYZ.z.ToString() + ")处破坏 " + e.blockname + " 方块。")
                     REM 此处卸载监听器
                     api.removeAfterActListener(EventKey.onDestroyBlock, cb)
                 End If
                 Return False
             End Function
        api.addAfterActListener(EventKey.onDestroyBlock, cb)

        api.logout("这是一个vb测试插件。" + "api version=" + api.VERSION)
    End Sub

End Class


REM 注意工程根命名空间为CSR，保证接口可被调用
namespace Global.CSR

Public Class Plugin
	Shared mapi as MCCSAPI
#Region "初始化通用接口，请勿随意更改"
    Public Shared Function onServerStart(pathandversion As String) As Int32
        Dim path as string, Version as string
        Dim commercial = False
        Dim pav() = pathandversion.Split(",".ToCharArray())
        If pav.Length > 1 Then
            path = pav.GetValue(0).ToString()
            Version = pav.GetValue(1).ToString()
            commercial = pav.GetValue(pav.Length - 1).Equals("1")
            mapi = New MCCSAPI(path, Version, commercial)
            If Not IsNothing(mapi) Then
                onStart(mapi)
                GC.KeepAlive(mapi)
                Return 0
            End If
        End If
        Console.WriteLine("Load failed.")
        Return -1
    End Function
#End Region

    Private Shared Sub onStart(api As MCCSAPI)
    	REM TODO  此处需要自行实现
        Vbtest.MyVBClass.init(api)
    End Sub
End Class

end namespace