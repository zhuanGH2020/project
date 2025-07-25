@echo off
chcp 65001 >nul
echo CSV转换工具 - 可执行文件构建器
echo ================================================
echo.

REM 检查Python是否安装
python --version >nul 2>&1
if errorlevel 1 (
    echo 错误: 未找到Python，请先安装Python 3.7+
    pause
    exit /b 1
)

echo Python已安装，开始构建...
echo.

REM 运行构建脚本
python build_csv_converter.py

if errorlevel 1 (
    echo.
    echo 构建失败，请检查错误信息
    pause
    exit /b 1
)

echo.
echo 构建完成！可执行文件在 ../ 目录中
echo.
echo 使用方法:
echo   在项目根目录运行: Tools\csv_converter.bat
echo   或直接运行: Tools\csv_converter.exe CSV Assets/Resources/Configs
echo.
pause 