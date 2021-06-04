using UnityEngine;
using MLAPI;

public class Login : MonoBehaviour
{
    public void Host()
    {
        var nm = NetworkManager.Singleton;
        nm.StartHost();
    }
}
