using Fusion;
using UnityEngine;

public class NetworkedPlayerSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef _networkedPlayerPrefab;

    public override void Spawned()
    {
        if (!Runner.IsServer) return;
        if (GlobalManagers.Instance.NetworkRunnerController.LocalGameMode == GameMode.Single) return; //If In Single Player, Don't Spawn Again
        PlayerJoined(Runner.LocalPlayer);
    }


    public void PlayerJoined(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;
        var playerObject = Runner.Spawn(_networkedPlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
        Runner.SetPlayerObject(playerRef, playerObject);
        playerObject.gameObject.name = "NetworkedPlayer " + playerRef.PlayerId;

        Debug.Log("Joined");
    }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;
        if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
        {
            NetworkedPlayer NetworkedPlayer = playerNetworkObject.GetComponent<NetworkedPlayer>();
            if (NetworkedPlayer != null) NetworkedPlayer.DespawnOwnedCharacter();
            Runner.Despawn(playerNetworkObject);
        }
        Runner.SetPlayerObject(playerRef, null);
    }
}
