# Sanmon Roadmap

## 🧩 核心模块(Module)

### 资源管理 (Asset)
- [ ] YooAsset停止游戏时"Async operation 'DownloadSchedulerOperation' has been aborted."警告
- [ ] Asset支持Unitask异步加载
- [ ] 一个子功能，Gameobject的内部池化
- [ ] 精灵图、图集加载 -> ui

### 配置系统 (Config)
- [ ] Table Luban 的可视化工具（Unity 参考：[LubanYoki](https://github.com/HinataYoki/LubanKit)；跨平台：Avalonia）
- [ ] 非表格配置（如游戏设置 -> 走Application.persistentDataPath）

### 存档系统 (Save)
- [ ] 确定方案：自研还是使用开源库？
- [ ] 开发期间JSON，运行时压缩为二进制？尽量不加密（精力花销）
- [ ] 实现快速加载与异步存档

### UI系统 (ui)
- [ ] UIHandle的实现

### 网络 (Net)
- [ ] 基于 fantasy 的局域网实现
- [ ] 基于 fantasy 的 P2P 实现
- [ ] 玩家自建服务器部署方案

---

## 🕹️ 游戏系统 (System)
- 待补充

---

## 📝 运行时记录 (Note)
- 待补充

---

## ✨ 实体与逻辑 (Entity)

### 管线系统 (pipline)
- 所有的严格逻辑均走pipline
- pipline处理管线会处理Entity的回调，回调用Effect写

### 组件系统 (Component)
- Entity的纯数据，只解决数据的存储方式，有就代表有对应的功能
- 受到Function的修改
- 组件存在异步初始化的情况，功能如何想办法访问到安全的数据呢？

### 功能系统 (Function)
- 处理Entity Component的独立模块，也会调用Effect，也是处理特殊节点才用
- 比如动画功能，物理功能等等

## 🧰 Helper (工具)

### 调试工具 (Debug)
- [ ] 绘制基础图形：圆、矩形、球、方体、圆柱、贝塞尔曲线

### 编辑器快捷

- [ ] ToolBar形式的停靠定位按钮
- [ ] 快速启动
- [ ] 资源包加载方式

---

