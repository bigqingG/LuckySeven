param([ValidateSet('refresh','stop-play','sync-text','pixel-skin','install-localization','localize-cards','table-layout','setup','edit-tests','play-tests','build')][string]$Command = 'refresh')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$projectName = Split-Path $projectRoot -Leaf
$editor = Get-Process Unity | Where-Object { $_.MainWindowTitle.StartsWith($projectName + ' -') } | Select-Object -First 1
if (!$editor) { throw "Open $projectRoot in Unity first." }
if (-not ('LuckySevenMenus' -as [type])) {
Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class LuckySevenMenus {
 [DllImport("user32.dll")] static extern IntPtr GetMenu(IntPtr h);
 [DllImport("user32.dll")] static extern IntPtr GetSubMenu(IntPtr h,int pos);
 [DllImport("user32.dll")] static extern int GetMenuItemCount(IntPtr h);
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetMenuString(IntPtr h,uint pos,StringBuilder text,int max,uint flags);
 [DllImport("user32.dll")] static extern uint GetMenuItemID(IntPtr h,int pos);
 [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h,uint msg,IntPtr wp,IntPtr lp);
 static uint Find(IntPtr menu) {
  for(int i=0;i<GetMenuItemCount(menu);i++) {
   var sub=GetSubMenu(menu,i);
   if(sub!=IntPtr.Zero) { uint found=Find(sub); if(found!=0) return found; }
   else { var name=new StringBuilder(256); GetMenuString(menu,(uint)i,name,256,0x400); if(name.ToString().StartsWith("Refresh\t")) return GetMenuItemID(menu,i); }
  }
  return 0;
 }
 public static bool Refresh(IntPtr window) { uint id=Find(GetMenu(window)); return id!=0 && PostMessage(window,0x111,(IntPtr)id,IntPtr.Zero); }
}
'@
}
if (![LuckySevenMenus]::Refresh($editor.MainWindowHandle)) { throw 'Could not locate Unity Assets > Refresh.' }
if ($Command -ne 'refresh') {
    $pending = Join-Path $projectRoot 'Temp/LuckySeven.command.pending'
    Set-Content -LiteralPath $pending -Value $Command
    Move-Item -LiteralPath $pending -Destination (Join-Path $projectRoot 'Temp/LuckySeven.command') -Force
}
Write-Output "Requested $Command for $projectName (PID $($editor.Id)). Results: Docs/Validation."
