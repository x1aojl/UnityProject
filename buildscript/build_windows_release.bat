:: unity命令号文档: https://docs.unity3d.com/Manual/CommandLineArguments.html

:: 关闭输出
@echo off

:: 设置Unity.exe的路径
set UNITY_EXE=D:/Applications/Unity/Editor/2020.3.16f1c1/Editor/Unity.exe

:: 检查Unity.exe是否存在
if not exist "%UNITY_EXE%" (
	echo Error: Unity editor's exe not found, path=%UNITY_EXE%
)

:: 设置参数
set ARCH=%1
set VERSION=%2
set VERSION_STR=%VERSION:.=_%
set PROJECT_DIR=%cd%/../client
set OUTPUT_DIR=%cd%/../output
set BIN_DIR=%cd%/../output/%ARCH%/bin/release_%VERSION_STR%
set INTERMEDIATE_DIR=%cd%/../output/%ARCH%/intermediate/client_%VERSION_STR%

::-- TODO: 对版本格式做检查(n.n.n.n)

:: 重新创建bin路径
if exist "%BIN_DIR%" (
	rmdir /s /q "%BIN_DIR%"
)
mkdir "%BIN_DIR%"

:: 重新创建intermediate路径
if exist "%INTERMEDIATE_DIR%" (
	rmdir /s /q "%INTERMEDIATE_DIR%"
)
mkdir "%INTERMEDIATE_DIR%"

:: 执行打包
%UNITY_EXE% -quit -batchmode -nographics -projectPath %PROJECT_DIR% -logFile %INTERMEDIATE_DIR%/log.txt^
             -executeMethod ResPacker.BuildPacket --buildRelease %OUTPUT_DIR% %VERSION% %ARCH%
			 
if %errorlevel% neq 0 (
	@echo Build unity packet failed. exit with %errorlevel%
)