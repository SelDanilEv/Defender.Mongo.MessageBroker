namespace TestBase.Model.Topic;

public record InvalidTextMessageT : TextMessageT
{
    public InvalidTextMessageT()
    {
    }

    public InvalidTextMessageT(string text) : base(text)
    {
    }

    public string? InvalidText { get; set; }
}
