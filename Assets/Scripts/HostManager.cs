using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace BOYAREngine.Game
{
    public class HostManager : NetworkBehaviour
    {
        public static HostManager Instance;

        public HostMessages Messages;

        [Header("Links")]
        [SerializeField] private HostCreate _hostCreate;

        [Header("Player Data")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnParent;
        [SerializeField] private Transform _hostSpawnParent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Messages = GetComponent<HostMessages>();
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnDisable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong id)
        {

            if (IsServer)
            {
                SpawnClientPlayer(id);
            }

            if (NetworkManager.Singleton.LocalClientId == id)
            {
                _hostCreate.SetThemeName();
                StartCoroutine(_hostCreate.SetQuestionPrice());
                StartCoroutine(RenamePlayer(id));

                GameManager.Instance.NetId = id;

                _hostCreate.SetGameStarted(true);
            }

            //AddPlayersToListClientRpc();
            //ReplaceNewPlayerClientRpc();
        }

        private void SpawnClientPlayer(ulong id)
        {
            var go = Instantiate(_playerPrefab, _playerSpawnParent);
            go.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);

            //GameManager.Instance.Players.Add(go);
        }

        private IEnumerator RenamePlayer(ulong id)
        {
            yield return new WaitForSeconds(.2f);

            var goList = GameObject.FindGameObjectsWithTag("Player");
            for (var i = 0; i < goList.Length; i++)
            {
                if (goList[i].GetComponent<NetworkObject>().OwnerClientId == id)
                {
                    goList[i].GetComponent<PlayerData>().Name.Value = GameManager.Instance.Name;
                }
            }
        }

        /*public IEnumerator FindHost()
        {
            yield return new WaitForSeconds(.2f);

            var host = GameObject.FindGameObjectWithTag("Host");
            host.transform.SetParent(_hostSpawnParent);
        }

        [ClientRpc]
        private void AddPlayersToListClientRpc()
        {
            StartCoroutine(AddPlayersToList());
        }

        private IEnumerator AddPlayersToList()
        {
            GameManager.Instance.Players = new List<GameObject>();
            yield return new WaitForSeconds(.2f);
            GameManager.Instance.Players = GameObject.FindGameObjectsWithTag("Player").ToList();
        }

        [ClientRpc]
        private void ReplaceNewPlayerClientRpc()
        {
            StartCoroutine(SetParentToNewPlayer());
        }

        private IEnumerator SetParentToNewPlayer()
        {
            yield return new WaitForSeconds(.2f);
            var goList = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in goList)
            {
                player.transform.SetParent(_playerSpawnParent, false);
            }
        }*/
    }
}

