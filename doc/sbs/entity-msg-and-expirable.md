# Saladim.QBot docs
## 实体, 更多事件及可过期类型

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
在前面我们订阅了`OnMessageReceived`事件, 该事件在私聊, 群聊消息收到时都会触发, ,但是由于是更广泛的事件, 所以我们不能直接从这个事件中获取更详细的信息比如到底是群消息呢还是私聊消息呢, 群号是什么又或者私聊的具体的人是谁, 一般地我们可以订阅更加详细的事件, 比如说`OnGroupMessageReceived`事件, 像这样:
```cs
client.OnGroupMessageReceived += Client_OnGroupMessageReceived;

//函数签名如下:
void Client_OnGroupMessageReceived(GroupMessage message, JoinedGroup group)
```
当然地, `OnGroupMessageReceived`和`OnMessageReceived`会在群消息收到时同时触发, 所以你无需担心某个事件会不会被截断.
其中顾名思义`message`是对应的群消息, `group`是收到消息的群实体, 由于`message`类型升级为了`GroupMessage`, 所以我们拥有更多属性可以使用, 常用的大概会有:

- `Group`, 该消息所在群聊
- `Author`(别名`Sender`), 该消息的发送者(细化为`GroupUser`类型)

`Group`属性是个`JoinedGroup`实例, 所以我们可以显式的向群里发送消息而不是使用消息窗口:
```cs
message.Group.SendMessageAsync("你好这是一条在群里的消息");
```
`JoinedGroup`类型的实例`Group`有很多很实用的属性, 它们大概有:
- `CreateTime`, 群创建的时间, 获取失败时会是`DateTime.MinValue`
- `Name`, 群名
- `GroupId`, 群号
- `GroupLevel`, 群等级
- `MaxMembersCount`, 群最大成员数
- `MembersCount`, 群人数
- `Members`, 群成员集合

上述所有属性都是可过期类型, 所以你需要使用`Value`来获取它的值

然后是`GroupUser`类型的`message.Author`, 它是`User`类型的子类, 它包含一系列关于这个用户在群里的信息, 常见的大概有:

- `Card`, 群名片的值, 没设置时为`string.Empty`
- `LastMessageSentTime`, 最后一条消息的发送时间
- `JoinTime`, 加群时间
- `GroupRole`, 群角色, 枚举类型, 可为`Owner`,`Admin`,`Member`
- `GroupTitle`, 群头衔
- `GroupLevel`, 群等级
- `MuteExpireTime`, 禁言到期时间, 非禁言状态时值为`1970-01-01 上午 8:00:00
`
- `IsAbleToChangeCard`, 是否允许改变群名片
- `CardOrNickname`, 群名片或者昵称, 在群名片为空时返回`Nickname`
- `FullName`, 返回 "群名片(昵称, qq号)" 或名片为空时 "昵称(qq号)" 格式的字符串

上述属性除后三个外其余都是可过期类型

同样地, 你也可以订阅私聊信息:
```cs
void Client_OnPrivateMessageReceived(PrivateMessage message, User user)
```

此外还有更多很常见的事件, 比如:
- `OnMessageRecalled`, 消息被撤回
- `OnPrivateMessageRecalled`, 私聊消息被撤回
- `OnGroupMemberBanned`, 群成员被禁言
- `OnGroupMemberCardChanged`, 群成员名片变更
- `OnGroupFileUploaded`, 群文件上传
- `OnGroupMemberIncreased`, 群成员增加
- `OnGroupMemberLiftBan`, 群成员被解禁
- `OnGroupAdminChanged`, 群管理员变更
- `OnGroupEssenceAdded`, 群精华消息增加
- `OnOfflineFileReceived`, 收到私聊离线文件

以及其他更多事件

### 主动从client获取实体
大部分实体你都可以从`client`触发的事件中获取, 但有时可能你有这样的需求: 用户发送一条带qq号的消息, 然后bot发送一条带@的消息(我们还没开始了解如何构建一个复杂的消息, 你现在只需理解构建带@的消息需要一个`User`实体). 这时候我们没有从消息得来的实体, 所以我们要主动请求一个实体. 还记得之前的`CqClient`实例吗? 我们现在要再次使用它:
```cs
var someGroup = client.GetGroup(1145141919);
```
上述代码使用client获取了一个群实体, 其中群号是`1145141919`, 现在我们使用这个实体就能像之前操作接收事件时的操作一样了.
这是一些常见的获取实体操作:
- `GetGroupUser`, 获取一个群用户
- `GetJoinedGroup`, 获取一个bot号加入了的群
- `GetGroup`, 获取一个群, 允许bot不在群内
- `GetUser`, 获取一个用户

上述实体你均可以放心的储存它们, 并且随时使用`Equals`或`==`,`!=`来比较, 或使用`GetHashCode`, 它们都使用了判断是否id相等来重写它们.

### 可过期类型

这里只是简单说一下, 在应用层你不用很关心可过期类型内部是怎么工作的  
除了`Value`属性, 还有一个`ValueAsync`属性, 当使用`Value`属性取值时如果值过期会阻塞调用, 同时开始调用api来获取新值, 虽然通常这不会耗费多余1s的时间, 但是有时你可能需要异步的取值操作, 所以你可以使用`ValueAsync`来取值, 它会立即返回一个返回值为新值的`Task`, 所以你可以将方法设为异步方法并且使用`await`关键字来等待值.  
此外还有一些属性, 不过我们一般很少会见到它们:
- `ExpireTime`, 它表示这个值会在什么时候过期.
- `HasValueCache`, 表示是否拥有值缓存, 在取值完成后值会被缓存下来, 直到值过期时再次尝试获取值.
- `IsExpired`, 值是否过期
- `TimeSpanExpired`, 重获值后值的有效长度

最后修改: 2022-12-11 12:38:17