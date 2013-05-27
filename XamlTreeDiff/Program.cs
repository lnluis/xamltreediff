using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XamlTreeDiff
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = "Paragraph";
            var b = a.GetHashCode();
            var c = "Paragraph";
            var d = c.GetHashCode();

            var streamReader = new StreamReader(@"C:\Users\lnluis\Downloads\VersionHistoryDifference\VersionHistoryDifference\bin\Debug\new.xml");
            var newText = streamReader.ReadToEnd();
            streamReader.Close();

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(newText));
            var newDocument = XDocument.Load(memoryStream);
            memoryStream.Close();

            streamReader = new StreamReader(@"C:\Users\lnluis\Downloads\VersionHistoryDifference\VersionHistoryDifference\bin\Debug\old.xml");
            var oldText = streamReader.ReadToEnd();
            streamReader.Close();

            memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(oldText));

            
            var oldDocument = XDocument.Load(memoryStream);

            memoryStream.Close();

            var test = new SchlampeEngine(oldText, newText);
            test.Compare();

            var childNodes = newDocument.Root.Descendants();
            Console.WriteLine("Displaying new doc descendants..");

            var aa =
                new StreamWriter(@"C:\Users\lnluis\Downloads\VersionHistoryDifference\VersionHistoryDifference\bin\Debug\merged.xml");
            aa.Write(test.Output);
            aa.Close();
            foreach (var childNode in childNodes.Where(x => (x.Name.LocalName == "Table" || x.Name.LocalName == "Paragraph") && x.Parent.Name.LocalName == "Section"))
            {
                Console.WriteLine(childNode);
            }
            Console.WriteLine("Displaying old doc descendants..");

            childNodes = oldDocument.Root.Descendants();
            foreach (var childNode in childNodes.Where(x => (x.Name.LocalName == "Table" || x.Name.LocalName == "Paragraph") && x.Parent.Name.LocalName == "Section"))
            {
                Console.WriteLine(childNode);
            }

            Console.Read();
        }
    }
}
