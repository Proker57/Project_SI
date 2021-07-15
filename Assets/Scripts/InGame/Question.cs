using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Question
{
    public string Price;
    public string Scenario;
    public string Type;
    [HideInInspector] public byte[] AudioData;
    [HideInInspector] public byte[] ImageData;

    public List<string> Answers;

    public bool IsMusic;
    public bool IsImage;
    public bool IsMarker;

    public Question(string price, string scenario, List<string> answers, string type = null, bool isMarker = false, byte[] audioData = null, byte[] imageData = null)
    {
        Price = price;
        Scenario = scenario;
        Type = type;
        IsMarker = isMarker;

        AudioData = audioData;
        ImageData = imageData;
        Answers = answers;

        if (audioData != null)
        {
            IsMusic = true;
        }

        if (ImageData != null)
        {
            IsImage = true;
        }
    }
}
