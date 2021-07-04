using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<string> Name = new NetworkVariable<string>(new NetworkVariableSettings {ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.OwnerOnly});
        public NetworkVariable<int> Points = new NetworkVariable<int>(new NetworkVariableSettings { ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.OwnerOnly });
        [Space]
        public ulong Id;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _pointsText;

        private void Awake()
        {
            Name.OnValueChanged += ChangeName;
            Points.OnValueChanged += ChangePoints;

            Id = NetworkManager.Singleton.LocalClientId;
        }

        private void ChangeName(string oldName, string newName)
        {
            Name.Value = newName;
            _nameText.text = Name.Value;
        }

        private void ChangePoints(int oldVar, int newVar)
        {
            Points.Value = newVar;
            _pointsText.text = Points.Value.ToString();
        }
    }
}
