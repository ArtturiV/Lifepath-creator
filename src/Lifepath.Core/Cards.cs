namespace Lifepath.Core;

public abstract record Card(string Id, string Title, string Text);

public sealed record TextCard(string Id, string Title, string Text, string NextCardId)
    : Card(Id, Title, Text);

public sealed record ChoiceCard(string Id, string Title, string Text, IReadOnlyList<ChoiceOption> Options)
    : Card(Id, Title, Text);

public sealed record ChoiceOption(string Label, string NextCardId);

public sealed record DieCard(string Id, string Title, string Text, IReadOnlyList<DieOutcome> Outcomes)
    : Card(Id, Title, Text)
{
    public string NextCardIdFor(int roll) =>
        Outcomes.FirstOrDefault(o => o.Matches(roll))?.NextCardId
        ?? throw new InvalidOperationException($"Die card '{Id}' has no outcome for a roll of {roll}.");
}

public sealed record DieOutcome(int Min, int Max, string NextCardId)
{
    // Convenience constructor for a single face, e.g. new DieOutcome(6, "lucky")
    public DieOutcome(int face, string nextCardId) : this(face, face, nextCardId) { }

    public bool Matches(int roll) => roll >= Min && roll <= Max;
}

public sealed record EndCard(string Id, string Title, string Text)
    : Card(Id, Title, Text);