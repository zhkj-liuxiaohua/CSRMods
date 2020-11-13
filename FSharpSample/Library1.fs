namespace FSharpSample
open CSR
type Class1() = //插件主体
      static member public Init(api:MCCSAPI)=
             let addResult = api.addAfterActListener(EventKey.onAttack, MCCSAPI.EventCab(fun _e -> 
                 let e=AttackEvent.getFrom(_e)//转换事件信息
                 printfn "%s攻击了%s"  e.playername  e.actortype//控制台输出
                 true//返回值
             ))// |> ignore 
             if addResult then printfn "[F#] addAfterActListener注册onAttack监听成功"
             
//api实现
namespace CSR
open CSR
  type public Plugin()=
        static let mutable mapi:MCCSAPI =null
        //插件主入口 请勿随意更改(由gxh翻译自CSRAPI)
        static member public onServerStart(pathandversion:string):int =
            let mutable result:int = -1
            let pav:string[] =pathandversion.Split(",".ToCharArray())
            if  pav.Length > 1 then
                let path:string = pav.GetValue(0).ToString()
                let Version:string = pav.GetValue(1).ToString()
                let commercial:bool = pav.GetValue(pav.Length - 1).Equals("1")
                mapi <- MCCSAPI(path, Version, commercial)
                if not (isNull(mapi))  then
                     printfn "[F#] plugin is Loading."
                     System.GC.KeepAlive(mapi);
                     Plugin.onStart(mapi)|>ignore
                     printfn "[F#] plugin Load Success."
                     result <- 0
            if result = -1 then  printf "Load failed."
            result
        static member  public  onStart(api:MCCSAPI)=
            //TODO  此处需要自行实现
            FSharpSample.Class1.Init(api)