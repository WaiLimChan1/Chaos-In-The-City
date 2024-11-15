using Fusion;
using UnityEngine;

public class VehicleSeat : NetworkBehaviour
{
    [SerializeField] [Networked] public Character character { get; set; }
    public bool isDriver;
}
