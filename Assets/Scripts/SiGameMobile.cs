using System.Collections.Generic;
using BOYAREngine.Parser;
using MLAPI;
using UnityEngine;

public class SiGameMobile : NetworkBehaviour
{
    public static SiGameMobile Instance;

    public List<Round> Rounds;

    public int ThemeIndexCurrent;
    public int QuestionIndexCurrent;

    public int Round;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ParsePackage()
    {
        Debug.Log("GameStarted");
        var p = new SiqParser();
        var package = GameManager.Instance.ChosenPackage;


        p.Parser(package);
        Debug.Log("Parsed");
        Rounds = p.Rounds;
    }

    public void JoinGame()
    {
        Debug.Log("Joined to the game");
    }
}
