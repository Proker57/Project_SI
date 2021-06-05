using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace BOYAREngine.Parser
{
    public class SiqParser
    {
        private string _roundName;
        public List<Round> Rounds = new List<Round>();

        private string _themeName;
        private string _info;
        private List<Theme> _themes = new List<Theme>();

        private string _price;
        private string _scenario;
        private string _type;
        private AudioClip _audio;
        private Texture2D _image;
        private List<string> _answers = new List<string>();
        private List<Question> _questions = new List<Question>();

        public void Parser(string packagePath)
        {
            using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Read, Encoding.UTF8))
            {
                foreach (var entry in archive.Entries)
                {
                    //Debug.Log(entry.FullName);
                }

                var content = archive.GetEntry("content.xml");
                using (var contentReader = new StreamReader(content?.Open() ?? throw new ArgumentNullException()))
                {
                    var doc = new XmlDocument();
                    doc.Load(contentReader);

                    var rounds = doc.GetElementsByTagName("round");

                    // <round>
                    foreach (XmlNode roundNode in rounds)
                    {
                        // Reload ThemeList
                        _themes = new List<Theme>();

                        // _roundName
                        _roundName = roundNode.Attributes?["name"].Value;

                        // <themes>
                        foreach (XmlNode roundChild in roundNode.ChildNodes)
                        {

                            // <theme>
                            foreach (XmlNode themeNode in roundChild)
                            {
                                // _themeName
                                _themeName = themeNode.Attributes?["name"].Value;

                                // <info>
                                

                                // <questions>
                                foreach (XmlNode themeChild in themeNode.ChildNodes)
                                {
                                    // Reload QuestionList
                                    _questions = new List<Question>();

                                    // <question>
                                    foreach (XmlNode questionNode in themeChild)
                                    {
                                        
                                        if (questionNode.Name == "question")
                                        {
                                            // Reload AnswerList
                                            _answers = new List<string>();
                                            // Price
                                            _price = questionNode.Attributes?["price"].Value;

                                            // <question data>
                                            foreach (XmlNode questionChild in questionNode.ChildNodes)
                                            {

                                                // <type>
                                                if (questionChild.Name == "type")
                                                {
                                                    _type = questionChild.Attributes?["name"].Value;
                                                }

                                                // <scenario>
                                                if (questionChild.Name == "scenario")
                                                {
                                                    // <atom>
                                                    foreach (XmlNode atom in questionChild.ChildNodes)
                                                    {
                                                        _image = null;

                                                        // Has Attribute
                                                        if (atom.Attributes?["type"] != null)
                                                        {
                                                            // Image
                                                            if (atom.Attributes?["type"].Value == "image")
                                                            {
                                                                var tex = new Texture2D(2, 2);
                                                                var imagePath = atom.InnerText;
                                                                var image = archive.GetEntry("Images/" + Uri.EscapeUriString(imagePath).Trim('@'));
                                                                if (image != null)
                                                                {
                                                                    var imageData = new byte[image.Length];
                                                                    image.Open().Read(imageData, 0, imageData.Length);
                                                                    tex.LoadImage(imageData);

                                                                    _scenario = null;
                                                                    _image = tex;
                                                                    _audio = null;
                                                                }
                                                            }

                                                            // Audio
                                                            if (atom.Attributes?["type"].Value == "voice")
                                                            {
                                                                var audioPath = atom.InnerText;
                                                                var audio = archive.GetEntry("Audio/" + Uri.EscapeUriString(audioPath).Trim('@'));
                                                                if (audio != null)
                                                                {
                                                                    _audio = AudioClip.Create("SongName", (int)audio.Length, 1, 48000, false);
                                                                    var audioData = new byte[audio.Length];
                                                                    audio.Open().Read(audioData, 0, audioData.Length);
                                                                    float[] f = ConvertData.ConvertByteToFloat(audioData);
                                                                    _audio.SetData(f, 0);
                                                                    Debug.Log(_audio);
                                                                    //                                                                var audioData = new byte[audio.Length];
                                                                    //                                                                audio.Open().Read(audioData, 0, audioData.Length);
                                                                    //                                                                _audio.GetData(audioData, 0);
                                                                    //
                                                                    //                                                                _scenario = null;
                                                                    //                                                                _image = null;
                                                                    //                                                                _audio = ;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _scenario = atom.InnerText;
                                                            _image = null;
                                                        }
                                                    }



                                                    //_scenario = questionChild.InnerText;


                                                    //                                                Debug.Log("Image");
                                                    //                                                var image = archive.GetEntry($"Images/{_scenario}");
                                                    //                                                //var data = File.ReadAllBytes(image?.FullName ?? throw new InvalidOperationException());
                                                    //                                                var tex = new Texture2D(20, 20);
                                                    //                                                var ms = new MemoryStream();
                                                    //                                                image?.Open().CopyTo(ms);
                                                    //                                                var data = ms.ToArray();
                                                    //                                                tex.LoadImage(data);
                                                    //                                                _image = tex;
                                                }

                                                // <right>
                                                if (questionChild.Name == "right")
                                                {
                                                    foreach (XmlNode answer in questionChild.ChildNodes)
                                                    {
                                                        _answers.Add(answer.InnerText);
                                                    }
                                                }

                                                // <wrong>
                                                if (questionChild.Name == "wrong")
                                                {
                                                    // TODO: wrong list
                                                }


                                            }
                                            // Add Question in List
                                            _questions.Add(new Question(_price, _scenario, _answers, _type, _audio, _image));
                                        }

                                        
                                    }

                                    // Add Theme in List
                                    _themes.Add(new Theme(_themeName, _questions, _info));
                                }
                            }

                            // Add Round in List
                            Rounds.Add(new Round(_roundName, _themes));
                        }
                    }
                }
            }
        }
    }
}

