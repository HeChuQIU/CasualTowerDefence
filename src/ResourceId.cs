namespace CasualTowerDefence;

using System;
using System.Text.RegularExpressions;

public partial record ResourceId : IEquatable<string>
{
    public const string BUILTIN_MOD_NAME = "builtin";

    public ResourceId(string type, string mod, string name)
    {
        if (!Pattern.IsMatch($"{type}@{mod}:{name}"))
        {
            throw new System.ArgumentException("无效资源标识符。");
        }

        Type = type;
        Mod = mod;
        Name = name;
    }

    public ResourceId(string resourceId)
    {
        var match = Pattern.Match(resourceId);
        if (!match.Success)
        {
            throw new System.ArgumentException("无效资源标识符。");
        }

        Type = match.Groups[1].Value;
        Mod = match.Groups[2].Value;
        Name = match.Groups[3].Value;
    }

    /// <summary>
    /// 资源标识符的正则表达式模式。
    /// <para>资源标识符格式：&lt;类型&gt;@&lt;所属模组名称&gt;:&lt;名称&gt;。</para>
    /// </summary>
    protected static Regex Pattern { get; } = PatternRegex();

    [GeneratedRegex(@"^(\w+)@(\w+):(\w+)$")]
    private static partial Regex PatternRegex();

    public string Type { get; }
    public string Mod { get; }
    public string Name { get; }

    public virtual bool Equals(string? other) => string.Equals(ToString(), other, StringComparison.Ordinal);

    public override string ToString() => $"{Type}@{Mod}:{Name}";

    public bool IsValid => Pattern.IsMatch($"{Type}@{Mod}:{Name}");

    public void Deconstruct(out string type, out string mod, out string name)
    {
        type = Type;
        mod = Mod;
        name = Name;
    }
}

public record TileResourceId : ResourceId
{
    public const string TYPE_NAME = "tile";
    public static TileResourceId Default { get; } = new(TileResourceId.BUILTIN_MOD_NAME, "default");

    public TileResourceId(string mod, string name) : base("tile", mod, name)
    {
    }

    public TileResourceId(string resourceId) : base(resourceId)
    {
        var match = Pattern.Match(resourceId);
        if (!match.Success || match.Groups[1].Value != TYPE_NAME)
        {
            throw new ArgumentException("标识符类型不是 tile。");
        }
    }
}
