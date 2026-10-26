using System.Text.Json;
using Lifepath.Core;

var path = args.Length > 0 ? args[0] : "lifepaths/test.json";

LifepathDefinition lifepath;
try
{
    lifepath = LifepathLoader.FromFile(path);
}
catch (Exception ex) when (ex is IOException or JsonException)
{
    Console.WriteLine($"Could not load '{path}': {ex.Message}");
    return;
}

Console.WriteLine($"=== {lifepath.Title} ===");
Console.WriteLine($"Starts at: {lifepath.StartCard.Title}");
Console.WriteLine();

foreach (var card in lifepath.Cards)
{
    Console.WriteLine($"[{card.Id}] {card.Title}: {Describe(card)}");
}

var seaCard = (DieCard)lifepath.GetCard("sea");
var roll = Random.Shared.Next(1, 7);
Console.WriteLine();
Console.WriteLine($"Rolling for '{seaCard.Title}': {roll} -> {seaCard.NextCardIdFor(roll)}");

string Describe(Card card) => card switch
{
    TextCard text => $"next -> {text.NextCardId}",
    ChoiceCard choice => string.Join(", ", choice.Options.Select(o => $"'{o.Label}' -> {o.NextCardId}")),
    DieCard die => string.Join(", ", die.Outcomes.Select(o => $"{o.Min}-{o.Max} -> {o.NextCardId}")),
    EndCard => "the end",
    _ => throw new ArgumentOutOfRangeException(nameof(card)),
};