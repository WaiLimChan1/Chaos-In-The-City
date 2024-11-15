using System.Collections.Generic;
using CITC.GameManager;
using Fusion;
using UnityEngine;

public class GameDirectorManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static GameDirectorManager Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Variables
    [UnityHeader("City Seed")]
    [SerializeField] [Networked] private int _citySeed { get; set; }

    [Header("Spawn Zones")]
    public float PLAY_ZONE;
    public float SPAWN_ZONE;
    public float DESTROY_ZONE;

    [Header("Hostile Survivor")]
    private int _hostileSurvivorCapacity = 15;
    private float _hostileSurvivorSpawnTime = 2.5f;
    private TickTimer _hostileSurvivorSpawnTimer;
    private float _hostileSurvivorGunSpawnChance = 0.40f;

    [Header("Zombie")]
    private int _zombieCapacity = 100;
    private float _zombieSpawnTime = 0.5f;
    private TickTimer _zombieSpawnTimer;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    private void BeginGeneratingCity()
    {
        //When the clients call the spawned function, it is guaranteed that the networked variables are already synced.
        if (Runner.IsServer) _citySeed = Random.Range(0, int.MaxValue);
        CityGenerator.Instance.GenerateCityAsync(_citySeed);
    }

    private void SpawnInitialLoot()
    {
        if (!Runner.IsServer) return;
        //for (int i = 0; i < 30; i++) NetworkedSpawnerManager.Instance.Rpc_SpawnZombie(RandomSpawnPoint);
        //NetworkedSpawnerManager.Instance.Rpc_SpawnHostileSurvivor(RandomSpawnPoint);
        NetworkedSpawnerManager.Instance.Rpc_SpawnAllGuns();
        NetworkedSpawnerManager.Instance.Rpc_SpawnAllGuns();
        NetworkedSpawnerManager.Instance.Rpc_SpawnAllMelees();
        NetworkedSpawnerManager.Instance.Rpc_SpawnAllThrowables();
        NetworkedSpawnerManager.Instance.Rpc_SpawnAllVehicles();
    }

    public override void Spawned()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        BeginGeneratingCity();
        SpawnInitialLoot();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Function
    private bool InsideAPlayZone(Vector2 position)
    {
        List<GameObject> AllPlayerSurvivors = GameObjectListManager.Instance.AllPlayerSurvivors.List;
        for (int i = 0; i < AllPlayerSurvivors.Count; i++)
        {
            if (Vector3.Distance(position, AllPlayerSurvivors[i].transform.position) < PLAY_ZONE)
                return true;
        }
        return false;
    }

    private Vector2 RandomSpawnPoint
    {
        get
        {
            List<GameObject> AllPlayerSurvivors = GameObjectListManager.Instance.AllPlayerSurvivors.List;

            Vector2 startingPosition;
            float randomAngle;
            Vector2 direction;
            Vector2 randomSpawnPoint;
            int tries = 100;
            do
            {
                if (AllPlayerSurvivors.Count > 0) startingPosition = AllPlayerSurvivors[Random.Range(0, AllPlayerSurvivors.Count)].transform.position;
                else startingPosition = Vector2.zero;

                randomAngle = Random.Range(0f, Mathf.PI * 2);
                direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

                randomSpawnPoint = startingPosition + direction * Random.Range(PLAY_ZONE, SPAWN_ZONE);

                tries--;
                if (tries <= 0) return Vector2.zero;

            } while (InsideAPlayZone(randomSpawnPoint));

            return randomSpawnPoint;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    private void HandleSpawningHostileSurvivor()
    {
        if (!Runner.IsServer) return; //Server Handles Spawning
        if (GameObjectListManager.Instance.AllHostileSurvivors.List.Count >= _hostileSurvivorCapacity) return; //Reached Capacity
        if (!_hostileSurvivorSpawnTimer.ExpiredOrNotRunning(Runner)) return; //Spawning Is Still On Cool Down

        Vector2 randomSpawnPoint = RandomSpawnPoint;
        NetworkedSpawnerManager.Instance.Rpc_SpawnHostileSurvivor(randomSpawnPoint);
        if (Random.Range(0f, 1.0f) <= _hostileSurvivorGunSpawnChance) NetworkedSpawnerManager.Instance.Rpc_SpawnRandomGun(randomSpawnPoint);
        _hostileSurvivorSpawnTimer = TickTimer.CreateFromSeconds(Runner, _hostileSurvivorSpawnTime);
    }

    private void HandleSpawningZombies()
    {
        if (!Runner.IsServer) return; //Server Handles Spawning
        if (GameObjectListManager.Instance.AllZombies.List.Count >= _zombieCapacity) return; //Reached Capacity
        if (!_zombieSpawnTimer.ExpiredOrNotRunning(Runner)) return; //Spawning Is Still On Cool Down

        NetworkedSpawnerManager.Instance.Rpc_SpawnZombie(RandomSpawnPoint);
        _zombieSpawnTimer = TickTimer.CreateFromSeconds(Runner, _zombieSpawnTime);
    }

    private void HandleDespawning(List<GameObject> gameObjectList)
    {
        if (!Runner.IsServer) return;

        List<GameObject> AllPlayerSurvivors = GameObjectListManager.Instance.AllPlayerSurvivors.List;

        for (int i = 0; i < gameObjectList.Count; i++)
        {
            if (gameObjectList[i].gameObject == null) continue;

            bool shouldBeDespawned = true;
            for (int j = 0; j < AllPlayerSurvivors.Count; j++)
            {
                if (AllPlayerSurvivors[j].gameObject == null) continue;
                if (Vector3.Distance(gameObjectList[i].transform.position, AllPlayerSurvivors[j].transform.position) < DESTROY_ZONE)
                {
                    shouldBeDespawned = false;
                    break;
                }
            }

            //No Players, then use Vector3.zero as center position
            if (AllPlayerSurvivors.Count == 0)
            {
                if (Vector3.Distance(gameObjectList[i].transform.position, Vector3.zero) < DESTROY_ZONE)
                {
                    shouldBeDespawned = false;
                }
            }
            
            if (shouldBeDespawned)
            {
                Runner.Despawn(gameObjectList[i].GetComponent<NetworkObject>());
            }
        }
       
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!CityGenerator.Instance.CityGenerationCompleted) return;
        HandleSpawningHostileSurvivor();
        HandleSpawningZombies();

        HandleDespawning(GameObjectListManager.Instance.AllHostileSurvivors.List);
        HandleDespawning(GameObjectListManager.Instance.AllZombies.List);
        //HandleDespawning(GameObjectListManager.Instance.AllWeapons.List);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
