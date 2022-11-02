# Saladim.QBotDotnet
<br>
<div align="center">
一个基于<a href="https://github.com/Mrs4s/go-cqhttp">go-cqhttp</a>的QQ机器人.net框架
</div>
<br>

### 一些情况简述
<br>
目前框架仍为完工,目前未来计划如下(完成度截止2022-11-1):

- [ ] 完成基层, 如go-cqhttp的所有post支持,所有api支持
- [ ] 完成基础应用层, 例如实现一个GroupUser类实现IUser接口,
包含`SendPrivateMessage`方法、`Name`,`NickName`等属性这一类(类似于隔壁[Discord.net](https://github.com/discord-net/Discord.Net))
- [ ] 完成高级应用层, 例如消息/命令解析处理管道等

~~暂时就只能想到这些了, 作为一个废物能活到我做完上面这三项就不错了~~

因为<a href="https://github.com/Mrs4s/go-cqhttp">go-cqhttp</a>基于AGPL-3.0, 故选择[AGPL-3.0](LICENSE.txt)许可证

隔壁dev azure的仓库地址(?) -> https://dev.azure.com/Saplonily/_git/Saladim.QBotDotnet

### 目前支持情况
<p>否 × ,略微支持 _ ,部分支持 * ,支持 √</p>

| 昵称 | 支持情况 |
| :---: | :---: |
|全部CQ码|*|
|消息实体|√|
|全部上报|√|
|Api调用|_|
|消息处理管线|×|
|指令解析管线|×|
|频道支持|×|
|Emit虚拟Post|√|
|重定向ApiCall目标|×|

### 仓库简述
0. `EmptyTest`项目是一些实验原生.net/C#的一些特性,与本项目无关  
0. `Legacy.QBotDotnet.WebSocket`项目是~过期~的项目,因为封装不咋好被废弃,现在这个项目编译不会被通过
0. `QBotDotnet.Core`项目是QBotDotnet的基础应用层的抽象,
*将* 包含`IUser`,`IClient`等接口或抽象类
0. `QBotDotnet.GoCqHttp`项目是`Core`项目的go-cqhttp实现, *未来**可能**会加入mirai的实现*,
支持Emit发射虚拟Post或重定向ApiCall的目标
0. `QBotDotnet.GoCqHttpTests`顾名思义是项目 3. 的单元测试,
大多使用Post/Api的虚拟重定向(即将原本被go-cqhttp的上报会被重定向至另一个模拟出来的),
`未来也可能加入压力测试等
0. `QBotDotnet.SharedImplement`共享的一个库, 与项目大致无关, 仅包含极其简单的一些通用实现
0. `SaladQQBot` 1. 这个废弃项目的使用,与 1. 情况类似,行为表现不会正常
0. `Test` 目前 3. 正在使用的控制台调用测试,在这里你可以找到一些使用的例子
0. 未来可能会加入的项目:
    - `QBotDotnet.Mirai` 见3.
    - `QBotDotnet.Extension.MessagePipeline` 消息处理管线
    - `QBotDotnet.Extension.CommandPipeline` 消息处理管线更高级封装, 包含简单指令的解析
