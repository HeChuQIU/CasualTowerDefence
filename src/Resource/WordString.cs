namespace CasualTowerDefence.Resource;

using System;
using System.Text.RegularExpressions;

public partial record WordString : IEquatable<string>
{
    public WordString(string value)
    {
        if (!Pattern.IsMatch(value))
        {
            throw new ArgumentException(InvalidMessage);
        }

        Value = value;
    }

    public virtual string InvalidMessage => $"无效标识符。需要匹配正则表达式：{Pattern}";

    public string Value { get; }

    protected static Regex Pattern { get; } = PatternRegex();

    [GeneratedRegex(@"^(\w+)$")]
    private static partial Regex PatternRegex();

    public virtual bool Equals(string? other) => string.Equals(ToString(), other, StringComparison.Ordinal);

    public override string ToString() => Value;

    public static implicit operator string(WordString wordString) => wordString.ToString();

    public static bool IsValid(string value) => Pattern.IsMatch(value);
}
