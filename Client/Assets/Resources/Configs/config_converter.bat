@echo off
chcp 65001 >nul
REM 配置文件编码转换工具 - 反向转换版本
REM 输入: Assets/Resources/Configs/ 目录
REM 输出: CSV/ 目录

set SRC_DIR=.
set DST_DIR=..\..\..\CSV

echo 配置文件编码转换工具
echo ================================================
echo 输入目录: %SRC_DIR%
echo 输出目录: %DST_DIR%
echo ================================================
echo.

REM 检查输入目录是否存在
if not exist "%SRC_DIR%" (
    echo 错误: 输入目录不存在 - %SRC_DIR%
    echo 请确保当前目录是Assets\Resources\Configs文件夹
    pause
    exit /b 1
)

REM 创建输出目录
if not exist "%DST_DIR%" (
    echo 创建输出目录: %DST_DIR%
    mkdir "%DST_DIR%"
)

echo 启动配置文件编码转换工具...
..\..\..\Tools\config_converter\config_converter.exe "%SRC_DIR%" "%DST_DIR%"

if errorlevel 1 (
    echo.
    echo 转换过程中出现错误，请检查:
    echo 1. 输入目录 %SRC_DIR% 是否存在
    echo 2. 输出目录 %DST_DIR% 是否有写入权限
    echo 3. 可执行文件 ..\..\..\Tools\config_converter\config_converter.exe 是否存在
    echo 4. 目标CSV文件是否被Excel或其他程序占用
    pause
    exit /b 1
)

echo.
echo ✓ 转换完成！
echo 转换后的文件已保存到: %DST_DIR%
pause 