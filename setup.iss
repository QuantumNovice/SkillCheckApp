; Inno Setup Script
; Save this as something like install_skillcheckapp.iss

[Setup]
AppName=SkillCheckApp
AppVersion=1.0
DefaultDirName={pf}\SkillCheckApp
DefaultGroupName=SkillCheckApp
OutputBaseFilename=SkillCheckAppSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\Debug\net8.0-windows\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\SkillCheckApp"; Filename: "{app}\SkillCheckApp.exe"
Name: "{group}\Uninstall SkillCheckApp"; Filename: "{uninstallexe}"
