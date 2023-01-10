using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WitcheryResurrectedSuggestions.Suggestions;

namespace WitcheryResurrectedSuggestions.Controllers;

[Route("suggestions")]
public class SuggestionsController : Controller
{
    private readonly IConfigurationManager _configurationManager;

    public SuggestionsController(IConfigurationManager configurationManager) =>
        _configurationManager = configurationManager;

    [HttpPost("add")]
    public async Task<ActionResult<int>> AddSuggestion([FromBody] Add add)
    {
        if (!await _configurationManager.IsAuthenticated(add.Pass)) return StatusCode(401);

        await using var context = new SuggestionsContext();

        var suggestion = new Suggestion
        {
            CreatorId = add.CreatorId,
            ThreadId = add.ThreadId,
            StateId = 1
        };

        await context.Suggestions.AddAsync(suggestion);

        await context.SaveChangesAsync();

        await context.SuggestionContent.AddAsync(new SuggestionContent
        {
            Id = suggestion.Id,
            Title = add.Title,
            Content = add.Content,
            CreatorName = add.CreatorName
        });

        return suggestion.Id;
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<UnnamedSuggestionView>> DeleteSuggestion([FromRoute] int id, [FromBody] string? pass)
    {
        if (!await _configurationManager.IsAuthenticated(pass)) return StatusCode(401);

        await using var context = new SuggestionsContext();

        var suggestion = await context.Suggestions.FindAsync(id);
        if (suggestion == null) return StatusCode(401);

        suggestion.DeletedAt = DateTime.Now;

        await context.SaveChangesAsync();

        return new UnnamedSuggestionView(id, suggestion);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<UnnamedSuggestionView>> UpdateSuggestion([FromRoute] int id,
        [FromBody] Update update)
    {
        if (!await _configurationManager.IsAuthenticated(update.Pass)) return StatusCode(401);

        await using var context = new SuggestionsContext();

        var suggestion = await context.Suggestions.FindAsync(id);

        if (suggestion == null) return StatusCode(404);

        suggestion.StateId = update.StateId;

        await context.SaveChangesAsync();

        return new UnnamedSuggestionView(id, suggestion);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UnnamedSuggestionView>> ById([FromRoute] int id)
    {
        await using var context = new SuggestionsContext();
        var suggestion = await context.Suggestions.FindAsync(id);

        if (suggestion == null) return StatusCode(404);

        return new UnnamedSuggestionView(id, suggestion);
    }

    [HttpGet("by_thread/{threadId}")]
    public async Task<ActionResult<UnnamedSuggestionView>> ByMessage([FromRoute] ulong threadId)
    {
        await using var context = new SuggestionsContext();
        var suggestion = await context.Suggestions.FirstOrDefaultAsync(suggestion => suggestion.ThreadId == threadId);

        if (suggestion == null) return StatusCode(404);
        return new UnnamedSuggestionView(suggestion.Id, suggestion);
    }

    [HttpGet("by_author/{authorId}")]
    public async Task<IEnumerable<UnnamedSuggestionView>> ByAuthor([FromRoute] ulong authorId)
    {
        await using var context = new SuggestionsContext();

        return (
            from suggestion in context.Suggestions
            where suggestion.CreatorId == authorId
            select new UnnamedSuggestionView(suggestion.Id, suggestion)
        ).ToList();
    }

    [HttpGet]
    public async Task<IEnumerable<SuggestionView>> GetSuggestions([FromQuery] string? last)
    {
        int? lastId;
        if (last == null)
        {
            lastId = null;
        }
        else
        {
            if (!int.TryParse(last, out var id)) return Enumerable.Empty<SuggestionView>();
            lastId = id;
        }

        await using var context = new SuggestionsContext();

        var suggestions = lastId.HasValue
            ? from suggestion in context.Suggestions where suggestion.Id > lastId select suggestion
            : context.Suggestions;

        return (
                from suggestion in suggestions
                join content in context.SuggestionContent on suggestion.Id equals content.Id
                select new SuggestionView(suggestion.Id, content.CreatorName, content.Content, suggestion.StateId))
            .Take(10)
            .ToList();
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Add
    {
        public ulong CreatorId { get; set; }
        public ulong ThreadId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatorName { get; set; }
        public string? Pass { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Update
    {
        public int StateId { get; set; }
        public string? Pass { get; set; }
    }
}