using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

namespace BOYAREngine.Game
{
    public class Auction : NetworkBehaviour
    {
        [SerializeField] private GameObject[] _pointPanels;
        [Space]
        [SerializeField] private Text[] _texts;

        public NetworkVariable<string>[] Bets = new NetworkVariable<string>[5];

        private int _currentBet;
        private bool _isFirstBet;
        private bool _isPassed;

        private void Start()
        {
            _isFirstBet = true;

            Bets[0].OnValueChanged += Bet1_OnValueChanged;
            Bets[1].OnValueChanged += Bet2_OnValueChanged;
            Bets[2].OnValueChanged += Bet3_OnValueChanged;
            Bets[3].OnValueChanged += Bet4_OnValueChanged;
            Bets[4].OnValueChanged += Bet5_OnValueChanged;

            ResetValues();
        }

        public void ResetValues()
        {
            foreach (var bet in Bets)
            {
                bet.Value = "0";
            }

            _isFirstBet = true;
            _isPassed = false;
            _currentBet = 0;
        }

        public void OnPass()
        {
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                if (GameManager.Instance.Players[i].GetComponent<PlayerData>().OwnerClientId == GameManager.Instance.NetId)
                {
                    PassServerRpc(i);
                }
            }

            _isPassed = true;
        }

        public void OnBet()
        {
            if (!_isPassed)
            {
                for (var i = 0; i < GameManager.Instance.Players.Count; i++)
                {
                    if (GameManager.Instance.Players[i].GetComponent<PlayerData>().OwnerClientId == GameManager.Instance.NetId)
                    {
                        if (GameManager.Instance.Players[i].GetComponent<PlayerData>().Points.Value >= GameManager.Instance.QuestionPrice)
                        {
                            if (_isFirstBet)
                            {
                                Bets[i].Value = GameManager.Instance.QuestionPrice.ToString();
                                _texts[i].text = Bets[i].Value;
                                _currentBet = GameManager.Instance.QuestionPrice;
                                GameManager.Instance.Players[i].GetComponent<PlayerData>().AuctionBet.Value = _currentBet;
                                _isFirstBet = false;
                            }
                            else
                            {
                                if (_currentBet < GameManager.Instance.Players[i].GetComponent<PlayerData>().Points.Value)
                                {
                                    Bets[i].Value = (_currentBet + 100).ToString();
                                    _texts[i].text = Bets[i].Value;
                                    _currentBet = int.Parse(Bets[i].Value);
                                    GameManager.Instance.Players[i].GetComponent<PlayerData>().AuctionBet.Value = _currentBet;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnVaBank()
        {
            if (!_isPassed)
            {
                for (var i = 0; i < GameManager.Instance.Players.Count; i++)
                {
                    if (GameManager.Instance.Players[i].GetComponent<PlayerData>().OwnerClientId == GameManager.Instance.NetId)
                    {
                        Bets[i].Value = GameManager.Instance.Players[i].GetComponent<PlayerData>().Points.Value.ToString();
                        _texts[i].text = Bets[i].Value;
                    }
                }
            }
        }

        // Change bet
        [ServerRpc(RequireOwnership = false)]
        private void ChangeBetServerRpc(int index, string newVar)
        {
            ChangeBetClientRpc(index, newVar);
        }

        [ClientRpc]
        private void ChangeBetClientRpc(int index, string newVar)
        {
            Bets[index].Value = newVar;
            _texts[index].text = Bets[index].Value;
        }

        // Pass
        [ServerRpc(RequireOwnership = false)]
        private void PassServerRpc(int index)
        {
            PassClientRpc(index);
        }

        [ClientRpc]
        private void PassClientRpc(int index)
        {
            Bets[index].Value = "Пас";
            _texts[index].text = Bets[index].Value;
        }

        public void TurnOnPanels()
        {
            for (var i = 0; i < GameManager.Instance.Players.Count; i++)
            {
                _pointPanels[i].SetActive(true);
            }
        }

        private void Bet1_OnValueChanged(string oldVar, string newVar)
        {
            ChangeBetServerRpc(0, newVar);
        }

        private void Bet2_OnValueChanged(string oldVar, string newVar)
        {
            ChangeBetServerRpc(1, newVar);
        }

        private void Bet3_OnValueChanged(string oldVar, string newVar)
        {
            ChangeBetServerRpc(2, newVar);
        }

        private void Bet4_OnValueChanged(string oldVar, string newVar)
        {
            ChangeBetServerRpc(3, newVar);
        }

        private void Bet5_OnValueChanged(string oldVar, string newVar)
        {
            ChangeBetServerRpc(4, newVar);
        }

    }
}

