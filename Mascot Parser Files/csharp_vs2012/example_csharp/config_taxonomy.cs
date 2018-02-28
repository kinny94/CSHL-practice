/*
##############################################################################
# file: config_taxonomy.cs                                                   #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_taxonomy.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class config_taxonomy
    {
        public static void Main(string[] argv)
        {
            if (argv.Length < 1)
            {
                Console.WriteLine(@"The location of the taxonomy file has to be specified as a parameter
The location should either be the full path to the taxonomy file
or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi.
A Mascot security sessionID can optionally be passed as a second 
parameter");
                return;
            }

            // A sessionID can optionally be passed as the second parameter
            // This will only be required if the 'file' is a URL
            ms_taxonomyfile file;
            if (argv.Length > 1)
            {
                ms_connection_settings cs = new ms_connection_settings();
                cs.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);
                cs.setSessionID(argv[1]);
                file = new ms_taxonomyfile(argv[0], cs);
            }
            else
            {
                file = new ms_taxonomyfile(argv[0]);
            }

            if (!file.isValid())
            {
                Console.WriteLine("There are errors. Cannot continue. The last error description:");
                Console.WriteLine(file.getLastErrorString());
                return;
            }

            int n = file.getNumberOfEntries();
            Console.WriteLine("There are {0} taxonomy choice entries configured:", n);

            // now get them all!
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(file.getEntryByNumber(i).getTitle());
                Console.Write("Include: ");
                for (int j = 0; j < file.getEntryByNumber(i).getNumberOfIncludeTaxonomies(); j++)
                {
                    if (j > 0) Console.Write(",");
                    Console.Write(file.getEntryByNumber(i).getIncludeTaxonomy(j));
                }
                Console.WriteLine();
                Console.Write("Exclude: ");
                for (int j = 0; j < file.getEntryByNumber(i).getNumberOfExcludeTaxonomies(); j++)
                {
                    if (j > 0) Console.Write(",");
                    Console.Write(file.getEntryByNumber(i).getExcludeTaxonomy(j));
                }
                Console.WriteLine();
                
            }
        }
    }
}

/*
config_taxonomy.exe c:\inetpub\mascot\config\taxonomy
Will give the following output:

There are 65 taxonomy choice entries configured:
All entries
Include: 1
Exclude: 0
. . Archaea (Archaeobacteria)
Include: 2157
Exclude:
. . Eukaryota (eucaryotes)
Include: 2759
Exclude:
. . . . Alveolata (alveolates)
Include: 33630
Exclude:
. . . . . . Plasmodium falciparum (malaria parasite)
Include: 5833
Exclude:
. . . . . . Other Alveolata
Include: 33630
Exclude: 5833
[...]
*/
