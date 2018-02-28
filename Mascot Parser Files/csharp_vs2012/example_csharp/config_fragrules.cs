/*
##############################################################################
# file: config_fragrules.cs                                                  #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_fragrules.cs,v $                                #
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
    public class config_fragrules
    {
        public static void Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                Console.Error.WriteLine(@"The location of the 'fragmentation_rules' file has to be specified as a parameter
The location should either be the full path to the fragmentation_rules file
or a URL to a Mascot server - e.g. http://mascot-searver/mascot/cgi
A Mascot security sessionID can optionally be passed as a second 
parameter");
                    return;
            }

            // A sessionID can optionally be passed as the second parameter
            // This will only be required if the 'file' is a URL
            ms_fragrulesfile file;
            if (argv.Length > 1)
            {
                ms_connection_settings cs = new ms_connection_settings();
                cs.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);
                cs.setSessionID(argv[1]);
                file = new ms_fragrulesfile(argv[0], cs);
            }
            else file = new ms_fragrulesfile(argv[0]);

            if (!file.isValid())
            {                
                Console.Error.WriteLine("Error number: {0}", file.getLastError());
                Console.Error.WriteLine("Error string: {0}", file.getLastErrorString());
                return;                
            }

            int n = file.getNumberOfInstruments();
            Console.WriteLine("{0} instruments are configured:", n);

            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(file.getInstrumentName(i));
            }

            // Now change ESI-QUAD-TOF
            ms_fragmentationrules instrument = file.getInstrumentByName("ESI-QUAD-TOF");
            instrument.setSeriesUsed(23, true);     // Add v series
            file.updateInstrumentByName("ESI-QUAD-TOF", instrument);

            // And delete MALDI-QIT-TOF
            file.deleteInstrumentByName("MALDI-QIT-TOF");

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

            Console.WriteLine("There are now {0} instruments configured.  The updated configuration file was saved to {1}",
                file.getNumberOfInstruments(),
                Path.GetFullPath(file.getFileName()));
        }
    }
}

/*
Running the program as

config_fragrules.exe c:\inetpub\mascot\config\fragmentation_rules

Will give the output

14 instruments are configured:
Default
ESI-QUAD-TOF
MALDI-TOF-PSD
ESI-TRAP
ESI-QUAD
ESI-FTICR
MALDI-TOF-TOF
ESI-4SECTOR
FTMS-ECD
ETD-TRAP
MALDI-QUAD-TOF
MALDI-QIT-TOF
MALDI-ISD
CID+ETD
There are now 13 instruments configured.  The updated configuration file was saved to c:\inetpub\mascot\config\fragmentation_rules.new
*/