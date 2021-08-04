using UnityEngine;
using MLAPI;

namespace BOYAREngine.Game
{
    public class HostCreate : NetworkBehaviour
    {
        [Header("Host menu")]
        [SerializeField] private GameObject _hostMenuGameObject;

        [Header("Player setup")]
        [SerializeField] private GameObject _hostPrefab;
        [SerializeField] private Transform _hostSpawnParent;

        public void OnHost()
        {
            NetworkManager.Singleton.StartHost();

            _hostMenuGameObject.SetActive(true);
            HostManager.Instance.SetupHostRound();
            RoundManager.Instance.ShowIntro(GameManager.Instance.Rounds[GameManager.Instance.Round].Name);
            SpawnHost(NetworkManager.Singleton.ServerClientId);
            StartRoundTimer();
        }

        private void SpawnHost(ulong id)
        {
            var go = Instantiate(_hostPrefab, _hostSpawnParent);
            go.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
            go.gameObject.GetComponent<HostData>().Name.Value = GameManager.Instance.Name;
        }

        private void StartRoundTimer()
        {
            RoundManager.Instance.StartRoundTimer();
        }
    }
}

