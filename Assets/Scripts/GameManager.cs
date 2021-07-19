using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOYAREngine.Game;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Transports.UNET;
using Parse;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Space]
    public HostCreate HostCreate;

    [Header("Network")]
    [SerializeField] private UNetTransport _uNet;

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
    public InputField InputIp;
    public Text IpAddressText;

    [Header("Vars")]
    public string PackagePath;
    public string ChosenPackage;
    public float Volume;
    public bool IsReadyToStart;

    [Header("Host Data")]
    public List<NetworkObject> QuestionButtonsList = new List<NetworkObject>();
    public int QuestionsLeft;

    [Header("Player Data")]
    public List<GameObject> Players = new List<GameObject>();
    public ulong NetId;
    public string Name;
    public int Points;

    [Header("Game Data")]
    public NetworkList<string> NetThemeNames = new NetworkList<string>();
    public NetworkDictionary<Vector2, string> NetQuestionPrice = new NetworkDictionary<Vector2, string>();
    [HideInInspector] public NetworkVariable<byte> NetQuestionRowCount = new NetworkVariable<byte>();
    [HideInInspector] public NetworkVariable<byte> NetQuestionColumnCount = new NetworkVariable<byte>();

    public int QuestionPrice;

    //
    public Text PackagePathText;

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
        //PackagePath = Path.Combine("/!Source", "DELETE");

        PackagePath = "/storage/emulated/0/Android/media/com.BOYAREGames.SiGameMobile";

        LoadPackageNames();

        // TODO Save value on device
        Volume = 1f;

        // TODO DELETE
        PackagePathText.text = PackagePath;
        IpAddressText.text = IpManager.GetIP(ADDRESSFAM.IPv4);

        Dropdown.onValueChanged.AddListener(delegate
        {
            Dropdown_OnValueChanged(Dropdown);
        });

        InputName.onValueChanged.AddListener(delegate
        {
            InputField_OnNameChanged(InputName);
        });

        InputIp.onValueChanged.AddListener(delegate
        {
            InputField_OnIpChanged(InputIp);
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

            PackagePathText.text = ChosenPackage;
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

    public void InputField_OnIpChanged(InputField input)
    {
        _uNet.ConnectAddress = input.text;
    }

    public void ChangeActivePlayer(int index)
    {
        ActivePlayer = index;

        foreach (var button in QuestionButtonsList)
        {
            button.ChangeOwnership(Players[index].GetComponent<NetworkObject>().OwnerClientId);
        }

        if (QuestionManager.Instance.NetQuestionType.Value != null && QuestionManager.Instance.NetQuestionType.Value.Equals("cat"))
        {
            QuestionManager.Instance.CatQuestionContinue();
        }

        //ActivePlayerChangeColorClientRpc(index);
        ChangeColorServerRpc(index);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeColorServerRpc(int index)
    {
        ActivePlayerChangeColorClientRpc(index);
    }

    [ClientRpc]
    private void ActivePlayerChangeColorClientRpc(int index)
    {
        ResetColorsClientRpc();

        // 191 121 164 Pink
        Players[index].GetComponent<Image>().color = new Color32(191, 121, 164, 255);
    }

    [ClientRpc]
    public void ResetColorsClientRpc()
    {
        foreach (var player in Players)
        {
            // 69 121 164  Blue
            player.GetComponent<Image>().color = new Color32(64, 121, 164, 255);
        }
    }
}
