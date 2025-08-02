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
            PluginMain.Instance.Vault.doOnceBool = true;

            PluginMain.Instance.Vault.safePosition = ev.Schematic.Position;

            CL.Info("Vault Button Engaged");

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
            PluginMain.Instance.Vault.audioSpeaker.Volume = 30.0f;

            //Alarm audio
            PluginMain.Instance.Vault.audioPlayerAlarm = AudioPlayer.CreateOrGet("AlarmPlayer", onIntialCreation: p =>
            {
                PluginMain.Instance.Vault.audioSpeakerAlarm = p.AddSpeaker("Alarm-Speaker", isSpatial: true, maxDistance: 4000f);
                PluginMain.Instance.Vault.audioSpeakerAlarm.Position = PluginMain.Instance.Vault.safePosition;

            });

            PluginMain.Instance.Vault.audioPlayerAlarm.AddClip("AlarmSFX", loop: true);
            PluginMain.Instance.Vault.audioSpeakerAlarm.Volume = 30.0f;

            Timing.CallDelayed(45.0f, () =>
            {
                PluginMain.Instance.Vault.audioSpeakerAlarm.Volume = 0.0f;
            });

        }
    }

    public static void SpawnCustomItem(string itemName, Vector3 spawnPosition)
    {
        var item = CustomItemsAPI.CustomItems.CreateItem(itemName);
        CustomItemsAPI.CustomItems.Spawn(item, spawnPosition, scale: Vector3.one);
    }

    public static void CustomItemReplace(float _callDelay)
    {

        CL.Debug("CustomItemReplaceTimerStart");

        Timing.CallDelayed(_callDelay, () =>
        {

            CL.Debug("CustomItemReplaceTimerUp");

            foreach (Pickup pickupItem in Pickup.List)
            {
                if (ItemType.KeycardO5 == pickupItem.Type)
                {
                    CL.Debug(pickupItem + " Pickup Detected");
                    rareItemList.Add(pickupItem);
                    CL.Debug(pickupItem + " Pickup added to array");
                }

                if (ItemType.GunLogicer == pickupItem.Type)
                {
                    CL.Debug(pickupItem + " Legendary Pickup Detected");
                    legendaryItemList.Add(pickupItem);
                    CL.Debug(pickupItem + " Pickup added to array");
                }
            }

            CL.Debug("--All items assigned--");

            int rareLen = rareItemList.Count;
            int legLen = legendaryItemList.Count;

            CL.Debug("Items counted");

            //Coin Spawns
            for (int i = 0; i < rareLen; i++)
            {
                CL.Debug(rareItemList[i] + " Rare Pickup Spawning");
                int randResult = UnityEngine.Random.Range(1, 100);
                CL.Debug("RandRange");
                Vector3 spawnPos = rareItemList[i].Position;

                if (randResult <= PluginMain.Instance.Config.VaultSideLegendaryCoinChance)
                {
                    CL.Debug("Rare | Leg");
                    SpawnCustomItem(PluginMain.Instance.Config.LegendaryCoinID, spawnPos);
                    CL.Debug("Spawned");
                }
                else if (randResult <= PluginMain.Instance.Config.VaultSideRareCoinChance)
                {
                    CL.Debug("Rare | Rare");
                    SpawnCustomItem(PluginMain.Instance.Config.RareCoinID, spawnPos);
                    CL.Debug("Spawned");
                }
                else if (randResult <= PluginMain.Instance.Config.VaultSideCommonCoinChance)
                {
                    CL.Debug("Rare | Common");
                    SpawnCustomItem(PluginMain.Instance.Config.CommonCoinID, spawnPos);
                    CL.Debug("Spawned");
                }
                else
                {
                    CL.Debug("Rare | Nothing");
                    SpawnCustomItem(PluginMain.Instance.Config.CommonCoinID, spawnPos);
                    CL.Debug("Nothing Spawned");

                }

            }

            //Main Pedestal Spawn - Item
            for (int i = 0; i < legLen; i++)
            {
                CL.Debug(legendaryItemList[i] + " Leg Pickup Spawning");
                Vector3 spawnPos = legendaryItemList[i].Position;

                string spawnedItem = PluginMain.Instance.Config.LegendaryItemsArray[URandom.Range(0, (PluginMain.Instance.Config.LegendaryItemsArray.Count) + 1)];
                SpawnCustomItem(spawnedItem, spawnPos);

                CL.Debug("Legendary item " + spawnedItem + " spawned");

            }

            //-Destroying Items-
            //SideSpawns
            for (int i = 0; i < rareLen; i++)
            {

                CL.Debug(rareItemList[i] + " Rare Pickup Destroying");
                rareItemList[i].Destroy();
                CL.Debug(rareItemList[i] + " Rare Pickup Destroyed");

            }

            //MainSpawn
            for (int i = 0; i < legLen; i++)
            {

                CL.Debug(legendaryItemList[i] + " Leg Pickup Destroying");
                legendaryItemList[i].Destroy();
                CL.Debug(legendaryItemList[i] + " Leg Pickup Destroyed");

            }
        });

    }
}
