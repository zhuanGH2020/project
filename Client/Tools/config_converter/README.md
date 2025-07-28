# Config Converter 工具

## 概述

Config Converter 是一个编码转换工具，用于将 Unity 项目中的配置文件从 UTF-8 编码转换为 GB2312 编码。此工具是参考 `csv_converter` 的架构设计，实现相反的转换功能。

## 功能特性

- **编码转换**: 将 UTF-8 编码的配置文件转换为 GB2312 编码
- **目录转换**: 从 `Assets\Resources\Configs` 转换到 `CSV` 目录  
- **批量处理**: 支持目录下所有 CSV 文件的批量转换
- **编码检测**: 自动检测源文件编码
- **独立可执行文件**: 生成 `config_converter.exe` 独立运行

## 参考架构

本工具严格参考 `Tools/csv_converter/` 的架构设计：

### 参考文件映射

| csv_converter | config_converter | 功能说明 |
|---------------|------------------|----------|
| `csv_converter.py` | `config_converter.py` | 主要转换逻辑 |
| `build_csv_converter.py` | `build_config_converter.py` | 构建脚本 |
| `build_csv_converter.bat` | `build_config_converter.bat` | 批处理构建脚本 |
| `test_converter.py` | `test_config_converter.py` | 测试脚本 |

### 架构特点

1. **一致的代码结构**: 保持与 csv_converter 相同的函数命名和组织方式
2. **相同的错误处理**: 使用一致的异常处理和用户提示
3. **统一的命令行接口**: 相同的参数格式和帮助信息
4. **一致的构建流程**: 使用相同的 PyInstaller 配置和构建步骤

## 文件结构

```
Tools/config_converter/
├── config_converter.py          # 主要转换工具
├── build_config_converter.py    # 构建脚本
├── build_config_converter.bat   # 批处理构建脚本
├── test_config_converter.py     # 测试脚本
├── config_converter.exe         # 生成的可执行文件
└── README.md                     # 说明文档
```

## 使用方法

### 1. 构建可执行文件

在 `Tools/config_converter/` 目录下运行：

```bash
# 方法1: 使用批处理脚本
build_config_converter.bat

# 方法2: 直接运行Python脚本
python build_config_converter.py
```

### 2. 使用转换工具

在项目根目录运行：

```bash
# 方法1: 使用项目根目录的批处理脚本
Tools\config_converter.bat

# 方法2: 直接运行可执行文件
Tools\config_converter.exe Assets/Resources/Configs CSV
```

### 3. 测试工具

```bash
cd Tools/config_converter/
python test_config_converter.py
```

## 技术细节

### 编码转换

- **源编码**: UTF-8 (Unity项目默认编码)
- **目标编码**: GB2312 (CSV工具链要求的编码)
- **检测库**: 使用 `chardet` 自动检测文件编码

### 依赖要求

```txt
chardet>=4.0.0  # 编码检测库
pyinstaller     # 构建工具（仅构建时需要）
```

### 错误处理

- 自动检测和处理编码问题
- 提供详细的错误信息和建议
- 支持部分文件转换失败的情况

## 与 csv_converter 的对比

| 特性 | csv_converter | config_converter |
|------|---------------|------------------|
| 转换方向 | GB2312 → UTF-8 | UTF-8 → GB2312 |
| 源目录 | CSV/ | Assets/Resources/Configs/ |
| 目标目录 | Assets/Resources/Configs/ | CSV/ |
| 使用场景 | 开发环境配置导入 | 配置数据导出 |

## 注意事项

1. **依赖库**: 需要安装 `chardet` 库进行编码检测
2. **目录结构**: 确保项目目录结构正确
3. **文件权限**: 确保目标目录有写入权限
4. **编码兼容**: GB2312 编码可能不支持某些特殊字符 