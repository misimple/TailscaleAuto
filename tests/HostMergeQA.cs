using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using TailscaleAuto;

class HostMergeQA {
 static int failures;
 static void Check(bool pass,string name){Console.WriteLine((pass?"PASS ":"FAIL ")+name);if(!pass)failures++;}
 static Snapshot Status(string addr,bool online=true,string ip="100.90.0.2"){
  return Core.ParseStatus("{\"BackendState\":\"Running\",\"Peer\":{\"peer-id\":{\"HostName\":\"target\",\"OS\":\"windows\",\"Online\":"+(online?"true":"false")+",\"TailscaleIPs\":[\""+ip+"\"],\"CurAddr\":\""+addr+"\"}}}");
 }
 [STAThread]static int Main(){
  Application.EnableVisualStyles();
  const string endpoint="192.168.7.8:41641";
  var first=Status(endpoint);var d=first.Devices.Single();
  Check(d.Id=="peer-id"&&d.CurAddr==endpoint&&d.OnlineKnown,"Peer identity and CurAddr parsed");
  d.Path="直连";d.LatencyMs="18.0 ms";d.Generation=42;d.Lan=new LanRdpResult("192.168.7.8","LAN 直达；TCP 3389 可用",0);
  var same=Core.CarryPaths(first,Status(endpoint)).Devices.Single();
  Check(same.Lan==d.Lan&&same.Generation==42&&same.LatencyMs=="18.0 ms","same peer retains LAN, path and latency");
  var moved=Core.CarryPaths(first,Status("192.168.7.9:41641")).Devices.Single();
  Check(moved.Lan==null&&moved.Path=="直连","CurAddr change invalidates only LAN");
  var changedIp=Core.CarryPaths(first,Status(endpoint,true,"100.90.0.3")).Devices.Single();
  Check(changedIp.Lan==null&&changedIp.Generation!=42,"Tailscale IP change invalidates LAN generation");
  var offline=Core.CarryPaths(first,Status(endpoint,false)).Devices.Single();
  Check(offline.Lan==null&&!offline.CanRdp,"offline disables both RDP paths");
  Check(Core.RdpArguments(same)=="/v:100.90.0.2"&&LanRdpService.Arguments(same.Lan)=="/v:192.168.7.8","RDP targets remain independent");
  string direct="pong from target (100.90.0.2) via "+endpoint+" in 18ms";
  string stale="{\"BackendState\":\"Running\",\"Peer\":{\"peer-id\":{\"OS\":\"windows\",\"Online\":true,\"TailscaleIPs\":[\"100.90.0.2\"],\"CurAddr\":\"192.168.7.9:41641\"}}}";
  using(var service=new LanRdpService((args,timeout,token)=>stale)){
   var rejected=service.Verify("peer-id","100.90.0.2",direct,System.Threading.CancellationToken.None);
   Check(rejected.Address==null&&rejected.Reason.Contains("CurAddr"),"fresh ping and status disagreement blocks LAN before TCP");
   var valid=new LanRdpResult("192.168.7.8","fixture",0);
   Check(service.IsCurrent(valid),"LAN result initially current");
   typeof(LanRdpService).GetMethod("Invalidate",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(service,null);
   Check(!service.IsCurrent(valid),"network invalidation closes LAN result");
  }
  using(var form=new MainForm(true)){
   form.Monitoring=new Monitor();form.Show();
   form.Render(new Snapshot{Valid=true,Devices={same}});
   var row=form.DeviceRows["peer-id"];
   Check(row.RdpButton.Enabled&&row.LanRdpButton.Enabled,"both RDP buttons enabled on valid result");
   Check(row.RdpButton.Bounds==new Rectangle(681,25,96,26)&&row.LanRdpButton.Bounds==new Rectangle(785,25,96,26),"existing button layout retained");
   form.Render(new Snapshot{Valid=true,Devices={moved}});
   Check(row.RdpButton.Enabled&&!row.LanRdpButton.Enabled,"address change disables LAN only");
   form.Render(new Snapshot{Valid=true,Devices={offline}});
   Check(!row.RdpButton.Enabled&&!row.LanRdpButton.Enabled,"offline disables both buttons");
   Lang.Set("zh-CN");row.ApplyLanguage();Check(row.LanRdpButton.Text=="局域网 RDP","Chinese LAN button");
   Lang.Set("en");row.ApplyLanguage();Check(row.LanRdpButton.Text=="LAN RDP"&&Lang.StatusMessage("局域网 RDP 未启动：地址、路由或端口未通过重新验证。").StartsWith("LAN RDP"),"English LAN button and notice");
   form.Exit();
  }
  Console.WriteLine("Failures: "+failures);return failures==0?0:1;
 }
}
