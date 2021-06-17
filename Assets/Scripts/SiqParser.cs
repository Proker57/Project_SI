using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using UnityEngine;

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
        private byte[] _audioData;
        private AudioClip _clip;
        private Texture2D _image;

        private List<string> _answers = new List<string>();
        private List<Question> _questions = new List<Question>();


        private int _questionsIteration;
        private int _atomIteration;

        public void Parser(string packagePath)
        {
            using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Read, Encoding.UTF8))
            {
                Debug.Log("Zip Read");
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

                                // <questions>
                                foreach (XmlNode questionsNode in themeNode.ChildNodes)
                                {
                                    // Reload QuestionList
                                    _questions = new List<Question>();

                                    // <info>
                                    if (_questionsIteration == 0)
                                    {
                                        if (questionsNode.Name == "info")
                                        {
                                            _info = questionsNode.FirstChild.InnerText;
                                            continue;
                                        }

                                        _questionsIteration++;
                                    }

                                    // <question>
                                    if (_questionsIteration == 1)
                                    {
                                        foreach (XmlNode questionNode in questionsNode)
                                        {
                                            if (questionNode.Name == "question")
                                            {
                                                // Reload AnswerList
                                                _answers = new List<string>();
                                                // Price
                                                _price = questionNode.Attributes?["price"].Value;

                                                _type = null;

                                                // <question data>
                                                bool skipType = false, skipScenario = false, skipRight = false, skipWrong = false;
                                                foreach (XmlNode questionChild in questionNode.ChildNodes)
                                                {
                                                    if (!skipType)
                                                    {
                                                        // <type>
                                                        if (questionChild.Name == "type")
                                                        {
                                                            _type = questionChild.Attributes?["name"].Value;
                                                            skipType = true;
                                                            continue;
                                                        }
                                                    }

                                                    if (!skipScenario)
                                                    {
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

                                                                            _image = tex;
                                                                            _audioData = null;
                                                                        }
                                                                    }

                                                                    // Audio
                                                                    if (atom.Attributes?["type"].Value == "voice")
                                                                    {
                                                                        var audioPath = atom.InnerText;
                                                                        var audio = archive.GetEntry("Audio/" + Uri.EscapeUriString(audioPath).Trim('@'));
                                                                        if (audio != null)
                                                                        {
                                                                            var buffer = new byte[audio.Length];
                                                                            using (var stream = audio.Open())
                                                                                stream.Read(buffer, 0, buffer.Length);
                                                                            using (var ms = new MemoryStream(buffer))
                                                                                _audioData = ms.ToArray();

                                                                            _image = null;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    _scenario = atom.InnerText;
                                                                    _image = null;
                                                                    _audioData = null;
                                                                }
                                                            }
                                                        }
                                                        skipScenario = true;
                                                        continue;
                                                    }

                                                    if (!skipRight)
                                                    {
                                                        // <right>
                                                        if (questionChild.Name == "right")
                                                        {
                                                            foreach (XmlNode answer in questionChild.ChildNodes)
                                                            {
                                                                _answers.Add(answer.InnerText);
                                                            }
                                                        }
                                                        skipRight = true;
                                                        continue;
                                                    }

                                                    if (!skipWrong)
                                                    {
                                                        // <wrong>
                                                        if (questionChild.Name == "wrong")
                                                        {
                                                            // TODO: wrong list
                                                        }
                                                        skipWrong = true;
                                                        continue;
                                                    }

                                                }

                                                // Add Question in List
                                                _questions.Add(new Question(_price, _scenario, _answers, _type, _audioData, _clip, _image));
                                            }
                                        }

                                        _questionsIteration = 0;
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