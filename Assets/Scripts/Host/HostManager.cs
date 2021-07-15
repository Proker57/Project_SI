using System.Collections;
using MLAPI;
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
        }

        private void SpawnClientPlayer(ulong id)
        {
            var go = Instantiate(_playerPrefab, _playerSpawnParent);
            go.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
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
    }
}

