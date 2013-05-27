using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XamlTreeDiff
{
    public class XamlDocument
    { 
        private const string ParentName = "Section";
        private readonly string[] _expectedLocalName = new[] {"Table", "Paragraph"};
        private readonly Dictionary<string, string[]> _contents = new Dictionary<string, string[]>();

        public XamlDocument(XDocument document)
        {
            if (document.Root == null)
                throw new ArgumentException("Document root should not be null");

            Document = document;
            GetFirstLevelNodes();
        }

        public XDocument Document { get; private set; }

        public Dictionary<string, string[]> Contents { get { return _contents; } }


        public IEnumerable<SchlampeNode> Schlampet { get; private set; }

        private void GetFirstLevelNodes()
        {
            // ReSharper disable PossibleNullReferenceException
            var firstLevelNodes = Document.Root.Descendants().Where(x => _expectedLocalName.Contains(x.Name.LocalName) && x.Parent.Name.LocalName == ParentName).ToList();
            
            CreateLookup(firstLevelNodes);

            CreateSchlampet();
        }

        private void CreateSchlampet()
        {
            var characters = XamlDiffEngineHelper.Explode(Document.Document.ToString());
            var words = XamlDiffEngineHelper.ConvertTokensToWords(characters);
            Schlampet = ConstructSchlampeNodes(words);


            //Contents.Add(key, words);
        }

        private void CreateLookup(IEnumerable<XElement> firstLevelNodes)
        {
            foreach (var firstLevelNode in firstLevelNodes)
            {
                var key = firstLevelNode.Attributes().FirstOrDefault(x => x.Name.LocalName.Equals("Name")).Value;
                var characters = XamlDiffEngineHelper.Explode(firstLevelNode.ToString());
                var words = XamlDiffEngineHelper.ConvertTokensToWords(characters);
                var schlampeNodes = ConstructSchlampeNodes(words);



                Contents.Add(key, words);
            }
        }

        private List<SchlampeNode> ConstructSchlampeNodes(IEnumerable<string> words)
        {
            var schlampeNodes = new List<SchlampeNode>();
            var index = 0;
            var wordsList = words.ToList();
            foreach (var word in wordsList)
            {
                if (XamlDiffEngineHelper.IsOpeningTag(word))
                {
                    var schlampeNode = new SchlampeNode {Node = word, Index = index};
                    schlampeNodes.Add(schlampeNode);
                    index++;
                    
                }
                    //Need to fix mapping of close tags, its index and words.
                else if (XamlDiffEngineHelper.IsClosingTag(word))
                {
                    var schlampeNode = new SchlampeNode {Index = index, Node = word};
                    if (index > 0)
                    {
                        schlampeNodes.LastOrDefault(x => x.ClosingTagNode == null).ClosingTagNode = schlampeNode;
                    }
                    index++;
                }
                else
                {
                    schlampeNodes.LastOrDefault().Value += word;
                }
            }
            return schlampeNodes;
        }
    }
}