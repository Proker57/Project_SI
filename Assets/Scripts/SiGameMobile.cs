using System.Collections.Generic;
using BOYAREngine.Parser;
using UnityEngine;

public class SiGameMobile : MonoBehaviour
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

    public void StartGame()
    {
        Debug.Log("GameStarted");
        var p = new SiqParser();
        var package = GameManager.Instance.ChosenPackage;


        p.Parser(package);
        Debug.Log("Parsed");
        Rounds = p.Rounds;
    }
}
