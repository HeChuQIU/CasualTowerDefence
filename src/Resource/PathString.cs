namespace CasualTowerDefence.Resource;

using System;
using System.Text.RegularExpressions;

public partial class PathString : IEquatable<string>
{
    public PathString(string value)
    {
        if (!Pattern.IsMatch(value))
        {
            throw new ArgumentException("无效路径。");
        }

        value = value.Trim('/');

        Value = value;
    }

    public string Value { get; }

    protected static Regex Pattern { get; } = PatternRegex();

    [GeneratedRegex(@"^(?!.*\/\/)[\w\/]+(\.[\w]+)?$")]
    private static partial Regex PatternRegex();

    public virtual bool Equals(string? other) => string.Equals(ToString(), other, StringComparison.Ordinal);

    public override string ToString() => Value;

    public static implicit operator string(PathString pathString) => pathString.ToString();

    public static bool IsValid(string value) => Pattern.IsMatch(value);
}
