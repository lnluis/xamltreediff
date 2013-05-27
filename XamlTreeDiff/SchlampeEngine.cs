using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace XamlTreeDiff
{
    public class SchlampeEngine
    {
        readonly StringBuilder _stringBuilder = new StringBuilder();

        public SchlampeEngine(string oldXaml, string newXaml)
        {
            OldDocumentXaml = SetupInitialContext(oldXaml);
            NewDocumentXaml = SetupInitialContext(newXaml);
        }

        public string Output { get; set; }

        protected XamlDocument NewDocumentXaml { get; private set; }

        protected XamlDocument OldDocumentXaml { get; private set; }

        private static XamlDocument SetupInitialContext(string xamlText)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xamlText)))
            {
                return new XamlDocument(XDocument.Load(memoryStream));
            }
        }

        public void Compare()
        {
            //FindMatches(OldDocumentXaml.Contents, NewDocumentXaml.Contents);
            BeginTheSchlampe(OldDocumentXaml.Schlampet.ToList(), NewDocumentXaml.Schlampet.ToList());


            Output = _stringBuilder.ToString();
        }

        private void BeginTheSchlampe(IList<SchlampeNode> oldSchlampet, IList<SchlampeNode> newSchlampet)
        {
            int oldWordIndex = 0, newWordIndex = 0;
            int continueForOldSchlampe = 0;
            while (true)
            {
                if (newWordIndex == newSchlampet.Count)
                {
                    while(oldWordIndex != oldSchlampet.Count)
                    {
                        _stringBuilder.Append(oldSchlampet[oldWordIndex].Node);
                        oldWordIndex++;
                    }
                    break;
                }
                var newNode = newSchlampet[newWordIndex];
                if (oldSchlampet[oldWordIndex].Key.Equals(newNode.Key))
                {
                    _stringBuilder.Append(oldSchlampet[oldWordIndex].Node);
                    oldWordIndex++;
                    newWordIndex++;
                }
                else
                {
                    int indexToStop = newSchlampet[newWordIndex].ClosingTagNode.Index + 1;
                    int getIndexToLastCloseNode = newSchlampet.IndexOf(newSchlampet[newWordIndex]);
                    //Keep writing the open tags and values
                    while (newSchlampet[newWordIndex].Index != indexToStop)
                    {
                            _stringBuilder.Append(newSchlampet[newWordIndex].Node);

                            if (newSchlampet[newWordIndex].Node.Contains("Paragraph"))
                            {
                                _stringBuilder.Append(newSchlampet[newWordIndex].Value);
                            }
                        newWordIndex++;
                    }
                    //Close the tags
                    int copyOfStartingIndex = --newWordIndex;
                    while (copyOfStartingIndex != getIndexToLastCloseNode-1)
                    {
                        _stringBuilder.Append(newSchlampet[copyOfStartingIndex].ClosingTagNode.Node);
                        copyOfStartingIndex--;
                    }

                    //Copy old 
                    while (oldWordIndex != oldSchlampet.Count)
                    {
                        var schlampeNode = newSchlampet.FirstOrDefault(x => x.Key == oldSchlampet[oldWordIndex].Key);
                        if (schlampeNode != null && oldSchlampet[oldWordIndex].Key.Contains(schlampeNode.Key))
                        {
                            _stringBuilder.Append(oldSchlampet[oldWordIndex].Node);
                            _stringBuilder.Append(oldSchlampet[oldWordIndex].Value);
                            _stringBuilder.Append(newSchlampet.FirstOrDefault(x => x.Key == oldSchlampet[oldWordIndex].Key).Value);
                            _stringBuilder.Append(oldSchlampet[oldWordIndex].ClosingTagNode.Node);
                            oldWordIndex++;
                            newWordIndex++;
                        }
                        else
                        {
                            _stringBuilder.Append(oldSchlampet[oldWordIndex].Node);
                            _stringBuilder.Append(oldSchlampet[oldWordIndex].Value);
                            _stringBuilder.Append(oldSchlampet[oldWordIndex].ClosingTagNode.Node);
                            oldWordIndex++;
                        }
                    }


                    //Continue
                    //dinagdag ko to ngayon 5/27/2013
                    newWordIndex++;
                    if (newSchlampet[getIndexToLastCloseNode-1].ClosingTagNode.Index == newSchlampet[newWordIndex].Index)
                        continue;
                    while (newWordIndex < newSchlampet.Count)
                    {
                        _stringBuilder.Append(newSchlampet[newWordIndex].Node);
                        
                        if (newSchlampet[newWordIndex].Node.Contains("Paragraph"))
                        {
                            _stringBuilder.Append(newSchlampet[newWordIndex].Value);
                            _stringBuilder.Append(newSchlampet[newWordIndex].ClosingTagNode.Node);
                        }
                        
                        newWordIndex++;
                    }

                    //if (newWordIndex == newSchlampet.Count)
                    //{
                    //    _stringBuilder.Append(newSchlampet[newWordIndex-1].ClosingTagNode.Node);
                    //}


                    //Close the opened tag
                    
                    _stringBuilder.Append(newSchlampet[getIndexToLastCloseNode - 1].ClosingTagNode.Node);
                    //Close remaining previous opened tags=
                    while (copyOfStartingIndex >= 0)
                    {
                        _stringBuilder.Append(newSchlampet[copyOfStartingIndex].ClosingTagNode.Node);
                        copyOfStartingIndex--;
                    }
                    //TODO: Determine how to stop the tag that was different and go back to old schlampe
                }

            }
        }

        void FindMatches(Dictionary<string, string[]> oldDocumentWords, Dictionary<string, string[]> newDocumentWords)
        {
            foreach (var oldDocumentKey in oldDocumentWords.Keys)
            {
                if (newDocumentWords.ContainsKey(oldDocumentKey))
                {
                    CompareContentsBetween(oldDocumentWords[oldDocumentKey], newDocumentWords[oldDocumentKey]);
                }
            }


        }

        void CompareContentsBetween(string[] oldDocumentWord, string[] newDocumentWord)
        {
            bool foundMatchAgain = false;
            for (var oldWordIndex = 0; oldWordIndex < oldDocumentWord.Length; oldWordIndex++)
            {
                int newWordIndex = oldWordIndex;
                if (foundMatchAgain)
                {
                    newWordIndex--;
                }
                if (oldWordIndex >= newDocumentWord.Length)
                    break;

                if (newDocumentWord[newWordIndex].Equals(oldDocumentWord[oldWordIndex]))
                {
                    _stringBuilder.Append(oldDocumentWord[oldWordIndex]);
                }
                else
                {
                    if (!oldDocumentWord[oldWordIndex].Equals(newDocumentWord[newWordIndex]))
                    {
                        var closingTag = FindClosingTag(oldDocumentWord, oldWordIndex);
                        while (true)
                        {
                            _stringBuilder.Append(oldDocumentWord[oldWordIndex]);
                            oldWordIndex++;
                            if (oldWordIndex >= oldDocumentWord.Length)
                                break;

                            if (newWordIndex >= newDocumentWord.Length)
                                break;

                            //if (newDocumentWord[newWordIndex].Equals(oldDocumentWord[oldWordIndex]))
                            //{
                            //    foundMatchAgain = true;
                            //    break;
                            //}
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            _stringBuilder.Append(newDocumentWord[newWordIndex]);
                            newWordIndex++;
                            if (newWordIndex >= newDocumentWord.Length)
                                break;

                            if (oldWordIndex >= oldDocumentWord.Length)
                                break;
                            if (newDocumentWord[newWordIndex].Equals(oldDocumentWord[oldWordIndex]))
                            {
                                _stringBuilder.Append(newDocumentWord[newWordIndex]);
                                foundMatchAgain = true;
                                break;
                            }
                        }
                    }

                }
            }

        }

        string FindClosingTag(string[] words, int index)
        {
            var tagNameToFind = words[index];
            for (var i = index; i < words.Length; i++)
            {
                var tagName = XamlDiffEngineHelper.GetTagName(words, tagNameToFind, i);
                if (!string.IsNullOrWhiteSpace(tagName) && XamlDiffEngineHelper.IsClosingTagOf(tagName))
                {
                    return words[index];
                }
            }
            return null;
        }
    }
}