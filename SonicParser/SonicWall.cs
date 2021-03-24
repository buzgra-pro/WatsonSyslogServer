using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;


namespace SonicParser
{
    class SonicWall
    {

        public string ConStr;
        public static SqlConnection SqlCon;
        public  DataTable Sonictable = new DataTable();
        public SqlCommand Cmd = new SqlCommand();
        public static long totRecs;
        public int BulkInsertBlockSize;

        string currentLine;
        string pvtDateTime;
        string pvtTag;
        string pvtSN;
        string pvtFW;
        string pvtPri;
        string pvtC;
        string pvtgCat;
        string pvtM;
        string pvtMsg;
        string pvtSrcMAC;
        string pvtSrcIP;
        string pvtSrcZone;
        string pvtnatSrc;
        string pvtDestMac;
        string pvtDestZone;
        string pvtDestIP;
        string pvtnatDst;
        string lastTagProcessed;
        string nextTag;
        string pvtProto;
        string pvtSent;
        string pvtRcvd;
        string pvtsPkt;
        string pvtrPkt;
        string pvtcDur;
        string pvtxRule;
        string pvtApp;
        string pvtAppName;
        string pvtSid;
        string pvtIpscat;
        string pvtTagN;
        string pvtVpnPolicy;
        Boolean seekingLastTag; 


        int TagCount;
        ListBox Lbi;
        string[] Cols = { "sn", "fw","pri","c","gcat","m","msg",
            "srcMac","natSrc","src","srcZone", "dstMac" ,"dst",
            "dstZone","natDst","proto","sent","rcvd","spkt","rpkt","cdur","rule"
         ,"vpnpolicy","note","app","appName","n"};
       
        public SonicWall(string[] lines, ListBox LB)
        {
            int num = 1;
            //Im the constructor
            BulkInsertBlockSize = 1000;
            Connectdb();
            totRecs = GetLastRecord(SqlCon.ConnectionString, "SELECT MAX(RecordNumber) AS MaxRec FROM SonicLog") + 1;

            Lbi = LB;
            foreach (string line in lines)
            {
                currentLine = line;
                TestDate(currentLine);
                TestTAG(currentLine);
             foreach (string col in Cols)
                {
                    processNextTag(col);

                }
                DoInsertToDataTable(lines.Length);
                num++;
            }


        }



        private void processNextTag(string Tag)
        {
            switch (Tag)
            {
                case "sn":
                    TestSN(currentLine);
                    break;
                case "fw":
                    TestFW(currentLine);
                    break;
                case "pri":
                    TestPri(currentLine);
                    break;
                case "c":
                    TestC(currentLine);
                    break;
                case "gcat":
                    TestgCat(currentLine);
                    break;

                case "m":
                    TestM(currentLine);
                    break;
                case "msg":
                    TestMsg(currentLine);
                    break;
                case "srcMac":

                    TestSRC(currentLine);
                    break;
                case "natSrc":
                    TestnatSrc(currentLine);
                    break;
                case "src":
                    TestSRC(currentLine);
                    break;
                case "srcZone":
                    TestSrcZone(currentLine);
                    break;
                case "dstMac":
                    TestDestMac(currentLine);
                    break;
                case "dst":
                    TestDestIP(currentLine);
                    break;
                case "dstZone":
                    TestDestZone(currentLine);
                    break;

                case "natDst":
                    TestDestZone(currentLine);
                    break;
                case "proto":
                    Testproto(currentLine);
                    break;
                case "sent":
                    TestSent(currentLine);
                    break;
                case "rcvd":
                    TestRcvd(currentLine);
                    break;
                case "spkt":
                    TestSpkt(currentLine);
                    break;
                case "rpkt":
                    TestRpkt(currentLine);
                    break;
                case "cdur":
                    TestCdur(currentLine);
                    break;
                case "rule":
                    TestxRule(currentLine);
                    break;
                case "vpnpolicy":
                    TestVpnPolicy(currentLine);
                    break;
                case "note":
                    break;
                case "app":
                    TestApp(currentLine);
                    break;
                case "appName":
                    TestAppName(currentLine);
                    break;
                case "n":
                    TestNTag(currentLine);
                    break;

                default:
                    break;
            }

        }


        string getTagName(string currentLine)
        {
            int Index = currentLine.IndexOf("=");
            string tmp = currentLine.Substring(0, Index);
            Lbi.Items.Add("Lasttag=" + lastTagProcessed + " Next tag=" + tmp);
            return (tmp);


        }

        Int32 GetLastRecord(string ConnectionStr, string Cmd)
        {

            SqlCommand CmdDat = new SqlCommand();
            SqlConnection Con;

            Con = null;
            try
            {
                Con = new SqlConnection();
                Con.ConnectionString = ConnectionStr;

                Con.Open();
                CmdDat.Connection = Con;
                CmdDat.CommandText = Cmd;
                CmdDat.CommandType = CommandType.Text;

                return Convert.ToInt32(CmdDat.ExecuteScalar());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                return 0;

            }
            finally
            {
                if ((Con != null))
                {
                    Con.Close();
                    Con.Dispose();
                }
            }


        }



        public void Connectdb()
        {

            //    Dim DL As MSDASC.DataLinks

            //  DL = New MSDASC.DataLinks
            if (SqlCon == null)
                SqlCon = new SqlConnection();
            // DL.PromptEdit(AdoCon)
            // Debug.Print NavCon.ConnectionString
            //AdoCon.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + txDB.Text
            //AdoCon.ConnectionString = "Provider=SQLNCLI11.1;Data Source=IT-2014-W8\SQL2014;Initial Catalog=Files;Integrated Security=True"
            //SqlCon.ConnectionString = "Data Source=IT-2020-GS\\SQL2019;Initial Catalog=FileAnalysis;Integrated Security=True";
            SqlCon.ConnectionString = "Data Source=localhost\\SQL2019;Initial Catalog=FileAnalysis;Integrated Security=True";

            SqlCon.Open();

            //Cmd.Connection = SqlCon;
            //Cmd.CommandType = CommandType.StoredProcedure;
            Sonictable.Columns.Clear();
            Sonictable.Columns.Add("RecordNumber", typeof(long));
            Sonictable.Columns.Add("sDateTime", typeof(string));
            Sonictable.Columns.Add("Tag", typeof(string));
            Sonictable.Columns.Add("SN", typeof(string));
            Sonictable.Columns.Add("FW", typeof(string));
            Sonictable.Columns.Add("Pri", typeof(string));
            Sonictable.Columns.Add("pvtC", typeof(string));
            Sonictable.Columns.Add("Cat", typeof(string));
            Sonictable.Columns.Add("pvtM", typeof(string));
            Sonictable.Columns.Add("Msg", typeof(string));
            Sonictable.Columns.Add("SrcMac", typeof(string));
            Sonictable.Columns.Add("SrcIP", typeof(string));
            Sonictable.Columns.Add("SrcZone", typeof(string));
            Sonictable.Columns.Add("natSrc", typeof(string));
            Sonictable.Columns.Add("DestMac", typeof(string));
            Sonictable.Columns.Add("DestZone", typeof(string));
            Sonictable.Columns.Add("DestIP", typeof(string));
            Sonictable.Columns.Add("natDst", typeof(string));
            Sonictable.Columns.Add("proto", typeof(string));
            Sonictable.Columns.Add("sent", typeof(string));
            Sonictable.Columns.Add("rcvd", typeof(string));
            Sonictable.Columns.Add("spkt", typeof(string));
            Sonictable.Columns.Add("rpkt", typeof(string));
            Sonictable.Columns.Add("cdur", typeof(string));
            Sonictable.Columns.Add("xrule", typeof(string));
            Sonictable.Columns.Add("app", typeof(string));
            Sonictable.Columns.Add("appName", typeof(string));
            Sonictable.Columns.Add("sid", typeof(string));
            Sonictable.Columns.Add("ipscat", typeof(string));
            Sonictable.Columns.Add("tagN", typeof(string));

        }

        public void DoInsertToDataTable(int totLinesToDo)
        {
            long recSoFar = Sonictable.Rows.Count;
            long isAstep = recSoFar % BulkInsertBlockSize; //a block of 1000
            totRecs++;
            long Remaining = totLinesToDo - totRecs;

            if ((isAstep == 0) && (recSoFar > 0)) //is this a BulkBlock
            {
                Application.DoEvents();
                DoBulkInsert(Sonictable); //
                Sonictable.Clear();
            }

            else { if (Remaining <=BulkInsertBlockSize) { DoBulkInsert(Sonictable); } }




       
            DataRow row;
            row = Sonictable.NewRow();
            row["RecordNumber"] =totRecs;
            row["sDateTime"] = pvtDateTime;
            row["Tag"] = pvtTag;
            row["SN"] = pvtSN;
            row["FW"] = pvtFW;
            row["Pri"] = pvtPri;
            row["pvtC"] = pvtC;
            row["Cat"] = pvtgCat;
            row["pvtM"] = pvtM;
            row["Msg"] = pvtMsg;
            row["SrcMac"] = pvtSrcMAC;
            row["SrcIP"] = pvtSrcIP;
            row["SrcZone"] = pvtSrcZone;
            row["natSrc"] = pvtnatSrc;
            row["DestMac"] = pvtDestMac;
            row["DestZone"] = pvtDestZone;
            row["DestIP"] = pvtDestIP;
            row["natDst"] = pvtnatDst;
            row["proto"] =pvtProto;
            row["sent"] =pvtSent;
            row["rcvd"] =pvtRcvd;
            row["spkt"] =pvtsPkt;
            row["rpkt"] =pvtrPkt;
            row["cdur"] =pvtcDur;
            row["xrule"] =pvtxRule;
            row["app"] =pvtApp;
            row["appName"] =pvtAppName;
            row["sid"] =pvtSid;
            row["ipscat"] =pvtIpscat;
            row["tagN"] = pvtTagN;
            Sonictable.Rows.Add(row);

            //table.Rows.Add(totRecs, pvtDateTime, pvtTag, pvtSN, pvtFW, pvtPri,
              //  pvtC, pvtgCat, pvtM, pvtMsg, pvtSrcMAC, pvtSrcIP, pvtSrcZone, pvtnatSrc, pvtDestMac, pvtDestZone, pvtDestIP, pvtnatDst);

        }




        public void DoBulkInsert(DataTable table)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(SqlCon);
            bulkCopy.DestinationTableName = "dbo.SonicLog";
            try
            {
                bulkCopy.WriteToServer(table);
            }
            catch (Exception ex)
            {
                Lbi.Items.Add(ex.Message);
                throw;
            }
         
        
        
        
        }




        private void TestDate(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<dateTime>[\d]{2}\x2F[\d]{2}\x2F[\d]{4}\x20[\d]{2}:[\d]{2}:[\d]{2})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtDateTime = matchResults.Groups["dateTime"].Value;
                    currentLine = BodyText.Substring(pvtDateTime.Length + 1);
                    lastTagProcessed = "Date";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestTAG(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<tag><[\d]{1,3}>)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtTag = matchResults.Groups["tag"].Value;
                    currentLine = BodyText.Substring(pvtTag.Length + 1);
                    lastTagProcessed = "Tag";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestSN(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<SN>sn=[^\x20]{12})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtSN = matchResults.Groups["SN"].Value;
              //      int index = matchResults.Index;
                //    currentLine = BodyText.Substring(index + pvtSN.Length + 1);
                    lastTagProcessed = "sn=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestFW(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<FW>fw=(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}))", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtFW = matchResults.Groups["FW"].Value;
           //         int index = matchResults.Index;
            //        currentLine = BodyText.Substring(index + pvtFW.Length + 1);
                    lastTagProcessed = "fw=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestPri(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<pri>pri=[\d])", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtPri = matchResults.Groups["pri"].Value;
            //        int index = matchResults.Index;
             ///       currentLine = BodyText.Substring(index + pvtPri.Length + 1);
                    lastTagProcessed = "pri=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestC(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"[\x20]c=(?<cat>[^\x20]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtC = matchResults.Groups["cat"].Value;
                   // int index = matchResults.Index;
                   // currentLine = BodyText.Substring(index + pvtC.Length + 1);
                    lastTagProcessed = "c=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestgCat(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>gcat=[\d]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtgCat = matchResults.Groups["cat"].Value;
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestM(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>m=[\d]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtM = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "m=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestMsg(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>msg=\x22[^\x22]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtMsg = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "msg=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestSRC(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>srcMac=[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtSrcMAC = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "srcMac=";
                }
                else
                {
                    // Match attempt failed
                    matchResults = Regex.Match(BodyText, @"src=(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase );
                    if (matchResults.Success)
                    {
                        pvtSrcIP = matchResults.Groups["cat"].Value;
                        lastTagProcessed = "src=";
                    }


                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }

        private void TestSrcZone(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>srcZone=[^\x20]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtSrcZone = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "srcZone=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestnatSrc(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtnatSrc = matchResults.Groups["cat"].Value;
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestDestMac(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>dstMac=[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtDestMac = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "dstMac=";
                }
                else
                {
                    // Match attempt failed


                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestDestIP(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"dst=(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtDestIP = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "dst=";
                }
                else
                {
                    // Match attempt failed

                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestDestZone(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<cat>dstZone=[^\x20]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtDestZone = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "dstZone=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestnatDest(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"natDst=(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtnatDst = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "natDst=";
                }
                else
                {
                    // Match attempt failed

                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void Testproto(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>proto=[^\x20]{0,16})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtProto = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "proto=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestSent(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>sent=[^\x20]{0,16})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtSent = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "sent=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestRcvd(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>rcvd=[^\x20]{0,16})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtRcvd = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "rcvd=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestSpkt(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>spkt=[^\x20]{0,10})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtsPkt = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "spkt=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestRpkt(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>rpkt=[^\x20]{0,10})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtrPkt = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "rpkt=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestCdur(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>cdur=[^\x20]{0,10})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtcDur = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "cdur=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestxRule(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>rule=[^\x20]+[\d]{1,3}\x20[^\x22]+)", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtxRule = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "rule=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestApp(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>app=[^\x20]{0,10})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtApp = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "app=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }
        private void TestAppName(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>appName=[^\x20]{0,50})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtAppName = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "appName";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }

        private void TestNTag(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>\x20n=(?<Record>[\d]+))", RegexOptions.IgnoreCase);
                if (matchResults.Success)
                {
                    pvtTagN = matchResults.Groups["Record"].Value;
                    lastTagProcessed = "n";
                    seekingLastTag = false;

                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }

        private void TestVpnPolicy(string BodyText)
        {
            Match matchResults = null;
            try
            {

                matchResults = Regex.Match(BodyText, @"(?<cat>vpnpolicy=[^\x20]{0,30})", RegexOptions.IgnoreCase );
                if (matchResults.Success)
                {
                    pvtVpnPolicy = matchResults.Groups["cat"].Value;
                    lastTagProcessed = "vpnpolicy=";
                }
                else
                {
                    // Match attempt failed
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

        }

    }
}
