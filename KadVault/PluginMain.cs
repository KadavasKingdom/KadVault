using KadVault.Handlers;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;
using MEC;
using ProjectMER.Events.Handlers;
using UnityEngine;

namespace KadVault;

public class PluginMain : Plugin<Config>
{
    public static PluginMain Instance;
    internal VaultSchemantic Vault = new();
    private readonly LabApiHandler labApiHandler = new();

    public override string Name => "KadVault";
    public override string Description => "Vault";
    public override string Author => "KadavaSmile";
    public override Version Version => new(0, 1);

    public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;

    public override void Enable()
    {
        Instance = this;
        Schematic.SchematicSpawned += SchematicHandler.Spawned;
        Schematic.ButtonInteracted += SchematicHandler.ButtonInteracted;
        CustomHandlersManager.RegisterEventsHandler(labApiHandler);
        AudioClipStorage.LoadClip(Config.SafeOpeningSFXFilePath, "DoorOpenSFX");
        AudioClipStorage.LoadClip(Config.AlarmSFXFilePath, "AlarmSFX");
    }

    public override void Disable()
    {
        Instance = null;
        Schematic.SchematicSpawned -= SchematicHandler.Spawned;
        Schematic.ButtonInteracted -= SchematicHandler.ButtonInteracted;
        CustomHandlersManager.UnregisterEventsHandler(labApiHandler);
    }
}