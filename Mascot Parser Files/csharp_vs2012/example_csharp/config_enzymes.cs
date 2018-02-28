/*
##############################################################################
# file: config_enzymes.cs                                                    #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_enzymes.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.IO;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class config_enzymes
    {
        public static void Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(@"The location of the enzymes file has to be specified as a parameter
The location should either be the full path to the enzymes file
or a URL to a Mascot server - e.g. http://mascot-searver/mascot/cgi
A Mascot security sessionID can optionally be passed as a second 
parameter");
                    return;
                }
            }

            // A sessionID can optionally be passed as the second parameter
            // This will only be required if the 'file' is a URL
            ms_enzymefile file;
            if (argv.Length > 1)
            {
                ms_connection_settings cs = new ms_connection_settings();
                cs.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);
                cs.setSessionID(argv[1]);
                file = new ms_enzymefile(argv[0], cs);
            }
            else file = new ms_enzymefile(argv[0]);

            if (!file.isValid())
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine("Error number: {0}", file.getLastError());
                    errorWriter.WriteLine("Error string: {0}", file.getLastErrorString());
                }
                return;
            }

            // how many enzymes do we have in total?
            int n = file.getNumberOfEnzymes();
            Console.WriteLine("There are {0} enzymes definitions available", n);

            // now get them all!
            for (int i = 0; i < n; i++)
            {
                ms_enzyme _enzyme = file.getEnzymeByNumber(i);
                Console.Write("{0}: ", _enzyme.getTitle());
                for (int c = 0; c < _enzyme.getNumberOfCutters(); c++)
                {
                    if (_enzyme.getCutterType(c) == ms_enzyme.cuttertype.NTERM_CUTTER)
                    {
                        Console.Write("nTerm - ");
                    }
                    else
                    {
                        Console.Write("cTerm - ");
                    }
                    Console.Write("{0}!{1}; ", _enzyme.getCleave(c), _enzyme.getRestrict(c));
                }
                Console.WriteLine();
                _enzyme = null;
            }

            // Now try updating the first one in the list to semi-specific
            ms_enzyme enzyme = file.getEnzymeByNumber(0);
            enzyme.setSemiSpecific(true);
            file.updateEnzymeByNumber(0, enzyme);

            // And delete V8-DE
            file.deleteEnzymeByName("V8-DE");

            // Finally, save the file under a new name
            if (argv[0].ToLower().StartsWith("http"))
            {
                file.setFileName("copy_enzymes.new");
            }
            else
            {
                //create a new file locally
                file.setFileName(argv[0] + ".new");          
            }
            file.save_file();
            Console.WriteLine("Updated enzymes file saved to {0}", Path.GetFullPath(file.getFileName()));
            Console.WriteLine("There are now {0} enzymes definitions available", file.getNumberOfEnzymes());
        }
    }
}

/*
 * Running the program as
 * 
 * config_enzymes.exe c:\inetpub\mascot\config\enzymes
 * 
 * will give the following output under Mascot Server 2.5
 * 
 * There are 22 enzymes definitions available
 * Trypsin: cTerm - KR!P;
 * Trypsin/P: cTerm - KR!;
 * Arg-C: cTerm - R!P;
 * Asp-N: nTerm - BD!;
 * Asp-N_ambic: nTerm - DE!;
 * Chymotrypsin: cTerm - FLWY!P;
 * CNBr: cTerm - M!;
 * CNBr+Trypsin: cTerm - M!; cTerm - KR!P;
 * Formic_acid: nTerm - D!; cTerm - D!;
 * Lys-C: cTerm - K!P;
 * Lys-C/P: cTerm - K!;
 * LysC+AspN: nTerm - BD!; cTerm - K!P;
 * Lys-N: nTerm - K!;
 * PepsinA: cTerm - FL!;
 * semiTrypsin: cTerm - KR!P;
 * TrypChymo: cTerm - FKLRWY!P;
 * TrypsinMSIPI: nTerm - J!; cTerm - KR!P; cTerm - J!;
 * TrypsinMSIPI/P: nTerm - J!; cTerm - JKR!;
 * V8-DE: cTerm - BDEZ!P;
 * V8-E: cTerm - EZ!P;
 * NoCleave: cTerm - J!ABCDEFGHIJKLMNOPQRSTUVWXYZ;
 * None: cTerm - KR!P;
 * Updated enzymes file saved to c:\inetpub\mascot\config\enzymes.new
 * There are now 21 enzymes definitions available
 * 
*/
