#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Config转换工具测试脚本
"""

import os
import tempfile
import shutil
from pathlib import Path


def create_test_config_files(test_dir):
    """创建测试用的配置文件"""
    
    # 创建测试目录
    test_path = Path(test_dir)
    test_path.mkdir(parents=True, exist_ok=True)
    
    # 测试数据（模拟Unity项目的配置文件格式）
    test_data = [
        ("Item.csv", "Id,Name,Type,Attack,Defense\nint,string,enum<ItemType>,int,int\n编号,名称,类型,攻击力,防御力\n1,铁剑,1,10,5\n2,木剑,1,8,3"),
        ("Monster.csv", "Id,Name,Type,HP,Attack\nint,string,enum<MonsterType>,int,int\n编号,名称,类型,生命值,攻击力\n1,普通僵尸,1,100,20\n2,精英僵尸,2,200,40"),
        ("Make.csv", "Id,Name,Cost,Result\nint,string,int,string\n编号,名称,消耗,结果\n1,制作铁剑,100,铁剑\n2,制作木剑,50,木剑"),
        ("Equip.csv", "Id,Name,Type,Level,Stats\nint,string,enum<EquipType>,int,string\n编号,名称,类型,等级,属性\n1,新手装备,1,1,基础属性\n2,高级装备,2,10,强化属性")
    ]
    
    # 创建文件（UTF-8编码，模拟Unity项目中的配置文件）
    for filename, content in test_data:
        file_path = test_path / filename
        with open(file_path, 'w', encoding='utf-8', newline='') as f:
            f.write(content)
        print(f"创建测试文件: {file_path}")
    
    return test_path


def test_converter():
    """测试转换工具"""
    
    print("Config转换工具测试")
    print("=" * 50)
    
    # 创建临时目录
    with tempfile.TemporaryDirectory() as temp_dir:
        temp_path = Path(temp_dir)
        
        # 创建源目录和目标目录
        src_dir = temp_path / "Assets" / "Resources" / "Configs"
        dst_dir = temp_path / "CSV"
        
        print(f"临时目录: {temp_path}")
        print(f"源目录: {src_dir}")
        print(f"目标目录: {dst_dir}")
        
        # 创建测试文件
        print("\n1. 创建测试配置文件...")
        create_test_config_files(src_dir)
        
        # 导入转换工具
        print("\n2. 测试转换功能...")
        try:
            # 模拟转换过程
            from config_converter import process_directory
            
            success = process_directory(str(src_dir), str(dst_dir))
            
            if success:
                print("✓ 转换测试成功")
                
                # 检查结果
                print("\n3. 检查转换结果...")
                dst_files = list(dst_dir.rglob("*.csv"))
                print(f"转换后文件数量: {len(dst_files)}")
                
                for file_path in dst_files:
                    print(f"  - {file_path.name}")
                    
                    # 验证编码转换
                    try:
                        with open(file_path, 'r', encoding='gb2312') as f:
                            content = f.read()
                            print(f"    ✓ GB2312编码验证成功: {len(content)} 字符")
                    except UnicodeDecodeError:
                        print(f"    ✗ GB2312编码验证失败")
                        return False
                    
            else:
                print("✗ 转换测试失败")
                return False
                
        except ImportError as e:
            print(f"✗ 导入错误: {e}")
            print("请确保 config_converter.py 在同一目录下")
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