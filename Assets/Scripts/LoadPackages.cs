using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class LoadPackages
{
    public static List<string> Names;
#if UNITY_EDITOR
    private const string FolderPath = @"F:\!Source\DELETE\";
#else
    private const string FolderPath = @"/storage/emulated/0/SIGameMobile";
#endif


    public static void LoadPackageNames()
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
            Debug.Log("Folder doesn't exist");
        }
        else
        {
            Names = Directory.GetFiles(FolderPath).ToList();
        }

        foreach (var name in Names)
        {
            Debug.Log(Path.GetFileName(name));
        }
    }
}
