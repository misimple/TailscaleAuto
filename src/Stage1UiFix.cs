using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace TailscaleAuto {
public partial class MainForm {
 LinkLabel WebsiteLink,GitHubLink; Panel FooterPanel; ComboBox LanguageSelector; Label LanguageLabel; bool languageUpdating;
 internal Action<string> OpenExternalLink=url=>{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});};
 protected override void OnLoad(EventArgs e){
  base.OnLoad(e);
  Lang.Set(settings.Language);
Text="Tailscale Auto v1.6.14.0-dev";Floating.Text="Tailscale Auto v1.6.14.0-dev 悬浮监控";
  WebsiteLink=new LinkLabel{Text="https://tailscale.com/",AutoSize=true,LinkColor=Palette.Accent,ActiveLinkColor=Palette.Green,VisitedLinkColor=Palette.Accent,BackColor=Palette.Canvas,TabStop=true,Cursor=Cursors.Hand};
  WebsiteLink.LinkClicked+=(a,x)=>{try{OpenExternalLink("https://login.tailscale.com/login?next_url=%2Fwelcome");}catch{}};
  GitHubLink=new LinkLabel{Text="GitHub: misimple",AutoSize=true,LinkColor=Palette.Accent,ActiveLinkColor=Palette.Green,VisitedLinkColor=Palette.Accent,BackColor=Palette.Canvas,TabStop=true,Cursor=Cursors.Hand};
  GitHubLink.LinkClicked+=(a,x)=>{try{OpenExternalLink("https://github.com/misimple");}catch{}};
  FooterPanel=new Panel{Dock=DockStyle.Bottom,Height=UiMetrics.FooterHeight,BackColor=Palette.Canvas};
  FooterPanel.Controls.Add(GitHubLink);FooterPanel.Controls.Add(WebsiteLink);
  FooterPanel.Resize+=(a,x)=>ArrangeFooterLinks();
  Controls.Add(FooterPanel);DeviceList.BringToFront();
  LanguageSelector=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=115,Height=26,Font=new Font("Microsoft YaHei UI",9)};
  LanguageSelector.Items.AddRange(new object[]{"English","简体中文"});LanguageSelector.SelectedIndex=Lang.Chinese?1:0;
  LanguageSelector.SelectedIndexChanged+=(a,x)=>{if(languageUpdating)return;string previous=settings.Language;string next=LanguageSelector.SelectedIndex==1?"zh-CN":"en";settings.Language=next;if(settings.Save())Lang.Set(next);else{settings.Language=previous;languageUpdating=true;LanguageSelector.SelectedIndex=previous=="zh-CN"?1:0;languageUpdating=false;}};
  LanguageLabel=new Label{Name="LanguageLabel",Text=Lang.Tr("语言"),ForeColor=Palette.Muted,AutoSize=false,TextAlign=ContentAlignment.MiddleRight};top.Controls.Add(LanguageLabel);top.Controls.Add(LanguageSelector);LanguageSelector.BringToFront();
  Lang.Changed+=ApplyLanguage;
  FormClosed+=(a,x)=>Lang.Changed-=ApplyLanguage;
  Resize+=(a,x)=>{ArrangeFooterLinks();ArrangeLanguageSelector();};
  using(var g=CreateGraphics())scale=g.DpiX/96f;
  var work=Screen.FromControl(this).WorkingArea;
  Size=new Size(Math.Min((int)(1020*scale),work.Width),Math.Min((int)(720*scale),work.Height));
  MinimumSize=new Size(Math.Min((int)(UiMetrics.MinimumWidth*scale),work.Width),Math.Min((int)(UiMetrics.MinimumHeight*scale),work.Height));
  FooterPanel.Height=(int)(UiMetrics.FooterHeight*scale);
  top.Paint+=StableHeaderPaint;top.Resize+=(a,x)=>{var area=Screen.FromControl(this).WorkingArea;MinimumSize=new Size(Math.Min((int)(UiMetrics.MinimumWidth*scale),area.Width),Math.Min((int)(UiMetrics.MinimumHeight*scale),area.Height));ArrangeLanguageSelector();};
  ArrangeTop();
  MinimumSize=new Size(Math.Min((int)(UiMetrics.MinimumWidth*scale),work.Width),Math.Min((int)(UiMetrics.MinimumHeight*scale),work.Height));
  ArrangeRows();
  ArrangeFooterLinks();
  ArrangeLanguageSelector();ApplyLanguage();
 }
 void ArrangeLanguageSelector(){if(LanguageSelector==null||LanguageLabel==null)return;float z=scale>0?scale:1;var layout=new TopLayout(UiMetrics.RightEdge(top.Width,z));LanguageLabel.Bounds=layout.Bounds(layout.LanguageLabelX,TopLayout.LanguageLabelWidth,z);LanguageSelector.Bounds=layout.Bounds(layout.SelectorX,TopLayout.SelectorWidth,z);}
void ApplyLanguage(){if(IsDisposed)return;languageUpdating=true;if(LanguageSelector!=null)LanguageSelector.SelectedIndex=Lang.Chinese?1:0;languageUpdating=false;Text="Tailscale Auto v1.6.14.0-dev";Floating.ApplyLanguage();top.Controls["DeviceTitle"].Text=Lang.Tr("设备");FloatToggle.Text=Lang.Tr("悬浮窗");FloatToggle.AccessibleName=Lang.Chinese?"悬浮窗":"Floating Window";((Label)top.Controls["AlphaLabel"]).Text=Lang.Tr("透明度");if(LanguageLabel!=null)LanguageLabel.Text=Lang.Tr("语言");ArrangeTop();if(rowMenu.Items.Count>=3){rowMenu.Items[0].Text=Lang.Tr("复制 Tailscale IP");rowMenu.Items[1].Text=Lang.Tr("设置本地显示别名…");rowMenu.Items[2].Text=Lang.Tr("清除本地别名");}if(Tray!=null&&Tray.ContextMenuStrip!=null&&Tray.ContextMenuStrip.Items.Count>=2){Tray.ContextMenuStrip.Items[0].Text=Lang.Tr("打开主窗口");Tray.ContextMenuStrip.Items[1].Text=Lang.Tr("退出程序");}foreach(var row in DeviceRows.Values)row.ApplyLanguage();UpdateLanButtons();tips.SetToolTip(status,Lang.Chinese?"每15秒更新设备；连接状态是本机最近完成的主动检测结果。右键设备可设置本地别名。":"Devices refresh every 15 seconds; connection status is the latest active probe result. Right-click a device to set a local alias.");if(!latest.Valid&&String.IsNullOrEmpty(latest.Message))status.Text=Lang.Tr("正在读取 Tailscale…");string time=LastListUpdated==DateTime.MinValue?"—":LastListUpdated.ToString("HH:mm:ss");Summary.Text=latest.Valid?Lang.Summary(devices.Count,devices.Count(d=>d.OnlineKnown&&d.Online),time):Lang.SummaryUnknown(devices.Count,time);UpdateStatus();top.Invalidate();}
 void ArrangeFooterLinks(){if(FooterPanel==null||WebsiteLink==null||GitHubLink==null)return;float z=scale>0?scale:1;int y=(FooterPanel.Height-WebsiteLink.Height)/2;GitHubLink.Location=new Point((int)(UiMetrics.Margin*z),y);WebsiteLink.Location=new Point((int)(UiMetrics.RightEdge(FooterPanel.Width,z)*z)-WebsiteLink.Width,y);}
 float GridWidth(){
  float z=scale>0?scale:1;
  return UiMetrics.RowWidth(DeviceList.Width,z)/z;
 }
 void StableHeaderPaint(object sender,PaintEventArgs e){
  var g=e.Graphics;float z=g.DpiX/96f;g.ScaleTransform(z,z);
  using(var bg=new SolidBrush(Palette.Canvas))g.FillRectangle(bg,0,80,top.Width/z,28);
  using(var b=new SolidBrush(Palette.Muted))using(var f=new Font("Microsoft YaHei UI",12,FontStyle.Bold,GraphicsUnit.Pixel)){
   var row=new RowLayout(GridWidth());
   g.DrawString(Lang.Tr("设备信息"),f,b,UiMetrics.Margin+row.IconRect.X,87);g.DrawString(Lang.Tr("在线状态"),f,b,UiMetrics.Margin+row.BadgeRect.X,87);g.DrawString(Lang.Tr("连接状态"),f,b,UiMetrics.Margin+row.PathRect.X,87);g.DrawString(Lang.Tr("延迟"),f,b,UiMetrics.Margin+row.LatencyRect.X,87);g.DrawString(Lang.Tr("操作"),f,b,UiMetrics.Margin+row.CopyRect.X,87);
  }
 }
}
}
