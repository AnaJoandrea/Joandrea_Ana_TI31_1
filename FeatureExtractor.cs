using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WordFrequency
{
    public class FeatureExtractor
    {
        private HashSet<string> _stopwords;
        private PorterStemmer _stemmer;
        private Dictionary<string, int> _globalDictionary;
        private int _currentWordIndex;

        public FeatureExtractor(string stopwordsFilePath)
        {
            _stemmer = new PorterStemmer();
            _globalDictionary = new Dictionary<string, int>();
            _currentWordIndex = 0;
            _stopwords = new HashSet<string>(
                File.ReadAllLines(stopwordsFilePath).Select(w => w.Trim().ToLower())
            );
        }

        public void ProcessDocuments(string[] xmlFilePaths, string outputFilePath, string sourceFolderName)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                foreach (string filePath in xmlFilePaths)
                {
                    string docName = Path.GetFileNameWithoutExtension(filePath);
                    XDocument xmlDoc = XDocument.Load(filePath);
                    string title = xmlDoc.Descendants("title").FirstOrDefault()?.Value ?? "";
                    var paragraphs = xmlDoc.Descendants("text").Descendants("p").Select(p => p.Value);
                    string fullText = title + " " + string.Join(" ", paragraphs);
                    var topicNodes = xmlDoc.Descendants("codes")
                                           .Where(c => (string)c.Attribute("class") == "bip:topics:1.0")
                                           .Descendants("code");

                    List<string> topics = topicNodes.Select(c => (string)c.Attribute("code")).ToList();
                    string topicsString = string.Join(" ", topics);
                    Dictionary<int, int> localVector = new Dictionary<int, int>();

                    MatchCollection words = Regex.Matches(fullText.ToLower(), @"[a-z]+");

                    foreach (Match match in words)
                    {
                        string word = match.Value;

                        if (_stopwords.Contains(word))
                            continue;

                        string stemmedWord = _stemmer.StemWord(word);

                        if (string.IsNullOrWhiteSpace(stemmedWord))
                            continue;

                        if (!_globalDictionary.TryGetValue(stemmedWord, out int wordId))
                        {
                            wordId = _currentWordIndex++;
                            _globalDictionary[stemmedWord] = wordId;
                        }

                        if (localVector.ContainsKey(wordId))
                            localVector[wordId]++;
                        else
                            localVector[wordId] = 1;
                    }

                    var rareVectorElements = localVector.OrderBy(kvp => kvp.Key)
                                                        .Select(kvp => $"{kvp.Key}:{kvp.Value}");

                    string rareVectorString = string.Join(" ", rareVectorElements);

                    string outputLine = $"{docName} # {rareVectorString} # {topicsString} # {sourceFolderName}";
                    writer.WriteLine(outputLine);
                }
            }
        }

        public void SaveGlobalDictionary(string dictionaryFilePath)
        {
            using (StreamWriter writer = new StreamWriter(dictionaryFilePath, false, Encoding.UTF8))
            {
                foreach (var kvp in _globalDictionary.OrderBy(k => k.Value))
                {
                    writer.WriteLine($"{kvp.Value} {kvp.Key}");
                }
            }
        }
    }
}