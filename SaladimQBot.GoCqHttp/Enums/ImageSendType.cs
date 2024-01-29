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

/// <inheritdoc cref="Core.ImageSendSubType"/>
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