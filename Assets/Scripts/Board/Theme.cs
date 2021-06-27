using System;
using System.Collections.Generic;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Serialization;

[Serializable]
public class Theme
{
    public string Name;
    public string Info;
    public List<Question> Questions;

    public Theme(string name, List<Question> questions, string info = null)
    {
        Name = name;
        Info = info;
        Questions = questions;
    }
}
