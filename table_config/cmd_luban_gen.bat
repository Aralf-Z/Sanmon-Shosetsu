set CLIENT=../client/Assets

dotnet ./Luban/Luban.dll ^
    -v ^
    -t all ^
    -c cs-simple-json^
    -d json ^
    --conf ./luban.conf ^
    -x json.outputDataDir=%CLIENT%/StreamingAssets/Tables ^
    -x outputCodeDir=%CLIENT%/GameConfig/Tables/CodeGen
pause