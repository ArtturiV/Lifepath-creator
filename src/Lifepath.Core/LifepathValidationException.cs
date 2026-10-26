namespace Lifepath.Core;

public sealed class LifepathValidationException(IReadOnlyList<string> errors)
    : Exception($"The lifepath has {errors.Count} problem(s).")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}