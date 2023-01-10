using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WitcheryResurrectedSuggestions.Suggestions;

public class SuggestionState
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class SuggestionTagType
{
    public ulong Id { get; set; }
    public string Name { get; set; }
}

[PrimaryKey(nameof(SuggestionId), nameof(TypeId))]
public class SuggestionTag
{
    public int SuggestionId { get; set; }
    public virtual Suggestion Suggestion { get; }

    public ulong TypeId { get; set; }
    public virtual SuggestionTagType Type { get; }
}

public class SuggestionModuleType
{
    public ulong Id { get; set; }
    public string Name { get; set; }
}

[PrimaryKey(nameof(SuggestionId), nameof(TypeId))]
public class SuggestionModule
{
    public int SuggestionId { get; set; }
    public virtual Suggestion Suggestion { get; }

    public ulong TypeId { get; set; }
    public virtual SuggestionModuleType Type { get; }
}

public class Suggestion
{
    public int Id { get; set; }
    public ulong ThreadId { get; set; }
    public ulong CreatorId { get; set; }

    public int StateId { get; set; }
    public virtual SuggestionState State { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual List<SuggestionTag> Tags { get; }

    public virtual List<SuggestionModule> Modules { get; }
}

public class SuggestionContent
{
    public int Id { get; set; }
    public string CreatorName { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}
