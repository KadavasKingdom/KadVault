using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace KadVault;

internal class VaultSchemantic
{
    public AnimationController SafeDoorAnim;
    public bool doOnceBool = false;
    public SchematicObject schematicRef;
    public SchematicObject schematicWalkwayRef;
    //Audio Variables
    public Transform safePosition;
    public AudioPlayer audioPlayer;
    public AudioPlayer audioPlayerAlarm;
    public Speaker audioSpeaker;
    public Speaker audioSpeakerAlarm;
    public List<Pickup> commonItemList = [];
    public List<Pickup> rareItemList = [];
    public List<Pickup> legendaryItemList = [];
}
