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
    public Vector3 safePosition;
    //Audio Variables
    public AudioPlayer audioPlayer;
    public AudioPlayer audioPlayerAlarm;
    public Speaker audioSpeaker;
    public Speaker audioSpeakerAlarm;

}
