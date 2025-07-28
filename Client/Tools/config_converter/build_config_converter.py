#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Config转换工具构建脚本
"""

import os
import sys
import subprocess
import shutil


def build_exe():
    """构建可执行文件"""
    print("构建Config转换工具...")
    
    # 构建命令
    cmd = [
        "pyinstaller",
        "--onefile",
        "--console",
        "--name=config_converter",
        "--distpath=.",
        "--workpath=build",
        "--specpath=build",
        "--clean",
        "config_converter.py"
    ]
    
    try:
        subprocess.check_call(cmd)
        print("✓ 构建成功")
        return True
    except subprocess.CalledProcessError as e:
        print(f"✗ 构建失败: {e}")
        return False
    except FileNotFoundError:
        print("✗ 未找到pyinstaller，请先安装: pip install pyinstaller")
        return False


def cleanup():
    """清理构建文件"""
    try:
        if os.path.exists("build"):
            shutil.rmtree("build")
        if os.path.exists("config_converter.spec"):
            os.remove("config_converter.spec")
    except Exception:
        pass


def main():
    """主函数"""
    if not os.path.exists("config_converter.py"):
        print("错误: 请在Tools/config_converter目录下运行此脚本")
        return 1
    
    if not build_exe():
        return 1
    
    cleanup()
    print("✓ 构建完成！")
    return 0


if __name__ == "__main__":
    sys.exit(main()) 