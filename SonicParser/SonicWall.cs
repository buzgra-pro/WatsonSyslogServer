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
        public static DataTable table = new DataTable();
        public SqlCommand Cmd = new SqlCommand();
        public static long totRecs;


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
        int TagCount;
        ListBox Lbi;


        public SonicWall(string[] lines, ListBox LB)
        {
            int num = 1;
            //Im the constructor
            Connectdb();
            Lbi = LB;
            foreach (string line in lines)
            {

                currentLine = line;
                TestDate(currentLine);
                TestTAG(currentLine);
                TestSN(currentLine);
                nextTag = getTagName(currentLine);
                TestFW(currentLine);
                nextTag = getTagName(currentLine);
                TestPri(currentLine);
                nextTag = getTagName(currentLine);
                TestC(currentLine);
                nextTag = getTagName(currentLine);
                TestgCat(currentLine);
                nextTag = getTagName(currentLine);
                TestM(currentLine);
                nextTag = getTagName(currentLine);
                TestMsg(currentLine);
                nextTag = getTagName(currentLine);

                while (TagCount < 10)
                {
                    nextTag = getTagName(currentLine);
                    processNextTag(nextTag);
                    TagCount++;

                }

                
                //
                DoInsertToDataTable(table);
                Console.WriteLine(num + " : " + currentLine); num++;
            }


        }



        private void processNextTag(string Tag)
        {
            switch (Tag)
            {

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
            SqlCon.ConnectionString = "Data Source=IT-2020-GS\\SQL2019;Initial Catalog=FileAnalysis;Integrated Security=True";
            SqlCon.Open();

            Cmd.Connection = SqlCon;
            Cmd.CommandType = CommandType.StoredProcedure;
            //       Cmd.CommandText = "CreateFileList";
            //       Cmd.ExecuteNonQuery();
            table.Columns.Clear();

            //    sDateTime, Tag, SN, FW, Pri, pvtC, Cat, pvtM, Msg, SrcMac, 
            //                         SrcIP, SrcZone, natSrc, DestMac, DestZone, DestIP, natDst)

            // Create four typed columns in the DataTable.
            table.Columns.Add("RecordNumber", typeof(long));
            table.Columns.Add("sDateTime", typeof(string));
            table.Columns.Add("Tag", typeof(string));
            table.Columns.Add("SN", typeof(string));
            table.Columns.Add("FW", typeof(string));
            table.Columns.Add("Pri", typeof(string));
            table.Columns.Add("pvtC", typeof(string));
            table.Columns.Add("Cat", typeof(string));
            table.Columns.Add("pvtM", typeof(string));
            table.Columns.Add("Msg", typeof(string));
            table.Columns.Add("SrcMac", typeof(string));
            table.Columns.Add("SrcIP", typeof(string));
            table.Columns.Add("SrcZone", typeof(string));
            table.Columns.Add("natSrc", typeof(string));
            table.Columns.Add("DestMac", typeof(string));
            table.Columns.Add("DestZone", typeof(string));
            table.Columns.Add("DestIP", typeof(string));
            table.Columns.Add("natDst", typeof(string));

        }


        public void DoBulkInsert()
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(SqlCon);
            bulkCopy.DestinationTableName = "dbo.FileList";
            bulkCopy.WriteToServer(table);
        }



        public void DoInsertToDataTable(DataTable table)
        {
            long recsDone = table.Rows.Count;
            long isAstep = recsDone % 100000;
            totRecs++;
            if ((isAstep == 0) && (recsDone > 0))
            {
                DoBulkInsert();
                table.Clear();

            }

            table.Rows.Add(totRecs, pvtDateTime, pvtTag, pvtSN, pvtFW, pvtPri,
                pvtC, pvtgCat, pvtM, pvtMsg, pvtSrcMAC, pvtSrcIP, pvtSrcZone, pvtnatSrc, pvtDestMac, pvtDestZone, pvtDestIP, pvtnatDst);

        }









        private void TestDate(string BodyText)
        {
            Match matchResults = null;
            try
            {
                matchResults = Regex.Match(BodyText, @"(?<dateTime>[\d]{2}\x2F[\d]{2}\x2F[\d]{4}\x20[\d]{2}:[\d]{2}:[\d]{2})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
                matchResults = Regex.Match(BodyText, @"(?<tag><[\d]{1,3}>)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
                matchResults = Regex.Match(BodyText, @"(?<SN>sn=[^\x20]{12})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtSN = matchResults.Groups["SN"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtSN.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<FW>fw=(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtFW = matchResults.Groups["FW"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtFW.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<pri>pri=[\d])", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtPri = matchResults.Groups["pri"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtPri.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\Ac=[\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtC = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtC.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\Agcat=[\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtgCat = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtgCat.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\Am=[\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtM = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtM.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\Amsg=\x22[^\x22]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtMsg = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtMsg.Length + 2);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\AsrcMac=[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtSrcMAC = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtSrcMAC.Length + 1);
                    lastTagProcessed = "srcMac=";
                }
                else
                {
                    // Match attempt failed
                    matchResults = Regex.Match(BodyText, @"src=(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (matchResults.Success)
                    {
                        pvtSrcIP = matchResults.Groups["cat"].Value;
                        int index = matchResults.Index;
                        currentLine = BodyText.Substring(index + pvtSrcIP.Length + 5);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\AsrcZone=[^\x20]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtSrcZone = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtSrcZone.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtnatSrc = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtnatSrc.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\AdstMac=[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+:[a-f0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtDestMac = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtDestMac.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"dst=(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtDestIP = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtDestIP.Length + 5);
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
                matchResults = Regex.Match(BodyText, @"(?<cat>\AdstZone=[^\x20]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtDestZone = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtDestZone.Length + 1);
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
                matchResults = Regex.Match(BodyText, @"natDst=(?<cat>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}):(?<Port>[^\x20]+))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchResults.Success)
                {
                    pvtnatDst = matchResults.Groups["cat"].Value;
                    int index = matchResults.Index;
                    currentLine = BodyText.Substring(index + pvtnatDst.Length + 7);
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



    }
}
