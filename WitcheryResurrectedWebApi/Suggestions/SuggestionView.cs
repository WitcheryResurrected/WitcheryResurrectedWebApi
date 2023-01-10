namespace WitcheryResurrectedWebApi.Suggestions;

public class SuggestionView
{
    public int Id { get; }
    public string AuthorName { get; }
    public string Content { get; }
    public int StateId { get; }

    public SuggestionView(int id, string authorName, string content, int stateId)
    {
        Id = id;
        AuthorName = authorName;
        Content = content;
        StateId = stateId;
    }
}
