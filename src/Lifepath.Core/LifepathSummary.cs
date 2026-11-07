using System.Text;

namespace Lifepath.Core;

public sealed record SummaryEntry(string Label, string Value);

public sealed record SummarySection(string Heading, IReadOnlyList<SummaryEntry> Entries);

public sealed record LifepathSummary(string LifepathTitle, IReadOnlyList<SummarySection> Sections, string Ending)
{
    public static LifepathSummary From(LifepathRun run)
    {
        if (!run.IsFinished)
        {
            throw new InvalidOperationException("Only a finished lifepath can be summarized.");
        }

        var sections = new List<SummarySection>();
        AddSection(sections, "Answers", run.Steps.Select(AnswerEntry));
        AddSection(sections, "Decisions", run.Steps.Select(DecisionEntry));

        return new LifepathSummary(run.Definition.Title, sections, run.CurrentCard.Title);
    }

    public string ToText()
    {
        var text = new StringBuilder();
        text.AppendLine(LifepathTitle);

        foreach (var section in Sections)
        {
            text.AppendLine();
            text.AppendLine(section.Heading);
            foreach (var entry in section.Entries)
            {
                text.AppendLine($"- {entry.Label}: {entry.Value}");
            }
        }

        text.AppendLine();
        text.AppendLine($"Ending: {Ending}");
        return text.ToString();
    }

    private static void AddSection(List<SummarySection> sections, string heading, IEnumerable<SummaryEntry?> entries)
    {
        var list = entries.OfType<SummaryEntry>().ToList();
        if (list.Count > 0)
        {
            sections.Add(new SummarySection(heading, list));
        }
    }

    private static SummaryEntry? AnswerEntry(PathStep step) =>
        step.Answer is { } answer ? new SummaryEntry(step.Card.Title, answer) : null;

    private static SummaryEntry? DecisionEntry(PathStep step) => step switch
    {
        { Choice: { } choice } => new SummaryEntry(step.Card.Title, choice.Label),
        { Roll: { } roll } => new SummaryEntry(step.Card.Title, $"rolled {roll}"),
        _ => null,
    };
}