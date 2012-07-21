!include 'LogicLib.nsh'
#Put your common includes here
!include 'MyIncludes.nsh'

Name "#mVersionDescription#"

RequestExecutionLevel highest

# General Symbol Definitions
!define REGKEY "SOFTWARE\$(^Name)"
!define VERSION #mVersionFile#
!define COMPANY "Your Corporation"
!define URL http://www.YourCompany.us

# Included files
!include Sections.nsh

# Variables
Var StartMenuGroup

# Installer pages
Page instfiles

# Installer attributes
OutFile #mEXEFilename#
InstallDir "C:\YourSoftware\#mVersionDescription#"
CRCCheck on
XPStyle on
#Icon "ConfigProject.ico"
SilentInstall silent
VIProductVersion #mVersionFile#
VIAddVersionKey ProductName "#mVersionDescription# Installer"
VIAddVersionKey ProductVersion "${VERSION}"
VIAddVersionKey CompanyName "${COMPANY}"
VIAddVersionKey CompanyWebsite "${URL}"
VIAddVersionKey FileVersion "${VERSION}"
VIAddVersionKey FileDescription ""
VIAddVersionKey LegalCopyright ""
    
#Log file for application
Var /Global LOGFILE
#NSIS Global memory dump file
Var /GLOBAL DUMPFILE

#GLOBAL#
#GLOBAL_FROM_INSTALLER#

# Installer sections
Section -Main SEC0000

   
    StrCpy $INSTDIR "$DITPATH\#mVersionDescription#"
    SetOutPath $INSTDIR
    SetOverwrite on

    CreateDirectory "$INSTDIR\Installer"
    StrCpy $LOGFILE "$INSTDIR\Installer\#mEXEFilename#.txt"
    Delete $LOGFILE
    
    nsislog::log $LOGFILE "Starting Main setup" 
    
    call ReadInstallerOutput
    
    call MainBodyInstaller
    
    nsislog::log $LOGFILE "Main setup complete" 
 
SectionEnd

# Installer functions
Function .onInstSuccess

FunctionEnd

Function .onInit
    InitPluginsDir
    StrCpy $StartMenuGroup "#mEXEFilename#"
FunctionEnd


Function ReadInstallerOutput
#These are the variables from WISE that need to be initialized.

    #READREG_FROM_INSTALLER#
FunctionEnd

Function MainBodyInstaller
#The wise code will be put in here.

    #BODY#
    
FunctionEnd 

    

