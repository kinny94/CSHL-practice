/*
##############################################################################
# file: http_client.cs                                                       #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/http_client.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.1 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using matrix_science.msparser;

namespace MsParserExamples
{
    class http_client
    {
        public static int Main(string[] argv)
        {
            Dictionary<string, bool> allowedCommands = new Dictionary<string, bool>
            {
                {"search", true},
                {"getresults", true},
                {"getseq", true}
            };

            Dictionary<string, string> options = GetOptions(argv);
            if ((!options.ContainsKey("url") || !options.ContainsKey("cmd") || !allowedCommands.ContainsKey(options["cmd"]))
                || (options["cmd"]=="search" && !options.ContainsKey("mgf"))
                || (options["cmd"]=="getresults" && !options.ContainsKey("taskID"))
                || (options["cmd"]=="getseq" && (!options.ContainsKey("accession") || !options.ContainsKey("database"))))
            {
                showUsage();
                return 1;
            }
            
            // Common code for all cases

            // Create connection settings
            // Any settings for web server authentication or a proxy server should be set here
            ms_connection_settings oSettings = new ms_connection_settings();
            oSettings.setUserAgent("CSharpTest/1.0 " + oSettings.getUserAgent());
            // try to auto detect any proxy settings
            oSettings.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);

            // Connect to the server
            ms_http_client oHttpClient = new ms_http_client(options["url"], oSettings);
            if (!oHttpClient.isValid())
            {
                showErrors(oHttpClient.getErrorHandler());
                return -1;
            }

            // Enable logging, mostly for debugging
            oHttpClient.getErrorHandler().setLoggingFile("log.txt", ms_errs.msg_sev.sev_debug3);

            // Create a Mascot security session
            // If security is disabled, the empty user name and password return the
            // 'all_secdisabledsession' session
            ms_http_client_session oSession = new ms_http_client_session();
            ms_http_client.LoginResultCode loginReturnCode = oHttpClient.userLogin((options.ContainsKey("username") ? options["username"] : ""),
                (options.ContainsKey("password") ? options["password"] : ""),
                oSession);
            if (loginReturnCode != ms_http_client.LoginResultCode.L_SUCCESS &&
                loginReturnCode != ms_http_client.LoginResultCode.L_SECURITYDISABLED)
            {
                Console.Error.WriteLine("Failed to login to {0} as user {1}. Return code id {2}",
                    options["url"], (options.ContainsKey("username") ? options["username"] : ""), (int) loginReturnCode);
                return -1;
            }
            else
            {
                Console.WriteLine("Logged in to {0} with session: {1}", options["url"], oSession.sessionId());
            }

            if (options["cmd"] == "search")
            {
                if (!doSearch(options, oHttpClient, oSession)) return -1;
            }
            else if (options["cmd"] == "getresults")
            {
                if (!doGetResults(options, oSession)) return -1;
            }
            else if (options["cmd"] == "getseq")
            {
                if(!doGetSeq(options, oSession)) return -1;
            }

            // and finally logout
            oSession.logout();
            return 0;
        }

        private static bool doSearch(Dictionary<string, string> options, ms_http_client oHttpClient, ms_http_client_session oSession)
        {
            string httpHeader = "Content-Type: multipart/mixed; boundary=---------FormBoundary4C9ByKKVofH";
            // You may need to alter the search conditions embedded in this string to match your own
            // dataset
            string prologue = @"-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""FORMVER""

1.01
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""SEARCH""

MIS
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""COM""

C# test
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""DB""

SwissProt
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""CLE""

Trypsin/P
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""PFA""

1
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""QUANTITATION""

None
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""TAXONOMY""

. . . . . . . . . . . . . . . . Homo sapiens (human)
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""MODS""

Carbamidomethyl (C)
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""IT_MODS""

Oxidation (M)
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""TOL""

10
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""TOLU""

ppm
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""PEP_ISOTOPE_ERROR""

1
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""ITOL""

0.1
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""ITOLU""

Da
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""CHARGE""

2+
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""MASS""

Monoisotopic
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""FORMAT""

Mascot generic
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""INSTRUMENT""

ESI-TRAP
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""REPORT""

AUTO
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""USERNAME""

Mascot Parser Test
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""USEREMAIL""

dcreasy@matrixscience.com
-----------FormBoundary4C9ByKKVofH
Content-Disposition: form-data; name=""FILE""; filename=""test.mgf""";
            string epilogue = "-----------FormBoundary4C9ByKKVofH--";

            // Request a Mascot task id for the search
            string taskID = "";
            ms_http_client_search oSearch = new ms_http_client_search(oSession, taskID);
            if (!oSearch.isValid())
            {
                showErrors(oSearch.getErrorHandler());
                return false;
            }

            // Submit the search
            Console.WriteLine("Submitting search");
            ms_http_helper_progress oProgress = new ms_http_helper_progress();
            if (oSession.submitSearch(oSearch, httpHeader, prologue, options["mgf"], epilogue, oProgress))
            {
                Console.WriteLine("Search submitted.  Task ID = {0}", oSearch.searchTaskId());
                bool success;
                int returnValue;
                int returnCode = (int) ms_http_client_search.SearchStatusCode.SS_UNKNOWN;
                bool complete = false;
                while (!complete)
                {
                    success = oSearch.getStatus(out returnCode, out returnValue);
                    if (!success)
                    {
                        Console.Error.WriteLine("Unable to call getStatus()");
                        return false;
                    }
                    if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_UNKNOWN)
                    {
                        Console.WriteLine("Unknown search id");
                        complete = false;
                    }
                    else if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_ASSIGNED)
                    {
                        Console.WriteLine("Search no started");
                        complete = false;
                    }
                    else if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_QUEUED)
                    {
                        Console.WriteLine("Search queued");
                        complete = false;
                    }
                    else if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_RUNNING)
                    {
                        Console.WriteLine("Search running: {0}% complete", returnValue);
                        complete = false;
                    }
                    else if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_COMPLETE)
                    {
                        Console.WriteLine("Search complete");
                        complete = true;
                    }
                    else if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_ERROR)
                    {
                        Console.WriteLine("Search error : {0}", returnValue);
                        complete = true;
                    }
                    else if (returnCode == (int)ms_http_client_search.SearchStatusCode.SS_SEARCH_CONTROL_ERROR)
                    {
                        Console.WriteLine("Search control error: {0}", returnValue);
                        complete = true;
                    }
                    // Sleep thread for 1 second
                    if(!complete) Thread.Sleep(1000);
                }

                // Search complete - get the relative path to the result file
                string remoteFileName;
                bool ok = oSearch.getResultsFileName(out remoteFileName);
                if (!ok)
                {
                    Console.Error.WriteLine("Unable to get filename for results file");
                    showErrors(oHttpClient.getErrorHandler());
                    return false;
                }

                // Display URL for search result report
                Console.WriteLine("Search result report can be viewed at this URL:");
                Console.WriteLine("{0}master_results_2.pl?file={1}", oHttpClient.baseUrl(),
                    remoteFileName);
            }
            else
            {
                Console.Error.WriteLine("Failed to submit search");
                showErrors(oSession.getErrorHandler());
                return false;
            }

            return true;
        }

        private static bool doGetResults(Dictionary<string, string> options, ms_http_client_session oSession)
        {
            ms_http_client_search oSearch = new ms_http_client_search(oSession, options["taskID"]);
            if (!oSearch.isValid())
            {
                showErrors(oSearch.getErrorHandler());
                return false;
            }
            string remoteFileName;
            bool ok = oSearch.getResultsFileName(out remoteFileName);
            if (!ok || string.IsNullOrWhiteSpace(remoteFileName))
            {
                Console.Error.WriteLine("Unable to get filename for results file");
                return false;
            }
            string localFileName = Path.GetFileName(remoteFileName);
            Console.WriteLine("Downloading remote results file ({0}) to {1}", remoteFileName, localFileName);
            ms_http_helper_progress oProgress = new ms_http_helper_progress();
            // Download the complete, uncompressed result file
            if (oSearch.downloadResultsFile(localFileName, oProgress))
            {
                Console.WriteLine("Downloaded remote file to {0}", localFileName);
            }
            else
            {
                Console.Error.WriteLine("Failed to download remote file to {0}", localFileName);
                return false;
            }
            return true;
        }

        private static bool doGetSeq(Dictionary<string, string> options, ms_http_client_session oSession)
        {
            VectorString accessions = new VectorString();
            vectori frames = new vectori();
            accessions.Add(options["accession"]);
            string localFileName = "sequence.xml";
            if (oSession.getSequenceFile(options["database"], accessions, frames, localFileName))
            {
                Console.WriteLine("Successfully saved sequence to {0}", localFileName);
                return true;
            }
            else
            {
                Console.Error.WriteLine("Failed to get sequence or failed to save it to {0}", localFileName);
                showErrors(oSession.getErrorHandler());
                return false;
            }
        }

        private static void showErrors(ms_errs ms_errs)
        {
            Console.Error.WriteLine("Error: {0}", ms_errs.getLastErrorString());
            for (int i = 1; i <= ms_errs.getNumberOfErrors(); i++)
            {
                Console.Error.WriteLine("Error number: {0} ({1} times) : {2}",
                    ms_errs.getErrorNumber(i),
                    ms_errs.getErrorRepeats(i) + 1,
                    ms_errs.getErrorString(i));
            }
            Console.Error.WriteLine();
        }

        private static void showUsage()
        {
            Console.Error.WriteLine(@"Usage {0} --url=<URL> --cmd=<command> [options]

    --url       Mascot Server URL in the form
                  http://your-server/mascot/cgi/
    --cmd       One of the following:
                  --cmd=search to submit a Mascot search
                  --cmd=getresults to download the search results file
                  --cmd=getseq to save protein sequence to XML file
    --mgf       Path to an MGF peak list file
                  Required if --cmd=search
    --taskID    Mascot task ID for the search
                  Required if --cmd=getresults
    --accession Accession string for protein
                  Required if --cmd=getseq
    --database  Mascot database name
                  Required if --cmd=getseq
    --username  Mascot Server username
                  Required if Mascot Security is enabled
    --password  Mascot Server password
                  May be required if Mascot Security is enabled

Note that the Mascot search parameters are hard coded towards the end of this
class, and may need modifying for different peak lists.", Path.GetFileName((new http_client()).GetType().Assembly.Location));
        }

        // Parses the passed arguments and returns a Dictionary containing only
        // valid commands
        private static Dictionary<string, string> GetOptions(string[] argv)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            Dictionary<string, bool> allowedCommands = new Dictionary<string, bool>
            {
                {"url",true},
                {"cmd",true},
                {"mgf",true},
                {"taskID",true},
                {"accession",true},
                {"database",true},
                {"username",true},
                {"password",true}
            };

            foreach (string arg in argv)
            {
                if(!arg.StartsWith("--")) continue;
                string arg2 = ReplaceFirst(arg, "--", "");
                string key=null, value=null;
                if (arg2.Contains("="))
                {
                    string[] split = arg2.Split('=');
                    if (split.Length == 2)
                    {
                        key = split[0];
                        value = split[1];
                    }
                    else if (split.Length == 1)
                    {
                        key = split[0];
                        value = "";
                    }
                }
                else
                {
                    key = arg2;
                    value = "";
                }

                if (!string.IsNullOrEmpty(key) && allowedCommands.ContainsKey(key) && allowedCommands[key] && !string.IsNullOrWhiteSpace(value))
                {
                    options.Add(key,value);
                }
            }

            return options;
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
