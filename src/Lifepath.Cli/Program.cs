Console.Write("What is your character's name? ");
string? name = Console.ReadLine();

if (string.IsNullOrWhiteSpace(name))
{
    name = "Nameless Wanderer";
}

var roll = Random.Shared.Next(1, 7);

Console.WriteLine($"{name} sets out on their journey...");
Console.WriteLine($"Fate rolls a {roll}.");