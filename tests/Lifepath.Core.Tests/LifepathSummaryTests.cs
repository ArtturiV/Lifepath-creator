namespace Lifepath.Core.Tests;

public class LifepathSummaryTests
{
    [Fact]
    public void From_GroupsAnswersAndDecisions()
    {
        var summary = LifepathSummary.From(FinishedRun());

        Assert.Equal("Test Lifepath", summary.LifepathTitle);
        Assert.Equal("Low", summary.Ending);
        Assert.Collection(summary.Sections,
            answers =>
            {
                Assert.Equal("Answers", answers.Heading);
                Assert.Equal(new[] { new SummaryEntry("Name", "Aldric") }, answers.Entries);
            },
            decisions =>
            {
                Assert.Equal("Decisions", decisions.Heading);
                Assert.Equal(
                    new[] { new SummaryEntry("Choice", "To the die"), new SummaryEntry("Die", "rolled 1") },
                    decisions.Entries);
            });
    }

    [Fact]
    public void From_LeavesOutEmptySections()
    {
        var run = new LifepathRun(TestLifepaths.Create("intro"));
        run.Continue();
        run.Choose(1);
        run.Finish();

        var summary = LifepathSummary.From(run);

        var section = Assert.Single(summary.Sections);
        Assert.Equal("Decisions", section.Heading);
    }

    [Fact]
    public void From_UnfinishedRun_Throws()
    {
        var run = new LifepathRun(TestLifepaths.Create());

        Assert.Throws<InvalidOperationException>(() => LifepathSummary.From(run));
    }

    [Fact]
    public void ToText_FormatsEverySection()
    {
        var text = LifepathSummary.From(FinishedRun()).ToText();

        const string expected = """
            Test Lifepath

            Answers
            - Name: Aldric

            Decisions
            - Choice: To the die
            - Die: rolled 1

            Ending: Low

            """;
        Assert.Equal(expected, text, ignoreLineEndingDifferences: true);
    }

    private static LifepathRun FinishedRun()
    {
        var run = new LifepathRun(TestLifepaths.Create(), rollDie: () => 1);
        run.WriteIn("Aldric");
        run.Continue();
        run.Choose(0);
        run.Roll();
        run.Finish();
        return run;
    }
}