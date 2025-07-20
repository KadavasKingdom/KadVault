using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
//using LightManagerAPI.Managers;
using MEC;
using ProjectMER.Events.Arguments;
using ProjectMER.Events.Handlers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pickup = Exiled.API.Features.Pickups.Pickup;
using Room = Exiled.API.Features.Room;

namespace KadVault
{
    public class KadVault : Plugin<Config>
    {
        public static KadVault Instance;
        public AnimationController SafeDoorAnim;
        private bool doOnceBool = false;
        public SchematicObject schematicRef;
        public SchematicObject schematicWalkwayRef;
        //Audio Variables
        public Transform safePosition;
        private AudioPlayer audioPlayer;
        private AudioPlayer audioPlayerAlarm;
        private Speaker audioSpeaker;
        private Speaker audioSpeakerAlarm;
        public Vector3 alarmSpeakerPosition;
        public Vector3 realBlockPos;
        public Vector3 itemPlacePos;
        public bool isMainPedestal;
        public static CoroutineHandle NpcUpdateHandler;
        public static Dictionary <Player, float> playerDistanceDict;

        public List<Pickup> commonItemList = new List<Pickup>();
        public List<Pickup> rareItemList = new List<Pickup>();
        public List<Pickup> legendaryItemList = new List<Pickup>();

        public override PluginPriority Priority => PluginPriority.Last;

        public void OnPluginLoad()
        {
            Exiled.API.Features.Log.Info("KadVault Loaded!");
        }

        public override void OnEnabled()
        {
            Instance = this;
            Schematic.SchematicSpawned += Spawned;
            Schematic.ButtonInteracted += ButtonInteracted;
            Exiled.Events.Handlers.Map.Decontaminating += Decontaminating;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            base.OnEnabled();
            AudioClipStorage.LoadClip(Config.SafeOpeningSFXFilePath, "DoorOpenSFX");
            AudioClipStorage.LoadClip(Config.AlarmSFXFilePath, "AlarmSFX");
        }

        public override void OnDisabled()
        {
            Instance = null;
            Schematic.SchematicSpawned -= Spawned;
            Schematic.ButtonInteracted -= ButtonInteracted;
            Exiled.Events.Handlers.Map.Decontaminating -= Decontaminating;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            base.OnDisabled();
        }

        public void Spawned(SchematicSpawnedEventArgs ev)
        {
            if (ev.Schematic.Name == "VaultInterior")
            {
                schematicWalkwayRef = ev.Schematic;

                commonItemList.Clear();
                rareItemList.Clear();
                legendaryItemList.Clear();

                CustomItemReplace(7.0f);

            }

            if (ev.Schematic.Name == "SafeDoor")
            {
                schematicRef = ev.Schematic;
                SafeDoorAnim = ev.Schematic.AnimationController;
                doOnceBool = false;

                Timing.CallDelayed(.2f, () =>
                {
                    for (int i = 0; i < SafeDoorAnim.Animators.Count; i++)
                    {
                        SafeDoorAnim.Animators[i].speed = 0.0f;

                    }


                });
            }
        }

        public static void OnRoundStarted()
        {

        }

        public void ButtonInteracted(ButtonInteractedEventArgs ev)
        {
            if (!doOnceBool)
            {
                doOnceBool = true;

                safePosition = ev.Schematic.transform;

                Exiled.API.Features.Log.Info("Vault Opening");

                Cassie.Message("ALERT . . LIGHT CONTAINMENT ZONE OMEGA ARMORY ACCESS AUTHORIZED . . OPENING SEQUENCE HAS BEGUN . . .", false, true, true);
                for (int i = 0; i < SafeDoorAnim.Animators.Count; i++)
                {
                    Exiled.API.Features.Log.Debug("Safe Door Animation Started");
                    SafeDoorAnim.Animators[i].speed = 1.0f;
                }

                //Turn Lights off
                var room = Room.Get(RoomType.Lcz173);
                room.TurnOffLights(3f);

                Timing.CallDelayed(1.5f, () =>
                {
                    room.Color = Color.black;
                });

                //Open 173 Gate
                room.Doors.First(x => x.IsGate).IsOpen = true;


                //Creates and Plays Safe Door Audio
                audioPlayer = AudioPlayer.CreateOrGet("DoorOpenPlayer", onIntialCreation: p =>
                {
                    p.transform.parent = safePosition;
                    audioSpeaker = p.AddSpeaker("SafeDoor-Speaker", isSpatial: true, maxDistance: 500f);
                    audioSpeaker.transform.parent = safePosition;
                    audioSpeaker.transform.localPosition = Vector3.zero;

                });

                audioPlayer.AddClip("DoorOpenSFX");
                audioSpeaker.Volume = 30.0f;

                alarmSpeakerPosition = safePosition.position;

                //Alarm audio
                audioPlayerAlarm = AudioPlayer.CreateOrGet("AlarmPlayer", onIntialCreation: p =>
                {
                    audioSpeakerAlarm = p.AddSpeaker("Alarm-Speaker", isSpatial: true, maxDistance: 4000f);
                    audioSpeakerAlarm.Position = alarmSpeakerPosition;

                });

                audioPlayerAlarm.AddClip("AlarmSFX", loop: true);
                audioSpeakerAlarm.Volume = 30.0f;

                Timing.CallDelayed(45.0f, () =>
                {
                    audioSpeakerAlarm.Volume = 0.0f;
                });

            }
        }


        public void CustomItemReplace(float _callDelay)
        {

            Log.Debug("CustomItemReplaceTimerStart");

            Timing.CallDelayed(_callDelay, () =>
            {

                Log.Debug("CustomItemReplaceTimerUp");

                foreach (Pickup pickupItem in Pickup.List)
                {
                    if (ItemType.KeycardO5 == pickupItem.Type)
                    {
                        Log.Debug(pickupItem + " Pickup Detected");
                        rareItemList.Add(pickupItem);
                        Log.Debug(pickupItem + " Pickup added to array");
                    }

                    if (ItemType.GunLogicer == pickupItem.Type)
                    {
                        Log.Debug(pickupItem + " Legendary Pickup Detected");
                        legendaryItemList.Add(pickupItem);
                        Log.Debug(pickupItem + " Pickup added to array");
                    }


                }



                Timing.CallDelayed(5f, () =>
                {
                    Log.Debug("---Replacing items!!!---");
                    int commonLen = 0;
                    int rareLen = rareItemList.Count;
                    int legLen = legendaryItemList.Count;

                    //CommonSpawns
                    /* for (int i = 0; i < commonLen; i++)
                     {
                         Log.Debug(commonItemList[i] + " Pickup Spawning");
                         int randResult = UnityEngine.Random.Range(1, 100);
                         Vector3 spawnPos = commonItemList[i].Position;
                         Log.Debug("Position Gathered");

                         if (randResult <= Config.VaultRandomRareCoinChance)
                         {
                             Log.Debug("Common | Rare");
                             //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                             //CustomItem.Get(Config.RareCoinID).Spawn(spawnPos);
                             CustomItemsAPI.CustomItems.Spawn("MedkitPlus", spawnPos, scale: Vector3.one).Spawn();
                             Log.Debug("Spawned");
                         }
                         else if (randResult <= Config.VaultRandomCommonChance)
                         {
                             Log.Debug("Common | Common");
                             //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                             CustomItemsAPI.CustomItems.Spawn("BallsGrenade", spawnPos, scale: Vector3.one).Spawn();
                             Log.Debug("Spawned");
                         }


                     }*/

                    //RareSpawns
                    for (int i = 0; i < rareLen; i++)
                    {
                        Log.Debug("---LoopBody---");
                        Log.Debug(rareItemList[i] + " Rare Pickup Spawning");
                        int randResult = UnityEngine.Random.Range(1, 100);
                        Log.Debug("RandRange");
                        Vector3 spawnPos = rareItemList[i].Position;

                        if (Config.customItemSideSpawns)
                        {
                            if (randResult <= Config.VaultSideLegendaryCoinChance)
                            {
                                Log.Debug("Rare | Leg");
                                string spawnedItem = Config.LegendaryItemsArray[UnityEngine.Random.Range(0, Config.LegendaryItemsArray.Count)];
                                LabApi.Features.Wrappers.Pickup pickup = CustomItemsAPI.CustomItems.Spawn(spawnedItem, spawnPos, scale: Vector3.one);
                                pickup.Spawn();

/*                                Timing.CallDelayed(0.3f, () =>
                                {
                                    LightManager.ShowLight(LightSerialManager.GetLightId(pickup.Serial));
                                });*/

                                Log.Debug("Spawned");
                            }
                            else if (randResult <= Config.VaultSideRareCoinChance)
                            {
                                Log.Debug("Rare | Rare");
                                CustomItemsAPI.CustomItems.Spawn(Config.LegendaryCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Spawned");
                            }
                            else if (randResult <= Config.VaultSideCommonCoinChance)
                            {
                                Log.Debug("Rare | Common");
                                CustomItemsAPI.CustomItems.Spawn(Config.RareCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Spawned");
                            }
                            else
                            {
                                Log.Debug("Rare | Nothing");
                                CustomItemsAPI.CustomItems.Spawn(Config.CommonCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Nothing Spawned");

                            }
                        }
                        else
                        {
                            if (randResult <= Config.VaultSideLegendaryCoinChance)
                            {
                                Log.Debug("Rare | Leg");
                                CustomItemsAPI.CustomItems.Spawn(Config.LegendaryCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Spawned");
                            }
                            else if (randResult <= Config.VaultSideRareCoinChance)
                            {
                                Log.Debug("Rare | Rare");
                                CustomItemsAPI.CustomItems.Spawn(Config.RareCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Spawned");
                            }
                            else if (randResult <= Config.VaultSideCommonCoinChance)
                            {
                                Log.Debug("Rare | Common");
                                CustomItemsAPI.CustomItems.Spawn(Config.CommonCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Spawned");
                            }
                            else
                            {
                                Log.Debug("Rare | Nothing");
                                CustomItemsAPI.CustomItems.Spawn(Config.CommonCoinID, spawnPos, scale: Vector3.one).Spawn();
                                Log.Debug("Nothing Spawned");

                            }

                        }


                    }

                    //LegendarySpawns
                    for (int i = 0; i < legLen; i++)
                    {
                        Log.Debug(legendaryItemList[i] + " Leg Pickup Spawning");
                        Vector3 spawnPos = legendaryItemList[i].Position;

                        string spawnedItem = Config.LegendaryItemsArray[UnityEngine.Random.Range(0, Config.LegendaryItemsArray.Count)];
                        LabApi.Features.Wrappers.Pickup pickup = CustomItemsAPI.CustomItems.Spawn(spawnedItem, spawnPos, scale: Vector3.one);
                        pickup.Spawn();

                        /*Timing.CallDelayed(0.3f, () =>
                        {
                            LightManager.ShowLight(LightSerialManager.GetLightId(pickup.Serial));
                        });*/
                        Log.Debug("Legendary item " + spawnedItem + " spawned");

                    }

                    Timing.CallDelayed(3f, () =>
                    {
                        //CommonSpawns
                        for (int i = 0; i < commonLen; i++)
                        {

                            Log.Debug(commonItemList[i] + " Pickup Destroying");
                            commonItemList[i].Destroy();
                            Log.Debug(commonItemList[i] + " Pickup Destroyed");

                        }

                    });

                    //RareSpawns
                    for (int i = 0; i < rareLen; i++)
                    {

                        Log.Debug(rareItemList[i] + " Rare Pickup Destroying");
                        rareItemList[i].Destroy();
                        Log.Debug(rareItemList[i] + " Rare Pickup Destroyed");

                    }

                    //LegendarySpawns
                    for (int i = 0; i < legLen; i++)
                    {

                        Log.Debug(legendaryItemList[i] + " Leg Pickup Destroying");
                        legendaryItemList[i].Destroy();
                        Log.Debug(legendaryItemList[i] + " Leg Pickup Destroyed");

                    }
                });

            });

        }


        public void Decontaminating(DecontaminatingEventArgs ev)
        {
            Timing.CallDelayed(30f, () =>
            {
                Log.Debug(schematicRef + " | Vault Schematic Culled");
                schematicRef.Destroy();
                schematicWalkwayRef.Destroy();
            });
        }
    }
}