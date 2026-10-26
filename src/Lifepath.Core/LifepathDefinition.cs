namespace Lifepath.Core;

public class LifepathDefinition
{
    private readonly Dictionary<string, Card> _cards;

    public LifepathDefinition(string title, string startCardId, IEnumerable<Card> cards)
    {
        Title = title;
        StartCardId = startCardId;
        _cards = cards.ToDictionary(card => card.Id);
    }

    public string Title { get; }
    public string StartCardId { get; }
    public IEnumerable<Card> Cards => _cards.Values;
    public Card StartCard => GetCard(StartCardId);

    public Card GetCard(string id) =>
        _cards.TryGetValue(id, out var card)
            ? card
            : throw new KeyNotFoundException($"No card with id '{id}'.");
}