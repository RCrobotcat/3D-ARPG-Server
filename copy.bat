@echo off
set "scriptDir=%~dp0"
xcopy "%scriptDir%\ServerCommon\IOCPNet\*.cs" "%scriptDir%\..\..\3DARPG_Demo\Assets\Scripts\Network\IOCPNet\" /I /Y
xcopy "%scriptDir%\ServerCommon\Protocols\*.cs" "%scriptDir%\..\..\3DARPG_Demo\Assets\Scripts\Network\Protocols\" /I /Y