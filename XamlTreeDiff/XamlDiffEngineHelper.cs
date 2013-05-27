using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XamlTreeDiff
{
    public static class XamlDiffEngineHelper
    {
        const string NameAttributeValuePattern = @"(?<=\bName="")[^""]+";

        public static string[] Explode(string value)
        {
            return Regex.Split(value, @"");
        }
        public static string GetNameAttributeValue(string node)
        {   
            var regex = new Regex(NameAttributeValuePattern);
            var match = regex.Match(node);
            var title = match.Value;
            return string.IsNullOrWhiteSpace(title) ? string.Empty : title;
        }

        public static string GetNodeName(string node)
        {
            var indexOfStartTag = node.IndexOf('<');
            var indexOfClosingTag = node.IndexOf('>');
             
            if (indexOfStartTag < 0 || indexOfClosingTag < 0)
                    return string.Empty;

            var nodeName = IsClosingTag(node) ? 
                                  node.Substring(indexOfStartTag + 2, indexOfClosingTag - 1) :
                                  node.Substring(indexOfStartTag + 1, indexOfClosingTag - 1);
            return nodeName;
        }

        public static bool IsTag(string item)
        {
            var isTag = IsOpeningTag(item) || IsClosingTag(item);
            return isTag;
        }

        public static string GetTagName(string[] words, string word, int index)
        {
            if (!IsClosingTag(word))
            {
                var indexOfStartTag = words[index].IndexOf('<');
                var indexOfClosingTag = words[index].IndexOf('>');
                if (indexOfStartTag < 0 || indexOfClosingTag < 0)
                    return null;
                var item = words[index].Substring(indexOfStartTag + 1, indexOfClosingTag-1);
                return item;
            }
            return null;
        }


        public static bool IsOpeningTag(string item)
        {
            return Regex.IsMatch(item, "^\\s*<[^/][^>]+>\\s*$");
        }

        public static bool IsClosingTag(string item)
        {
            return Regex.IsMatch(item, "^\\s*</[^>]+>\\s*$");
        }

        public static bool IsStartOfTag(string val)
        {
            return val == "<";
        }

        public static bool IsEndOfTag(string val)
        {
            return val == ">";
        }

        public static bool IsClosingTagOf(string item)
        {
            return Regex.IsMatch(item, string.Format("^\\s*</{0}[^>]*>\\s*$",item));
        }

        public static bool IsWhiteSpace(string value)
        {
            return Regex.IsMatch(value, "\\s");
        }

        public static string[] ConvertTokensToWords(string[] characterString)
        {
            var mode = Mode.Character;
            var currentWord = String.Empty;
            var words = new List<string>();
            foreach (var character in characterString)
            {
                switch (mode)
                {
                    case Mode.Character:

                        if (IsStartOfTag(character))
                        {
                            AddToken(currentWord, words);

                            currentWord = "<";
                            mode = Mode.Tag;
                        }
                        else if (Regex.IsMatch(character, @"\s", RegexOptions.ECMAScript))
                        {
                            AddToken(currentWord, words);
                            currentWord = character;
                            mode = Mode.Whitespace;
                        }
                        else if (Regex.IsMatch(character, @"[\w\#@]+", RegexOptions.IgnoreCase | RegexOptions.ECMAScript))
                        {
                            currentWord += character;
                        }
                        else
                        {
                            AddToken(currentWord, words);
                            currentWord = character;
                        }

                        break;
                    case Mode.Tag:

                        if (IsEndOfTag(character))
                        {
                            currentWord += ">";

                            words.Add(currentWord);
                            currentWord = "";

                            mode = IsWhiteSpace(character) ? Mode.Whitespace : Mode.Character;
                        }
                        else
                        {
                            currentWord += character;
                        }

                        break;
                    case Mode.Whitespace:

                        if (IsStartOfTag(character))
                        {
                            AddToken(currentWord, words);
                            currentWord = "<";
                            mode = Mode.Tag;
                        }
                        else if (Regex.IsMatch(character, "\\s"))
                        {
                            currentWord += character;
                        }
                        else
                        {
                            AddToken(currentWord, words);

                            currentWord = character;
                            mode = Mode.Character;
                        }

                        break;
                }


            }
            AddToken(currentWord, words);
            return words.ToArray();
        }

        private static void AddToken(string currentWord, List<string> words)
        {
            if (currentWord != String.Empty && !currentWord.Contains("\r\n"))
            {
                words.Add(currentWord);
            }
        }
    }
}