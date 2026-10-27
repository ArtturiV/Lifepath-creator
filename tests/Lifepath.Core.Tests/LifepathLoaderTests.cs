using System.Text.Json;

namespace Lifepath.Core.Tests;

public class LifepathLoaderTests
{
    [Fact]
    public void FromJson_LoadsEveryCardType()
    {
        const string json = """
            {
              "title": "Every Card",
              "startCardId": "name",
              "cards": [
                { "type": "write-in", "id": "name", "title": "Name", "text": "", "nextCardId": "intro" },
                { "type": "text", "id": "intro", "title": "Intro", "text": "", "nextCardId": "choice" },
                {
                  "type": "choice", "id": "choice", "title": "Choice", "text": "",
                  "options": [ { "label": "Roll", "nextCardId": "die" } ]
                },
                {
                  "type": "die", "id": "die", "title": "Die", "text": "",
                  "outcomes": [ { "min": 1, "max": 6, "nextCardId": "end" } ]
                },
                { "type": "end", "id": "end", "title": "End", "text": "" }
              ]
            }
            """;

        var lifepath = LifepathLoader.FromJson(json);

        Assert.Equal("Every Card", lifepath.Title);
        Assert.IsType<WriteInCard>(lifepath.GetCard("name"));
        Assert.IsType<TextCard>(lifepath.GetCard("intro"));
        Assert.IsType<ChoiceCard>(lifepath.GetCard("choice"));
        Assert.IsType<EndCard>(lifepath.GetCard("end"));

        var die = Assert.IsType<DieCard>(lifepath.GetCard("die"));
        Assert.Equal(new DieOutcome(1, 6, "end"), Assert.Single(die.Outcomes));
    }

    [Fact]
    public void FromJson_UnknownCardType_Throws()
    {
        const string json = """
            { "title": "T", "startCardId": "a", "cards": [ { "type": "dice", "id": "a", "title": "A", "text": "" } ] }
            """;

        Assert.ThrowsAny<JsonException>(() => LifepathLoader.FromJson(json));
    }

    [Fact]
    public void FromJson_MissingTitle_Throws()
    {
        const string json = """
            { "title": "T", "startCardId": "a", "cards": [ { "type": "end", "id": "a", "text": "" } ] }
            """;

        Assert.ThrowsAny<JsonException>(() => LifepathLoader.FromJson(json));
    }

    [Fact]
    public void FromJson_InvalidLifepath_ThrowsValidationException()
    {
        const string json = """
            {
              "title": "T",
              "startCardId": "a",
              "cards": [
                { "type": "text", "id": "a", "title": "A", "text": "", "nextCardId": "missing" },
                { "type": "end", "id": "end", "title": "End", "text": "" }
              ]
            }
            """;

        var exception = Assert.Throws<LifepathValidationException>(() => LifepathLoader.FromJson(json));

        Assert.Contains(exception.Errors, error => error.Contains("links to 'missing'"));
    }
}