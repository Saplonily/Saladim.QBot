namespace EmptyTest;

public class Cout
{
    public static ostream cout = new(Console.WriteLine);

    static void Main(string[] args)
    {
        _ = cout << "Hello World!";
    }
}

public class ostream
{
    private string innerString = "";
    private Action<string> outputAction;
    public ostream(Action<string> outputAction)
    {
        this.outputAction = outputAction;
    }

    public static ostream operator <<(ostream ostream, string str)
    {
        ostream.innerString += str;
        ostream.outputAction.Invoke(str);
        return ostream;
    }
}