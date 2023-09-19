using System;
using System.Text;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {

        static string packetEnums = "";
        static string genPackets = "";
        static string clientManagerStr = "";
        static string serverManagerStr = "";
        static ushort packetId;

        public static void Main(string[] args)
        {

            string PDLPath = "../PDL.xml";

            if (args.Length >= 1) PDLPath = args[0];

            Console.WriteLine("Path:" + PDLPath);
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };


            using (XmlReader r = XmlReader.Create(PDLPath, settings))
            {
                packetEnums = "";
                genPackets = "";
                packetId = 1;

                r.MoveToContent(); //move to main content directly
                while (r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(r);
                    }
                }

                var genPacketStr = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", genPacketStr);

                var clientMgr = string.Format(PacketFormat.managerFileFormat, clientManagerStr);
                File.WriteAllText("ClientPacketManager.cs", clientMgr);
                var serverMgr = string.Format(PacketFormat.managerFileFormat, serverManagerStr);
                File.WriteAllText("ServerPacketManager.cs", serverMgr);
            }
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType != XmlNodeType.Element) return;
            if (!r.Name.ToLower().Equals("packet")) return;

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName)) return;

            Tuple<string, string, string> members = ParseMembers(r);
            if (members == null) return;

            //{0} : class Name {1}: members {2} Serialize {3}Deserialize
            genPackets += string.Format(PacketFormat.packetFormat, packetName, members.Item1, members.Item2, members.Item3);

            if (!string.IsNullOrEmpty(packetEnums)) packetEnums += "\n";
            packetEnums += string.Format(PacketFormat.PacketIDForamt, packetName, packetId);

            if (packetName.StartsWith("C_") || packetName.StartsWith("c_"))
                serverManagerStr += string.Format(PacketFormat.managerRegisterFormat, packetName);
            else if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                clientManagerStr += string.Format(PacketFormat.managerRegisterFormat, packetName);

            packetId++;


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">reader</param>
        /// <returns>{item1}: members {item2} Serialize {item3}Deserialize</returns>
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {

            StringBuilder memberStr = new StringBuilder();
            StringBuilder serializeStr = new StringBuilder();
            StringBuilder deserializeStr = new StringBuilder();

            //string packetName = r["name"];
            int depth = r.Depth + 1;


            while (r.Read())
            {
                if (r.Depth != depth) break;
                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("member name is empty");
                    return null;
                }


                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "sbyte":
                    case "byte":
                        memberStr.AppendFormat(PacketFormat.memberFormat, memberType, memberName);
                        serializeStr.AppendFormat(PacketFormat.serializeByteFormat, memberName);
                        deserializeStr.AppendFormat(PacketFormat.deserializeByteFormat, memberName);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "doutble":
                        memberStr.AppendFormat(PacketFormat.memberFormat, memberType, memberName);
                        serializeStr.AppendFormat(PacketFormat.serializeFormat, memberName, memberType);
                        deserializeStr.AppendFormat(PacketFormat.deserializeFormat, memberName, ParseConvertFormat(memberType), memberType);
                        break;
                    case "string":
                        memberStr.AppendFormat(PacketFormat.memberFormat, memberType, memberName);
                        serializeStr.AppendFormat(PacketFormat.serializeStringFormat, memberName);
                        deserializeStr.AppendFormat(PacketFormat.deserializeStringFormat, memberName);
                        break;
                    case "list":
                        var listMem = ParseList(r);
                        if (listMem == null) return null;
                        memberStr.Append(listMem.Item1);
                        serializeStr.Append(listMem.Item2);
                        deserializeStr.Append(listMem.Item3);
                        break;
                    default:
                        break;
                }

                memberStr.Append("\n\t");
            }

            return new Tuple<string, string, string>(memberStr.ToString(), serializeStr.ToString(), deserializeStr.ToString());
        }


        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            StringBuilder memStr = new StringBuilder();
            StringBuilder serializeStr = new StringBuilder();
            StringBuilder deserializeStr = new StringBuilder();

            if (!r.Name.ToLower().Equals("list")) return null;
            if (string.IsNullOrEmpty(r["name"])) return null;

            string variableName = r["name"].ToLower();
            string structName = FirstCharToUpper(variableName);

            Tuple<string, string, string> members = ParseMembers(r);
            if (members == null) return null;

            memStr.AppendFormat(PacketFormat.listMemberFormat, structName, variableName);

            memStr.AppendFormat(PacketFormat.structFormat, structName, variableName, members.Item1, members.Item2, members.Item3);

            serializeStr.AppendFormat(PacketFormat.serializeListFormat, variableName);
            deserializeStr.AppendFormat(PacketFormat.deserializelistFormat, variableName, structName);


            return new Tuple<string, string, string>(memStr.ToString(), serializeStr.ToString(), deserializeStr.ToString());
        }


        #region String Parse
        static string ParseConvertFormat(string membertype)
        {
            switch (membertype)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "doutble":
                    return "ToDouble";
            }
            return "";
        }

        static string FirstCharToUpper(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str[0].ToString().ToUpper() + str.Substring(1);
        }
        #endregion
    }
}