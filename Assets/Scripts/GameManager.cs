using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOYAREngine.Game;
using MLAPI;
using MLAPI.Transports.UNET;
using Parse;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Network")]
    [SerializeField] private UNetTransport _uNet;

    [Header("Logic")]
    public List<Round> Rounds;
    public int ActivePlayer;

    public int ThemeIndexCurrent;
    public int QuestionIndexCurrent;
    public int Round;

    [Header("UI")]
    public Button CreateHostButton;
    public Text CreateHostText;
    public Text CreateHostDescription;
    public Image CreateHostImage;
    public List<string> PackageFileNames;
    public Dropdown Dropdown;
    public InputField InputName;
    public InputField InputIp;
    public Text IpAddressText;

    [Header("Package Data")]
    public Text AuthorNameText;
    public Text DescriptionText;

    [Header("Vars")]
    public string PackagePath;
    public string ChosenPackage;
    public float Volume;
    public bool IsPacketChosen;

    [Header("Host Data")]
    public List<NetworkObject> QuestionButtonsList = new List<NetworkObject>();
    public int QuestionsLeft;

    [Header("Player Data")]
    public List<GameObject> Players = new List<GameObject>();
    public ulong NetId;
    public string Name;

    [Header("Game Data")]
    public List<string> ThemeNames = new List<string>();
    public Dictionary<Vector2, string> QuestionPrice = new Dictionary<Vector2, string>();

    public int QuestionPriceCurrent;

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
            // PC
        PackagePath = Path.Combine("/!Source", "DELETE");
            // Mobile
        //PackagePath = "/storage/emulated/0/Android/media/com.BOYAREGames.SiGameMobile";

        LoadPackageNames();

        // TODO Save value on device
        Volume = 1f;

        PackagePathText.text = PackagePath;
        IpAddressText.text = IpManager.GetIP(ADDRESSFAM.IPv4);

        SetListeners();
    }

    public void LoadPackageNames()
    {
        if (!Directory.Exists(PackagePath))
        {
            Directory.CreateDirectory(PackagePath);
        }

        var emptyItem = new List<string> { "Выберите пакет вопросов" };
        Dropdown.ClearOptions();
        ClearPackageInfo();
        DisableCreateHostButton();
        Dropdown.AddOptions(emptyItem);

        PackageFileNames = Directory.GetFiles(PackagePath, "*.siq").Select(Path.GetFileName).ToList();
        Dropdown.AddOptions(PackageFileNames);

        PackagePathText.text = PackagePath;
    }

    public void ParsePackage()
    {
        var p = new SiqParser();
        var package = ChosenPackage;

        p.Parser(package);
        Rounds = p.Rounds;
    }

    public void ClearData()
    {
        QuestionButtonsList = new List<NetworkObject>();
        ThemeNames = new List<string>();
        QuestionPrice = new Dictionary<Vector2, string>();
    }

    private void SetListeners()
    {
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

    public void Dropdown_OnValueChanged(Dropdown dropdown)
    {
        var activeColor = new Color32(1, 100, 99, 255);
        if (dropdown.value != 0)
        {
            ChosenPackage = $"{PackagePath}/{PackageFileNames[dropdown.value - 1]}";

            PackagePathText.text = ChosenPackage;
            CreateHostButton.interactable = true;
            CreateHostText.color = Color.white;
            CreateHostImage.color = Color.white;
            CreateHostDescription.color = activeColor;
            IsPacketChosen = true;

            var ap = new AuthorParser();
            ap.Parse(ChosenPackage);
            AuthorNameText.text = ap.AuthorName;
            DescriptionText.text = ap.Description;
        }
        else
        {
            DisableCreateHostButton();
            ClearPackageInfo();
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

        if (QuestionManager.Instance.NetQuestionType.Value != null)
        {
            QuestionTypeCat();
            QuestionTypeAuction();
        }

        HostManager.Instance.Messages.ChangeColorServerRpc(index);
    }

    private void DisableCreateHostButton()
    {
        CreateHostButton.interactable = false;
        CreateHostText.color = Color.black;
        CreateHostImage.color = Color.black;
        CreateHostDescription.color = Color.black;
    }

    private void ClearPackageInfo()
    {
        AuthorNameText.text = "";
        DescriptionText.text = "";
    }

    private void QuestionTypeCat()
    {
        if (QuestionManager.Instance.NetQuestionType.Value.Equals("cat"))
        {
            QuestionManager.Instance.CatQuestionContinue();
        }
    }

    private void QuestionTypeAuction()
    {
        if (QuestionManager.Instance.NetQuestionType.Value.Equals("auction"))
        {
            QuestionManager.Instance.AuctionQuestionContinue();
        }
    }
}
