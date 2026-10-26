namespace Lifepath.Core;

public sealed record PathStep(Card Card, ChoiceOption? Choice = null, int? Roll = null);

public sealed class LifepathRun
{
    private readonly List<PathStep> _steps = [];
    private readonly Func<int> _rollDie;

    public LifepathRun(LifepathDefinition definition, Func<int>? rollDie = null)
    {
        Definition = definition;
        CurrentCard = definition.StartCard;
        _rollDie = rollDie ?? (() => Random.Shared.Next(1, DieCard.Sides + 1));
    }

    public LifepathDefinition Definition { get; }
    public Card CurrentCard { get; private set; }
    public IReadOnlyList<PathStep> Steps => _steps;
    public bool IsFinished => CurrentCard is EndCard;

    public void Continue()
    {
        var card = Expect<TextCard>();
        Advance(new PathStep(card), card.NextCardId);
    }

    public void Choose(int optionIndex)
    {
        var card = Expect<ChoiceCard>();
        if (optionIndex < 0 || optionIndex >= card.Options.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(optionIndex));
        }

        var option = card.Options[optionIndex];
        Advance(new PathStep(card, Choice: option), option.NextCardId);
    }

    public int Roll()
    {
        var roll = _rollDie();
        RecordRoll(roll);
        return roll;
    }

    public void RecordRoll(int roll)
    {
        var card = Expect<DieCard>();
        if (roll < 1 || roll > DieCard.Sides)
        {
            throw new ArgumentOutOfRangeException(nameof(roll), roll, $"A roll must be between 1 and {DieCard.Sides}.");
        }

        Advance(new PathStep(card, Roll: roll), card.NextCardIdFor(roll));
    }

    private T Expect<T>() where T : Card =>
        CurrentCard as T
        ?? throw new InvalidOperationException(
            $"Current card '{CurrentCard.Id}' is a {CurrentCard.GetType().Name}, not a {typeof(T).Name}.");

    private void Advance(PathStep step, string nextCardId)
    {
        _steps.Add(step);
        CurrentCard = Definition.GetCard(nextCardId);
    }
}