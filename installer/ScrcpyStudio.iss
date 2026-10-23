; ============================================================================
;  Scrcpy Studio 安装包构建脚本 (Inno Setup 7)
;  用法: "D:\Inno Setup 7\ISCC.exe" 源码\installer\ScrcpyStudio.iss
; ============================================================================

#define MyAppName        "Scrcpy Studio"
#define MyAppVersion     "5.0.1"
#define MyAppPublisher   "angusdevgo"
#define MyAppURL         "https://github.com/angusdevgo/Scrcpy-Studio"
#define MyAppExeName     "ScrcpyStudio.exe"
#define MyAppId          "{{B0D3F20A-0E7E-4232-8CFF-0DAEFE723116}"

; 成品目录（相对本 .iss 所在目录）
#define DistDir          "..\..\成品"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} 安装程序
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

; 默认安装到 当前用户\AppData\Local\Programs\Scrcpy Studio
; 向导中可自由改到任意盘（如 D:\Scrcpy Studio）
DefaultDirName={autopf}\Scrcpy Studio
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no

; 免管理员：无需 UAC。这样程序才能把内嵌引擎释放到安装目录下的 core\，
; 保证「装到哪个盘就全在哪个盘」，不会散落到 C 盘其他位置。
PrivilegesRequired=lowest

; 仅 64 位 Windows
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0

; 外观
WizardStyle=modern
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
ShowLanguageDialog=auto

; 压缩
Compression=lzma2/ultra64
SolidCompression=yes

; 输出
OutputDir={#DistDir}
OutputBaseFilename=ScrcpyStudio-Setup-{#MyAppVersion}
LicenseFile={#DistDir}\LICENSE.txt

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english";          MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#DistDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#DistDir}\使用说明.txt";    DestDir: "{app}"; Flags: ignoreversion
Source: "{#DistDir}\LICENSE.txt";     DestDir: "{app}"; Flags: ignoreversion

[Icons]
; 开始菜单
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Comment: "安卓免安装投屏工具"
Name: "{autoprograms}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
; 桌面：始终创建，不依赖任务勾选
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Comment: "安卓免安装投屏工具"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 首次运行时程序会把内嵌引擎释放到 {app}\core，卸载时一并清理
Type: filesandordirs; Name: "{app}\core"
Type: files;          Name: "{app}\scrcpy_gui.cfg"
Type: dirifempty;     Name: "{app}"
