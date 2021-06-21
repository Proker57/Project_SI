using System;
using System.Collections.Generic;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;

[Serializable]
public class Round
{
    public string Name;
    public List<Theme> Themes;

    public Round(string name, List<Theme> themes)
    {
        Name = name;
        Themes = themes;
    }
}
