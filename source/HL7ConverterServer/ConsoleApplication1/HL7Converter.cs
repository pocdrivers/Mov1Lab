using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HL7ToXMLServer
{
    class HL7Converter
    {
        // xml document object.
        private static XmlDocument _xmlDoc;
        // define the elements to include into the HTML conversion here
        // map defining the translation for the HTML table
        private static Dictionary<String, String> entryMap = new Dictionary<string, string>();
        public static bool showAllInHtml = false;

        /* Code Project functions start 
         this code function and the used sub-functions were taken from the 
         sample on: https://www.codeproject.com/Articles/29670/Converting-HL-to-XML
         and should be seen as a demonstration of HL7 parsing. Also need to check the Code Project Licensing model
         or we need to re-write this funcion ;)
         */
        // creates a full blown xml of the HL7 message translating the entries to tags
        public static string ConvertToXml(string sHL7)
        {
            // Go and create the base XML
            //_xmlDoc = CreateXmlDoc();

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

            /// Go through each segment in the message
            /// and first get the fields, separated by pipe (|),
            /// then for each of those, get the field components,
            /// separated by carat (^), and check for
            /// repetition (~) and also check each component
            /// for subcomponents, and repetition within them too.
            /*for (int i = 0; i < sHL7Lines.Length; i++)
            {
                // Don't care about empty lines
                if (sHL7Lines[i] != string.Empty)
                {
                    // Get the line and get the line's segments
                    string sHL7Line = sHL7Lines[i];
                    string[] sFields = HL7Converter.GetMessgeFields(sHL7Line);

                    // Create a new element in the XML for the line
                    XmlElement el = _xmlDoc.CreateElement(sFields[0]);
                    _xmlDoc.DocumentElement.AppendChild(el);

                    // For each field in the line of HL7
                    for (int a = 0; a < sFields.Length; a++)
                    {
                        // Create a new element
                        XmlElement fieldEl = _xmlDoc.CreateElement(sFields[0] +
                                             "." + a.ToString());

                        /// Part of the HL7 specification is that part
                        /// of the message header defines which characters
                        /// are going to be used to delimit the message
                        /// and since we want to capture the field that
                        /// contains those characters we need
                        /// to just capture them and stick them in an element.
                       /* if (sFields[a] != @"^~\&")
                        {
                            /// Get the components within this field, separated by carats (^)
                            /// If there are more than one, go through and create an element for
                            /// each, then check for subcomponents, and repetition in both.
                            string[] sComponents = HL7Converter.GetComponents(sFields[a]);
                            if (sComponents.Length > 1)
                            {
                                for (int b = 0; b < sComponents.Length; b++)
                                {
                                    XmlElement componentEl = _xmlDoc.CreateElement(sFields[0] +
                                               "." + a.ToString() +
                                               "." + b.ToString());

                                    string[] subComponents = GetSubComponents(sComponents[b]);
                                    if (subComponents.Length > 1)
                                    // There were subcomponents
                                    {
                                        for (int c = 0; c < subComponents.Length; c++)
                                        {
                                            // Check for repetition
                                            string[] subComponentRepetitions =
                                                     GetRepetitions(subComponents[c]);
                                            if (subComponentRepetitions.Length > 1)
                                            {
                                                for (int d = 0;
                                                     d < subComponentRepetitions.Length;
                                                     d++)
                                                {
                                                    XmlElement subComponentRepEl =
                                                      _xmlDoc.CreateElement(sFields[0] +
                                                      "." + a.ToString() +
                                                      "." + b.ToString() +
                                                      "." + c.ToString() +
                                                      "." + d.ToString());
                                                    subComponentRepEl.InnerText =
                                                         subComponentRepetitions[d];
                                                    componentEl.AppendChild(subComponentRepEl);
                                                }
                                            }
                                            else
                                            {
                                                XmlElement subComponentEl =
                                                  _xmlDoc.CreateElement(sFields[0] +
                                                  "." + a.ToString() + "." +
                                                  b.ToString() + "." + c.ToString());
                                                subComponentEl.InnerText = subComponents[c];
                                                componentEl.AppendChild(subComponentEl);

                                            }
                                        }
                                        fieldEl.AppendChild(componentEl);
                                    }
                                    else // There were no subcomponents
                                    {
                                        string[] sRepetitions =
                                           HL7Converter.GetRepetitions(sComponents[b]);
                                        if (sRepetitions.Length > 1)
                                        {
                                            XmlElement repetitionEl = null;
                                            for (int c = 0; c < sRepetitions.Length; c++)
                                            {
                                                repetitionEl =
                                                  _xmlDoc.CreateElement(sFields[0] + "." +
                                                  a.ToString() + "." + b.ToString() +
                                                  "." + c.ToString());
                                                repetitionEl.InnerText = sRepetitions[c];
                                                componentEl.AppendChild(repetitionEl);
                                            }
                                            fieldEl.AppendChild(componentEl);
                                            el.AppendChild(fieldEl);
                                        }
                                        else
                                        {
                                            componentEl.InnerText = sComponents[b];
                                            fieldEl.AppendChild(componentEl);
                                            el.AppendChild(fieldEl);
                                        }
                                    }
                                }
                                el.AppendChild(fieldEl);
                            }
                            else
                            {
                                fieldEl.InnerText = sFields[a];
                                el.AppendChild(fieldEl);
                            }
                        }
                        else
                        {
                            fieldEl.InnerText = sFields[a];
                            el.AppendChild(fieldEl);
                        }
                    }
                }
            }*/
            return finalMessage; //_xmlDoc.OuterXml;
        }

        /// Split a line into its component parts based on pipe.
        private static string[] GetMessgeFields(string s)
        {
            return s.Split('|');
        }

        
        /// Get the components of a string by splitting based on carat.
        private static string[] GetComponents(string s)
        {
            return s.Split('^');
        }

        /// Get the subcomponents of a string by splitting on ampersand.
        private static string[] GetSubComponents(string s)
        {
            return s.Split('&');
        }

        /// Get the repetitions within a string based on tilde.
        private static string[] GetRepetitions(string s)
        {
            return s.Split('~');
        }

        /// Create the basic XML document that represents the HL7 message
        private static XmlDocument CreateXmlDoc()
        {
            XmlDocument output = new XmlDocument();
            XmlElement rootNode = output.CreateElement("HL7Message");
            output.AppendChild(rootNode);
            return output;
        }

        /* Code Project functions end */

        // convert the passed XML string to HTML output
        public static string ConvertToHtml(string xmlMessage)
        {
            BuildMap();
            StringBuilder html = new StringBuilder("<h1 align=\"center\">HL7 Result Values Passed to HIS/LIS</h1><br><br><table align='center' " +
             "border='1' class='xmlTable'>\r\n");
            html.Append(ConvertToTable(xmlMessage));
            html.Append("</table>");
            return html.ToString();
        }
        
        // converts the passed xml string to a html table
        private static string ConvertToTable(string xmlMessage)
        {
            StringBuilder html = new StringBuilder();
            try
            {
                XDocument xDocument = XDocument.Parse(xmlMessage);
                XElement root = xDocument.Root;

                var xmlAttributeCollection = root.Elements().Attributes();

                foreach (var ele in root.Elements())
                {
                    if (!ele.HasElements)
                    {
                        string elename = "";
                        
                        elename = ele.Name.ToString();
                        if (showEntry(elename))
                        {
                            html.Append("<tr>");
                            html.Append("<td bgcolor='LightSlateGray'>" + ConvertToReadableTableEntry(elename) + "</td>");
                            html.Append("<td bgcolor='lightgrey'>" + ele.Value + "</td>");
                            html.Append("</tr>");
                        }
                    }
                    else
                    {
                        string elename = "";
                        elename = ele.Name.ToString();
                        if (showEntry(elename))
                        {
                            if (showAllInHtml)
                            {
                                if (!elename.Contains("."))
                                {
                                    html.Append("<tr bgcolor='darkgrey'>");
                                    html.Append("<td align='center' colspan='2'>" + ConvertToReadableTableEntry(elename) + "</td>");
                                    html.Append("</tr>");
                                    html.Append(ConvertToTable(ele.ToString()));
                                }
                                else
                                {
                                    html.Append("<tr>");
                                    html.Append("<td bgcolor='darkgrey'>" + ConvertToReadableTableEntry(elename) + "</td>");
                                    html.Append("<td bgcolor='lightgrey'>" + ConvertToTable(ele.ToString()) + "</td>");
                                    html.Append("</tr>");
                                }
                            }
                            else
                            {
                                html.Append(ConvertToTable(ele.ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return xmlMessage;
                // Returning the original string incase of error.
            }
            return html.ToString();

        }

        // function deciding if an element should be shown
        private static bool showEntry(string elementName)
        {
            if (entryMap.ContainsKey("ALL")) return true;;
            return entryMap.ContainsKey(elementName);
        }

        // convert passed element name to displayable entry
        private static string ConvertToReadableTableEntry(string elementName)
        {
            String returnValue = elementName;
            if (showEntry(elementName))
            {
                entryMap.TryGetValue(elementName, out returnValue);
            }
            if (String.IsNullOrEmpty(returnValue)) returnValue = elementName;
            return returnValue;
        }

        // buzid the temporary conversion map.
        private static void BuildMap()
        {
            if (entryMap.Count()==0)
            {
                if (showAllInHtml)
                {
                    entryMap.Add("ALL", "ALL");
                }
                else
                {
                    entryMap.Add("MSH", "Header");
                    entryMap.Add("MSH.6", "Result Sent To HIS/LIS Date");
                    entryMap.Add("OBX", "Row");
                    entryMap.Add("OBX.3", "Result Name");
                    entryMap.Add("OBX.5", "Result Value");
                }
            }
        }
    }
}
