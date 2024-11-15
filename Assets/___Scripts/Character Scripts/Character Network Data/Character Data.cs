using Fusion;
using UnityEngine;
using static Character;

public struct CharacterData : INetworkInput
{
    public Vector2 MovementDirection;
    public Vector3 MouseWorldPosition;

    public ChosenWeaponSlot ChosenWeaponSlot;
    public bool TriggeredAttack;
    public bool TriggeredPunch;
    public bool TriggeredReload;
    public bool TriggeredPickUp;
    public bool TriggeredDropChosenWeapon;
}
