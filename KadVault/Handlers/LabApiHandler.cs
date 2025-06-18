using LabApi.Events.CustomHandlers;
using MEC;

namespace KadVault.Handlers;

internal class LabApiHandler : CustomEventsHandler
{
    public override void OnServerLczDecontaminationStarted()
    {
        Timing.CallDelayed(30f, () =>
        {
            var vault = PluginMain.Instance.Vault;
            var schematicRef = vault.schematicRef;
            CL.Debug(schematicRef + " | Vault Schematic Culled");
            schematicRef.Destroy();
            vault.schematicWalkwayRef.Destroy();
        });
    }
}
