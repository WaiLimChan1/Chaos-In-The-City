using UnityEngine;
using Fusion;

public class CharacterSpawner : NetworkBehaviour
{
    private NetworkedPlayer _networkedPlayer;

    [SerializeField] private NetworkPrefabRef _playerSurvivorPrefab;

    private void Awake()
    {
        _networkedPlayer = GetComponent<NetworkedPlayer>();
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnPlayerSurvivor(PlayerRef playerRef)
    {
        if (NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;

        var spawnPoint = new Vector3(0, 0, 0);
        var character = Runner.Spawn(_playerSurvivorPrefab, spawnPoint, Quaternion.identity, playerRef);
        character.GetComponent<PlayerSurvivor>().NetworkedPlayer = _networkedPlayer;
        _networkedPlayer.OwnedCharacter = character;
    }
}
