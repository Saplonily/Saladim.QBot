# Saladim.QBot docs
## 实体及可过期类型

### 消息实体及从消息获取实体
在前面我们订阅了消息收到事件, 然后粗略的过了一下如何获取消息的一些内容, 现在让我们深入探索一下.  
```cs
private static void Client_OnMessageReceived(Message message)
{
    string s = $"{message.Author.Nickname.Value} 说: {message.MessageEntity.RawString}";
    Console.WriteLine(s);
}
```
首先我们来看`message`的`Author`属性(别名`Sender`), 顾名思义, 是这条消息的发送者, 属性的类型为`User`实体. 在这里, 我们将所有的`CqEntity`的子类叫做cq实体或简称实体, 它们都有一个公共的属性`Client`, 通过它我们可以知道哪些实体是从属于哪个client的. 回到`User`实体, 我们常用的属性大概是:
- `Nickname` 昵称
- `Age` 设置的年龄, 不可见时为0
- `Qid` 设置的qid
- `UserId` qq号

你可能注意到我们获取昵称时还要获取它的`Value`, 这是因为`Nickname`的值是一个*可过期类型*, 在代码里它是`Expirable<T>`泛型类的特化, 每次获取值时都会检查值是否过时了, 初始状态是过时状态, 当过时时获取值时会隐式通过client调用获取信息的api. 在这里`Nickname`就是`Expirable<string>`的类型, 一般来说我们只需要它的`Value`属性, 其他方法和属性我们之后再介绍, 现在我们知道我们可以通过`Value`获取值就行了. 当然地, 因为我们的可过期类型每次取值时会保证值尽可能的不过时, 同时我们重写了`User`的`==`运算符为判断qq号是否相等. 大部分实体都重写了`Equals`和`==`, 所以你可以放心的储存它们的引用和直接判等.  

然后是`message`的`MessageEntity`属性, 它储存了这个消息的内容主体, 内部维护了一个消息链和一段原始消息文本. 通过它的`RawString`属性我们可以获取原始消息文本, 比如"@Saplonily 我@你了"这个消息, 它开头是一个实体@, @到了一个人, 那么 获取到的原始消息会变成"[CQ:at,qq=2748166392] 我@你了", 我们称一段消息中的非文本部分为消息中的"非文本节点", 它和文本部分统称为"消息链节点", 节点在gocq底层中就体现为"CQ码". 比如上述的"[CQ:at,qq=2748166392] 我@你了"包含两个节点:

- at节点, 其中at的目标的qq号为"2748166392"
- 文本节点, 内容为" 我@你了"

因为CQ码需要"[,]"这三个字符进行转义, 所以如果文本节点中出现了这三个字符那么会被以HTML实体形式转义, 具体如下:
|字符|对应实体转义序列|
|:-:|:---------:|
| & | &amp;amp; |
| [ | &amp;#91; |
| ] | &amp;#93; |
| , | &amp;#44; |


有时候使用`RawString`属性能满足大部分需求, 但是可能有些时候你需要知道这段消息到底@了多少个人, 你总不可能自己写正则匹配所有[]中的内容, 所以这时候我们需要获取它的`Chain`属性, `Chain`属性内包含一个`MessageChainNodes`只读集合, 你可以像这样去找到所有的at节点:
```cs
var chainNodes = message.MessageEntity.Chain.MessageChainNodes;
var allAtNode =
    from node in chainNodes
    where node is MessageChainAtNode
    select node as MessageChainAtNode;
foreach(var atNode in allAtNode)
{
    Console.WriteLine($"{atNode.User.Nickname.Value}被@了");
}
```
现在向群里发送类似这种信息(注意实体@):
"@Saplonily @Saplonily @Saladim.bot @Saladim.bot @Saplonily"
日志大概会是这个样子(无需在意ApiCall的Debug日志输出):
```log
[Debug][Client/ApiCall] Ready for api 'get_stranger_info' call: {
  "user_id": 2748166392,
  "no_cache": true
}
Saplonily被@了
Saplonily被@了
[Debug][Client/ApiCall] Ready for api 'get_stranger_info' call: {
  "user_id": 2259113381,
  "no_cache": true
}
Saladim.bot被@了
Saladim.bot被@了
Saplonily被@了
```
去除掉我们现在还不关心的隐式api调用日志, 它是这个样子的:
```log
Saplonily被@了
Saplonily被@了
Saladim.bot被@了
Saladim.bot被@了
Saplonily被@了
```
很符合我们的预期

接下来, 你可能会觉得, 啊我获取at节点好难啊, 光是获取节点集合就要这么一长串的属性获取`message.MessageEntity.Chain.MessageChainNodes`, 之后甚至还搞了个LINQ表达式, 这多麻烦啊.  
是的, 所以我们给消息链实体提供了一系列的预制方法, 比如直接从消息链获取所有At节点:
```cs
var chain = message.MessageEntity.Chain;
var allAtNode = chain.AllAt();
```
可能你还是觉得好麻烦啊, 一般我们只是关心消息实体内容怎么样而不是深究消息链长什么样, 所以我们同样提供了这一系列的方法直接到消息实体上:
```cs
var entity = message.MessageEntity;
var allAtNode = entity.AllAt();
```
这样看上去就好看多了  
(注: at节点的`UserName`可空属性并不是用户名, 该属性仅是在发送这个消息时若对应用户不在群里时使用的备用昵称, 如果要获取用户昵称请使用`Nickname`属性)
最后, 一些常用的方法有:
- `AllAt()` 所有at节点
- `AllImage()` 所有image节点
- `FirstAt()` 第一个at节点, 如果没有则抛出异常
- `FirstAtOrNull()` 第一个at节点, 如果没有返回`null`
- `Mentioned(User user)` 该消息是否提及(at)了`user`这个用户
- `Replied(Message message)` 该消息是否回复了`message`这条消息


### 细分消息事件
在前面我们订阅了`OnMessageReceived`事件, 该事件在私聊, 群聊消息收到时都会触发, 但是由于是更广泛的事件, 所以我们不能直接从这个事件中获取更详细的信息, 一般我们可以订阅更加详细的事件, 比如说`OnGroupMessageReceived`事件, 像这样:
```cs
client.OnGroupMessageReceived += Client_OnGroupMessageReceived;

//函数签名如下:
void Client_OnGroupMessageReceived(GroupMessage message, JoinedGroup group)
```
其中顾名思义`message`是对应的群消息, `group`是收到消息的群实体
TODO ...
### 主动从client获取实体
TODO ...

### 可过期类型
TODO ...