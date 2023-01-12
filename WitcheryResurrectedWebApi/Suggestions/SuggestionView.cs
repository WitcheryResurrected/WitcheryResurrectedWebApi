using System.Collections.Generic;
using System.Linq;

namespace WitcheryResurrectedWebApi.Suggestions;

public class SuggestionView
{
    public int Id { get; }

    public ulong AuthorId { get; }
    public string AuthorName { get; }

    public ulong ThreadId { get; }
    public string Title { get; }
    public string Content { get; }

    public SuggestionState State { get; }
    public bool Deleted { get; }

    public List<SuggestionTagType> Tags { get; }
    public List<SuggestionModuleType> Modules { get; }

    public SuggestionView(Suggestion suggestion, SuggestionContent content)
    {
        Id = suggestion.Id;
        AuthorId = suggestion.CreatorId;
        AuthorName = content.CreatorName;
        ThreadId = suggestion.ThreadId;
        Title = content.Title;
        Content = content.Content;
        State = suggestion.State;
        Deleted = suggestion.DeletedAt != null;
        Tags = suggestion.Tags.Select(tag => tag.Type).ToList();
        Modules = suggestion.Modules.Select(tag => tag.Type).ToList();
    }
}