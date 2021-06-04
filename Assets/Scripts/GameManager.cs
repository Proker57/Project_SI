using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum StateMachine
    {
        MainMenu,
        Start
    }
    public StateMachine State;

    public string PackagePath;

    private void Start()
    {
        Instance = this;
#if UNITY_EDITOR
        PackagePath = @"F:\!Source\";
#else
        PackagePath = @"/storage/emulated/0/SITestFolder";
#endif
        //CheckForPackagePathExist(PackagePath);
        LoadPackages.LoadPackageNames();
    }





    private static void CheckForPackagePathExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
