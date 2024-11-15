using Fusion;
using CITC.GameManager;
using UnityEngine;

public class NetworkedPlayer : NetworkBehaviour, IBeforeUpdate
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Functions
    public static bool CanUseNetworkedPlayer(NetworkedPlayer networkedPlayer) { return networkedPlayer != null && networkedPlayer != default; }
    public static bool CanUseNetworkedPlayerOwnedCharacter(NetworkedPlayer networkedPlayer)  { return CanUseNetworkedPlayer(networkedPlayer) && networkedPlayer.OwnedCharacter != null && networkedPlayer.OwnedCharacter != default; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    private CharacterSpawner _characterSpawner;
    private bool _firstSpawnRPCSent;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [UnityHeader("Player Name & Id")]
    [Networked] private string _playerName { get; set; }
    public string PlayerName { get { return _playerName; } }
    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SetPlayerName(string playerName)
    {
        _playerName = playerName;
    }

    public int PlayerId { get { return Object.InputAuthority.PlayerId; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Networked] public NetworkObject OwnedCharacter { get; set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public Color PlayerColor { get { return EntityColorManager.Instance.GetPlayerColor(PlayerId); } }
    public double GetPing() { return Runner.GetPlayerRtt(Runner.LocalPlayer) * 1000.0f; }

    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public override void Spawned()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllNetworkedPlayers, this.gameObject);

        _characterSpawner = GetComponent<CharacterSpawner>();

        if (Object.HasInputAuthority)
        {
            GameUIManager.Instance.SetUp(this);
            LocalCameraManager.Instance.SetUp(this);
            Rpc_SetPlayerName(GlobalManagers.Instance.NetworkRunnerController.LocalPlayerName);

            _firstSpawnRPCSent = false;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Chat 
    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.All)]
    public void Rpc_SendChatBoxMessage(string text)
    {
        ChatBoxGameUI.Instance.AddMessageToChatBox(Runner.SimulationTime, PlayerName, PlayerColor, text);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Logic Functions
    public void BeforeUpdate()
    {

    }

    public void FixedUpdate()
    {

        if (Object.HasInputAuthority)
        {
            //First Spawn
            if (!_firstSpawnRPCSent && CityGenerator.Instance.CityGenerationCompleted && OwnedCharacter == null)
            {
                _characterSpawner.Rpc_SpawnPlayerSurvivor(Runner.LocalPlayer);
                _firstSpawnRPCSent = true;
            }

            //Revive
            if (!CanUseNetworkedPlayerOwnedCharacter(this))
            {
                if (Input.GetKey(KeyCode.R)) _characterSpawner.Rpc_SpawnPlayerSurvivor(Runner.LocalPlayer);
            }
        }
    }

    public void DespawnOwnedCharacter()
    {
        Runner.Despawn(OwnedCharacter);
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------
}
