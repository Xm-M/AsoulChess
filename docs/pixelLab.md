# PixelLab 中文阅读手册（整合版）

> 官方文档入口：[https://www.pixellab.ai/docs](https://www.pixellab.ai/docs)  
> 本文把站点**结构、用法、常见能力、FAQ、入门流程**译成中文，便于速查；**每个具体工具的细节**仍以官网为准（文档会持续更新）。

---

## 一、PixelLab 是什么

**PixelLab** 面向**画师与独立游戏开发者**，用 AI **生成与编辑像素画**，主要产出游戏资产（角色、动画、旋转、地图、UI 等）。

官方说明的接入方式包括（详见第二节）：

- 浏览器里的**轻量创作页**  
- **角色（Characters）**一站式流程（四/八方向 + 动画等）  
- 浏览器里的 **PixelLab Pixelorama**（开源 Pixelorama + PixelLab AI）  
- **Aseprite 插件**（本地专业像素软件内调用）  
- **Vibe Coding / MCP**（给 Cursor、Claude Code、VS Code 等 AI 编程助手接工具）  
- **HTTP API**（自建管线、工具链）

计算在**云端 GPU**上跑，本机不必很强显卡（见 FAQ）。

---

## 二、七种使用方式（官方「Ways to Use」）

| # | 名称 | 说明 | 适合谁 |
|---|------|------|--------|
| 1 | **Simple Web Creator** | 浏览器打开即用，支持桌面与手机。使用 **PixFlux**（中～超大图）与 **BitForge**（小～中图）等模型。 | 快速试效果、手机随手出图 |
| 2 | **Characters（新）** | 用文字生成**游戏向角色**：**4 或 8 方向**视图、行走/奔跑/待机等**一键动画**，可导出精灵表或单帧；内置预览。 | 要「从角色到多方向+动画」一条龙 |
| 3 | **PixelLab Pixelorama** | 开源 **Pixelorama** 与 PixelLab 集成，**浏览器里**完整编辑 + AI。 | 想要「正经编辑器 + AI」且不装 Aseprite |
| 4 | **Aseprite 扩展** | 在本地 **Aseprite** 里直接用 PixelLab 菜单。安装见账号页的 Tools，文档：[Installation](https://www.pixellab.ai/docs/installation) | 已有 Aseprite 工作流 |
| 5 | **Vibe Coding（AI Agent Toolkit）** | 通过 **MCP** 让 IDE 里的 AI 调生成接口；文档提到支持 **Godot** 等无头开发场景。 | 想用 AI 写代码同时出像素资产 |
| 6 | **API** | 在自己的软件/管线里调用后端。文档：[API Docs](https://api.pixellab.ai/v1/docs)；Python 有 [pixellab-python](https://github.com/pixellab-code/pixellab-python)。 | 自动化、批处理、自研工具 |
| 7 | **视频教程** | YouTube：[@PixelLab_AI](https://www.youtube.com/@PixelLab_AI) | 跟视频一步步做 |

**注意**：官方在「Pixelorama」条目里先写「桌面与手机浏览器」，后面又写「仅桌面浏览器、不支持手机」——以你打开 [editor](https://www.pixellab.ai/editor) 时**实际提示**为准。

**常用链接**

- 轻量网页创作：[Create](https://www.pixellab.ai/create)  
- 浏览器编辑器：[Editor / Pixelorama](https://www.pixellab.ai/editor)  
- Discord 社区：[邀请链接](https://discord.gg/pBeyTBF8T7)（文档页提供）

---

## 三、能力地图（官网侧栏分类 → 中文理解）

下面按 [文档侧栏](https://www.pixellab.ai/docs) 的模块整理「**这类工具一般干什么**」。具体子页名称（Pro / old / v2 等）会随产品迭代，查官网最准。

### 3.1 Introduction（入门）

| 文档主题 | 中文要点 |
|----------|----------|
| Introduction | 总览：多种使用方式、面向美术与开发者 |
| Ways to use PixelLab | 见本文第二节 |
| Installation (Aseprite) | Aseprite 插件下载与安装 |
| Introduction to Pixelorama | 浏览器版 Pixelorama 集成说明 |
| FAQ | 见本文第五节 |

### 3.2 Guides（指南类）

| 文档主题 | 中文要点 |
|----------|----------|
| Init images and inpainting | **初始图（Init）** + **局部重绘（Inpaint）** 的典型工作流（见本文第四节） |
| Creating maps | 做地图相关流程 |
| Rotating a character | 角色多方向旋转 |

### 3.3 Create image（生成图）

侧栏包含多种尺寸/模式，例如（名称以官网为准）：

- 按风格参考生成（Pro）  
- S-M / M-XL 等档位生成  
- **Image to image（depth）** 等图生图  
- **Pose to image** 姿态到图  
- **Image to pixel art** 图转像素风  

**理解**：都是「**文生图 / 图生图**」在不同分辨率、约束与参考下的变体。

### 3.4 Edit image（编辑图）

- 通用 **Edit**、去背景、改尺寸、**Unzoom pixel art**（像素画放大后再「缩回」类处理）等。  
- **Pro** 版本往往带更强或更多选项。

### 3.5 Rotate（旋转 / 多方向）

- 角色或物体 **4 / 8 方向**精灵视图。  
- **Pro**：例如 **8 方向精灵**一键类工作流。  
- 适合**俯视、斜 45°、等角**等需要方向帧的游戏。

### 3.6 Animate（动画）

常见能力（官网首页与文档反复出现）：

- **文字描述生成动画**（Animate with text，有 New / Pro / old 等版本线）  
- **骨骼驱动动画**、**动画到动画**迁移  
- **自动角色动画**、插帧、换装、改姿势等（多带 Pro 标记）

### 3.7 Map（地图 / 瓦片）

- 创建地图、扩展地图（多版本 v2 / old）  
- **纹理、Tileset、等角瓦片**等  
- 文档强调可做到较大场景（如 **400×400** 量级描述，以官网为准）

### 3.8 Inpaint（局部重绘）

- **Inpaint**、**Inpaint v3**、**Inpaint M-L（pixpatch v2）** 等不同代工具。

### 3.9 Reduce colors（减色）

- 把图**限制到更少颜色数**，贴近「掌机/复古」调色板。

### 3.10 Experimental / Extra tools

- 行走角色试验、试穿、多图合成等实验功能  
- **Create UI elements**：按钮、血条、菜单项等 **UI 像素组件**（有 Pro）

### 3.11 Tool options（工具选项）

侧栏列有：**General、Init image、Inpainting、Guidance、Character、Colors、Camera、Projection** 等——控制**参考图强度、引导、角色/颜色/相机**等生成行为；具体每项含义以对应英文文档为准。

---

## 四、入门工作流：Init 图 + Inpaint（官方 Getting Started 摘要）

以下内容来自官网「入门」类页面（当前与 *Init images and inpainting* 绑定在同一套说明里），译成步骤方便你跟做。

### 4.1 打开工具

在 **Aseprite** 或 **Pixelorama** 中：

1. 菜单选 **`Create image` → `Create S-M image`**（小～中图；具体名称以你版本为准）。  
2. 若 PixelLab 面板关了：  
   - **菜单**：`Edit > PixelLab > Open plugin`  
   - **快捷键**：`Ctrl + Space + P`（Windows；Mac 以官方为准）

### 4.2 强烈建议：使用 Init Image（初始图）

- **Init image 可选**，但能**明显提高可控性**。  
- 可以只是一张**很草的线稿/色块**，再写文字描述（例：`Human mage`），勾选 **Use init image**，其余先默认，点 **Generate**。

### 4.3 不满意？用 Inpaint（局部重绘）

1. 回到主菜单，选 **Inpaint**。  
2. 会在图上叠一层 **Inpaint 层**：在**想改的地方涂黑**（模型**只改黑色区域**，其余保持）。  
3. 可把**当前生成结果**再当作新的 **Init image**；适当**降低 Init image strength**，模型更有自由度改局部。  
4. 输出方式可选 **Modify current layer** 等，让修改直接落在当前图层。  
5. 可多次：**先修头、再修手**，也可 **Inpaint + 在图上画草图（如火球）** 再生成，反复迭代。

**核心概念**

| 术语 | 作用 |
|------|------|
| **Init image** | 给 AI 构图/风格锚点，草图也行 |
| **Inpaint** | 只动蒙版区域，其余像素尽量保留 |
| **Init image strength** | 越高越贴原图；越低改动越大 |

---

## 五、FAQ 精选（官方 FAQ 译要）

### 免费试用

- **试用码在哪**：在介绍免费试用的**视频末尾**；官方视频：[YouTube 说明](https://www.youtube.com/watch?v=Tbmfh4pBPeo)。  
- **为什么试用里工具不全**：试用次数有限，只开放**最容易上手**的一部分工具。

### 一般问题

- **Aseprite 插件怎么拿**：见 [installation#get-aseprite](https://www.pixellab.ai/docs/installation#get-aseprite)。  
- **会不会用我的图训练模型**：官方称**不会**用用户输入或生成内容训练模型。  
- **系统**：Windows / Mac / Linux 可用。  
- **要不要好显卡**：**不用**；算力在云端。  
- **能否看历史一共用了多少次生成**：官方称**不提供**该统计，且**一般不会硬性限流**，除非滥用成问题。  
- **Steam 游戏能用 AI 素材吗**：官方称**可以**；并引用 Steam 对 **Pre-Generated / Live-Generated** 的披露要求（上架问卷需说明 AI 用法与护栏）。原文见 [Steam 公告](https://steamcommunity.com/groups/steamworks/announcements/detail/3862463747997849619)。  
- **能否商用生成图**：**可以**；要求**不要用这些图去训练新模型**。  
- **为什么订阅制**：官方解释包括云端 GPU 成本、技术迭代快、月付风险低等；也提到未来**可能**考虑一次性或本地运行。  
- **月中能否升级套餐**：可以，**按剩余天数比例付费**。  
- **Aseprite 插件一直 Connecting…**：可能与 **WebSocket 被墙/网络差** 有关；可换网络、试 VPN、换电脑。官方称 Aseprite 网络能力有限，难以完美优化弱网。  
- **照片转像素风**：可先缩小照片，再当作 **init image**，用例如 **Create image (style, old)** 或「生成大图」类工具配合。  
- **能否找回上一次生成的参数**：在 **Aseprite 扩展**里可找到历史设置与输出；打开 **Advanced options** 显示 **Load previous settings**。

### 与隐私相关的简述（Introduction 页）

- 官方称**默认不存储**生成图与输入图，除非某工具或文档**明确写出**例外。

---

## 六、官方 YouTube 教程索引（标题译意）

频道：[PixelLab_AI](https://www.youtube.com/@PixelLab_AI)

| 官方标题（大意） | 链接 |
|------------------|------|
| 用骨骼做复杂动画 | [youtu.be/RVVuDhyEbRI](https://youtu.be/RVVuDhyEbRI) |
| 生成物品道具 | [youtu.be/iBMq3P_Fazk](https://youtu.be/iBMq3P_Fazk) |
| 用 Inpaint 做风格统一角色 | [youtu.be/68BYzLoLh-U](https://youtu.be/68BYzLoLh-U) |
| 生成角色 | [youtu.be/h0D-oVAFdMs](https://youtu.be/h0D-oVAFdMs) |
| Edit 工具怎么用 | [youtu.be/XhmpenTmPLg](https://youtu.be/XhmpenTmPLg) |
| 从一段动画到另一段动画 | [youtu.be/owkamgYVWAs](https://youtu.be/owkamgYVWAs) |
| 等角地图与瓦片 | [youtu.be/CuBvG9mfQng](https://youtu.be/CuBvG9mfQng) |
| 横版地图与 Tileset | [youtu.be/H-dPJKmKr1E](https://youtu.be/H-dPJKmKr1E) |
| 高视角俯视 Tileset | [youtu.be/jPPznIEK7HY](https://youtu.be/jPPznIEK7HY) |
| 用文字生成动画 | [youtu.be/H98R_o2nw30](https://youtu.be/H98R_o2nw30) |
| 旋转工具 | [youtu.be/ufQ72nGORC0](https://youtu.be/ufQ72nGORC0) |
| 攻击动作生成 | [youtu.be/wijMirHw6QQ](https://youtu.be/wijMirHw6QQ) |

---

## 七、文档里提到的模型名（混个眼熟）

| 名称 | 大致定位（非严格技术定义） |
|------|------------------------------|
| **PixFlux** | Web 轻量创作里偏 **中～更大尺寸** 的生成 |
| **BitForge** | Web 轻量创作里偏 **小～中尺寸** 的生成 |

具体分辨率与定价以官网当前页为准。

---

## 八、你如何用这个 `pixelLab.md`

1. **先决定入口**：只浏览器 → Create / Editor；要专业流程 → Aseprite 插件。  
2. **先学两件事**：**Init image** + **Inpaint**（第四节）。  
3. **按资产类型翻第三节**：做地图找 Map；做四八方向找 Rotate；做 UI 找 Extra 里 UI elements。  
4. **卡住先翻第五节 FAQ**（连接、商用、Steam）。  
5. **细节参数**打开官网对应英文页 + 用浏览器翻译；或看第六节视频。

---

## 九、修订记录

| 日期 | 说明 |
|------|------|
| 2026-04-03 | 初版：根据 [pixellab.ai/docs](https://www.pixellab.ai/docs) 公开页面与侧栏结构整理；Pixelorama 移动端描述以官网矛盾处已注明。 |
