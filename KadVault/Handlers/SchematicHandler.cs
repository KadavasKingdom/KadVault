using CalamityCustomItems.Items.PassiveItem;
using CalamityStatsTracker;
using CustomItemsAPI;
using CustomItemsAPI.Items;
using InventorySystem.Items.Firearms.Modules;
using MapGeneration;
using MEC;
using ProjectMER.Events.Arguments;
using UnityEngine;

namespace KadVault.Handlers;

internal class SchematicHandler
{
    public static List<Pickup> commonItemList = [];
    public static List<Pickup> rareItemList = [];
    public static List<Pickup> legendaryItemList = [];
    public static void Spawned(SchematicSpawnedEventArgs ev)
    {
        if (ev.Schematic.Name == "VaultInterior")
        {
            PluginMain.Instance.Vault.schematicWalkwayRef = ev.Schematic;

            commonItemList.Clear();
            rareItemList.Clear();
            legendaryItemList.Clear();

            CustomItemReplace(10.0f);

        }

        if (ev.Schematic.Name == "SafeDoor")
        {
            PluginMain.Instance.Vault.schematicRef = ev.Schematic;
            PluginMain.Instance.Vault.SafeDoorAnim = ev.Schematic.AnimationController;
            PluginMain.Instance.Vault.doOnceBool = false;

            Timing.CallDelayed(.2f, () =>
            {
                for (int i = 0; i < PluginMain.Instance.Vault.SafeDoorAnim.Animators.Count; i++)
                {
                    PluginMain.Instance.Vault.SafeDoorAnim.Animators[i].speed = 0.0f;
                }
            });
        }
    }

    public static void ButtonInteracted(ButtonInteractedEventArgs ev)
    {
        if (!PluginMain.Instance.Vault.doOnceBool)
        {
            foreach (Item item in ev.Player.Items)
            {
                if (item.Type != ItemType.KeycardCustomTaskForce)
                    continue;

                if (!item.IsCustom())
                    continue;

                if (!item.TryGetCustomItem(out VaultAccessCard _))
                    continue;

                OpenVault(ev);
                item.DropItem().Destroy();
                return;
            }

            ev.Player.SendHint("You need a <b>Vault Access Card</b> to open the vault!\n<size=22><color=#8f8f8f><i>You can find one of these hidden somewhere in the facility.</i></color></size>", 5f);
        }
    }

    private static void OpenVault(ButtonInteractedEventArgs ev)
    {
        PluginMain.Instance.Vault.doOnceBool = true;

        PluginMain.Instance.Vault.safePosition = ev.Schematic.Position;

        CL.Info("Vault Button Engaged");
        if (!PluginMain.Instance.Config.disableStatsTracking)
        {
            RoundStatsTracker.AddStatEvent("KadVault", "Vault", "Vault Button Engaged", $" Player = {ev.Player.Nickname} , Class = {ev.Player.Role}");
        }
        float xptogive = 100f;
        XPSystem.BackEnd.XpSystemAPI.AddXP(ev.Player, xptogive, $"Open <color=red>Vault</color> [+{xptogive}]");
        Cassie.Message("ALERT . . LIGHT CONTAINMENT ZONE OMEGA ARMORY ACCESS AUTHORIZED . . OPENING SEQUENCE HAS BEGUN . . .", false, true, true);
        for (int i = 0; i < PluginMain.Instance.Vault.SafeDoorAnim.Animators.Count; i++)
        {
            CL.Info("Safe Door Animation Started");
            PluginMain.Instance.Vault.SafeDoorAnim.Animators[i].speed = 1.0f;
        }

        //Turn Lights off
        var room = Room.Get(RoomName.Lcz173).First();
        room.LightController.FlickerLights(3f);

        Timing.CallDelayed(1.5f, () =>
        {
            room.LightController.OverrideLightsColor = Color.black;
        });

        //Open 173 Gate
        foreach (var item in room.Doors)
        {
            item.IsOpened = true;
        }


        //Creates and Plays Safe Door Audio
        PluginMain.Instance.Vault.audioPlayer = AudioPlayer.CreateOrGet("DoorOpenPlayer", onIntialCreation: p =>
        {
            p.transform.position = PluginMain.Instance.Vault.safePosition;
            PluginMain.Instance.Vault.audioSpeaker = p.AddSpeaker("SafeDoor-Speaker", isSpatial: true, maxDistance: 500f);
            PluginMain.Instance.Vault.audioSpeaker.transform.position = PluginMain.Instance.Vault.safePosition;
            PluginMain.Instance.Vault.audioSpeaker.transform.localPosition = Vector3.zero;

        });

        PluginMain.Instance.Vault.audioPlayer.AddClip("DoorOpenSFX");
        PluginMain.Instance.Vault.audioSpeaker.Volume = 1.0f;

        //Alarm audio
        PluginMain.Instance.Vault.audioPlayerAlarm = AudioPlayer.CreateOrGet("AlarmPlayer", onIntialCreation: p =>
        {
            PluginMain.Instance.Vault.audioSpeakerAlarm = p.AddSpeaker("Alarm-Speaker", isSpatial: true, maxDistance: 4000f);
            PluginMain.Instance.Vault.audioSpeakerAlarm.Position = PluginMain.Instance.Vault.safePosition;

        });

        PluginMain.Instance.Vault.audioPlayerAlarm.AddClip("AlarmSFX", loop: true);
        PluginMain.Instance.Vault.audioSpeakerAlarm.Volume = 1.0f;

        Timing.CallDelayed(45.0f, () =>
        {
            PluginMain.Instance.Vault.audioSpeakerAlarm.Volume = 0.0f;
        });

    }


    public static Pickup SpawnCustomItem(string itemName, Vector3 spawnPosition)
    {
        CustomItemBase item = CustomItemsAPI.CustomItems.CreateItem(itemName);
        Pickup pickup = CustomItemsAPI.CustomItems.Spawn(item, spawnPosition, scale: Vector3.one);
        if (!PluginMain.Instance.Config.disableStatsTracking)
        {
            RoundStatsTracker.AddStatEvent("KadVault", "Vault", "VaultItemSpawned", $" Item = {item.CustomItemName}");
        }

        return pickup;
    }

    public static void CustomItemReplace(float _callDelay)
    {

        PluginMain.Instance.PrintDebug("CustomItemReplaceTimerUp");

        Timing.CallDelayed(_callDelay, () =>
        {

            PluginMain.Instance.PrintDebug("CustomItemReplaceTimerUp");

            foreach (Pickup pickupItem in Pickup.List)
            {
                if (ItemType.KeycardO5 == pickupItem.Type)
                {
                    PluginMain.Instance.PrintDebug(pickupItem + " Pickup Detected");
                    rareItemList.Add(pickupItem);
                    PluginMain.Instance.PrintDebug(pickupItem + " Pickup added to array");
                }

                if (ItemType.GunLogicer == pickupItem.Type)
                {
                    PluginMain.Instance.PrintDebug(pickupItem + " Legendary Pickup Detected");
                    legendaryItemList.Add(pickupItem);
                    PluginMain.Instance.PrintDebug(pickupItem + " Pickup added to array");
                }
            }

            PluginMain.Instance.PrintDebug("--All items assigned--");

            int rareLen = rareItemList.Count;
            int legLen = legendaryItemList.Count;

            Timing.CallDelayed(2f, () =>
            {

                PluginMain.Instance.PrintDebug("Items counted");

                int StatsCommon = 0;
                int StatsRare = 0;
                int StatsLegendary = 0;

                //Coin Spawns
                for (int i = 0; i < rareLen; i++)
                {
                    PluginMain.Instance.PrintDebug(rareItemList[i] + " Rare Pickup Spawning");
                    int randResult = UnityEngine.Random.Range(1, 100);
                    PluginMain.Instance.PrintDebug("RandRange");
                    Vector3 spawnPos = rareItemList[i].Position;

                    if (randResult <= PluginMain.Instance.Config.VaultSideLegendaryCoinChance)
                    {
                        PluginMain.Instance.PrintDebug("Rare | Leg");
                        SpawnCustomItem(PluginMain.Instance.Config.LegendaryCoinID, spawnPos);
                        StatsLegendary++;
                        PluginMain.Instance.PrintDebug("Spawned");
                    }
                    else if (randResult <= PluginMain.Instance.Config.VaultSideRareCoinChance)
                    {
                        if (URandom.Range(1, 100) > 33)
                        {
                            PluginMain.Instance.PrintDebug("Rare | Rare");
                            SpawnCustomItem(PluginMain.Instance.Config.RareCoinID, spawnPos);
                            StatsRare++;
                            PluginMain.Instance.PrintDebug("Spawned");
                        }
                        else
                        {
                            PluginMain.Instance.PrintDebug("Rare | CustomItem");
                            SpawnCustomItem(PluginMain.Instance.Config.UtilityItemsArray.RandomItem(), spawnPos);
                            StatsRare++;
                            PluginMain.Instance.PrintDebug("Spawned");
                        }

                    }
                    else if (randResult <= PluginMain.Instance.Config.VaultSideCommonCoinChance)
                    {
                        PluginMain.Instance.PrintDebug("Rare | Common");
                        SpawnCustomItem(PluginMain.Instance.Config.CommonCoinID, spawnPos);
                        StatsCommon++;
                        PluginMain.Instance.PrintDebug("Spawned");
                    }
                    else
                    {
                        PluginMain.Instance.PrintDebug("Rare | Nothing");
                        SpawnCustomItem(PluginMain.Instance.Config.CommonCoinID, spawnPos);
                        StatsCommon++;
                        PluginMain.Instance.PrintDebug("Nothing Spawned");

                    }

                }

                if (!PluginMain.Instance.Config.disableStatsTracking)
                {
                    RoundStatsTracker.AddStatEvent("KadVault", "Vault", "VaultCoinsSpawned", $" Common = {StatsCommon} , Rare = {StatsRare} , Legendary = {StatsLegendary}");
                }

                //Main Pedestal Spawn - Item
                for (int i = 0; i < legLen; i++)
                {
                    PluginMain.Instance.PrintDebug(legendaryItemList[i] + " Leg Pickup Spawning");
                    Vector3 spawnPos = legendaryItemList[i].Position;

                    string spawnedItem = PluginMain.Instance.Config.LegendaryItemsArray[URandom.Range(0, PluginMain.Instance.Config.LegendaryItemsArray.Count)];
                    var pickup = SpawnCustomItem(spawnedItem, spawnPos);

                    PluginMain.Instance.PrintDebug("Legendary item " + spawnedItem + " spawned");

                    if (pickup is FirearmPickup firearm && firearm.Base.Template.TryGetModule(out MagazineModule module) && module != null)
                        module.ServerSetInstanceAmmo(firearm.Serial, module.AmmoMax);

                }

                Timing.CallDelayed(5f, () =>
                {
                    //-Destroying Items-
                    //SideSpawns
                    for (int i = 0; i < rareLen; i++)
                    {

                        PluginMain.Instance.PrintDebug(rareItemList[i] + " Rare Pickup Destroying");
                        rareItemList[i].Destroy();

                    }

                    //MainSpawn
                    for (int i = 0; i < legLen; i++)
                    {

                        PluginMain.Instance.PrintDebug(legendaryItemList[i] + " Leg Pickup Destroying");
                        legendaryItemList[i].Destroy();

                    }
                });
            });
        });

    }
}
