@echo off
rmdir /s /q bundle 
FOR /d /r . %%d IN ("bin") DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN ("obj") DO @IF EXIST "%%d" rd /s /q "%%d"
del /s /q win-x64-bundle
del /s /q linux-x64-bundle
del /s /q linux-arm64-bundle
del *.zip
del /s /q bin
echo Cleaning completed
