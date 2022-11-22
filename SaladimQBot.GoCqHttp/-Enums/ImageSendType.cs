namespace SaladimQBot.GoCqHttp;

public enum ImageSendType
{
    Invalid,
    [NameIn(-1)]
    Normal,
    [NameIn("flash")]
    Flash,
    [NameIn("show")]
    Show
}

public enum ImageSendSubType
{
    [NameIn(-1)]
    Invalid,
    [NameIn(0)]
    Normal,
    [NameIn(1)]
    Meme,
    [NameIn(2)]
    HotMeme,
    [NameIn(3)]
    MemeFight,
    [NameIn(4)]
    WiseMeme,
    [NameIn(7)]
    Sticker,
    [NameIn(8)]
    Selfie,
    [NameIn(9)]
    StickerAd,
    [NameIn(11)]
    Unknown, //实际收发消息会出现11,目前尚不明确复现方法,疑似特殊的动图?
    //10 - 有待测试
    [NameIn(13)]
    HotSearch
}

public enum ImageShowType
{
    [NameIn(-1)]
    Invalid,
    [NameIn(40000)]
    Normal,
    [NameIn(40001)]
    Phantom,
    [NameIn(40002)]
    Shake,
    [NameIn(40003)]
    Birthday,
    [NameIn(40004)]
    LoveYou,
    [NameIn(40005)]
    FindFriends
}