<p align="center">
    <img alt="Serein" src="https://socialify.git.ci/Zaitonn/Serein/image?description=1&descriptionEditable=%E6%96%B0%E6%97%B6%E4%BB%A3%E6%9E%81%E7%AE%80%E6%9C%8D%E5%8A%A1%E5%99%A8%E9%9D%A2%E6%9D%BF&font=KoHo&logo=https%3A%2F%2Fzaitonn.github.io%2FSerein%2FSerein.png&owner=1&pattern=Circuit%20Board&theme=Light">
    <br>
    <img alt="GitHub Stars" src="https://img.shields.io/github/stars/Zaitonn/Serein?color=blue">
    <a href="https://github.com/Zaitonn/Serein/releases/latest">
        <img src="https://img.shields.io/github/v/release/Zaitonn/Serein?color=blue">
    </a>
    <a href="https://github.com/Zaitonn/Serein/releases/latest">
        <img alt="GitHub all releases" src="https://img.shields.io/github/downloads/Zaitonn/Serein/total?color=blue">
    </a>
    <a href="https://github.com/Zaitonn/Serein/actions/workflows/Build.yml">
    <img alt="GitHub bulid" src="https://img.shields.io/github/workflow/status/Zaitonn/Serein/Build/main?color=blue">
    </a>
    <a href="https://github.com/Zaitonn/Serein">
        <img alt="GitHub repo file count" src="https://img.shields.io/github/languages/code-size/Zaitonn/Serein">
    </a>
    <a href="https://www.codefactor.io/repository/github/zaitonn/serein">
        <img alt="CodeFactor Grade" src="https://img.shields.io/codefactor/grade/github/Zaitonn/Serein/main?color=blue">
    </a>
</p>


- 一个基于`.NET 6`和`NET Framework 4.7.2`的新时代我的世界极简服务器面板 
- **下载最新版：[https://github.com/Zaitonn/Serein/releases/latest](https://github.com/Zaitonn/Serein/releases/latest)**
- Github仓库：[https://github.com/Zaitonn/Serein](https://github.com/Zaitonn/Serein)
- Minebbs：[https://www.minebbs.com/resources/serein.4169/](https://www.minebbs.com/resources/serein.4169/)
- 查看 **[教程](Tutorial.md)** 丨 __[帮助](Help.md)__

---

- [🖥运行环境 Environment](#运行环境-environment)
    - [缺少运行库？](#缺少运行库)
- [💖特点 Feature](#特点-feature)
  - [控制面板](#控制面板)
    - [简洁的状态信息显示](#简洁的状态信息显示)
    - [方便的一键控制按钮](#方便的一键控制按钮)
    - [简单且可自定义的控制台](#简单且可自定义的控制台)
    - [日志系统](#日志系统)
    - [后台运行](#后台运行)
  - [插件管理](#插件管理)
  - [正则](#正则)
  - [定时任务](#定时任务)
  - [机器人](#机器人)
  - [成员管理](#成员管理)
  - [设置](#设置)
- [✨第三方插件/整合包 Plugin](#第三方插件整合包-plugin)
- [💖发电 Donate](#发电-donate)
- [📒关于 About](#关于-about)

---

## 🖥运行环境 Environment
Win7或WinServer2012以上

> 更低的系统版本不保证能稳定运行，必要时务必备份数据  

> x86理论可正常运行，但考虑到bds好像不能在x86下运行(？)，故还是推荐64位

#### 缺少运行库？
[下载 .NET 6.0 桌面运行时](https://dotnet.microsoft.com/download/dotnet/6.0/runtime/desktop/x64)  
[下载 .NET Framework运行时](https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net472)

## 💖特点 Feature
### 控制面板
![控制台](imgs/console.png)
#### 简洁的状态信息显示
- 显示有关服务器的主要信息（状态、版本、难度、存档名称、运行时长和启动进程的CPU占用率），快捷了解当前服务器状态
- 理论上可适配所有服务器（包括BE和JE服务器），若遇无法识别可以自行修改配置文件

#### 方便的一键控制按钮
- 包括大部分日常使用的操作，帮助您一键管理服务器

#### 简单且可自定义的控制台
- 使用自研的正则匹配原始输出中的彩色样式，并使用内嵌浏览器渲染输出
- 还有`语法高亮`的显示主题可供选择！可自动用不同颜色高亮`Info`、`Error`、`Warning`、插件前缀和长数字
- 得益于使用内嵌浏览器渲染控制台，故您可以在`./console`文件夹中自定义背景颜色、高亮颜色、高亮内容、字体大小颜色等风格

  
> 可能需要一点点HTML5、CSS3和JS的入门技巧

#### 日志系统

> 此功能需在设置中开启

- 自动将控制台中的输出输出到日志文件，不用担心后台刷屏找不到想要的信息了

#### 后台运行
![后台运行提示](imgs/protect.png)

- 当服务器运行时关闭Serein将最小化至托盘，再也不用怕点错当场跑路

### 插件管理
![插件管理](imgs/plugin.png)
- 自动识别插件文件夹，以分组方式列出所有的插件
- 一键导入插件，帮助小白服主解决导入难题
- 快捷启用/禁用插件，快速排除问题和更新服务器
- 直达文件夹，再也不用怕插件太多找不到插件在哪
  
### 正则
![正则](imgs/regex.png)

- 简单易上手的正则表达式和[命令系统](Command.md)，可以执行命令、发送消息（需要配置机器人）甚至备份存档（基于cmd，需要自行下载7z）等高级功能
- 通过此功能还能实现自动回复，与群信息互通和状态输出等功能
- 备注功能，再也不用担心写的是啥
- 简单明了的配置文件，可快捷分享或导入

### 定时任务
![定时任务](imgs/task.png)

- 简单易上手的Cron表达式和[命令系统](Command.md)，可以执行命令、发送消息（需要配置机器人）甚至备份存档（基于cmd命令行，需要自行下载7z）等高级功能
- 通过此功能还能实现定时命令、准点报时和定时备份等功能
- 自动提示下一次执行时间，再也不用怕搞出逆天的表达式了
- 备注功能，再也不用担心写的是啥
- 简单明了的配置文件，可快捷分享或导入

### 机器人
![机器人](imgs/bot.png)

- 通过Websocket连接本地的机器人（需要自行配置）实现消息收发功能
- 允许多开`Serein`实现群组服的消息互通功能
- 显示状态和其他统计信息，帮助您了解运行状态
- 可以直接通过扣扣控制服务器或消息互通（需要自行配置）

### 成员管理
![成员管理](imgs/members.png)
- 将用户的游戏ID与QQ号绑定，有效防止熊孩子使用小号作恶
- 一键删除/修改绑定用户，快捷管理

### 设置
![设置](imgs/setting.png)
> 设置内容因版本不同而有所差别

- 提供尽可能的的设置，满足不同人的使用需求
- 将鼠标指针放在不同设置上方，可查看关于这一项的详细提示

## ✨第三方插件/整合包 Plugin
> **选配选配选配选配选配选配选配选配选配选配选配**  
> 你需要自行判断其安全性和可用性，使用时出现问题与`Serein`无关

- [Serein机器人整合包](https://www.minebbs.com/threads/serein.12192/)
- [Serein - 新的机器人内置常用正则](https://www.minebbs.com/resources/serein.4204/)
- [PSerein - 机器人辅助插件](https://www.minebbs.com/resources/pserein.4211/)


## 💖发电 Donate
>如果觉得这个软件对你很有帮助的话可以稍微鼓励一下  
我会很感谢你的(*￣3￣)╭❤

[![爱发电](imgs/afdian.png)](https://afdian.net/@Zaiton)

| Supporters | 发电金额 |日期|
|---- |----: |----|
| [binggggg](https://www.minebbs.com/members/binggggg.12096/)| ￥20.00 | 2022.7.10|
| [binggggg](https://www.minebbs.com/members/binggggg.12096/)| ￥20.00 | 2022.7.9|

## 📒关于 About
> [About](About.md)
