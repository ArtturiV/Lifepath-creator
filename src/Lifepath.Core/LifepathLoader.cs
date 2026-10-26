using System.Text.Json;

namespace Lifepath.Core;

public static class LifepathLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public static LifepathDefinition FromFile(string path) => FromJson(File.ReadAllText(path));

    public static LifepathDefinition FromJson(string json)
    {
        var file = JsonSerializer.Deserialize<LifepathFile>(json, Options)
            ?? throw new JsonException("The lifepath file is empty.");

        return new LifepathDefinition(file.Title, file.StartCardId, file.Cards);
    }
}

// Mirrors the JSON file's structure. Only used while loading.
internal sealed record LifepathFile(string Title, string StartCardId, IReadOnlyList<Card> Cards);