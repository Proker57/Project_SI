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
    public AudioClip Clip;
    public Texture2D Image;
    public List<string> Answers;
    public bool IsMusic;
    public bool isImage;

    public Question(string price, string scenario, List<string> answers, string type = null, byte[] audioData = null, AudioClip clip = null, Texture2D image = null)
    {
        Price = price;
        Scenario = scenario;
        Answers = answers;
        AudioData = audioData;
        Clip = clip;
        Type = type;
        Image = image;

        if (audioData != null)
        {
            IsMusic = true;
        }

        if (image != null)
        {
            isImage = true;
        }
    }
}
