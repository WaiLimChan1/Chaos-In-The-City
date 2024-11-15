using CITC.GameManager;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedSpawnerManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static NetworkedSpawnerManager Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Member Variables
    [Header("Character Prefabs")]
    [SerializeField] private NetworkPrefabRef _playerSurvivorPrefab;
    [SerializeField] private NetworkPrefabRef _hostileSurvivorPrefab;
    [SerializeField] private NetworkPrefabRef _zombiePrefab;

    [Header("Weapon Prefabs")]
    [SerializeField] private NetworkPrefabRef[] _gunPrefabs;

    [SerializeField] private NetworkPrefabRef[] _meleePrefabs;

    [SerializeField] private NetworkPrefabRef[] _throwablePrefabs;

    [Header("Projectile Prefabs")]
    [SerializeField] private NetworkPrefabRef _bulletPrefab;
    [SerializeField] private NetworkPrefabRef _explosivePrefab;

    [Header("Vehicle Prefabs")]
    [SerializeField] private NetworkPrefabRef[] _vehiclePrefabs;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnHostileSurvivor(Vector2 spawnPoint)
    {
        var character = Runner.Spawn(_hostileSurvivorPrefab, spawnPoint, Quaternion.identity);
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnZombie(Vector2 spawnPoint)
    {
        var character = Runner.Spawn(_zombiePrefab, spawnPoint, Quaternion.identity);
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnGun(int index)
    {
        if (index < 0) return;
        if (index >= _gunPrefabs.Length) return;

        float spawnRange = 20;
        var spawnPoint = new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), 0);
        var gun = Runner.Spawn(_gunPrefabs[index], spawnPoint, Quaternion.identity);
        gun.GetComponent<Gun>().SetUp(spawnPoint);
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnRandomGun(Vector2 spawnPoint)
    {
        var gun = Runner.Spawn(_gunPrefabs[Random.Range(0, _gunPrefabs.Length)], spawnPoint, Quaternion.identity);
        gun.GetComponent<Gun>().SetUp(spawnPoint);
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnAllGuns()
    {
        float spawnRange = 30;
        foreach (var gunPrefab in _gunPrefabs)
        {
            var spawnPoint = new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), 0);
            var gun = Runner.Spawn(gunPrefab, spawnPoint, Quaternion.identity);
            gun.GetComponent<Gun>().SetUp(spawnPoint);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnAllMelees()
    {
        float spawnRange = 10;
        foreach (var meleePrefab in _meleePrefabs)
        {
            var spawnPoint = new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), 0);
            var gun = Runner.Spawn(meleePrefab, spawnPoint, Quaternion.identity);
            gun.GetComponent<Melee>().SetUp(spawnPoint);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnAllThrowables()
    {
        float spawnRange = 10;
        foreach (var throwablePrefab in _throwablePrefabs)
        {
            var spawnPoint = new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), 0);
            var throwable = Runner.Spawn(throwablePrefab, spawnPoint, Quaternion.identity);
            throwable.GetComponent<Throwable>().SetUp(spawnPoint);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnAllVehicles()
    {
        float spawnRange = 30;
        foreach (var vehiclePrefab in _vehiclePrefabs)
        {
            var spawnPoint = new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), 0);
            var vehicle = Runner.Spawn(vehiclePrefab, spawnPoint, Quaternion.identity);
            vehicle.GetComponent<Vehicle>().SetUp(spawnPoint);
        }
    }



    public NetworkObject SpawnBullet()
    {
        return Runner.Spawn(_bulletPrefab);
    }

    public NetworkObject SpawnExplosive()
    {
        return Runner.Spawn(_explosivePrefab);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
