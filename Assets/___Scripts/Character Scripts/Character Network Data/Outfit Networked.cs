using Fusion;
using UnityEngine;
using CITC.GameManager;

public class OutfitNetworked : NetworkBehaviour
{
    [SerializeField] [Networked] public int BackpackIndex { get; set; }
    [SerializeField] [Networked] public int BodyIndex { get; set; }
    [SerializeField] [Networked] public int ShirtIndex { get; set; }
    [SerializeField] [Networked] public int HairIndex { get; set; }
    [SerializeField] [Networked] public int MustacheIndex { get; set; }
    [SerializeField] [Networked] public int MaskIndex { get; set; }

    public void CopyFrom(OutfitManager.Outfit other)
    {
        BackpackIndex = other.BackpackIndex;
        BodyIndex = other.BodyIndex;
        ShirtIndex = other.ShirtIndex;
        HairIndex = other.HairIndex;
        MustacheIndex = other.MustacheIndex;
        MaskIndex = other.MaskIndex;
    }
}
