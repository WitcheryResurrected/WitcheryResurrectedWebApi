using Microsoft.EntityFrameworkCore;

namespace WitcheryResurrectedWebApi.Suggestions;

public class SuggestionsContext : DbContext
{
    public DbSet<SuggestionState> SuggestionStates { get; set; }
    public DbSet<Suggestion> Suggestions { get; set; }
    public DbSet<SuggestionTagType> SuggestionTagTypes { get; set; }
    public DbSet<SuggestionTag> SuggestionTags { get; set; }
    public DbSet<SuggestionModuleType> SuggestionModuleTypes { get; set; }
    public DbSet<SuggestionModule> SuggestionModules { get; set; }
    public DbSet<SuggestionContent> SuggestionContent { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options
        .UseLazyLoadingProxies()
        .UseSqlite("Data Source=suggestions.sqlite")
        .UseSnakeCaseNamingConvention();
}
