using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace OSMToSCT
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo dir;
            FileInfo singleFile;
            String path;

            if (args.Length < 1)
            {
                Console.WriteLine("No path specified. Enter path.");
                path = Console.ReadLine().Trim('"', '\'');
            }
            else
            {
                path = args[0].Trim('"', '\'');
            }

            try
            {
                dir = new DirectoryInfo(path);
            }
            catch (ArgumentException argException)
            {
                Console.WriteLine("Error: " + argException.Message);
                Console.ReadLine();
                return;
            }

            if (!dir.Exists)
            {
                singleFile = new FileInfo(path);

                if (singleFile.Exists)
                {
                    ConvertToSCT(singleFile);
                    Console.WriteLine("Done. Press enter to close.");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid path. Press enter to continue.");
                    Console.Read();
                    return;
                }
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Name.ToUpper().Contains(".OSM") || file.Name.ToUpper().Contains(".XML"))
                {
                    Console.WriteLine("Converting " + file.Name);
                    ConvertToSCT(file);
                }
                else
                {
                    Console.WriteLine("Skipping " + file.Name);
                }
            }

            Console.WriteLine("Done. Press enter to close.");
            Console.ReadLine();
        }

        protected static void ConvertToSCT(FileInfo file)
        {
            FileInfo newFile;
            StreamWriter newFileWriter;
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator xpNodeIterator;
            List<int> nodeOrderList;
            Dictionary<int, Point> nodeDict;
            decimal decLatitude;
            decimal decLongitude;
            int nodeId;

            nodeOrderList = new List<int>();
            nodeDict = new Dictionary<int, Point>();

            newFile = new FileInfo(file.FullName.Replace(".osm", "").Replace(".xml", "") + ".txt");
            newFileWriter = newFile.CreateText();

            try
            {
                xpDoc = new XPathDocument(file.OpenRead());
                xpNav = xpDoc.CreateNavigator();

                // Iterate throught the node definitions
                xpNodeIterator = xpNav.Select("/osm/node");

                while (xpNodeIterator.MoveNext())
                {
                    try
                    {
                        nodeId = Int32.Parse(xpNodeIterator.Current.GetAttribute("id", ""));
                        decLatitude = Decimal.Parse(xpNodeIterator.Current.GetAttribute("lat", ""));
                        decLongitude = Decimal.Parse(xpNodeIterator.Current.GetAttribute("lon", ""));

                        nodeDict.Add(nodeId, new Point() { Latitude = decLatitude, Longitude = decLongitude });
                    }
                    catch (FormatException formatException)
                    {
                        Console.WriteLine("Error parsing lat/lon: " + xpNodeIterator.Current.ToString() + Environment.NewLine + formatException.Message);
                    }
                }

                // Iterate through the way definition
                xpNodeIterator = xpNav.Select("/osm/way/nd");

                while (xpNodeIterator.MoveNext())
                {
                    try
                    {
                        nodeId = Int32.Parse(xpNodeIterator.Current.GetAttribute("ref", ""));
                        nodeOrderList.Add(nodeId);
                    }
                    catch (FormatException formatException)
                    {
                        Console.WriteLine("Error parsing lat/lon: " + xpNodeIterator.Current.ToString() + Environment.NewLine + formatException.Message);
                    }
                }
            }
            catch (XmlException xmlException)
            {
                Console.WriteLine("XML Error: " + xmlException.ToString());
            }
            catch (ArgumentException argException)
            {
                Console.WriteLine("Argument Error: " + argException.ToString());
            }

            // Write out the nodes in order
            foreach (int nodeRef in nodeOrderList)
            {
                if (nodeDict.ContainsKey(nodeRef))
                {
                    newFileWriter.WriteLine(String.Format("\t{0} {1}",
                                                          LatitudeDecimalToDMS(nodeDict[nodeRef].Latitude),
                                                          LongitudeDecimalToDMS(nodeDict[nodeRef].Longitude)));
                }
            }

            newFileWriter.Flush();
            newFileWriter.Close();
            newFileWriter.Dispose();
        }

        protected static String LatitudeDecimalToDMS(decimal latitudeDecimal)
        {
            String latitudeDMS;
            decimal latitudeM;
            decimal latitudeS;
            int latitudeSRemainder;

            latitudeDMS = "";

            if (latitudeDecimal >= 0)
            {
                latitudeDMS += "N";
            }
            else
            {
                latitudeDecimal = -latitudeDecimal;
                latitudeDMS += "S";
            }

            latitudeM = (latitudeDecimal - (int)latitudeDecimal) * 60;
            latitudeS = (latitudeM - (int)latitudeM) * 60;
            latitudeSRemainder = (int)((latitudeS - (int)latitudeS) * 1000);

            latitudeDMS += String.Format("{0:000}.{1:00}.{2:00}.{3:000}",
                                         (int)latitudeDecimal,
                                         (int)latitudeM,
                                         (int)latitudeS,
                                         latitudeSRemainder);

            return latitudeDMS;
        }

        protected static String LongitudeDecimalToDMS(decimal longitudeDecimal)
        {
            String longitudeDMS;
            decimal longitudeM;
            decimal longitudeS;
            int longitudeSRemainder;

            longitudeDMS = "";

            if (longitudeDecimal >= 0)
            {
                longitudeDMS += "E";
            }
            else
            {
                longitudeDecimal = -longitudeDecimal;
                longitudeDMS += "W";
            }

            longitudeM = (longitudeDecimal - (int)longitudeDecimal) * 60;
            longitudeS = (longitudeM - (int)longitudeM) * 60;
            longitudeSRemainder = (int)((longitudeS - (int)longitudeS) * 1000);


            longitudeDMS += String.Format("{0:000}.{1:00}.{2:00}.{3:000}",
                                         (int)longitudeDecimal,
                                         (int)longitudeM,
                                         (int)longitudeS,
                                         longitudeSRemainder);

            return longitudeDMS;
        }

        protected struct Point
        {
            public decimal Latitude;
            public decimal Longitude;
        }
    }
}
