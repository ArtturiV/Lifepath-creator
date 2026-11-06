using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lifepath.Core;

// Reads a die outcome written either as { "roll": 3, ... } or as { "min": 1, "max": 2, ... }.
internal sealed class DieOutcomeJsonConverter : JsonConverter<DieOutcome>
{
    public override DieOutcome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Deserialize<DieOutcomeJson>(ref reader, options)
            ?? throw new JsonException("A die outcome cannot be null.");

        return json switch
        {
            { Roll: { } roll, Min: null, Max: null } => new DieOutcome(roll, roll, json.NextCardId),
            { Roll: null, Min: { } min, Max: { } max } => new DieOutcome(min, max, json.NextCardId),
            _ => throw new JsonException(
                $"The die outcome leading to '{json.NextCardId}' needs either \"roll\" or both \"min\" and \"max\"."),
        };
    }

    public override void Write(Utf8JsonWriter writer, DieOutcome value, JsonSerializerOptions options) =>
        throw new NotSupportedException("Writing lifepaths to JSON is not supported.");

    private sealed record DieOutcomeJson(string NextCardId, int? Roll = null, int? Min = null, int? Max = null);
}