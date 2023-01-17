# Saladim.QBot
<br>
<div align="center">
一个基于<a href="https://github.com/Mrs4s/go-cqhttp">go-cqhttp</a>的QQ机器人.net框架  

![LICENSE](https://img.shields.io/github/license/Saladim-org/Saladim.QBot)

</div>
<br>



## 文档: 请见仓库 [Saladim.QBot-docs](https://github.com/saladim-org/Saladim.QBot-docs)

## 介绍

### 开源协议
以MIT协议开源, 如使用请标注来源

### 一些情况简述
<br>
目前框架仍为完工,目前未来计划如下:

- [x] 完成基层, 如go-cqhttp的所有post支持,所有api支持
- [x] 完成基础应用层, 例如实现一个GroupUser类实现IUser接口,
包含`SendPrivateMessage`方法、`Name`,`NickName`等属性这一类(类似于隔壁[Discord.net](https://github.com/discord-net/Discord.Net))
- [ ] 完成高级应用层, 例如消息/命令解析处理管道等

~~暂时就只能想到这些了, 作为一个废物能活到我做完上面这三项就不错了~~

### 目前支持情况

否 `×`, 部分支持`_`, 大部分支持`*`, 完全支持`√`

| 昵称         | 支持情况 |
| :----------- | :------: |
| CQ码         |    *     |
| 消息实体     |    √     |
| 全部上报     |    √     |
| Api调用      |    *     |
| 消息处理管线 |    √     |
| 指令解析管线 |    ×     |
| 频道支持     |    ×     |
| 实体         |    *     |
| 消息构建器   |    √     |
| 消息转发     |    √     |

- 上述大部分支持会随实体的需求而支持, 例如如果`GroupUser`新增了`BanAsync`方法, 那么同步地, `BanGroupUserAction`这个api会被支持

## 支持计划

### 什么不会被支持?
大部分qq的协议将会尽全力支持, 但是有以下内容因为某些原因或种种因素将不会被主动支持:

- bot号主动发起临时会话
    - 引起骚扰问题
    - 可能会导致账号被冻结([详见issue](https://github.com/Mrs4s/go-cqhttp/issues/1331#issuecomment-1020001951))
- 红包相关协议
    - 敏感行为不被支持
- 获取临时会话消息信息
    - 可能原因同第一项
    - go-cqhttp中没有将临时会话消息入数据库(可能是因为某些原因)

### 近期正在计划什么?
- [x] 完善最后的`request`的接收和处理
- [x] 新开一个简易的指令框架
- [ ] 支持从CQ码字符串到消息链的转换
- [x] 实现Session的概念

### 什么不会计划在近期支持?

- [ ] 群匿名消息收发
    - 暂未想到如何整合到`message`这个实体中

## 仓库简述
1. `EmptyTest`项目是一些实验原生.net/C#的一些特性,与本项目无关  
2. `SaladimQBot.Core`项目是Saladim.QBot的基础应用层的抽象,
*将* 包含`IUser`,`IClient`等接口或抽象类
3. `SaladimQBot.GoCqHttp`项目是`Core`项目的go-cqhttp实现, *未来**可能**会加入mirai的实现*, 正在支持Emit发射虚拟Post或重定向ApiCall的目标
4. `SaladimQBot.GoCqHttpTests`顾名思义是项目 3. 的单元测试,
大多使用Post/Api的虚拟重定向(即将原本被go-cqhttp的上报会被重定向至另一个模拟出来的),
`未来也可能加入压力测试等
5. `SaladimQBot.Shared`共享的一个库, 与项目大致无关, 仅包含极其简单的一些通用实现
6. `SaladimConsole` 目前 3. 正在使用的控制台调用测试,在这里你可以找到一些使用的例子
7. `SaladimOffbot` 与6差不多, 但是更加完善
8. 未来可能会加入的项目:
    - `SaladimQBot.Mirai` 见3.
    - `SaladimQBot.Extensions.MessagePipeline` 消息处理管线
    - `SaladimQBot.Extensions.CommandPipeline` 消息处理管线更高级封装, 包含简单指令的解析

## 联系我们
> 如果你贡献了任何代码你可以在这里写下你的名字及联系方式和少量描述

- Saplonily [Saplonily@outlook.com](mailto:Saplonily@outlook.com)
