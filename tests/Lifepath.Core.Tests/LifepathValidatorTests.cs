namespace Lifepath.Core.Tests;

public class LifepathValidatorTests
{
    [Fact]
    public void ValidLifepath_HasNoErrors()
    {
        var errors = LifepathValidator.Validate("name", TestLifepaths.Cards);

        Assert.Empty(errors);
    }

    [Fact]
    public void MissingStartCard_IsReported()
    {
        var errors = LifepathValidator.Validate("nope", TestLifepaths.Cards);

        Assert.Contains(errors, error => error.Contains("start card 'nope' does not exist"));
    }

    [Fact]
    public void BrokenLink_IsReported()
    {
        var errors = LifepathValidator.Validate("a", [Text("a", "missing"), End("end")]);

        Assert.Contains(errors, error => error.Contains("links to 'missing'"));
    }

    [Fact]
    public void DuplicateId_IsReported()
    {
        var errors = LifepathValidator.Validate("a", [Text("a", "end"), End("end"), End("end")]);

        Assert.Contains(errors, error => error.Contains("'end' is used by more than one card"));
    }

    [Theory]
    [InlineData(1, 4, 4, 6, "has 2 outcomes for a roll of 4")]
    [InlineData(1, 2, 4, 6, "has no outcome for a roll of 3")]
    [InlineData(2, 3, 4, 6, "has no outcome for a roll of 1")]
    [InlineData(1, 2, 3, 5, "has no outcome for a roll of 6")]
    public void DieCoverageProblems_AreReported(
        int firstMin, int firstMax, int secondMin, int secondMax, string expectedError)
    {
        var die = new DieCard("die", "Die", "",
        [
            new DieOutcome(firstMin, firstMax, "end"),
            new DieOutcome(secondMin, secondMax, "end"),
        ]);

        var errors = LifepathValidator.Validate("die", [die, End("end")]);

        Assert.Contains(errors, error => error.Contains(expectedError));
    }

    [Fact]
    public void DieRangeOutsideOneToSix_IsReported()
    {
        var die = new DieCard("die", "Die", "", [new DieOutcome(0, 6, "end")]);

        var errors = LifepathValidator.Validate("die", [die, End("end")]);

        Assert.Contains(errors, error => error.Contains("outside 1-6"));
    }

    [Fact]
    public void NoChoices_IsReported()
    {
        var choice = new ChoiceCard("choice", "Choice", "", []);

        var errors = LifepathValidator.Validate("choice", [choice, End("end")]);

        Assert.Contains(errors, error => error.Contains("has no options"));
    }

    [Fact]
    public void MissingTitle_IsReported()
    {
        var text = new TextCard("text", "", "", "end");

        var errors = LifepathValidator.Validate("text", [text, End("end")]);

        Assert.Contains(errors, error => error.Contains("is missing a title"));
    }

    [Fact]
    public void UnreachableCard_IsReported()
    {
        var errors = LifepathValidator.Validate("a", [Text("a", "end"), End("end"), End("orphan")]);

        Assert.Contains(errors, error => error.Contains("'orphan' can never be reached"));
    }

    [Fact]
    public void EndlessLoop_IsReported()
    {
        var errors = LifepathValidator.Validate("a", [Text("a", "b"), Text("b", "a"), End("end")]);

        Assert.Contains(errors, error => error.Contains("'a' can never lead to an end card"));
    }

    [Fact]
    public void LoopWithAnExit_IsAllowed()
    {
        var die = new DieCard("die", "Die", "",
        [
            new DieOutcome(1, 3, "die"),
            new DieOutcome(4, 6, "end"),
        ]);

        var errors = LifepathValidator.Validate("die", [die, End("end")]);

        Assert.Empty(errors);
    }

    private static TextCard Text(string id, string nextCardId) => new(id, id, "", nextCardId);

    private static EndCard End(string id) => new(id, id, "");
}