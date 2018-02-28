/*
##############################################################################
# file: repeat_search.cs                                                #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/repeat_search.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class repeat_search
    {
        public static void Main(string[] argv)
        {
            if (argv.Length < 1)
            {
                usage();
                return;
            }
            repeatSearch(argv[0]);
        }

        private static bool repeatSearch(string filename)
        {
            bool success = false;

            ms_mascotresfile file = new ms_mascotresfile(filename, 0, "");
            if (file.isValid())
            {
                StringBuilder s = new StringBuilder();            // Build up a MIME format string with all parameters
                s.Append("----12345\n");
                s.Append("Content-Disposition: form-data; name=\"QUE\"");
                s.Append("\n\n");

                // Parameters section
                int count = 1;
                string key = file.enumerateSectionKeys(ms_mascotresfile.section.SEC_PARAMETERS, count);
                while (key.Length != 0)
                {
                    string val = file.getSectionValueStr(ms_mascotresfile.section.SEC_PARAMETERS, key);
                    // To search against a different database, add && key != "DB"
                    if (val.Length > 0 && !key.Equals("INTERMEDIATE") && !key.Equals("RULES") && !key.Equals("INTERNALS") && !key.Equals("SEARCH"))
                    {
                        s.Append(string.Format("{0}={1}\n", key, val));
                    }
                    key = file.enumerateSectionKeys(ms_mascotresfile.section.SEC_PARAMETERS, ++count);
                }
                // To search against a different DB add: s.Append("DB=MY_DB\n");
                //s.Append("DB=repeat\n");

                // Most flexible to repeat each search as a 'sequence' search.
                s.Append("SEARCH=SQ\n");

                // For ms-ms data, tell nph-mascot where to find the ions data
                s.Append("INTERMEDIATE=").Append(filename).Append("\n");

                // Now the repeat search data
                for (int q = 1; q <= file.getNumQueries(); q++)
                {
                    s.Append(file.getRepeatSearchString(q)).Append("\n");
                }
                s.Append("----12345--\n");

                // Start nph-mascot.exe, and redirect the output to tmp.txt
                // Note that for Unix, you may need to use ./nph-mascot.exe
                try
                {
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = @"./nph-mascot.exe";
                    start.Arguments = "4 -commandline";
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardInput = true;
                    start.UseShellExecute = false;

                    using (Process process = Process.Start(start))
                    {
                        using (StreamWriter writer = process.StandardInput)
                        {
                            writer.Write(s.ToString());
                            writer.Flush();
                        }
                        using (StreamReader reader = process.StandardOutput)
                        {
                            string buf;
                            while ((buf = reader.ReadLine()) != null)
                            {
                                if (buf.Contains("SUCCESS"))
                                {
                                    if ((buf = reader.ReadLine()) != null)
                                    {
                                        Console.WriteLine(buf);
                                        compareResults(file, buf);
                                        success = true;
                                    }
                                }
                                else if (buf.Contains("ERROR"))
                                {

                                }
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Cannot open results file {0}", filename);
                Console.WriteLine(file.getLastErrorString());
            }

            return success;
        }

        private static void compareResults(ms_mascotresfile originalSearch, string repeatedSearchFileName)
        {
            ms_mascotresfile repeatedSearch = new ms_mascotresfile(repeatedSearchFileName, 0, "");
            Boolean anyReport = false;
            if (repeatedSearch.isValid())
            {
                if (originalSearch.anyPMF())
                {
                    // Use protein summary
                    ms_proteinsummary originalResults = new ms_proteinsummary(originalSearch,
                        (uint) ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS, 0, 1, null, null);
                    ms_proteinsummary repeatedResults = new ms_proteinsummary(repeatedSearch,
                        (uint)ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS, 0, 1, null, null);

                    ms_protein originalProt = originalResults.getHit(1);
                    ms_protein repeatedProt = repeatedResults.getHit(1);
                    if (originalProt != null & repeatedProt != null)
                    {
                        double diff = repeatedProt.getScore() - originalProt.getScore();
                        if (diff > 10)
                        {
                            Console.WriteLine("Protein score is {0} higher for search {1} than {2}",
                                diff, repeatedSearchFileName, originalSearch.getFileName());
                            anyReport = true;
                        }
                    }
                }
                else
                {
                    // use peptide summary
                    ms_peptidesummary originalResults = new ms_peptidesummary(originalSearch,
                        (uint)ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS, 0, 1, null, 0, 0, null);

                    ms_peptidesummary repeatedResults = new ms_peptidesummary(repeatedSearch,
                        (uint)ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS, 0, 1, null, 0, 0, null);

                    // Compare peptide scores
                    for (int q = 1; q <= originalSearch.getNumQueries(); q++)
                    {
                        ms_peptide pepOriginal = originalResults.getPeptide(q, 1);
                        ms_peptide pepRepeated = repeatedResults.getPeptide(q, 1);
                        if (pepOriginal != null && pepRepeated != null)
                        {
                            double diff = pepRepeated.getIonsScore() - pepOriginal.getIonsScore();
                            if (diff > 10)
                            {
                                Console.WriteLine("Query {0} has core {1} higher for search {2} than {3}",
                                    q, diff, repeatedSearchFileName, originalSearch.getFileName());
                                anyReport = true;
                            }
                        }
                    }
                }
                if (!anyReport)
                {
                    Console.WriteLine("Similar results for {0} and {1}", originalSearch.getFileName(), repeatedSearchFileName);
                }
            }
            else
            {
                Console.WriteLine("Invalid repeat search: {0}", repeatedSearch.getLastErrorString());
            }
        }


        private static void usage()
        {
            Console.WriteLine("Usage: repeat_search <results_file> ");
            Console.WriteLine("Given a mascot results file name, repeat the search ");
            Console.WriteLine("against the same data");
            Console.WriteLine("   result_file is a full path to a result file");
            Console.WriteLine("The program must be run from the mascot cgi directory");
        }
    }
}

/*
C:\inetpub\mascot\cgi>repeat_search.exe ..\data\F981123.dat
Will give the following output:

../data/20150728/F002065.dat
Similar results for ..\data\F981123.dat and ../data/20150728/F002065.dat
*/
