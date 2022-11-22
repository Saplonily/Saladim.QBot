namespace SaladimQBot.Core;

public enum ImageSendType
{
    Invalid,
    Normal,
    Flash,
    Show
}

public enum ImageSendSubType
{
    Invalid,
    Normal,
    Meme,
    HotMeme,
    MemeFight,
    WiseMeme,
    Sticker,
    Selfie,
    StickerAd,
    Unknown, //实际收发消息会出现,目前尚不明确复现方法
    //10 - 有待测试
    HotSearch
}

public enum ImageShowType
{
    Invalid,
    Normal,
    Phantom,
    Shake,
    Birthday,
    LoveYou,
    FindFriends
}