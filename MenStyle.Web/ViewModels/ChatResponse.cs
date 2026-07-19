namespace MenStyle.Web.ViewModels;

public class ChatResponse
{
    public string Reply { get; set; } = string.Empty;

    public List<ChatProductSuggestion> Products { get; set; } = new();

    public List<ChatQuickReply> QuickReplies { get; set; } = new();

    public string ActionText { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;
}

public class ChatProductSuggestion
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string Price { get; set; } = string.Empty;

    public string OldPrice { get; set; } = string.Empty;

    public int SalePercent { get; set; }

    public string Url { get; set; } = string.Empty;
}

public class ChatQuickReply
{
    public string Text { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;
}