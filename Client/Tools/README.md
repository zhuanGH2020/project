# 游戏开发工具集

## 工具列表

### CSV编码转换工具
将Excel编辑的GB2312编码CSV文件转换为Unity使用的UTF-8编码。

**作用**：解决Excel编辑和Unity加载CSV文件时的编码冲突问题。

**使用方法**：
```bash
# 1. 构建可执行文件
Tools/Src/build_csv_converter.bat

# 2. 使用转换工具
Tools/csv_converter.bat
```

**工作流程**：
1. 在Excel中编辑CSV文件，保存到 `CSV/` 目录
2. 运行 `Tools/csv_converter.bat` 进行转换
3. 转换后的UTF-8文件自动保存到 `Assets/Resources/Configs/` 目录
4. Unity正常加载，无乱码问题

## 开发说明

### 添加新工具
1. 在 `Tools/Src/` 目录下添加工具源码
2. 创建对应的构建脚本 `build_工具名.bat`
3. 在README中添加工具说明

### 构建工具
```bash
# 进入源码目录
cd Tools/Src

# 运行对应的构建脚本
build_工具名.bat
```

## 注意事项
- 所有工具都遵循统一的目录结构和命名规范
- 构建后的可执行文件直接放在Tools目录下
- 每个工具都有独立的构建脚本和说明文档 