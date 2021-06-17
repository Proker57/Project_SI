using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum StateMachine
    {
        MainMenu,
        Start
    }
    public StateMachine State;

    public Button CreateHostButton;
    public List<string> Names;
    public Dropdown Dropdown;

    public string PackagePath;
    public string ChosenPackage;
    public float Volume;
    public bool IsReadyToStart;


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
        //PackagePath = Path.Combine("/!Source", "DELETE");

        PackagePath = GetDataFolder();
        //PackagePath = Path.Combine("/storage", "emulated", "0", "SIGameMobile");
        //PackagePath = "/storage/emulated/0/SIGameMobile";
        //PackagePath = Application.persistentDataPath + "/SIGameMobile";
        LoadPackageNames();


        // TODO Save value on device
        Volume = 1f;

//        ChosenPackage = Path.Combine("/storage", "emulated", "0", "SIGameMobile", "Ugaday_melodiyu.siq");
//        CreateHostButton.interactable = true;
//        IsReadyToStart = true;

        //DebugText.text = PackagePath;
        Dropdown.onValueChanged.AddListener(delegate
        {
            Dropdown_OnValueChanged(Dropdown);
        });
    }

    public void LoadPackageNames()
    {
        if (!Directory.Exists(PackagePath))
        {
            DebugText.text = "—оздана нова€ папка";
            Directory.CreateDirectory(PackagePath);
        }
        else
        {
            DebugText.text = "—читываю архивы в папке";
            //Names = Directory.GetFiles(PackagePath, "*.siq").Select(Path.GetFileName).ToList();
        }

        //Names = Directory.GetFiles(PackagePath, "*.siq").Select(Path.GetFileName).ToList();
        Names = Directory.GetFiles(PackagePath).Select(Path.GetFileName).ToList();
        Dropdown.AddOptions(Names);
    }

    public void Dropdown_OnValueChanged(Dropdown dropdown)
    {
        if (dropdown.value != 0)
        {
            Debug.Log("Index changed");
            ChosenPackage = $"{PackagePath}/{Names[dropdown.value - 1]}";

            DebugText.text = ChosenPackage;
            CreateHostButton.interactable = true;
            IsReadyToStart = true;
        }
        else
        {
            CreateHostButton.interactable = false;
        }
    }

    public static string GetDataFolder()
    {
        var temp = (Application.persistentDataPath.Replace("Android", "")).Split(new string[] { "//" }, System.StringSplitOptions.None);

        return (temp[0] + "/SIGameMobile");
    }
}
