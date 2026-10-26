using Lifepath.Core;

var lifepath = new LifepathDefinition(
    title: "Test Lifepath",
    startCardId: "birth",
    cards:
    [
        new TextCard("birth", "Birth", "You are born under a grey sky.", "family"),
        new ChoiceCard("family", "Family", "Who raised you?",
        [
            new ChoiceOption("Your mother, a fisher", "sea"),
            new ChoiceOption("Your uncle, a soldier", "war"),
        ]),
        new DieCard("sea", "The Sea", "The sea gives and the sea takes.",
        [
            new DieOutcome(1, 2, "drowned"),
            new DieOutcome(3, 6, "adulthood"),
        ]),
        new TextCard("war", "War", "You learn to hold a spear before a spoon.", "adulthood"),
        new EndCard("drowned", "Lost at Sea", "Your story ends beneath the waves."),
        new EndCard("adulthood", "Adulthood", "You come of age, ready for adventure."),
    ]);

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