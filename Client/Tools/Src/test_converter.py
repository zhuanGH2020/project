#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
CSV转换工具测试脚本
"""

import os
import tempfile
import shutil
from pathlib import Path


def create_test_csv_files(test_dir):
    """创建测试用的CSV文件"""
    
    # 创建测试目录
    test_path = Path(test_dir)
    test_path.mkdir(parents=True, exist_ok=True)
    
    # 测试数据
    test_data = [
        ("item.csv", "Id,Name,Type,Attack,Defense\nint,string,enum<ItemType>,int,int\n编号,名称,类型,攻击力,防御力\n1,铁剑,1,10,5\n2,木剑,1,8,3"),
        ("monster.csv", "Id,Name,Type,HP,Attack\nint,string,enum<MonsterType>,int,int\n编号,名称,类型,生命值,攻击力\n1,普通僵尸,1,100,20\n2,精英僵尸,2,200,40"),
        ("config.csv", "Key,Value,Description\nstring,string,string\n键,值,描述\nlanguage,zh_CN,语言设置\nvolume,0.8,音量设置")
    ]
    
    # 创建文件
    for filename, content in test_data:
        file_path = test_path / filename
        with open(file_path, 'w', encoding='gb2312', newline='') as f:
            f.write(content)
        print(f"创建测试文件: {file_path}")
    
    return test_path


def test_converter():
    """测试转换工具"""
    
    print("CSV转换工具测试")
    print("=" * 50)
    
    # 创建临时目录
    with tempfile.TemporaryDirectory() as temp_dir:
        temp_path = Path(temp_dir)
        
        # 创建源目录和目标目录
        src_dir = temp_path / "src"
        dst_dir = temp_path / "dst"
        
        print(f"临时目录: {temp_path}")
        print(f"源目录: {src_dir}")
        print(f"目标目录: {dst_dir}")
        
        # 创建测试文件
        print("\n1. 创建测试文件...")
        create_test_csv_files(src_dir)
        
        # 导入转换工具
        print("\n2. 测试转换功能...")
        try:
            # 模拟转换过程
            from csv_converter import process_directory
            
            success = process_directory(str(src_dir), str(dst_dir))
            
            if success:
                print("✓ 转换测试成功")
                
                # 检查结果
                print("\n3. 检查转换结果...")
                dst_files = list(dst_dir.rglob("*.csv"))
                print(f"转换后文件数量: {len(dst_files)}")
                
                for file_path in dst_files:
                    print(f"  - {file_path.name}")
                    
            else:
                print("✗ 转换测试失败")
                return False
                
        except ImportError as e:
            print(f"✗ 导入错误: {e}")
            print("请确保 csv_converter.py 在同一目录下")
            return False
        except Exception as e:
            print(f"✗ 测试失败: {e}")
            return False
    
    print("\n✓ 所有测试完成")
    return True


if __name__ == "__main__":
    success = test_converter()
    if not success:
        exit(1) 