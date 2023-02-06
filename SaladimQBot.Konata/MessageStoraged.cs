using System.Text.Json;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using SqlSugar;

namespace SaladimQBot.Konata;

[SugarTable("message_storaged")]
internal class MessageStoraged
{
    [SugarColumn(ColumnName = "msg_id", IsNullable = false, IsPrimaryKey = true)]
    public int MessageId { get; set; }

    [SugarColumn(ColumnName = "content", IsNullable = false)]
    public string Content { get; set; } = null!;

    [SugarColumn(ColumnName = "group_id", IsNullable = true, DefaultValue = null)]
    public uint? GroupId { get; set; }

    [SugarColumn(ColumnName = "group_name", IsNullable = true, DefaultValue = null)]
    public string? GroupName { get; set; }

    [SugarColumn(ColumnName = "user_id", IsNullable = false)]
    public uint UserId { get; set; }

    [SugarColumn(ColumnName = "member_card", IsNullable = true, DefaultValue = null)]
    public string? MemberCard { get; set; }
}
