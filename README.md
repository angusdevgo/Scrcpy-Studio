<div align="center">

# ⚡ Scrcpy Studio

**专为开发者与极客打造的现代化 Android 投屏控制工作台**

基于 [Genymobile/scrcpy](https://github.com/Genymobile/scrcpy) 二次开发 · 零延迟 · 双击无黑框 · 高刷超清 · Linear 极客暗黑美学

[![GitHub stars](https://img.shields.io/github/stars/angusdevgo/scrcpy-studio?style=for-the-badge&color=00E1AA)](https://github.com/angusdevgo/scrcpy-studio/stargazers)
[![GitHub license](https://img.shields.io/github/license/angusdevgo/scrcpy-studio?style=for-the-badge&color=7928CA)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows)](https://github.com/angusdevgo/scrcpy-studio)
[![Upstream](https://img.shields.io/badge/Upstream%20scrcpy-v5.0.1-00E1AA?style=for-the-badge&logo=android)](https://github.com/Genymobile/scrcpy/releases/tag/v5.0.1)

[功能特性](#-功能特性) • [快速上手](#-快速上手) • [二改亮点](#-二改核心亮点) • [编译构建](#-编译构建) • [快捷键指南](#-投屏快捷键指南)

</div>

---

## 📖 项目简介

**Scrcpy Studio** 是对知名开源 Android 投屏神器 [scrcpy](https://github.com/Genymobile/scrcpy) 进行深度定制与体验升级的二次开发版本。

官方版本的 scrcpy 虽然性能强悍，但缺乏直观便捷的参数调控界面，且在 Windows 下直接双击运行时总会弹出一个显眼的 CMD 控制台黑框。**Scrcpy Studio** 在保留官方极致性能与零延迟特性的基础上，移除了黑框干扰，并新增了专用的 **Studio 现代化控制面板**，让高刷画质调节、多设备管理、无线连接与常用开关变得触手可及。

> 🔄 **上游同步状态**：底层源码已对齐官方 **scrcpy v5.0.1**（2025 最新稳定版），完整继承 D3D11VA 硬件解码、多线程解码器、FFmpeg 9.0.2 / SDL 3.4.18 / platform-tools 37.0.1 等上游全部改进。

---

## 🔄 上游同步记录

### v4.1 → v5.0.1（本次同步）
| 模块 | 上游关键改进 |
|:---|:---|
| **视频解码** | 新增 **D3D11VA 硬件解码**（Windows）、VA-API（Linux）、VideoToolbox（macOS）；解码器独立线程化，软解自动回退 |
| **新增参数** | `--hwdec`（硬件解码模式）、`--video-buffer`、`--keep-active`、`--no-window-aspect-ratio-lock`、`--render-fit`、`--background-color` |
| **依赖升级** | FFmpeg **9.0.2**、SDL **3.4.18**、dav1d **1.5.4**、platform-tools **37.0.1** |
| **稳定性** | 修复 Windows H.265 硬解、AV1 硬解会话重置、YUVJ420P 帧兼容、视频调节器背压控制 |

> 自定义改动（GUI 工作台、控制台隐藏补丁、Studio 文档）已全量重放到新版本基线之上，`git diff v5.0.1` 仅保留本项目自有增量。

---

## ✨ 功能特性

### 🎨 极客暗黑 Studio 控制面板
- **设计美学**：采用类似 **Linear / Raycast** 的现代极简暗黑设计规范，微光描边与高辨识度状态流。
- **设备热插拔感知**：实时监测 USB / Wi-Fi 已连接的 Android 设备，支持一键切换活动设备。
- **无线调试配对**：支持在界面输入 IP 端口一键建立 ADB Wi-Fi 调试投屏连接。

### 🚀 画质与性能极致调校
- **高刷突破**：支持原生 **120 FPS / 90 FPS / 60 FPS** 帧率自由切换。
- **超清渲染**：支持指定渲染分辨率上限（4K / 2K / 1080P）或点对点原始输出。
- **现代编解码**：支持自由选择 **H.264 / H.265 (HEVC) / AV1** 硬件加速编解码器。
- **硬件解码加速**：v5.0.1 起底层支持 **D3D11VA** GPU 硬解，大幅降低 4K 高码率投屏时的 CPU 占用。
- **码率自适应**：轻松设定传输码率（最高支持 64 Mbps 极高码率无损压制）。

### ⚡ 常用快捷功能一键开启
- **息屏投屏** (`--turn-screen-off`)：投屏时关闭手机物理屏幕，省电且防止发热。
- **窗口置顶** (`--always-on-top`)：将投屏窗口固定于桌面最前端，方便边看边操作。
- **保持常亮** (`--stay-awake`)：投屏过程中阻止 Android 设备休眠锁定。
- **音频同步转发**：支持开启/关闭设备声音实时回传到电脑音响。
- **显示触摸轨迹** (`--show-touches`)：演示与录屏时直观显示手指点击圆点。

### 🧭 内嵌系统导航栏（固定在投屏窗口底部）
- **窗口内固定**：直接绘制在 scrcpy 投屏窗口底部，随窗口移动/缩放/全屏，不再是独立悬浮窗。
- **系统三键直通**：**返回 / 主页 / 最近任务** 一键直达，另有音量 ± 与电源键。
- **矢量图标**：内嵌绘制的实心风格图标（DPI 自适应，高分辨率屏自动放大），悬停/按下带极光青胶囊高亮。
- **不遮挡画面**：为导航栏预留底部区域，视频等比缩放，不会盖住手机内容。
- **原生零依赖**：纯 C + SDL3 渲染，无需额外进程或外部资源。

> 另保留可选的 **悬浮导航栏**（Dashboard「快捷操作」中手动开启），适合多显示器或超宽屏场景。

---

## 🛠️ 二改核心亮点

### 1. 双击无感静默运行（零 CMD 黑框）
在底层 C 语言客户端入口 (`app/src/main.c`) 中打入了控制台智能调度补丁：
- 当用户双击 `scrcpy.exe` 或通过 GUI 控制台拉起时，自动隐藏控制台黑框窗口，告别突兀黑框。
- 当用户在终端中输入参数执行时，完整保留控制台输出与日志打印。

### 2. 投屏窗口内嵌导航栏（C/SDL3 原生绘制）
- 在 `app/src/navbar.c` 中实现：底部预留条带 + 矢量图标绘制 + 鼠标命中测试 + `INJECT_KEYCODE` 控制消息下发。
- 返回 / 主页 / 最近任务 / 音量± / 电源一键直达，全屏下同样可用，DPI 自适应。

### 3. 独立纯净 GUI 工作台（零运行依赖）
- 界面基于 C# 纯代码绘制，调用 Windows 原生 GDI+ 与底层 API 渲染，**无需安装臃肿的第三方运行时或大型 IDE**。
- 内置自动化编译脚本，借助 Windows 预装的 `.NET Framework` 即可实现秒级极速编译。

---

## 🚀 快速上手

### 方式一：直接运行成品
1. 解压便携包，保持 `ScrcpyGUI.exe` 与 `core\` 目录同级（引擎已内置在 `core\`）；
2. 手机开启 **开发者选项** 与 **USB 调试** 并连接电脑；
3. 双击打开 `ScrcpyGUI.exe`，点击 **「🚀 启动超清投屏」** 即可。

### 方式二：从源码编译 GUI

#### 环境要求
- Windows 10 / 11
- 已自带 .NET Framework 4.x（Windows 自带，无需额外安装）

#### 编译步骤
```cmd
# 1. 克隆本仓库
git clone https://github.com/angusdevgo/scrcpy-studio.git
cd scrcpy-studio/scrcpy-gui

# 2. 运行一键编译脚本
build_gui.bat
```
编译完成后，会在当前目录下生成独立的 `ScrcpyGUI.exe`。

### 方式三：从源码编译 scrcpy.exe（含控制台隐藏补丁）

#### 环境要求
- Windows 10 / 11 x64
- [MSYS2](https://www.msys2.org/)（UCRT64 环境）

#### 编译步骤
在 MSYS2 **UCRT64** 终端中执行：
```bash
# 1. 安装工具链与依赖（SDL3 / FFmpeg / libusb 版本与官方发布版一致）
pacman -S --needed mingw-w64-ucrt-x86_64-gcc mingw-w64-ucrt-x86_64-meson \
                 mingw-w64-ucrt-x86_64-ninja mingw-w64-ucrt-x86_64-pkgconf \
                 mingw-w64-ucrt-x86_64-sdl3 mingw-w64-ucrt-x86_64-ffmpeg \
                 mingw-w64-ucrt-x86_64-libusb

# 2. 配置并编译（仅客户端，portable 模式）
cd /c/path/to/scrcpy-studio
meson setup build-win64 --buildtype=release --strip -Db_lto=true \
      -Dcompile_server=false -Dportable=true
meson compile -C build-win64
```
产物位于 `build-win64/app/scrcpy.exe`。

> 注：`scrcpy-server` 需使用与客户端同版本的官方文件（本仓库便携包已内置 v5.0.1），
> 服务端需 JDK + Android SDK 才能从源码构建，日常二改无需重新编译。
> 构建验证：`objdump -p build-win64/app/scrcpy.exe | grep -i GetConsoleWindow` 应能查到导入符号。

---

## ⌨️ 投屏快捷键指南

在投屏窗口中操作时，可使用以下快捷键（默认快捷键修饰键为 `Alt` 或 `Ctrl`）：

| 快捷键 | 功能说明 |
|:---|:---|
| `Mod + F` | 切换窗口全屏 / 退出全屏 |
| `Mod + G` | 恢复 1:1 像素完美显示比例 |
| `Mod + W` | 调整窗口大小以适应画面黑边 |
| `Mod + H` | 模拟按下手机 Home 键 |
| `Mod + B` / `Mod + Backspace` | 模拟按下手机 返回（Back）键 |
| `Mod + S` | 模拟打开 多任务切换（App Switch）|
| `Mod + P` | 模拟电源键（开/关屏幕） |
| `Mod + O` | 关闭手机物理屏幕（保持电脑投屏继续） |
| `Mod + Shift + O` | 重新点亮手机物理屏幕 |
| `Mod + ↑ / ↓` | 调节手机音量增减 |
| `Mod + R` | 画面旋转 90 度 |
| `Ctrl + V` | 将电脑剪贴板文字直接粘贴输入到手机 |

---

## 📜 开源协议

本项目遵循 [Apache License 2.0](LICENSE) 开源协议。
底层投屏技术归属于原作者 [Genymobile](https://github.com/Genymobile/scrcpy)。
