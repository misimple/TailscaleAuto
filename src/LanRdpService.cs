using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace TailscaleAuto {
public sealed class LanRdpResult {
 public readonly string Address, Reason;
 public readonly long NetworkVersion;
 public readonly DateTime ExpiresUtc;
 public LanRdpResult(string address,string reason,long version) {
  Address=address;Reason=reason;NetworkVersion=version;ExpiresUtc=DateTime.UtcNow.AddSeconds(30);
 }
}

// All network operations are independent of WinForms. Host supplies the bounded CLI runner.
public sealed class LanRdpService : IDisposable {
 public delegate string Cli(string args,int timeout,CancellationToken token);
 readonly Cli cli;
 long version;
 readonly RouteCallback routeCallback;
 IntPtr routeHandle;
 readonly bool watching;
 [UnmanagedFunctionPointer(CallingConvention.Winapi)]
 delegate void RouteCallback(IntPtr context,IntPtr row,int notification);
 [DllImport("iphlpapi.dll")] static extern uint NotifyRouteChange2(ushort family,RouteCallback callback,IntPtr context,byte initial,out IntPtr handle);
 [DllImport("iphlpapi.dll")] static extern uint CancelMibChangeNotify2(IntPtr handle);
 public event Action Invalidated;
 public LanRdpService(Cli cli) {
  this.cli=cli;
  routeCallback=(a,b,c)=>Invalidate();
  watching=NotifyRouteChange2(2,routeCallback,IntPtr.Zero,0,out routeHandle)==0;
  NetworkChange.NetworkAddressChanged+=AddressChanged;
 }
 void AddressChanged(object sender,EventArgs args){Invalidate();}
 void Invalidate(){Interlocked.Increment(ref version);var h=Invalidated;if(h!=null)h();}
 public bool IsCurrent(LanRdpResult r) {
  return watching&&r!=null&&r.Address!=null&&r.NetworkVersion==Interlocked.Read(ref version)&&r.ExpiresUtc>DateTime.UtcNow;
 }
 public void Dispose(){
  NetworkChange.NetworkAddressChanged-=AddressChanged;
  if(routeHandle!=IntPtr.Zero){CancelMibChangeNotify2(routeHandle);routeHandle=IntPtr.Zero;}
  Interlocked.Increment(ref version);
 }
 public static IPEndPoint ParseCandidate(string value) {
  var m=Regex.Match(value??"",@"\A([0-9]{1,3}(?:\.[0-9]{1,3}){3}):([0-9]{1,5})\z");
  IPAddress ip;int port;
  if(!m.Success||!IPAddress.TryParse(m.Groups[1].Value,out ip)||ip.ToString()!=m.Groups[1].Value||
   !Int32.TryParse(m.Groups[2].Value,NumberStyles.None,CultureInfo.InvariantCulture,out port)||port<1||port>65535)return null;
  var b=ip.GetAddressBytes();
  // v1: only RFC1918, never CGNAT/Tailscale, public, multicast or loopback.
  if(!(b[0]==10||(b[0]==172&&b[1]>=16&&b[1]<=31)||(b[0]==192&&b[1]==168)))return null;
  return new IPEndPoint(ip,port);
 }
 public static IPEndPoint PingEndpoint(string output,string target) {
  IPEndPoint result=null;
  foreach(var raw in (output??"").Split('\n')){
   var m=Regex.Match(raw.Trim(),@"\Apong from .+ \(([^)]+)\) via (.+) in [0-9.]+(?:ms|s)\z");
   if(m.Success&&m.Groups[1].Value==target)result=ParseCandidate(m.Groups[2].Value);
  }
  return result;
 }
 static Dictionary<string,object> ReadObject(Dictionary<string,object> d,string key) {
  object v;return d!=null&&d.TryGetValue(key,out v)?v as Dictionary<string,object>:null;
 }
 static object Value(Dictionary<string,object> d,string key){object v;return d.TryGetValue(key,out v)?v:null;}
 public static string MatchPeer(string json,string id,string target) {
  var root=new JavaScriptSerializer{MaxJsonLength=16*1024*1024}.Deserialize<Dictionary<string,object>>(json);
  if(root==null||!Object.Equals(Value(root,"BackendState"),"Running"))return null;
  var peers=ReadObject(root,"Peer");if(peers==null)return null;
  var matches=peers.Where(kv=>{
   var p=kv.Value as Dictionary<string,object>;
   var ips=p==null?null:Value(p,"TailscaleIPs") as System.Collections.IEnumerable;
   return ips!=null&&ips.Cast<object>().Any(x=>Object.Equals(x,target));
  }).ToArray();
  if(matches.Length!=1||matches[0].Key!=id)return null;
  var peer=(Dictionary<string,object>)matches[0].Value;
  return Object.Equals(Value(peer,"Online"),true)&&String.Equals(Value(peer,"OS") as string,"windows",StringComparison.OrdinalIgnoreCase)
    ? Value(peer,"CurAddr") as string:null;
 }
 public LanRdpResult Verify(string id,string target,string ping,CancellationToken token) {
  long start=Interlocked.Read(ref version);
  Func<string,LanRdpResult> fail=reason=>new LanRdpResult(null,reason,start);
  try {
   if(!watching)return fail("无法订阅 Windows 路由变更");
   IPAddress parsed;if(!IPAddress.TryParse(target,out parsed)||parsed.AddressFamily!=AddressFamily.InterNetwork)return fail("目标无有效 Tailscale IPv4");
   token.ThrowIfCancellationRequested();
   if(ping==null)ping=cli("ping --c 2 --timeout 2s --until-direct=false "+parsed,5500,token);
   var endpoint=PingEndpoint(ping,target);
   if(endpoint==null)return fail("本次探测没有匹配目标的私网 IPv4 直连端点");
   var candidate=ParseCandidate(MatchPeer(cli("status --json",5000,token),id,target));
   if(candidate==null||!candidate.Equals(endpoint))return fail("当前 Peer CurAddr 与本次直连端点不一致");
   var source=PhysicalSource(candidate.Address,token);
   if(source==null)return fail("Windows 未确认物理 LAN 同子网直达路由");
   using(var tcp=new TcpClient(new IPEndPoint(source,0))){
    var connect=tcp.BeginConnect(candidate.Address,3389,null,null);
    using(var wait=connect.AsyncWaitHandle){
     if(WaitHandle.WaitAny(new[]{wait,token.WaitHandle},1500)!=0)return fail("TCP 3389 超时或已取消");
     tcp.EndConnect(connect);
    }
   }
   if(token.IsCancellationRequested||start!=Interlocked.Read(ref version))return fail("检测期间网络已变化");
   return new LanRdpResult(candidate.Address.ToString(),"LAN 直达；TCP 3389 可用",start);
  }catch(Exception ex){return fail("LAN 检测失败："+ex.Message);}
 }
 static IPAddress PhysicalSource(IPAddress ip,CancellationToken token) {
  // Resolve the actual unconstrained route first, then validate that very interface.
  string script=@"$ErrorActionPreference='Stop'
$ip='"+ip.ToString()+@"'
$r=Test-NetConnection $ip -DiagnoseRouting -WarningAction SilentlyContinue
if(-not $r.RouteDiagnosticsSucceeded -or $r.SelectedNetRoute.NextHop -ne '0.0.0.0'){exit 2}
$i=$r.OutgoingInterfaceIndex
$a=Get-NetAdapter -Physical | Where-Object { $_.ifIndex -eq $i -and $_.Status -eq 'Up' -and $_.HardwareInterface -and $_.MediaType -in @('802.3','Native 802.11') }
if(-not $a){exit 3}
$s=$r.SelectedSourceAddress
if($s.IPAddress){$s=$s.IPAddress}
$v=Get-NetIPAddress -InterfaceIndex $i -AddressFamily IPv4 -IPAddress $s
if($v.AddressState -ne 'Preferred'){exit 4}
[Console]::WriteLine(('{0}|{1}|{2}|{3}' -f $s,$v.PrefixLength,$i,$r.SelectedNetRoute.DestinationPrefix))";
  string exe=System.IO.Path.Combine(Environment.SystemDirectory,@"WindowsPowerShell\v1.0\powershell.exe");
  using(var p=Process.Start(new ProcessStartInfo(exe,"-NoProfile -NonInteractive -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(script))) {
   UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true
  })){
   var output=p.StandardOutput.ReadToEndAsync();var error=p.StandardError.ReadToEndAsync();
   var timer=Stopwatch.StartNew();
   while(!p.WaitForExit(100)){if(token.IsCancellationRequested||timer.ElapsedMilliseconds>8000){try{p.Kill();}catch{}return null;}}
   if(p.ExitCode!=0||!Task.WaitAll(new Task[]{output,error},500))return null;
   var parts=output.Result.Trim().Split('|');
   IPAddress src,network;int bits,idx,routeBits;
   if(parts.Length!=4||!IPAddress.TryParse(parts[0],out src)||src.AddressFamily!=AddressFamily.InterNetwork||
    !Int32.TryParse(parts[1],out bits)||bits<1||bits>31||!Int32.TryParse(parts[2],out idx)||idx<1)return null;
   var route=parts[3].Split('/');
   if(route.Length!=2||!IPAddress.TryParse(route[0],out network)||network.AddressFamily!=AddressFamily.InterNetwork||
    !Int32.TryParse(route[1],out routeBits)||routeBits<1||routeBits>32)return null;
   if(src.Equals(ip)||!SameSubnet(src,ip,bits)||!SameSubnet(network,ip,routeBits))return null;
   return src;
  }
 }
 public static bool SameSubnet(IPAddress a,IPAddress b,int prefix) {
  if(prefix<1||prefix>32||a.AddressFamily!=AddressFamily.InterNetwork||b.AddressFamily!=AddressFamily.InterNetwork)return false;
  var x=a.GetAddressBytes();var y=b.GetAddressBytes();
  for(int i=0;i<prefix;i++)if(((x[i/8]^y[i/8])&(128>>(i%8)))!=0)return false;
  return true;
 }
 public static string Arguments(LanRdpResult r){if(r==null||ParseCandidate(r.Address+":3389")==null)throw new InvalidOperationException("无有效 LAN 地址");return "/v:"+r.Address;}
}
}
