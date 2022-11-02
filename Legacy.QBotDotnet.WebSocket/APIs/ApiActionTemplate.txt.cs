using System.Text.Json.Serialization;

namespace QBotDotnet.CqWebSocket.Internal;

public class 名字 : CQApi
{
	[JsonIgnore]
	public override string ApiName { get => "yourapinamehere"; }

	[JsonPropertyName("group_id")]
	public Int64 GroupId { get; set; }

	[JsonPropertyName(" ")]
	public string Something { get; set; }

	public 名字(long groupId)
	{
		GroupId = groupId;
	}
}