using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace FixRandomSpawn;

[MinimumApiVersion(305)]
public sealed partial class Plugin : BasePlugin
{
    public override string ModuleName { get; } = "FixRandomSpawn";
    public override string ModuleVersion { get; } = "1.1.5";
    public override string ModuleAuthor { get; } = "xstage";

    private CCSGameRules _gameRules = null!;
    private readonly MemoryPatch _memoryPatch = new();
    
    public override void Load(bool hotReload)
    {
        var patch = PluginGameData.Resolve(ModuleDirectory);
        Logger.LogInformation("Using gamedata from {source}", patch.Source);

        _memoryPatch.Init(patch.Signature);

        if (!IsConditionalJump(patch.Offset))
        {
            throw new InvalidOperationException($"Patch site is not a conditional jump, gamedata is outdated for this CS2 build ({patch.Source})");
        }

        _memoryPatch.Apply(patch.Bytes, patch.Offset);

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        RegisterEventHandler<EventRoundPrestart>(OnRoundPrestart);

        if (hotReload) 
        {
            InitGameRules();
        }
    }

    public override void Unload(bool hotReload)
    {
        _memoryPatch.Restore();
    }

    // jcc rel8 (70-7F) or jcc rel32 (0F 80-8F)
    private bool IsConditionalJump(int offset)
    {
        byte op = _memoryPatch.Read<byte>(offset);

        return op is >= 0x70 and <= 0x7F
            || op == 0x0F && _memoryPatch.Read<byte>(offset + 1) is >= 0x80 and <= 0x8F;
    }

    private void InitGameRules()
    {
        try
        {
            _gameRules = FindGameRules();
        }
        catch (Exception ex)
        {
            Logger.LogError("{errorMsg}", ex.Message);
        }
    }

    private static CCSGameRules FindGameRules()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;
        return gameRules ?? throw new Exception("Not found CCSGameRules");
    }
}