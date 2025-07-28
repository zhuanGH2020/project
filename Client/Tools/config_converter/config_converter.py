#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Config编码转换工具
将UTF-8编码的配置文件转换为GB2312编码
用法: python config_converter.py src_dir dst_dir
"""

import os
import sys
import shutil
import argparse
from pathlib import Path


def detect_encoding(file_path):
    """
    检测文件编码
    返回检测到的编码，如果检测失败返回None
    """
    import chardet
    
    with open(file_path, 'rb') as f:
        raw_data = f.read()
        result = chardet.detect(raw_data)
        return result['encoding']


def convert_config_encoding(src_file, dst_file, src_encoding='utf-8', dst_encoding='gb2312'):
    """
    转换单个配置文件的编码
    
    Args:
        src_file: 源文件路径
        dst_file: 目标文件路径
        src_encoding: 源文件编码
        dst_encoding: 目标文件编码
    """
    try:
        # 读取源文件
        with open(src_file, 'r', encoding=src_encoding, errors='ignore') as f:
            content = f.read()
        
        # 确保目标目录存在
        os.makedirs(os.path.dirname(dst_file), exist_ok=True)
        
        # 写入目标文件
        with open(dst_file, 'w', encoding=dst_encoding, newline='') as f:
            f.write(content)
        
        print(f"✓ 转换成功: {src_file} -> {dst_file}")
        return True
        
    except Exception as e:
        print(f"✗ 转换失败: {src_file} - {str(e)}")
        return False


def process_directory(src_dir, dst_dir):
    """
    处理整个目录的配置文件
    
    Args:
        src_dir: 源目录
        dst_dir: 目标目录
    """
    src_path = Path(src_dir)
    dst_path = Path(dst_dir)
    
    if not src_path.exists():
        print(f"错误: 源目录不存在 - {src_dir}")
        return False
    
    # 创建目标目录
    dst_path.mkdir(parents=True, exist_ok=True)
    
    # 查找所有配置文件（CSV格式）
    config_files = list(src_path.rglob("*.csv"))
    
    if not config_files:
        print(f"警告: 在源目录中没有找到配置文件 - {src_dir}")
        return True
    
    print(f"找到 {len(config_files)} 个配置文件")
    
    success_count = 0
    total_count = len(config_files)
    
    for config_file in config_files:
        # 计算相对路径
        rel_path = config_file.relative_to(src_path)
        dst_file = dst_path / rel_path
        
        # 检测文件编码
        detected_encoding = detect_encoding(config_file)
        
        if detected_encoding and detected_encoding.lower() in ['utf-8', 'utf-8-sig']:
            # 转换编码
            if convert_config_encoding(config_file, dst_file, detected_encoding, 'gb2312'):
                success_count += 1
        else:
            # 直接复制文件（假设已经是GB2312或其他编码）
            try:
                dst_file.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(config_file, dst_file)
                print(f"✓ 复制文件: {config_file} -> {dst_file}")
                success_count += 1
            except Exception as e:
                print(f"✗ 复制失败: {config_file} - {str(e)}")
    
    print(f"\n转换完成: {success_count}/{total_count} 个文件处理成功")
    return success_count == total_count


def main():
    """主函数"""
    parser = argparse.ArgumentParser(
        description='Config编码转换工具 - 将UTF-8编码的配置文件转换为GB2312编码',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
使用示例:
  python config_converter.py src_dir dst_dir
  python config_converter.py ./Assets/Resources/Configs ./CSV
        """
    )
    
    parser.add_argument('src_dir', help='源目录路径（包含UTF-8编码的配置文件）')
    parser.add_argument('dst_dir', help='目标目录路径（输出GB2312编码的配置文件）')
    
    args = parser.parse_args()
    
    print("Config编码转换工具")
    print("=" * 50)
    print(f"源目录: {args.src_dir}")
    print(f"目标目录: {args.dst_dir}")
    print("=" * 50)
    
    # 检查依赖
    try:
        import chardet
    except ImportError:
        print("错误: 缺少依赖库 'chardet'")
        print("请运行: pip install chardet")
        return 1
    
    # 处理目录
    success = process_directory(args.src_dir, args.dst_dir)
    
    if success:
        print("\n✓ 所有文件处理完成")
        return 0
    else:
        print("\n✗ 部分文件处理失败")
        return 1


if __name__ == "__main__":
    sys.exit(main()) 