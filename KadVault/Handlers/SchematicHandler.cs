using GameCore;
using MapGeneration;
using MEC;
using ProjectMER.Events.Arguments;
using UnityEngine;

namespace KadVault.Handlers;

internal class SchematicHandler
{
    public static void Spawned(SchematicSpawnedEventArgs ev)
    {
        if (ev.Schematic.Name == "VaultInterior")
        {
            PluginMain.Instance.Vault.schematicWalkwayRef = ev.Schematic;

            PluginMain.Instance.Vault.commonItemList.Clear();
            PluginMain.Instance.Vault.rareItemList.Clear();
            PluginMain.Instance.Vault.legendaryItemList.Clear();

            CustomItemReplace(7.0f);

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

            PluginMain.Instance.Vault.safePosition = ev.Schematic.transform;

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
                p.transform.parent = PluginMain.Instance.Vault.safePosition;
                PluginMain.Instance.Vault.audioSpeaker = p.AddSpeaker("SafeDoor-Speaker", isSpatial: true, maxDistance: 500f);
                PluginMain.Instance.Vault.audioSpeaker.transform.parent = PluginMain.Instance.Vault.safePosition;
                PluginMain.Instance.Vault.audioSpeaker.transform.localPosition = Vector3.zero;

            });

            PluginMain.Instance.Vault.audioPlayer.AddClip("DoorOpenSFX");
            PluginMain.Instance.Vault.audioSpeaker.Volume = 30.0f;

            PluginMain.Instance.Vault.alarmSpeakerPosition = PluginMain.Instance.Vault.safePosition.position;

            //Alarm audio
            PluginMain.Instance.Vault.audioPlayerAlarm = AudioPlayer.CreateOrGet("AlarmPlayer", onIntialCreation: p =>
            {
                PluginMain.Instance.Vault.audioSpeakerAlarm = p.AddSpeaker("Alarm-Speaker", isSpatial: true, maxDistance: 4000f);
                PluginMain.Instance.Vault.audioSpeakerAlarm.Position = PluginMain.Instance.Vault.alarmSpeakerPosition;

            });

            PluginMain.Instance.Vault.audioPlayerAlarm.AddClip("AlarmSFX", loop: true);
            PluginMain.Instance.Vault.audioSpeakerAlarm.Volume = 30.0f;

            Timing.CallDelayed(45.0f, () =>
            {
                PluginMain.Instance.Vault.audioSpeakerAlarm.Volume = 0.0f;
            });

        }
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
                    PluginMain.Instance.Vault.rareItemList.Add(pickupItem);
                    CL.Debug(pickupItem + " Pickup added to array");
                }

                if (ItemType.GunLogicer == pickupItem.Type)
                {
                    CL.Debug(pickupItem + " Legendary Pickup Detected");
                    PluginMain.Instance.Vault.legendaryItemList.Add(pickupItem);
                    CL.Debug(pickupItem + " Pickup added to array");
                }


            }



            Timing.CallDelayed(5f, () =>
            {
                int commonLen = 0;
                int rareLen = PluginMain.Instance.Vault.rareItemList.Count;
                int legLen = PluginMain.Instance.Vault.legendaryItemList.Count;

                //CommonSpawns
                /* for (int i = 0; i < commonLen; i++)
                 {
                     CL.Debug(commonItemList[i] + " Pickup Spawning");
                     int randResult = UnityEngine.Random.Range(1, 100);
                     Vector3 spawnPos = commonItemList[i].Position;
                     CL.Debug("Position Gathered");

                     if (randResult <= Config.VaultRandomRareCoinChance)
                     {
                         CL.Debug("Common | Rare");
                         //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                         //CustomItem.Get(Config.RareCoinID).Spawn(spawnPos);
                         CustomItemsAPI.CustomItems.Spawn("MedkitPlus", spawnPos, scale: Vector3.one).Spawn();
                         CL.Debug("Spawned");
                     }
                     else if (randResult <= Config.VaultRandomCommonChance)
                     {
                         CL.Debug("Common | Common");
                         //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                         CustomItemsAPI.CustomItems.Spawn("BallsGrenade", spawnPos, scale: Vector3.one).Spawn();
                         CL.Debug("Spawned");
                     }


                 }*/

                //RareSpawns
                for (int i = 0; i < rareLen; i++)
                {
                    CL.Debug(PluginMain.Instance.Vault.rareItemList[i] + " Rare Pickup Spawning");
                    int randResult = URandom.Range(1, 100);
                    CL.Debug("RandRange");
                    Vector3 spawnPos = PluginMain.Instance.Vault.rareItemList[i].Position;

                    if (randResult <= PluginMain.Instance.Config.VaultSideLegendaryCoinChance)
                    {
                        CL.Debug("Rare | Leg");
                        CustomItemsAPI.CustomItems.Spawn(PluginMain.Instance.Config.LegendaryCoinID, spawnPos, scale: Vector3.one).Spawn();
                        CL.Debug("Spawned");
                    }
                    else if (randResult <= PluginMain.Instance.Config.VaultSideRareCoinChance)
                    {
                        CL.Debug("Rare | Rare");
                        CustomItemsAPI.CustomItems.Spawn(PluginMain.Instance.Config.RareCoinID, spawnPos, scale: Vector3.one).Spawn();
                        CL.Debug("Spawned");
                    }
                    else if (randResult <= PluginMain.Instance.Config.VaultSideCommonCoinChance)
                    {
                        CL.Debug("Rare | Common");
                        CustomItemsAPI.CustomItems.Spawn(PluginMain.Instance.Config.CommonCoinID, spawnPos, scale: Vector3.one).Spawn();
                        CL.Debug("Spawned");
                    }
                    else
                    {
                        CL.Debug("Rare | Nothing");
                        CustomItemsAPI.CustomItems.Spawn(PluginMain.Instance.Config.CommonCoinID, spawnPos, scale: Vector3.one).Spawn();
                        CL.Debug("Nothing Spawned");

                    }

                }

                //LegendarySpawns
                for (int i = 0; i < legLen; i++)
                {
                    CL.Debug(PluginMain.Instance.Vault.legendaryItemList[i] + " Leg Pickup Spawning");
                    Vector3 spawnPos = PluginMain.Instance.Vault.legendaryItemList[i].Position;

                    string spawnedItem = PluginMain.Instance.Config.LegendaryItemsArray[URandom.Range(0, PluginMain.Instance.Config.LegendaryItemsArray.Count)];
                    Pickup pickup = CustomItemsAPI.CustomItems.Spawn(spawnedItem, spawnPos, scale: Vector3.one);
                    pickup.Spawn();
                    /*
                    Timing.CallDelayed(0.3f, () =>
                    {
                        LightManager.ShowLight(LightSerialManager.GetLightId(pickup.Serial));
                    });
                    */
                    CL.Debug("Legendary item " + spawnedItem + " spawned");

                }

                Timing.CallDelayed(3f, () =>
                {
                    //CommonSpawns
                    for (int i = 0; i < commonLen; i++)
                    {

                        CL.Debug(PluginMain.Instance.Vault.commonItemList[i] + " Pickup Destroying");
                        PluginMain.Instance.Vault.commonItemList[i].Destroy();
                        CL.Debug(PluginMain.Instance.Vault.commonItemList[i] + " Pickup Destroyed");

                    }

                });

                //RareSpawns
                for (int i = 0; i < rareLen; i++)
                {

                    CL.Debug(PluginMain.Instance.Vault.rareItemList[i] + " Rare Pickup Destroying");
                    PluginMain.Instance.Vault.rareItemList[i].Destroy();
                    CL.Debug(PluginMain.Instance.Vault.rareItemList[i] + " Rare Pickup Destroyed");

                }

                //LegendarySpawns
                for (int i = 0; i < legLen; i++)
                {

                    CL.Debug(PluginMain.Instance.Vault.legendaryItemList[i] + " Leg Pickup Destroying");
                    PluginMain.Instance.Vault.legendaryItemList[i].Destroy();
                    CL.Debug(PluginMain.Instance.Vault.legendaryItemList[i] + " Leg Pickup Destroyed");

                }
            });

        });

    }
}
