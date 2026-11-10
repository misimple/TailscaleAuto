using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TailscaleAuto {
public partial class MainForm {
 bool checkingLan;

 void UpdateLanButtons(){foreach(var row in DeviceRows.Values)SetLanButton(row);}

 void SetLanButton(DeviceRow row){
  var d=row.Device;
  var result=d==null?null:d.Lan;
  bool enabled=!checkingLan&&row.Valid&&d!=null&&d.CanRdp&&Monitoring!=null&&Monitoring.Lan.IsCurrent(result);
  string reason=checkingLan?Lang.Tr("正在重新验证 LAN"):
   !row.Valid||d==null||!d.Online?Lang.Tr("目标未在线"):
   result==null?Lang.Tr("等待有效 LAN 直连检测"):
   enabled?result.Address+" — "+Lang.LanReason(result.Reason):
   result.Address==null?Lang.LanReason(result.Reason):Lang.Tr("LAN 检测结果已失效");
  row.SetLan(enabled,reason);
 }

 async void LaunchLanRdp(string id){
  var d=devices.FirstOrDefault(x=>x.Id==id);
  if(checkingLan||d==null||!latest.Valid||!d.CanRdp||Monitoring==null||!Monitoring.Lan.IsCurrent(d.Lan))return;
  checkingLan=true;UpdateLanButtons();
  try{
   var result=await Monitoring.CheckLaunch(id,d.IP,d.Generation,d.Lan);
   if(IsDisposed||exiting)return;
   var now=devices.FirstOrDefault(x=>x.Id==id);
   if(!latest.Valid||now==null||!now.CanRdp||now.IP!=d.IP||now.Generation!=d.Generation||now.CurAddr!=d.CurAddr||!Monitoring.Lan.IsCurrent(result)){
    ShowNotice("局域网 RDP 未启动：地址、路由或端口未通过重新验证。",8,true);return;
   }
   Process.Start(new ProcessStartInfo(Path.Combine(Environment.SystemDirectory,"mstsc.exe"),LanRdpService.Arguments(result)){UseShellExecute=true});
  }catch(Exception ex){if(!IsDisposed)ShowNotice("无法启动局域网远程桌面："+ex.Message,10,true);}
  finally{checkingLan=false;if(!IsDisposed)UpdateLanButtons();}
 }
}
}
