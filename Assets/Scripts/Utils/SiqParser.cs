using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Debug = UnityEngine.Debug;

namespace Parse
{
    public class SiqParser
    {
        // Round
        private string _roundName;
        public List<Round> Rounds = new List<Round>();

        // Theme
        private string _themeName;
        private string _info;
        private List<Theme> _themes;

        // Question
        private string _price;
        private string _scenario;
        private string _type;
        private string _catTheme;
        private string _catPrice;
        private bool _isMarker;
        private byte[] _audioData;
        private byte[] _imageQuestionData;
        private byte[] _imageAnswerData;

        private List<string> _answers = new List<string>();
        private List<Question> _questions = new List<Question>();

        // Local logic
        private int _questionsIteration;
        private int _atomIteration;

        public void Parser(string packagePath)
        {
            using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Read, Encoding.UTF8))
            {
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

                                _info = null;

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
                                                _catTheme = null;
                                                _catPrice = null;

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

                                                            if (_type.Equals("cat"))
                                                            {
                                                                if (questionChild.HasChildNodes)
                                                                {
                                                                    _catTheme = questionChild.ChildNodes[0].InnerText;
                                                                    _catPrice = questionChild.ChildNodes[1].InnerText;
                                                                }
                                                                
                                                            }

                                                            skipType = true;
                                                            continue;
                                                        }
                                                    }

                                                    if (!skipScenario)
                                                    {
                                                        // <scenario>
                                                        if (questionChild.Name == "scenario")
                                                        {
                                                            _isMarker = false;
                                                            _imageQuestionData = null;
                                                            _imageAnswerData = null;

                                                            // <atom>
                                                            foreach (XmlNode atom in questionChild.ChildNodes)
                                                            {
//                                                                _imageQuestionData = null;
//                                                                _imageAnswerData = null;

                                                                // Has Attribute
                                                                if (atom.Attributes?["type"] != null)
                                                                {
                                                                    // Atom marker for image should be in the answer block
                                                                    if (atom.Attributes?["type"].Value == "marker")
                                                                    {
                                                                        _isMarker = true;
                                                                    }

                                                                    // Image
                                                                    if (atom.Attributes?["type"].Value == "image")
                                                                    {
                                                                        var imagePath = atom.InnerText;
                                                                        var image = archive.GetEntry("Images/" + Uri.EscapeUriString(imagePath).Trim('@'));
                                                                        if (image != null)
                                                                        {
                                                                            var imageData = new byte[image.Length];
                                                                            image.Open().Read(imageData, 0, imageData.Length);

                                                                            if (_isMarker == false)
                                                                            {
                                                                                _imageQuestionData = imageData;
                                                                                _scenario = null;
                                                                            }
                                                                            else
                                                                            {
                                                                                _imageAnswerData = imageData;
                                                                            }
                                                                            
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

                                                                            _imageQuestionData = null;
                                                                            _imageAnswerData = null;
                                                                            _scenario = null;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    _scenario = atom.InnerText;

                                                                    //_imageQuestionData = null;
                                                                    //_imageAnswerData = null;
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
                                                _questions.Add(new Question(_price, _scenario, _answers, _type, _isMarker, _audioData, _imageQuestionData, _imageAnswerData, _catTheme, _catPrice));
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