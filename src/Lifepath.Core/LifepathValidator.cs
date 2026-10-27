namespace Lifepath.Core;

public static class LifepathValidator
{
    public static IReadOnlyList<string> Validate(string startCardId, IReadOnlyList<Card> cards)
    {
        var errors = new List<string>();

        // Duplicate ids
        var duplicateIds = cards
            .GroupBy(card => card.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var id in duplicateIds)
        {
            errors.Add($"Card id '{id}' is used by more than one card.");
        }

        var cardsById = cards.DistinctBy(card => card.Id).ToDictionary(card => card.Id);

        // Start card
        var startExists = cardsById.ContainsKey(startCardId);
        if (!startExists)
        {
            errors.Add($"The start card '{startCardId}' does not exist.");
        }

        // Broken links
        var brokenLinks =
            from card in cards
            from target in LinksOf(card)
            where !cardsById.ContainsKey(target)
            select $"Card '{card.Id}' links to '{target}', which does not exist.";

        errors.AddRange(brokenLinks);

        var missingTitles =
            from card in cards
            where string.IsNullOrWhiteSpace(card.Title)
            select $"Card '{card.Id}' is missing a title.";

        errors.AddRange(missingTitles);

        // Card-specific rules
        foreach (var card in cards)
        {
            switch (card)
            {
                case ChoiceCard choice when choice.Options.Count == 0:
                    errors.Add($"Choice card '{choice.Id}' has no options.");
                    break;

                case DieCard die:
                    errors.AddRange(ValidateDieCard(die));
                    break;
            }
        }

        // Cards that can never be reached
        if (startExists)
        {
            var reachable = Search(
                [startCardId],
                id => LinksOf(cardsById[id]).Where(cardsById.ContainsKey));

            errors.AddRange(cardsById.Keys
                .Where(id => !reachable.Contains(id))
                .Select(id => $"Card '{id}' can never be reached from the start card."));
        }

        // Cards that can never lead to an end card
        var linksInto = cardsById.Values
            .SelectMany(card => LinksOf(card).Select(target => (From: card.Id, To: target)))
            .ToLookup(link => link.To, link => link.From);

        var canFinish = Search(
            cardsById.Values.OfType<EndCard>().Select(card => card.Id),
            id => linksInto[id]);

        errors.AddRange(cardsById.Keys
            .Where(id => !canFinish.Contains(id))
            .Select(id => $"Card '{id}' can never lead to an end card (dead end or endless loop)."));

        return errors;
    }

    private static IEnumerable<string> LinksOf(Card card) => card switch
    {
        TextCard text => [text.NextCardId],
        WriteInCard writeIn => [writeIn.NextCardId],
        ChoiceCard choice => choice.Options.Select(option => option.NextCardId),
        DieCard die => die.Outcomes.Select(outcome => outcome.NextCardId),
        _ => [],
    };

    private static IEnumerable<string> ValidateDieCard(DieCard card)
    {
        foreach (var outcome in card.Outcomes)
        {
            if (outcome.Min > outcome.Max)
            {
                yield return $"Die card '{card.Id}' has a range where min ({outcome.Min}) is greater than max ({outcome.Max}).";
            }

            if (outcome.Min < 1 || outcome.Max > DieCard.Sides)
            {
                yield return $"Die card '{card.Id}' has a range {outcome.Min}-{outcome.Max} outside 1-{DieCard.Sides}.";
            }
        }

        for (var face = 1; face <= DieCard.Sides; face++)
        {
            var matches = card.Outcomes.Count(outcome => outcome.Matches(face));
            if (matches == 0)
            {
                yield return $"Die card '{card.Id}' has no outcome for a roll of {face}.";
            }
            else if (matches > 1)
            {
                yield return $"Die card '{card.Id}' has {matches} outcomes for a roll of {face}.";
            }
        }
    }

    // Breadth-first search: returns every id reachable from the starting ids.
    private static HashSet<string> Search(IEnumerable<string> startIds, Func<string, IEnumerable<string>> neighbours)
    {
        var visited = new HashSet<string>(startIds);
        var queue = new Queue<string>(visited);

        while (queue.TryDequeue(out var id))
        {
            foreach (var next in neighbours(id))
            {
                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }
}