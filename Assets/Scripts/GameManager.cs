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

    private void Start()
    {
        Instance = this;
        PackagePath = Path.Combine("/!Source", "DELETE");
        //"F:/!Source/DELETE";

        //PackagePath = Path.Combine("/storage", "emulated", "0", "SIGameMobile");
        LoadPackageNames();

        Dropdown.onValueChanged.AddListener(delegate
        {
            Dropdown_OnValueChanged(Dropdown);
        });

        // TODO Save value on device
        Volume = 1f;

        ChosenPackage = Path.Combine("/storage", "emulated", "0", "SIGameMobile", "Ugaday_melodiyu.siq");
        CreateHostButton.interactable = true;
        IsReadyToStart = true;

        DebugText.text = Path.Combine("/storage", "emulated", "0", "SIGameMobile", "Ugaday_melodiyu.siq");
    }

    public void LoadPackageNames()
    {
        if (!Directory.Exists(PackagePath))
        {
            Directory.CreateDirectory(PackagePath);
            Debug.Log("Folder doesn't exist");
        }
        else
        {
            Names = Directory.GetFiles(PackagePath, "*.siq").Select(Path.GetFileName).ToList();
        }

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
}
