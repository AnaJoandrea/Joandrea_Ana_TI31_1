using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace WordFrequency
{
    public class DocumentData
    {
        public string DocName { get; set; }
        public Dictionary<int, int> SparseVector { get; set; } = new Dictionary<int, int>();
        public List<string> Topics { get; set; } = new List<string>();
        public string FolderType { get; set; }
    }

    public class FeatureExtractor
    {
        private HashSet<string> stopwords;
        private Dictionary<string, int> globalDictionary;
        private List<string> globalWordsList;
        private List<DocumentData> processedDocuments;
        private PorterStemmer stemmer;

        public FeatureExtractor(string stopwordsPath)
        {
            stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            globalDictionary = new Dictionary<string, int>();
            globalWordsList = new List<string>();
            processedDocuments = new List<DocumentData>();
            stemmer = new PorterStemmer();

            LoadStopwords(stopwordsPath);
        }

        private void LoadStopwords(string path)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                MatchCollection matches = Regex.Matches(line.ToLowerInvariant(), @"[a-z]+");
                foreach (Match match in matches)
                {
                    stopwords.Add(match.Value);
                }
            }

            string[] extraContractions = {
                "ve", "re", "ll", "don", "doesn", "didn", "won", "wouldn",
                "couldn", "shouldn", "isn", "aren", "wasn", "weren", "hasn", "haven", "hadn"
            };
            foreach (string c in extraContractions)
            {
                stopwords.Add(c);
            }
        }

        public void AddDocuments(string[] xmlFiles, string folderLabel)
        {
            foreach (string file in xmlFiles)
            {
                DocumentData docData = ParseXmlDocument(file, folderLabel);
                if (docData != null)
                {
                    processedDocuments.Add(docData);
                }
            }
        }

        private DocumentData ParseXmlDocument(string filePath, string folderLabel)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            StringBuilder contentBuilder = new StringBuilder();

            XmlNode titleNode = xmlDoc.SelectSingleNode("//title");
            if (titleNode != null)
            {
                contentBuilder.Append(titleNode.InnerText).Append(" ");
            }

            XmlNodeList paragraphNodes = xmlDoc.SelectNodes("//text/p");
            if (paragraphNodes != null && paragraphNodes.Count > 0)
            {
                foreach (XmlNode pNode in paragraphNodes)
                {
                    contentBuilder.Append(pNode.InnerText).Append(" ");
                }
            }
            else
            {
                XmlNode textNode = xmlDoc.SelectSingleNode("//text");
                if (textNode != null)
                {
                    contentBuilder.Append(textNode.InnerText).Append(" ");
                }
            }

            List<string> topics = new List<string>();
            XmlNodeList codeNodes = xmlDoc.SelectNodes("//codes[@class='bip:topics:1.0']/code");
            if (codeNodes != null)
            {
                foreach (XmlNode codeNode in codeNodes)
                {
                    string codeAttr = codeNode.Attributes?["code"]?.Value;
                    if (!string.IsNullOrEmpty(codeAttr))
                    {
                        topics.Add(codeAttr);
                    }
                }
            }

            Dictionary<int, int> localFreq = new Dictionary<int, int>();
            string rawText = contentBuilder.ToString().ToLowerInvariant();

            MatchCollection matches = Regex.Matches(rawText, @"[a-z]+");

            foreach (Match match in matches)
            {
                string word = match.Value;

                if (word.Length <= 1 || stopwords.Contains(word))
                    continue;

                string root = stemmer.StemWord(word);

                if (string.IsNullOrEmpty(root) || root.Length <= 1 || stopwords.Contains(root))
                    continue;

                if (!globalDictionary.TryGetValue(root, out int wordIndex))
                {
                    wordIndex = globalWordsList.Count;
                    globalDictionary[root] = wordIndex;
                    globalWordsList.Add(root);
                }

                if (localFreq.ContainsKey(wordIndex))
                    localFreq[wordIndex]++;
                else
                    localFreq[wordIndex] = 1;
            }

            return new DocumentData
            {
                DocName = Path.GetFileNameWithoutExtension(filePath),
                SparseVector = localFreq,
                Topics = topics,
                FolderType = folderLabel
            };
        }

        public void SaveResults(string outputPath)
        {
            using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {

                foreach (string word in globalWordsList)
                {
                    writer.WriteLine($"@attribute {word}");
                }

                writer.WriteLine();
                writer.WriteLine("@data");

                foreach (var doc in processedDocuments)
                {
                    var sortedPairs = doc.SparseVector
                        .OrderBy(kvp => kvp.Key)
                        .Select(kvp => $"{kvp.Key}:{kvp.Value}");

                    string sparseString = string.Join(" ", sortedPairs);
                    string topicsString = string.Join(" ", doc.Topics);

                    writer.WriteLine($"{doc.DocName} # {sparseString} # {topicsString} # {doc.FolderType}");
                }
            }
        }

        public void SaveGlobalDictionary(string dictionaryPath)
        {
            using (StreamWriter writer = new StreamWriter(dictionaryPath, false, Encoding.UTF8))
            {
                for (int i = 0; i < globalWordsList.Count; i++)
                {
                    writer.WriteLine($"{i} {globalWordsList[i]}");
                }
            }
        }
    }
}