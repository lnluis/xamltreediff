using System.Xml.Linq;

namespace XamlTreeDiff
{
    /// <summary>
    /// The schlampe, duh?
    /// </summary>
    public class SchlampeNode
    {

        /// <summary>
        /// The closing node of the node
        /// </summary>
        public SchlampeNode ClosingTagNode { get; set; }

        /// <summary>
        /// The text value if it has any
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Complete node element with its attributes
        /// </summary>
        public string Node { get; set; }

        /// <summary>
        /// Index in the explosion of the tokens
        /// </summary>
        public int Index { get; set; }


        /// <summary>
        /// Unique identifier for comparison between other node
        /// </summary>
        public string Key {
            get { return XamlDiffEngineHelper.GetNameAttributeValue(Node); }
        }


        /// <summary>
        /// Custom name will be used to replace the WPF controls with the custom controls
        /// </summary>
        public string CustomName
        {
            get { return Node.Replace(XamlDiffEngineHelper.GetNodeName(Node), string.Format("Custom{0}", XamlDiffEngineHelper.GetNodeName(Node))); }
        }

    }
}