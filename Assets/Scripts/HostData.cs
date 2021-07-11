using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class HostData : NetworkBehaviour
    {
        public NetworkVariable<string> Name = new NetworkVariable<string>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.Everyone });
        [Space]
        public ulong Id;
        [SerializeField] private Text _nameText;

        private void Awake()
        {
            Name.OnValueChanged += ChangeName;
        }

        private void Start()
        {
            gameObject.transform.SetParent(GameObject.FindGameObjectWithTag("HostParent").transform, false);
        }

        private void ChangeName(string oldName, string newName)
        {
            Name.Value = newName;
            _nameText.text = newName;
        }
    }
}

