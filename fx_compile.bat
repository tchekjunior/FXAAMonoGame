C:\Users\gulde\Documents\Visual Studio 2013\Projects\WPF_MG\WPF_MG\HLSL

:: base path
set pfad1="C:\Users\gulde\Documents\Visual Studio 2013\Projects\FXAATest"
set pfad2="C:\Users\gulde\Documents\Visual Studio 2013\Projects\FXAATest"

:: go to directory
c:
cd "C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\"

echo "Compiling fxaa.fx"
:: compile effect.fx
2MGFX.exe %pfad1%\fxaa.fx %pfad2%\fxaa.xnb /Profile:DirectX_11

pause