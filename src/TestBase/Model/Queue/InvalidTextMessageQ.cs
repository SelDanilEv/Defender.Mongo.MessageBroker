namespace TestBase.Model.Queue;

public record InvalidTextMessageQ : TextMessageQ
{
    public InvalidTextMessageQ()
    {
    }

    public InvalidTextMessageQ(string text) : base(text)
    {
    }

    public string? InvalidText { get; set; }
}
