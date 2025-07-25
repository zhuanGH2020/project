@echo off
chcp 65001 >nul
REM CSV编码转换工具 - 固定路径版本
REM 输入: CSV/ 目录
REM 输出: Assets/Resources/Configs/ 目录

set SRC_DIR=..\CSV
set DST_DIR=..\Assets\Resources\Configs

echo CSV编码转换工具
echo ================================================
echo 输入目录: %SRC_DIR%
echo 输出目录: %DST_DIR%
echo ================================================
echo.

REM 检查输入目录是否存在
if not exist "%SRC_DIR%" (
    echo 错误: 输入目录不存在 - %SRC_DIR%
    echo 请确保项目根目录下有CSV文件夹
    pause
    exit /b 1
)

REM 创建输出目录
if not exist "%DST_DIR%" (
    echo 创建输出目录: %DST_DIR%
    mkdir "%DST_DIR%"
)

echo 启动CSV编码转换工具...
csv_converter\csv_converter.exe "%SRC_DIR%" "%DST_DIR%"

if errorlevel 1 (
    echo.
    echo 转换过程中出现错误，请检查:
    echo 1. 输入目录 %SRC_DIR% 是否存在
    echo 2. 输出目录 %DST_DIR% 是否有写入权限
    echo 3. 可执行文件 csv_converter.exe 是否存在
    pause
    exit /b 1
)

echo.
echo ✓ 转换完成！
echo 转换后的文件已保存到: %DST_DIR%
pause
