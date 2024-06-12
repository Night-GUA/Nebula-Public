﻿using Nebula.Compat;
using Nebula.Modules.GUIWidget;
using Nebula.Roles;
using Nebula.Scripts;
using System.Reflection;
using Virial;
using Virial.Assignable;
using Virial.Configuration;
using Virial.Text;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Nebula;

public class NebulaImpl : INebula
{
    public static NebulaImpl Instance = null!;

    private static List<object> allModules = new();
    private static Dictionary<Type, object> moduleFastMap = new();

    private static List<(Type type,Func<object> generator)> allDefinitions = new();
    private static Dictionary<Type, Func<object>> definitionFastMap = new();

    public NebulaImpl()
    {
        Instance = this;

        allModules.AddRange([Nebula.Modules.Language.API, GUI.API, ConfigurationsAPI.API]);
        allDefinitions.AddRange([
            (typeof(ModAbilityButton), () => new ModAbilityButton()), 
            (typeof(Timer), () => new Timer())
            ]);
    }

    public string APIVersion => typeof(NebulaAPI).Assembly.GetName().Version?.ToString() ?? "Undefined";

    public Virial.Media.IResourceAllocator NebulaAsset => NebulaResourceManager.NebulaNamespace;

    public Virial.Media.IResourceAllocator InnerslothAsset => NebulaResourceManager.InnerslothNamespace;

    public Virial.Media.IResourceAllocator? GetAddonResource(string addonId) => NebulaAddon.GetAddon(addonId);
    public Virial.Media.IResourceAllocator GetCallingAddonResource(Assembly assembly) => AddonScriptManager.ScriptAssemblies.FirstOrDefault(a => a.Assembly == assembly )?.Addon!;

    T? INebula.Get<T>() where T : class
    {
        var type = typeof(T);
        if (moduleFastMap.TryGetValue(type, out var module))
            return module as T;
        var result = allModules.FirstOrDefault(m => m.GetType().IsAssignableTo(type));
        if (result != null) moduleFastMap[type] = result;
        return result as T;
    }

    T? INebula.Instantiate<T>() where T : class
    {
        var type = typeof(T);
        if (definitionFastMap.TryGetValue(type, out var module))
            return module as T;
        var result = allDefinitions.FirstOrDefault(m => m.type.IsAssignableTo(type)).generator;
        if (result != null) definitionFastMap[type] = result;
        return result?.Invoke() as T;
    }

    Virial.Game.Game? INebula.CurrentGame => NebulaGameManager.Instance;

    //ショートカット
    Virial.Configuration.Configurations Configurations => ConfigurationsAPI.API;
    Virial.Media.GUI GUILibrary => GUI.API;
    Virial.Media.Translator Language => Nebula.Modules.Language.API;
}

internal static class UnboxExtension
{
    internal static PlayerModInfo Unbox(this Virial.Game.Player player) => (PlayerModInfo)player;
    internal static CustomEndCondition Unbox(this Virial.Game.GameEnd end) => (CustomEndCondition)end;
    internal static PlayerTaskState Unbox(this Virial.Game.PlayerTasks taskInfo) => (PlayerTaskState)taskInfo;
}

public static class ComponentHolderHelper
{
    static public GameObject Bind(this ComponentHolder holder, GameObject gameObject)
    {
        holder.BindComponent(new GameObjectBinding(gameObject));
        return gameObject;
    }
}