using System;
using System.Collections.Generic;
using MLAPI.NetworkVariable;
using UnityEngine;

[Serializable]
public class Question
{
    public string Price;
    public string Scenario;
    public string Type;
    [HideInInspector] public byte[] AudioData;
    [HideInInspector] public byte[] ImageData;


    //public AudioClip Clip;
    //public Texture2D Image;

    public List<string> Answers;

    public bool IsMusic;
    public bool IsImage;

    //public Question(string price, string scenario, List<string> answers, string type = null, byte[] audioData = null, Texture2D image = null)
    public Question(string price, string scenario, List<string> answers, string type = null, byte[] audioData = null, byte[] imageData = null)
    {
        Price = price;
        Scenario = scenario;
        Type = type;

        AudioData = audioData;
        ImageData = imageData;
        Answers = answers;

        //Clip = clip;
        //Image = image;

        if (audioData != null)
        {
            IsMusic = true;
        }

        //if (image != null)
        if (ImageData != null)
        {
            IsImage = true;
        }
    }
}
