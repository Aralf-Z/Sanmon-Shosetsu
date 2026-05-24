# Sanmon Roadmap

## 🧩 核心模块(Module)

### 资源管理 (Asset)
- 加载移出 Resource 加载，脱离运行包
- 评估：采用 YooAsset?
- 评估：采用 Qf.ResKit?

### 配置系统 (Config)
- Table Luban 的可视化工具（Unity 参考：[LubanYoki](https://github.com/HinataYoki/LubanKit)；跨平台：Avalonia）
- 非表格配置（如游戏设置）

### 存档系统 (Save)
- 确定方案：自研还是使用开源库？
- 开发期间JSON，运行时压缩为二进制？尽量不加密（精力花销）
- 实现快速加载与异步存档

### 网络 (Net)
- 基于 fantasy 的局域网实现
- 基于 fantasy 的 P2P 实现
- 玩家自建服务器部署方案

---

## 🕹️ 游戏系统 (System)
- 待补充

---

## 📝 运行时上下文 (Note)
- 待补充

---

## ✨ 实体与逻辑 (Entity)

### 效果系统 (Effect)
- 逻辑处理单元，每秒固定刷新
- 实体内部逻辑避免链式调用
- 支持时序执行
- 每个 Effect 记录施加者与携带者，方便 Debug

---

## 🛠️ 开发工具包 (DevToolKit)
- 待补充

---

## 🧰 Helper (工具)

### 调试工具 (Debug)
- 绘制基础图形：圆、矩形、球、方体、圆柱、贝塞尔曲线

---

