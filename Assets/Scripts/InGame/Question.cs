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
    [HideInInspector] public byte[] ImageQuestionData;
    [HideInInspector] public byte[] ImageAnswerData;

    public List<string> Answers;

    public bool IsMusic;
    public bool IsQuestionImage;
    public bool IsAnswerImage;
    public bool IsMarker;

    public Question(string price, string scenario, List<string> answers, string type = null, bool isMarker = false, byte[] audioData = null, byte[] imageQuestionData = null, byte[] imageAnswerData = null)
    {
        Price = price;
        Scenario = scenario;
        Type = type;
        IsMarker = isMarker;

        AudioData = audioData;
        ImageQuestionData = imageQuestionData;
        ImageAnswerData = imageAnswerData;
        Answers = answers;

        if (audioData != null)
        {
            IsMusic = true;
        }

        if (ImageQuestionData != null)
        {
            IsQuestionImage = true;
        }

        if (ImageAnswerData != null)
        {
            IsAnswerImage = true;
        }
    }
}
