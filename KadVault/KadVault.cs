using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using MapEditorReborn.Events.EventArgs;
using MapEditorReborn.Events.Handlers;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

                /*foreach (var Block in ev.Schematic.SchematicData.Blocks.Where(x => x.BlockType == BlockType.Pickup))
                {

                    //Pedestal Types:
                    //1 - Main, Legendary Spawn
                    //2 - Side, Rare Spawn
                    //3 - Random, Common Spawn
                    int pedestalType = 3;

                    if (Block.Name.Contains("Main"))
                    {
                        pedestalType = 1;
                    }

                    if (Block.Name.Contains("Rare"))
                    {
                        pedestalType = 2;
                    }

                    if (Block.Name.Contains("Deposit"))
                    {
                        pedestalType = 3;
                    }

                    

                    //CoinSpawn(Block.Position + schematicWalkwayRef.Position, pedestalType);


                }*/
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
                //Dog shit spawning system
                /*Timing.CallDelayed(3.0f, () =>
                {
                    for (int i = 0; i < SafeDoorAnim.Animators.Count; i++)
                    {
                        SafeDoorAnim.Animators[i].speed = 0.0f;

                    }

                    foreach (var Block in ev.Schematic.SchematicData.Blocks.Where(x => x.BlockType == BlockType.Pickup))
                    {
                        var roomPos = Exiled.API.Features.Room.Get(RoomType.Lcz173).Position;
                        var roomRotRel = ev.Schematic.RelativeRotation;
                        var roomRotOrg = ev.Schematic.OriginalRotation;
                        var roomRot = ev.Schematic.Rotation;

                        //----
                        //if(Block.Name.Contains("Pedestal"))
                        if (!Block.Name.Contains("Button"))
                        {
                            //Pedestal Types:
                            //1 - Main, Legendary Spawn
                            //2 - Side, Rare Spawn
                            //3 - Random, Common Spawn
                            int pedestalType = 3;

                            if (Block.Name.Contains("Main"))
                            {
                                pedestalType = 1;
                            }

                            if (Block.Name.Contains("Rare"))
                            {
                                pedestalType = 2;
                            }

                            if (Block.Name.Contains("Deposit"))
                            {
                                pedestalType = 3;
                            }

                            Log.Debug(((int)(roomRotRel.y)));

                            if (((int)(roomRotRel.y)) == 270)
                            {
                                realBlockPos = new Vector3(Block.Position.z * -1, Block.Position.y, Block.Position.x);
                                itemPlacePos = ev.Schematic.Position + realBlockPos;
                                itemPlacePos = new Vector3(itemPlacePos.x, itemPlacePos.y, itemPlacePos.z);
                                CoinSpawn(itemPlacePos, pedestalType);
                                Log.Info("1");
                                if (Config.Debug)
                                {
                                    Log.Info("--LockerFound--");
                                    Log.Info("270 spawning successful.");
                                    //Confirmed Works when roomrot is -0.7 (april)
                                }
                            }
                            else if (((int)(roomRotRel.y)) == -90 || ((int)(roomRotRel.y)) == -89)
                            {
                                if (((int)(roomRot.w)) == 1)
                                {

                                    realBlockPos = new Vector3(Block.Position.x, Block.Position.y, Block.Position.z * 1);
                                    itemPlacePos = ev.Schematic.Position + realBlockPos;
                                    itemPlacePos = new Vector3(itemPlacePos.x, itemPlacePos.y, itemPlacePos.z);
                                    CoinSpawn(itemPlacePos, pedestalType);
                                    Log.Info("2");
                                    if (Config.Debug)
                                    {
                                        Log.Info("--LockerFound--");
                                        Log.Info("-90 && w 1 spawning successful.");
                                        //tried z, x*-1
                                        //tried z, x 0,0,0,1
                                        //tried x, z - Confirmed to work
                                    }
                                }
                                else if (((int)(roomRot.w * 10)) == 7)
                                {
                                    realBlockPos = new Vector3(Block.Position.z * -1, Block.Position.y, Block.Position.x);
                                    itemPlacePos = ev.Schematic.Position + realBlockPos;
                                    CoinSpawn(itemPlacePos, pedestalType);
                                    Log.Info("3");
                                    if (Config.Debug)
                                    {
                                        Log.Info("--LockerFound--");
                                        Log.Info("-90 && w .7 spawning successful.");

                                    }
                                }
                                else if (((int)(roomRot.w * 10)) == -7)
                                {
                                    realBlockPos = new Vector3(Block.Position.z, Block.Position.y, Block.Position.x * -1);
                                    itemPlacePos = ev.Schematic.Position + realBlockPos;
                                    CoinSpawn(itemPlacePos, pedestalType);
                                    Log.Info("3.5");
                                    if (Config.Debug)
                                    {
                                        Log.Info("--LockerFound--");
                                        Log.Info("-90 && w .7 spawning successful.");

                                    }
                                }
                                else if (((int)(roomRot.w)) == -1)
                                {

                                    realBlockPos = new Vector3(Block.Position.x, Block.Position.y, Block.Position.z * -1);
                                    itemPlacePos = ev.Schematic.Position + realBlockPos;
                                    itemPlacePos = new Vector3(itemPlacePos.x, itemPlacePos.y, itemPlacePos.z);
                                    CoinSpawn(itemPlacePos, pedestalType);
                                    Log.Info("2");
                                    if (Config.Debug)
                                    {
                                        Log.Info("--LockerFound--");
                                        Log.Info("-90 && w 1 spawning successful.");
                                        //tried z, x*-1
                                        //tried z, x 0,0,0,1
                                        //tried x, z - Confirmed to work
                                    }
                                }
                                else
                                {

                                    realBlockPos = new Vector3(Block.Position.x * -1, Block.Position.y, Block.Position.z * -1);
                                    itemPlacePos = ev.Schematic.Position + realBlockPos;
                                    CoinSpawn(itemPlacePos, pedestalType);
                                    Log.Info("4");
                                    if (Config.Debug)
                                    {
                                        Log.Info("--LockerFound--");
                                        Log.Info("-90 spawning successful.");
                                        //Confirmed to work if roomrot y = 1 DONE
                                        //-90, -1
                                    }
                                }
                            }
                            else
                            {
                                realBlockPos = new Vector3(Block.Position.z, Block.Position.y, Block.Position.x);
                                itemPlacePos = ev.Schematic.Position + realBlockPos;
                                itemPlacePos = new Vector3(itemPlacePos.x * -1, itemPlacePos.y, itemPlacePos.z);
                                CoinSpawn(itemPlacePos, pedestalType);
                                Log.Info("5");
                                if (Config.Debug)
                                {
                                    Log.Info("--LockerFound--");
                                    Log.Info("null spawning successful.");
                                    //roomrot was also 1.0 when thiw works
                                }
                            }

                            if (Config.Debug)
                            {

                                Log.Info("Block Name: " + Block.Name);
                                Log.Info("Rotation: " + Block.Rotation);
                                Log.Info("Block Position" + Block.Position);
                                Log.Info("Schematic Position" + ev.Schematic.Position);
                                Log.Info("Room Position" + roomPos);
                                Log.Info("Room Rotation Rel: " + roomRotRel);
                                Log.Info("Room Rotation Org: " + roomRotOrg);
                                Log.Info("Room Rotation: " + roomRot);
                                Log.Info("realBlockPos: " + realBlockPos);
                                Log.Info("itemPlacePos: " + itemPlacePos);

                            }
                        }
                    }

                });
*/
            }


        }

        public void CoinSpawn(Vector3 _position, int pedestalType)
        {
            //ID's:
            //Legendary Coin - 503
            //Rare Coin - 502
            //Common Coin - 501
            int randResult = UnityEngine.Random.Range(1, 100);

            if (Config.LegendaryOnlyEvent)
            {
                if (pedestalType == 1 || pedestalType == 2)
                {
                    CustomItem.Get((uint)Config.LegendaryCoinID).Spawn(_position);
                }
                else
                {
                    CustomItem.Get((uint)Config.RareCoinID).Spawn(_position);
                }
                return;
            }

            if (pedestalType == 1)
            {
                if (randResult <= Config.VaultMainLegendaryCoinChance)
                {
                    CustomItem.Get((uint)Config.LegendaryCoinID).Spawn(_position);
                }
                else
                {
                    CustomItem.Get((uint)Config.RareCoinID).Spawn(_position);
                }
            }

            if (pedestalType == 2)
            {
                if (randResult <= Config.VaultSideLegendaryCoinChance)
                {
                    CustomItem.Get((uint)Config.LegendaryCoinID).Spawn(_position);
                }
                else if (randResult <= Config.VaultSideRareCoinChance)
                {
                    CustomItem.Get((uint)Config.RareCoinID).Spawn(_position);
                }
                else
                {
                    CustomItem.Get((uint)Config.CommonCoinID).Spawn(_position);

                }
            }

            if (pedestalType == 3)
            {
                if (randResult <= Config.VaultRandomRareCoinChance)
                {
                    CustomItem.Get((uint)Config.RareCoinID).Spawn(_position);
                }
                else if (randResult <= Config.VaultRandomCommonChance)
                {
                    CustomItem.Get((uint)Config.CommonCoinID).Spawn(_position);
                }

            }


        }

        public void ButtonInteracted(ButtonInteractedEventArgs ev)
        {
            if (!doOnceBool)
            {
                doOnceBool = true;

                /*if (SafeDoorAnim.AttachedSchematic != ev.Schematic)
                    return;*/
                safePosition = ev.Schematic.transform;

                Exiled.API.Features.Log.Info("Vault Button Engaged");

                PluginAPI.Core.Cassie.Message("ALERT . . LIGHT CONTAINMENT ZONE OMEGA ARMORY ACCESS AUTHORIZED . . OPENING SEQUENCE HAS BEGUN . . .", false, true, true);
                for (int i = 0; i < SafeDoorAnim.Animators.Count; i++)
                {
                    Exiled.API.Features.Log.Info("Safe Door Animation Started");
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
                /* alarmSpeakerPosition.x += -5.0f;
                 alarmSpeakerPosition.z += 10.0f;*/

                //Alarm audio
                audioPlayerAlarm = AudioPlayer.CreateOrGet("AlarmPlayer", onIntialCreation: p =>
                {
                    audioSpeakerAlarm = p.AddSpeaker("Alarm-Speaker", isSpatial: true, maxDistance: 4000f);
                    audioSpeakerAlarm.Position = alarmSpeakerPosition;
                    //audioSpeakerAlarm.transform.localPosition = Vector3.zero;

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
                //customItemList = null;

                Log.Debug("CustomItemReplaceTimerUp");

                foreach (Pickup pickupItem in Pickup.List)
                {

                    if (CustomItem.Get(Config.CommonCoinSpawnID).Check(pickupItem))
                    {
                        Log.Debug(pickupItem + " Pickup Detected");
                        commonItemList.Add(pickupItem);
                        Log.Debug(pickupItem + " Pickup added to array");
                    }

                    if (CustomItem.Get(Config.RareCoinSpawnID).Check(pickupItem))
                    {
                        Log.Debug(pickupItem + " Rare Pickup Detected");
                        rareItemList.Add(pickupItem);
                        Log.Debug(pickupItem + " Rare Pickup added to array");
                    }

                    if (CustomItem.Get(Config.LegendaryCoinSpawnID).Check(pickupItem))
                    {
                        Log.Debug(pickupItem + " Leg Pickup Detected");
                        legendaryItemList.Add(pickupItem);
                        Log.Debug(pickupItem + " Leg Pickup added to array");
                    }

                }

                

                Timing.CallDelayed(5f, () =>
                {
                    int commonLen = commonItemList.Count;
                    int rareLen = rareItemList.Count;
                    int legLen = legendaryItemList.Count;

                    //CommonSpawns
                    for (int i = 0; i < commonLen; i++)
                    {
                        Log.Debug(commonItemList[i] + " Pickup Spawning");
                        int randResult = UnityEngine.Random.Range(1, 100);
                        Log.Debug("RandRange");
                        Vector3 spawnPos = commonItemList[i].Position;
                        Log.Debug("Position Gathered");

                        if (randResult <= Config.VaultRandomRareCoinChance)
                        {
                            Log.Debug("Common | Rare");
                            //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                            CustomItem.Get(Config.RareCoinID).Spawn(spawnPos);
                            Log.Debug("Spawned");
                        }
                        else if (randResult <= Config.VaultRandomCommonChance)
                        {
                            Log.Debug("Common | Common");
                            //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                            CustomItem.Get(Config.CommonCoinID).Spawn(spawnPos);
                            Log.Debug("Spawned");
                        }


                    }

                    //RareSpawns
                    for (int i = 0; i < rareLen; i++)
                    {
                        Log.Debug(commonItemList[i] + " Rare Pickup Spawning");
                        int randResult = UnityEngine.Random.Range(1, 100);
                        Log.Debug("RandRange");
                        Vector3 spawnPos = rareItemList[i].Position;
                        Log.Debug("Position Gathered");

                        if (randResult <= Config.VaultSideLegendaryCoinChance)
                        {
                            Log.Debug("Rare | Leg");
                            //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                            CustomItem.Get(Config.LegendaryCoinID).Spawn(spawnPos);
                            Log.Debug("Spawned");
                        }
                        else
                        {
                            Log.Debug("Rare | Rare");
                            //Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                            CustomItem.Get(Config.RareCoinID).Spawn(spawnPos);
                            Log.Debug("Spawned");
                        }
                        /*else
                        {
                            Log.Debug("Rare | Common");
                            Pickup.CreateAndSpawn(ItemType.Adrenaline, spawnPos);
                            //CustomItem.Get(511).Spawn(spawnPos);
                            Log.Debug("Spawned");
                        }*/

                    }

                    //LegendarySpawns
                    for (int i = 0; i < legLen; i++)
                    {
                        Log.Debug(commonItemList[i] + " Leg Pickup Spawning");
                        int randResult = UnityEngine.Random.Range(1, 100);
                        Log.Debug("RandRange");
                        Vector3 spawnPos = legendaryItemList[i].Position;
                        Log.Debug("Position Gathered");
                        uint spawnedItem = Config.LegendaryItemsArray[UnityEngine.Random.Range(0, Config.LegendaryItemsArray.Count)];
                        CustomItem.Get(spawnedItem).Spawn(spawnPos);
                        Log.Debug("Legendary item " + spawnedItem + " spawned");

                    }


                    //CommonSpawns
                    for (int i = 0; i < commonLen; i++)
                    {

                        Log.Debug(commonItemList[i] + " Pickup Destroying");
                        commonItemList[i].Destroy();
                        Log.Debug(commonItemList[i] + " Pickup Destroyed");

                    }

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