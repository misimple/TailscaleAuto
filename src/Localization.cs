using System;
using System.Collections.Generic;

namespace TailscaleAuto {
public static class Lang {
 public static string Code="en";
 public static event Action Changed;
 static readonly Dictionary<string,string> English=new Dictionary<string,string>{
  {"设备","Device"},{"设备信息","Device"},{"在线状态","Status"},{"连接状态","Connection"},{"延迟","Latency"},{"操作","Actions"},
  {"在线","Online"},{"离线","Offline"},{"未确认","Unknown"},{"直连","Direct"},{"中继","Relay"},{"本机","This PC"},
  {"复制IP","Copy IP"},{"局域网 RDP","LAN RDP"},{"悬浮窗","Floating"},{"透明度","Opacity"},{"语言","Language"},
  {"打开主窗口","Open Main Window"},{"退出程序","Exit"},{"设置本地显示别名…","Edit Alias…"},{"清除本地别名","Clear Alias"},
  {"复制 Tailscale IP","Copy Tailscale IP"},{"复制 ","Copy "},{"关闭悬浮窗","Close Floating Window"},{"本地显示别名","Local Alias"},{"保存","Save"},{"取消","Cancel"},
  {"原名：","Original name: "},{"仅保存到本机；留空恢复原名（最多80字）。","Saved locally; leave blank to restore the original name (max 80 characters)."},
  {"正在读取设备…","Loading devices…"},{"正在读取 Tailscale…","Loading Tailscale…"},{"未知系统","Unknown system"},{"无法确认","Unknown"},
  {"已复制 Tailscale IP：","Copied Tailscale IP: "},{"无法复制IP，剪贴板忙，请稍后重试。","Unable to copy IP; clipboard is busy. Please try again."},
  {"正在重新验证 LAN","Rechecking LAN"},{"目标未在线","Target offline"},{"等待有效 LAN 直连检测","Waiting for LAN check"},{"LAN 检测结果已失效","LAN check expired"},
  {"没有当前直连端点","No current direct endpoint"},{"LAN 直达；TCP 3389 可用","Direct LAN; TCP 3389 available"},
  {"无法订阅 Windows 路由变更","Cannot watch Windows routes"},{"目标无有效 Tailscale IPv4","No valid Tailscale IPv4"},
  {"本次探测没有匹配目标的私网 IPv4 直连端点","No matching private IPv4 direct endpoint"},
  {"当前 Peer CurAddr 与本次直连端点不一致","Peer endpoint changed"},
  {"Windows 未确认物理 LAN 同子网直达路由","No verified physical LAN route"},
  {"TCP 3389 超时或已取消","TCP 3389 timed out or canceled"},{"检测期间网络已变化","Network changed during check"},
  {"地址或路由已变化，请等待重新检测","Address or route changed; await recheck"}
 };
 public static bool Chinese{get{return Code=="zh-CN";}}
 public static void Set(string code){string next=String.Equals(code,"zh-CN",StringComparison.OrdinalIgnoreCase)?"zh-CN":"en";if(Code==next)return;Code=next;var h=Changed;if(h!=null)h();}
 public static string Tr(string value){if(String.IsNullOrEmpty(value)||Chinese)return value;string translated;return English.TryGetValue(value,out translated)?translated:value;}
 public static string LanReason(string value){if(Chinese||String.IsNullOrEmpty(value))return value;if(value.StartsWith("LAN 检测失败："))return "LAN check failed: "+value.Substring("LAN 检测失败：".Length);return Tr(value);}
 public static string Summary(int count,int online,string time){return Chinese?count+" 台设备 · "+online+" 台在线 · 更新时间 "+time:count+" devices · "+online+" online · Updated "+time;}
 public static string SummaryUnknown(int count,string time){return Chinese?count+" 台设备 · 在线状态未确认 · 更新时间 "+time:count+" devices · Online status unknown · Updated "+time;}
 public static string StatusMessage(string value){if(Chinese||String.IsNullOrEmpty(value))return value;return value.Replace("Tailscale 未就绪：","Tailscale is not ready: ").Replace("。请在 Tailscale 客户端检查服务和登录状态。",". Check the Tailscale client and sign-in status.").Replace("无法读取 Tailscale 状态：","Unable to read Tailscale status: ").Replace("。请检查本机服务和登录状态。",". Check the local service and sign-in status.").Replace("Tailscale 数据解析失败：","Tailscale data parsing failed: ").Replace("监控异常：","Monitor error: ").Replace("探测异常：","Probe error: ").Replace("已复制 Tailscale IP：","Copied Tailscale IP: ").Replace("无法复制IP，剪贴板忙，请稍后重试。","Unable to copy IP; clipboard is busy. Please try again.").Replace("未找到 Tailscale。请安装并登录官方 Tailscale 客户端。","Tailscale was not found. Please install and sign in to the official Tailscale client.").Replace("无法启动远程桌面客户端：","Unable to start Remote Desktop client: ").Replace("局域网 RDP 未启动：地址、路由或端口未通过重新验证。","LAN RDP was not started: address, route, or port failed revalidation.").Replace("无法启动局域网远程桌面：","Unable to start LAN Remote Desktop: ").Replace("设置保存失败：","Settings save failed: ").Replace("设置已恢复保存","Settings saved successfully").Replace("命令超时","Command timed out").Replace("已取消","Canceled");}
}
}
