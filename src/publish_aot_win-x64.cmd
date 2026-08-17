@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { $env:PATH = ($env:PATH -split ';' | Where-Object { $_ -notmatch 'mysql' }) -join ';'; dotnet publish 'AzureTools/AzureTools.csproj' -c Release -r win-x64 /p:PublishAot=true /p:PublishSingleFile=true /p:DebugType=None /p:DebugSymbols=false -o publish/aot_win-x64}"
