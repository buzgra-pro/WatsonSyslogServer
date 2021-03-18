using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SyslogServer
{
    class SonicWall
    {
        string pvtBuffer;
        public SonicWall(string receivedData)
        {
            //Im the constructor
            pvtBuffer = receivedData;
        }

   private void TestDate(string subjectString) { 
        bool foundMatch = false;

            try
            {
                foundMatch = Regex.IsMatch(subjectString, @"(?<dateTime>[\d]{2}\x2F[\d]{2}\x2F[\d]{4}\x20[\d]{2}:[\d]{2}:[\d]{2})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }
}

private void DestIP(string subjectString)
        {
            string resultString;
            try
            {
                resultString = Regex.Replace(subjectString, @"natDst=(?<DestIP>(?<Oct1>[\d]{1,3})\.(?<Oct2>[\d]{1,3})\.(?<Oct3>[\d]{1,3})\.(?<Oct4>[\d]{1,3}))", "${DestIP}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }



        }

    }
}
