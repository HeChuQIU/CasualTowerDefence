namespace CasualTowerDefence.Resource;

using System;
using System.Text.RegularExpressions;

public partial record ResourceIdBase : IEquatable<string>
{
    public ResourceIdBase(string resourceId)
    {
        var ss = Split(resourceId);
        Type = ss[0];
        Mod = ss[1];
        Path = ss[2];
    }

    public ResourceIdBase(string type, string mod, string path)
    {
        if (!Pattern.IsMatch(Concat(type, mod, path)))
        {
            throw new ArgumentException("无效资源标识符。");
        }

        Type = type;
        Mod = mod;
        Path = path;
    }

    public ResourceIdBase()
    {
        Type = DEFAULT_TYPE_NAME;
        Mod = BUILTIN_MOD_NAME;
        Path = DEFAULT_PATH_NAME;
    }

    private string Concat(string type, string mod, string path) => $"{type}@{mod}:{path}";

    protected string[] Split(string resourceId)
    {
        var match = Pattern.Match(resourceId);
        if (!match.Success)
        {
            throw new ArgumentException("无效资源标识符。");
        }

        return [match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value];
    }

    public const string DEFAULT_TYPE_NAME = "default";
    public const string BUILTIN_MOD_NAME = "builtin";
    public const string DEFAULT_PATH_NAME = "default";
    public string Type { get; protected set; }
    public string Mod { get; protected set; }
    public string Path { get; protected set; }
    public virtual bool Equals(string? other) => string.Equals(ToString(), other, StringComparison.Ordinal);
    public override string ToString() => Concat(Type, Mod, Path);

    /// <summary>
    /// 资源标识符的正则表达式模式。
    /// <para>资源标识符格式：&lt;类型&gt;@&lt;所属模组名称&gt;:&lt;路径&gt;。</para>
    /// </summary>
    [GeneratedRegex(@"^([^@:]+)@([^@:]+):([^@:]+)$")]
    private static partial Regex PatternRegex();

    protected static Regex Pattern { get; } = PatternRegex();
}

public record ResourceId : ResourceIdBase
{
    public ResourceId(WordString type, WordString mod, PathString path) : base(type, mod, path)
    {
    }

    public ResourceId(string resourceId) : base(resourceId)
    {
        if (WordString.IsValid(Type) && WordString.IsValid(Mod) && PathString.IsValid(Path))
        {
            throw new ArgumentException("资源标识符类型、模组名称或路径无效。");
        }
    }

    public static readonly WordString BuiltinModName = new("builtin");

    public static implicit operator string(ResourceId resourceId) => resourceId.ToString();
}

public record TypeResourceIdBase : ResourceIdBase
{
    protected virtual string TypeName => "default";

    public TypeResourceIdBase() : base()
    {
        Initialize(new WordString(BUILTIN_MOD_NAME), new PathString(DEFAULT_PATH_NAME));
    }

    public TypeResourceIdBase(WordString mod, PathString path) : base()
    {
        Initialize(mod, path);
    }

    public TypeResourceIdBase(string resourceId) : base(resourceId)
    {
        ValidateType();
        ValidateModAndPath();
    }

    private void Initialize(WordString mod, PathString path)
    {
        Type = TypeName;
        Mod = mod;
        Path = path;
    }

    private void ValidateType()
    {
        if (Type != TypeName)
        {
            throw new ArgumentException("资源标识符类型错误。");
        }
    }

    private void ValidateModAndPath()
    {
        if (WordString.IsValid(Mod) && PathString.IsValid(Path))
        {
            throw new ArgumentException("资源标识符模组名称或路径无效。");
        }
    }
}

public record TileResourceId : TypeResourceIdBase
{
    protected override string TypeName => "tile";

    public TileResourceId() : base()
    {
    }

    public TileResourceId(WordString mod, PathString path) : base(mod, path)
    {
    }

    public TileResourceId(string resourceId) : base(resourceId)
    {
    }
}

public record TextureResourceId : TypeResourceIdBase
{
    protected override string TypeName => "texture";

    public TextureResourceId() : base()
    {
    }

    public TextureResourceId(WordString mod, PathString path) : base(mod, path)
    {
    }

    public TextureResourceId(string resourceId) : base(resourceId)
    {
    }
}
