using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FishNet.Component.Spawning
{
    /// <summary>
    /// Spawns a player object for clients when they connect.
    /// </summary>
    [AddComponentMenu("FishNet/Component/PlayerSpawner")]
    public class PlayerSpawner : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// Called on the server when a player is spawned.
        /// </summary>
        public event Action<NetworkObject> OnSpawned;
        #endregion

        #region Serialized.
        /// <summary>
        /// Prefab to spawn for the player.
        /// </summary>
        [Tooltip("Prefab to spawn for the player.")]
        [SerializeField]
        private NetworkObject _playerPrefab;

        /// <summary>
        /// Sets the PlayerPrefab to use.
        /// </summary>
        /// <param name="nob"></param>
        public void SetPlayerPrefab(NetworkObject nob) => _playerPrefab = nob;

        /// <summary>
        /// True to add player to the active scene when no global scenes are specified through the SceneManager.
        /// </summary>
        [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
        [SerializeField]
        private bool _addToDefaultScene = true;
        /// <summary>
        /// Areas in which players may spawn.
        /// </summary>
        [Tooltip("Areas in which players may spawn.")]
        public Transform[] Spawns = new Transform[0];
        public Transform cubeTransform;
        #endregion

        #region Private.
        /// <summary>
        /// First instance of the NetworkManager found. This will be either the NetworkManager on or above this object, or InstanceFinder.NetworkManager.
        /// </summary>
        private NetworkManager _networkManager;
        /// <summary>
        /// Next spawns to use.
        /// </summary>
        private int _nextSpawn;
        #endregion

        private void Awake()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
                _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }

        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        private void InitializeOnce()
        {
            _networkManager = GetComponentInParent<NetworkManager>();
            if (_networkManager == null)
                _networkManager = InstanceFinder.NetworkManager;
            
            if (_networkManager == null)
            {
                NetworkManagerExtensions.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }

            _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }

        /// <summary>
        /// Called when a client loads initial scenes after connecting.
        /// </summary>
        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;
            if (_playerPrefab == null)
            {
                NetworkManagerExtensions.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }

            Vector3 position;
            Quaternion rotation;
            SetSpawn(_playerPrefab.transform, out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, conn);

            //If there are no global scenes 
            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }

        /// <summary>
        /// Sets a spawn position and rotation.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
         private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            if (cubeTransform == null)
            {
                cubeTransform = GameObject.Find("Cube")?.transform;
            }

            if (cubeTransform != null)
            {
                // Randomize the height between minSpawnHeight and maxSpawnHeight above the Cube
                float fixedHeight = 5.0f; // Fixed height above the Cube

                // Randomize X and Z positions around the Cube's position based on the Cube's scale
                float randomX = UnityEngine.Random.Range(cubeTransform.position.x - cubeTransform.localScale.x / 2, cubeTransform.position.x + cubeTransform.localScale.x / 2);
                float randomZ = UnityEngine.Random.Range(cubeTransform.position.z - cubeTransform.localScale.z / 2, cubeTransform.position.z + cubeTransform.localScale.z / 2);

                // Set the spawn position with fixed Y offset (above the Cube) and randomized X, Z
                pos = new Vector3(randomX, cubeTransform.position.y + fixedHeight, randomZ);

                rot = cubeTransform.rotation; // Use Cube's rotation
            }
            else
            {
                // Default spawn if no Cube is set
                pos = prefab.position;
                rot = prefab.rotation;
            }
        }
    }
}