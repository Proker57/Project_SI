using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using Parse;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Logic")]
    public List<Round> Rounds;
    public int ActivePlayer = 0;

    public int ThemeIndexCurrent;
    public int QuestionIndexCurrent;
    public int Round;

    [Header("UI")]
    public Button CreateHostButton;
    public List<string> PackageFileNames;
    public Dropdown Dropdown;
    public InputField InputName;

    [Header("Vars")]
    public string PackagePath;
    public string ChosenPackage;
    public float Volume;
    public bool IsReadyToStart;

    [Header("Host Data")]
    public List<NetworkObject> QuestionButtonsList = new List<NetworkObject>();

    [Header("Player Data")]
    public List<GameObject> Players = new List<GameObject>();
    public string Name;
    public int Points;

    [Header("Game Data")]
    public NetworkList<string> NetThemeNames = new NetworkList<string>();
    public NetworkDictionary<Vector2, string> NetQuestionPrice = new NetworkDictionary<Vector2, string>();
    public NetworkVariable<byte> NetQuestionRowCount = new NetworkVariable<byte>();
    public NetworkVariable<byte> NetQuestionColumnCount = new NetworkVariable<byte>();

    public int QuestionPrice;

    //
    public Text DebugText;

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

    private void Start()
    {
        PackagePath = Path.Combine("/!Source", "DELETE");

        //PackagePath = "/storage/emulated/0/Android/media/com.BOYAREGames.SiGameMobile";

        LoadPackageNames();

        // TODO Save value on device
        Volume = 1f;

        // TODO DELETE
        DebugText.text = PackagePath;

        Dropdown.onValueChanged.AddListener(delegate
        {
            Dropdown_OnValueChanged(Dropdown);
        });

        InputName.onValueChanged.AddListener(delegate
        {
            InputField_OnNameChanged(InputName);
        });
    }

    public void LoadPackageNames()
    {
        if (!Directory.Exists(PackagePath))
        {
            Directory.CreateDirectory(PackagePath);
        }

        PackageFileNames = Directory.GetFiles(PackagePath, "*.siq").Select(Path.GetFileName).ToList();
        Dropdown.AddOptions(PackageFileNames);
    }

    public void ParsePackage()
    {
        var p = new SiqParser();
        var package = ChosenPackage;


        p.Parser(package);
        Rounds = p.Rounds;
    }

    public void Dropdown_OnValueChanged(Dropdown dropdown)
    {
        if (dropdown.value != 0)
        {
            Debug.Log("Index changed");
            ChosenPackage = $"{PackagePath}/{PackageFileNames[dropdown.value - 1]}";

            DebugText.text = ChosenPackage;
            CreateHostButton.interactable = true;
            IsReadyToStart = true;
        }
        else
        {
            CreateHostButton.interactable = false;
        }
    }

    public void InputField_OnNameChanged(InputField input)
    {
        if (input.text != null)
        {
            Name = input.text;
        }
    }

    public void ChangeActivePlayer(int index)
    {
        foreach (var button in QuestionButtonsList)
        {
            button.ChangeOwnership(Players[index].GetComponent<NetworkObject>().OwnerClientId);
        }
    }
}
