# Saladim.QBot docs

## go-cqhttp环境配置

本框架依赖go-cqhttp(下文简称gocq)以接受和处理来自qq的消息.
你可以到go-cqhttp的[github页面](https://github.com/Mrs4s/go-cqhttp/releases/tag/v1.0.0-rc3)上下载. 截止目前它的最新版本是v1.0.0-rc3.

大部分内容已在fast-start.md上写明, 这里是引用:

>**go-cqhttp**  
>go-cqhttp是一个基于 Mirai 以及 MiraiGo 的 OneBot Golang 原生实现, 
>本框架使用它来接收来自qq的消息及调用响应api, 
>截止2022-11-26, 测试使用的go-cqhttp版本为v1.0.0-rc3, 你可以在[Github >Release](https://github.com/Mrs4s/go-cqhttp/releases/tag/v1.0.0-rc3)>上下载它  
>
>下载下来后, 使用命令行打开它, 会提示缺少配置文件, 此时按照需要选择通讯方式, 本>框架目前只支持http协议和正向websocket协议, 所以请选择02
>![config](../config-show.png)
>过后目录下会生成`config.yml`文件, 手动配置如下:
>
>必须步骤:
>- 在如下的位置的uin与password处填写bot号的账号与密码(或为空使用扫码登录)
>```yml
>account: # 账号相关
>  uin: 1233456 # QQ账号
>  password: '' # 密码为空时使用扫码登录
>```
>- 更改如下消息上报数据类型为`array`
>message:
>```yml
>  post-format: array
>```
>- 更改并记住所需的http或websocket地址与端口如下(此时http通讯地址为`http://>127.0.0.1:5700`,反向http地址为`http://127.0.0.1:5701`,正向websocket地>址为`ws://127.0.0.1:8080`):
>```yml
>  - http: # HTTP 通信设置
>      address: 127.0.0.1:5700 # HTTP监听地址
>
>      post:           # 反向HTTP POST地址列表
>      - url: http://127.0.0.1:5701/ # 地址
>
>  # 正向WS设置
>  - ws:
>      # 正向WS服务器监听地址
>      address: 127.0.0.1:8080
>
>```
>
>现在重新使用命令行打开, 进行所需的登录步骤, 此时跟随引导即可.
>如果看到go-cqhttp内接收消息与bot qq号的同步那么说明配置完成.

其中仍需赘述下, 在末尾的这里:
```yml
  - http: # HTTP 通信设置
      address: 127.0.0.1:5700 # HTTP监听地址

      post:           # 反向HTTP POST地址列表
      - url: http://127.0.0.1:5701/ # 地址

  # 正向WS设置
  - ws:
      # 正向WS服务器监听地址
      address: 127.0.0.1:8080

```
如果你打算使用http通讯, 不打算使用webSocket时你可以将ws项直接删除, 不过我们推荐你使用webSocket通讯, 这样你可以知道何时gocq异常发生并结束或者何时bot号离线等. 使用http通讯时只有当主动发起api请求时才会的值gocq端异常. 此时你同样的可以将http项删除, 避免gocq反向http post时发生的一系列上报失败警告.

其中在`CqHttpClient`类的构造器里的requestUrl指api请求的根地址, 如上配置文件则为`http://127.0.0.1:5700`, listenerUrl指介绍上报的根地址, 如上述配置文件为`http://127.0.0.1:5701`

最后修改: 2022-12-10 17:26:17