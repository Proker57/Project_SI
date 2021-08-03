using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace BOYAREngine.Game
{
    public class AuthorParser
    {
        public string AuthorName;
        public string Description;

        public void Parse(string packagePath)
        {
            using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Read, Encoding.UTF8))
            {
                var content = archive.GetEntry("content.xml");
                using (var contentReader = new StreamReader(content?.Open() ?? throw new ArgumentNullException()))
                {
                    var doc = new XmlDocument();
                    doc.Load(contentReader);

                    var author = doc.GetElementsByTagName("authors");

                    var description = doc.GetElementsByTagName("package").Item(0)?.Attributes?["name"].InnerText;
                    AuthorName = author[0].InnerText;
                    Description = description;
                }
            }
        }
    }
}

