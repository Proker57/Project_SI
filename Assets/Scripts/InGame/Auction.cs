using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class Auction : MonoBehaviour
    {
        [SerializeField] private Text[] _texts;

        public NetworkVariable<int>[] _bets = new NetworkVariable<int>[5];

        private void Start()
        {
            _bets[0].OnValueChanged += Bet1_OnValueChanged;
            _bets[1].OnValueChanged += Bet2_OnValueChanged;
            _bets[2].OnValueChanged += Bet3_OnValueChanged;
            _bets[3].OnValueChanged += Bet4_OnValueChanged;
            _bets[4].OnValueChanged += Bet5_OnValueChanged;

            ResetValues();
        }

        private void ResetValues()
        {
            foreach (var bet in _bets)
            {
                bet.Value = 0;
            }
        }


        public void OnPass()
        {
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<PlayerData>().OwnerClientId == GameManager.Instance.NetId)
                {
                    _texts[i].text = "Пас";
                }
            }
        }

        public void OnBet()
        {
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<PlayerData>().OwnerClientId == GameManager.Instance.NetId)
                {
                    _bets[i].Value += 100;
                    _texts[i].text = _bets[i].Value.ToString();
                }
            }
        }

        public void OnVaBank()
        {
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<PlayerData>().OwnerClientId == GameManager.Instance.NetId)
                {
                    _bets[i].Value = GameManager.Instance.Points;
                    _texts[i].text = _bets[i].Value.ToString();
                }
            }
        }

        private void Bet1_OnValueChanged(int oldVar, int newVar)
        {
            _texts[0].text = _bets[0].Value.ToString();
        }

        private void Bet2_OnValueChanged(int oldVar, int newVar)
        {
            _texts[1].text = _bets[1].Value.ToString();
        }

        private void Bet3_OnValueChanged(int oldVar, int newVar)
        {
            _texts[2].text = _bets[2].Value.ToString();
        }

        private void Bet4_OnValueChanged(int oldVar, int newVar)
        {
            _texts[3].text = _bets[3].Value.ToString();
        }

        private void Bet5_OnValueChanged(int oldVar, int newVar)
        {
            _texts[4].text = _bets[4].Value.ToString();
        }
    }
}

