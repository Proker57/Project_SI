using System;
using System.Collections.Generic;

[Serializable]
public class Theme
{
    public string Name;
    public string Info;
    public List<Question> Questions;

    public Theme(string name, List<Question> questions, string info = null)
    {
        Name = name;
        Questions = questions;
        Info = info;
    }
}
