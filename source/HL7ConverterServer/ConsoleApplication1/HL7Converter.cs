using System.Text.RegularExpressions;

namespace HL7ToXMLServer
{
    class HL7Converter
    {
        /* Code Project functions start 
         this code function and the used sub-functions were taken from the 
         sample on: https://www.codeproject.com/Articles/29670/Converting-HL-to-XML
         and should be seen as a demonstration of HL7 parsing. Also need to check the Code Project Licensing model
         or we need to re-write this funcion ;)
         */
        // Transform the message in a form that can be interpreted by the middleman and the application
        public static string ConvertToTemplate(string sHL7)
        {
            // HL7 message segments are terminated by carriage returns,
            // so to get an array of the message segments, split on carriage return
            string[] sHL7Lines = sHL7.Split('\r');

            // Now we want to replace any other unprintable control
            // characters with whitespace otherwise they'll break the XML
            for (int i = 0; i < sHL7Lines.Length; i++)
            {
                sHL7Lines[i] = Regex.Replace(sHL7Lines[i], @"[^ -~]", "");
            }

            string finalMessage = string.Join("", sHL7Lines);

            return finalMessage;
        }
    }
}
