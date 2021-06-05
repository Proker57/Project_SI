using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Question
{
    public string Price;
    public string Scenario;
    public string Type;
    public Texture2D Image;
    public AudioClip Audio;
    public List<string> Answers;

    public Question(string price, string scenario, List<string> answers, string type = null, AudioClip audio = null, Texture2D image = null)
    {
        Price = price;
        Scenario = scenario;
        Answers = answers;
        Type = type;
        Image = image;
        Audio = audio;
    }
}
