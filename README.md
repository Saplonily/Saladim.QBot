# Saladim.QBot
<br>
<div align="center">
一个基于<a href="https://github.com/Mrs4s/go-cqhttp">go-cqhttp</a>的QQ机器人.net框架
</div>
<br>

## 快速开始
> **见[doc/fast-start.md](./doc/fast-start.md)**

## 介绍

### 一些情况简述
<br>
目前框架仍为完工,目前未来计划如下:

- [x] 完成基层, 如go-cqhttp的所有post支持,所有api支持
- [x] 完成基础应用层, 例如实现一个GroupUser类实现IUser接口,
包含`SendPrivateMessage`方法、`Name`,`NickName`等属性这一类(类似于隔壁[Discord.net](https://github.com/discord-net/Discord.Net))
- [ ] 完成高级应用层, 例如消息/命令解析处理管道等

~~暂时就只能想到这些了, 作为一个废物能活到我做完上面这三项就不错了~~

因为<a href="https://github.com/Mrs4s/go-cqhttp">go-cqhttp</a>基于AGPL-3.0, 故选择[AGPL-3.0](LICENSE.txt)许可证

### 目前支持情况
<p>否 × ,略微支持 _ ,部分支持 * ,支持 √</p>

| 昵称 | 支持情况 |
| :---: | :---: |
|全部CQ码|*|
|消息实体|√|
|全部上报|√|
|Api调用|*|
|消息处理管线|×|
|指令解析管线|×|
|频道支持|×|
|Emit虚拟Post|√|
|重定向ApiCall目标|_|
|类似discord.net的实体|*|

- 上述大部分支持会随实体的需求而支持, 例如如果`GroupUser`新增了`BanAsync`方法, 那么同步地, `BanGroupUserAction`这个api会被支持


### 仓库简述
1. `EmptyTest`项目是一些实验原生.net/C#的一些特性,与本项目无关  
2. `SaladimQBot.Core`项目是Saladim.QBot的基础应用层的抽象,
*将* 包含`IUser`,`IClient`等接口或抽象类
3. `SaladimQBot.GoCqHttp`项目是`Core`项目的go-cqhttp实现, *未来**可能**会加入mirai的实现*, 正在支持Emit发射虚拟Post或重定向ApiCall的目标
4. `SaladimQBot.GoCqHttpTests`顾名思义是项目 3. 的单元测试,
大多使用Post/Api的虚拟重定向(即将原本被go-cqhttp的上报会被重定向至另一个模拟出来的),
`未来也可能加入压力测试等
5. `SaladimQBot.Shared`共享的一个库, 与项目大致无关, 仅包含极其简单的一些通用实现
6. `Saladim` 目前 3. 正在使用的控制台调用测试,在这里你可以找到一些使用的例子
7. `Saladim.Wpf` 与6差不多, 但是是使用wpf构建的gui项目
8. 未来可能会加入的项目:
    - `SaladimQBot.Mirai` 见3.
    - `SaladimQBot.Extensions.MessagePipeline` 消息处理管线
    - `SaladimQBot.Extensions.CommandPipeline` 消息处理管线更高级封装, 包含简单指令的解析

目前TODO:
- [x] 完成应用层(即IUser,IMessage,IGroupMessage等的实现)
- [ ] 更深层次的应用层

### 联系我们
> 如果你贡献了任何代码你可以在这里写下你的名字及联系方式和少量描述

- Saplonily [Saplonily@outlook.com](mailto:Saplonily@outlook.com)
