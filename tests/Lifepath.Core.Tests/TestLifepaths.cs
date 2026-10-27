namespace Lifepath.Core.Tests;

// Uses every card type, so ValidLifepath_HasNoErrors fails if the validator forgets one.
internal static class TestLifepaths
{
    public static IReadOnlyList<Card> Cards { get; } =
    [
        new WriteInCard("name", "Name", "What is your name?", "intro"),
        new TextCard("intro", "Intro", "Your story begins.", "choice"),
        new ChoiceCard("choice", "Choice", "Where to?",
        [
            new ChoiceOption("To the die", "die"),
            new ChoiceOption("To the end", "end"),
        ]),
        new DieCard("die", "Die", "Roll the die.",
        [
            new DieOutcome(1, 2, "low"),
            new DieOutcome(3, 6, "end"),
        ]),
        new EndCard("low", "Low", "You rolled low."),
        new EndCard("end", "End", "The end."),
    ];

    public static LifepathDefinition Create(string startCardId = "name") =>
        new("Test Lifepath", startCardId, Cards);
}