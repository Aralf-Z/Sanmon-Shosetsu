REM https://www.datable.cn/docs/reference/cli

dotnet ./luban/Luban.dll ^
    -v ^
    -t lua ^
    -c lua-lua^
    --conf ./luban.conf ^
    -x outputCodeDir=../luaScripts/config ^
    -x outputDataDir=../luaScripts/config
pause