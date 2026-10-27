using System.Text.Json;

namespace Lifepath.Core.Tests;

public class BundledLifepathTests
{
    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "BundledLifepaths");

    public static IEnumerable<object[]> LifepathFiles() =>
        Directory.GetFiles(Folder, "*.json")
            .Select(path => Path.GetFileName(path))
            .Where(name => name != "index.json")
            .Select(name => new object[] { name });

    [Theory]
    [MemberData(nameof(LifepathFiles))]
    public void BundledLifepath_IsValid(string fileName)
    {
        var json = File.ReadAllText(Path.Combine(Folder, fileName));

        try
        {
            LifepathLoader.FromJson(json);
        }
        catch (LifepathValidationException ex)
        {
            Assert.Fail($"{fileName} has problems:\n{string.Join("\n", ex.Errors)}");
        }
    }

    [Fact]
    public void Index_ListsExactlyTheBundledFiles()
    {
        var json = File.ReadAllText(Path.Combine(Folder, "index.json"));
        var index = JsonSerializer.Deserialize<IndexEntry[]>(json, JsonSerializerOptions.Web)!;

        var listed = index.Select(entry => entry.File).Order();
        var actual = LifepathFiles().Select(row => (string)row[0]).Order();

        Assert.Equal(actual, listed);
    }

    public sealed record IndexEntry(string File);
}