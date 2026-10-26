using System.Text.Json;
using Lifepath.Core;

var path = args.Length > 0 ? args[0] : "lifepaths/test.json";

LifepathDefinition lifepath;
try
{
    lifepath = LifepathLoader.FromFile(path);
}
catch (LifepathValidationException ex)
{
    Console.WriteLine($"'{path}' has {ex.Errors.Count} problem(s):");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
    return;
}
catch (Exception ex) when (ex is IOException or JsonException)
{
    Console.WriteLine($"Could not load '{path}': {ex.Message}");
    return;
}

var run = new LifepathRun(lifepath);

Console.WriteLine($"=== {lifepath.Title} ===");

while (!run.IsFinished)
{
    PrintCard(run.CurrentCard);

    switch (run.CurrentCard)
    {
        case TextCard:
            WaitForEnter("[Press Enter to continue]");
            run.Continue();
            break;

        case ChoiceCard choiceCard:
            run.Choose(AskForChoice(choiceCard));
            break;

        case DieCard:
            if (AskForOwnRoll() is int ownRoll)
            {
                run.RecordRoll(ownRoll);
            }
            else
            {
                Console.WriteLine($"You rolled a {run.Roll()}.");
            }
            break;

        default:
            throw new InvalidOperationException($"Unknown card type: {run.CurrentCard.GetType().Name}");
    }
}

PrintCard(run.CurrentCard);
PrintSummary(run);

void PrintCard(Card card)
{
    Console.WriteLine();
    Console.WriteLine($"--- {card.Title} ---");
    Console.WriteLine(card.Text);
}

void WaitForEnter(string prompt)
{
    Console.Write(prompt);
    Console.ReadLine();
}

int AskForChoice(ChoiceCard card)
{
    for (var i = 0; i < card.Options.Count; i++)
    {
        Console.WriteLine($"  {i + 1}. {card.Options[i].Label}");
    }

    while (true)
    {
        Console.Write("Choose: ");
        if (int.TryParse(Console.ReadLine(), out var number) && number >= 1 && number <= card.Options.Count)
        {
            return number - 1;
        }

        Console.WriteLine($"Please enter a number between 1 and {card.Options.Count}.");
    }
}

int? AskForOwnRoll()
{
    while (true)
    {
        Console.Write($"[Press Enter to roll, or type your own roll (1-{DieCard.Sides})] ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        if (int.TryParse(input, out var number) && number is >= 1 and <= DieCard.Sides)
        {
            return number;
        }

        Console.WriteLine($"Please enter a number between 1 and {DieCard.Sides}, or just press Enter.");
    }
}

void PrintSummary(LifepathRun run)
{
    Console.WriteLine();
    Console.WriteLine("=== Your path ===");

    foreach (var step in run.Steps)
    {
        var detail = step switch
        {
            { Choice: { } choice } => $" (chose: {choice.Label})",
            { Roll: { } roll } => $" (rolled {roll})",
            _ => "",
        };
        Console.WriteLine($"- {step.Card.Title}{detail}");
    }

    Console.WriteLine($"- {run.CurrentCard.Title}");
}