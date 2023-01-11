namespace Saladim.Offbot.Services;

public class RandomService
{
    public Random Random { get; protected set; }

    public RandomService()
    {
        var dateTimeNow = DateTime.Now;
        Random = new(dateTimeNow.Millisecond + dateTimeNow.Second + dateTimeNow.Day + dateTimeNow.Minute);
    }
}
