using MLAPI;
using UnityEngine;

namespace BOYAREngine.Net
{
    public class ClientJoin : MonoBehaviour
    {
        [Header("States")] [SerializeField] private GameObject _mainStateGameObject;
        [SerializeField] private GameObject _gameStateGameObject;

        public void OnClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}

