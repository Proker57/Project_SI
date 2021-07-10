using MLAPI;
using UnityEngine;

namespace BOYAREngine.Net
{
    public class ClientJoin : MonoBehaviour
    {
        public void OnClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}

