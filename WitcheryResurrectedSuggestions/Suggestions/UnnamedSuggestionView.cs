namespace WitcheryResurrectedSuggestions.Suggestions;

public class UnnamedSuggestionView
{
    public int Id { get; }
    public string AuthorId { get; }
    public string ThreadId { get; }
    public int StateId { get; }
    public bool Deleted { get; }

    public UnnamedSuggestionView(int id, Suggestion suggestion)
    {
        Id = id;
        AuthorId = suggestion.CreatorId.ToString();
        ThreadId = suggestion.ThreadId.ToString();
        StateId = suggestion.StateId;
        Deleted = suggestion.DeletedAt.HasValue;
    }
}
