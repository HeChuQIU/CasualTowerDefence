namespace CasualTowerDefence.Resource;

using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public class ModLoader
{
    public ModLoader(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; set; }

    public void LoadMod(IServiceCollection services, string modPath)
    {
        Logger.Information("Loading mod {modPath}", modPath);

        var success = ProjectSettings.LoadResourcePack(modPath);
        if (!success)
        {
            Logger.Error("Failed to load mod {modPath}", modPath);
            return;
        }

        Logger.Information("Loaded mod {modPath}", modPath);
    }

    public void LoadMods(IServiceCollection services)
    {
        var modFolder = ProjectSettings.GlobalizePath("user://mods");
        var modPaths = Directory.GetFiles(modFolder, "*.zip");
        LoadMods(services, modPaths);
    }

    public void LoadMods(IServiceCollection services, IEnumerable<string> modPaths)
    {
        foreach (var modPath in modPaths)
        {
            LoadMod(services, modPath);
        }
    }

    public void LoadMods(IServiceCollection services, Func<IEnumerable<string>> modPathProvider) =>
        LoadMods(services, modPathProvider());

    public void LoadMods(IServiceCollection services,
        Func<IServiceCollection, IEnumerable<string>> modPathProvider) => LoadMods(services, modPathProvider(services));
}
