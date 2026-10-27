namespace Lifepath.Core.Tests;

public class LifepathRunTests
{
    [Fact]
    public void NewRun_StartsAtStartCard()
    {
        var run = new LifepathRun(TestLifepaths.Create());

        Assert.Equal("name", run.CurrentCard.Id);
        Assert.Empty(run.Steps);
        Assert.False(run.IsFinished);
    }

    [Fact]
    public void WriteIn_RecordsTrimmedAnswerAndMovesOn()
    {
        var run = new LifepathRun(TestLifepaths.Create("name"));

        run.WriteIn("  Aldric ");

        Assert.Equal("intro", run.CurrentCard.Id);
        var step = Assert.Single(run.Steps);
        Assert.Equal("Aldric", step.Answer);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void WriteIn_BlankAnswer_Throws(string answer)
    {
        var run = new LifepathRun(TestLifepaths.Create("name"));

        Assert.Throws<ArgumentException>(() => run.WriteIn(answer));
        Assert.Equal("name", run.CurrentCard.Id);
    }

    [Fact]
    public void Continue_MovesToNextCard()
    {
        var run = new LifepathRun(TestLifepaths.Create("intro"));

        run.Continue();

        Assert.Equal("choice", run.CurrentCard.Id);
    }

    [Theory]
    [InlineData(0, "die")]
    [InlineData(1, "end")]
    public void Choose_FollowsChosenOption(int optionIndex, string expectedCardId)
    {
        var run = new LifepathRun(TestLifepaths.Create("choice"));

        run.Choose(optionIndex);

        Assert.Equal(expectedCardId, run.CurrentCard.Id);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Choose_InvalidIndex_Throws(int optionIndex)
    {
        var run = new LifepathRun(TestLifepaths.Create("choice"));

        Assert.Throws<ArgumentOutOfRangeException>(() => run.Choose(optionIndex));
    }

    [Theory]
    [InlineData(1, "low")]
    [InlineData(2, "low")]
    [InlineData(3, "end")]
    [InlineData(6, "end")]
    public void Roll_FollowsOutcomeForRolledFace(int face, string expectedCardId)
    {
        var run = new LifepathRun(TestLifepaths.Create("die"), rollDie: () => face);

        var roll = run.Roll();

        Assert.Equal(face, roll);
        Assert.Equal(expectedCardId, run.CurrentCard.Id);
        Assert.Equal(face, Assert.Single(run.Steps).Roll);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void RecordRoll_OutOfRange_Throws(int roll)
    {
        var run = new LifepathRun(TestLifepaths.Create("die"));

        Assert.Throws<ArgumentOutOfRangeException>(() => run.RecordRoll(roll));
    }

    [Fact]
    public void ActionForWrongCardType_Throws()
    {
        var run = new LifepathRun(TestLifepaths.Create("intro"));

        Assert.Throws<InvalidOperationException>(() => run.Roll());
    }

    [Fact]
    public void FullRun_RecordsEveryStepAndFinishes()
    {
        var run = new LifepathRun(TestLifepaths.Create(), rollDie: () => 1);

        run.WriteIn("Aldric");
        run.Continue();
        run.Choose(0);
        run.Roll();

        Assert.True(run.IsFinished);
        Assert.Equal("low", run.CurrentCard.Id);
        Assert.Equal(new[] { "name", "intro", "choice", "die" }, run.Steps.Select(step => step.Card.Id));
    }
}