/*
##############################################################################
# file: config_modfile.cs                                                    #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_modfile.cs,v $                                #
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
    public class config_modfile
    {

        public static void Main(string[] argv)
        {
            if (argv.Length < 1)
            {
                Console.WriteLine(@"The location of the mod_file has to be specified as a parameter
The location should either be the full path to the mod file
or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi.
A Mascot security sessionID can optionally be passed as a second 
parameter");
                return;
            }

            // we need ms_masses class instance
            // in practive, it has to be also read from a disc file
            // but, for simplicity, we will use default masses
            ms_masses massesFile = new ms_masses();

            // A sessionID can optionally be passed as the second
            // parameter.  This will only be required if the 'file' is a URL
            ms_modfile file;
            if (argv.Length > 1)
            {
                ms_connection_settings cs = new ms_connection_settings();
                cs.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);
                cs.setSessionID(argv[1]);
                file = new ms_modfile(argv[0], massesFile, false, cs);
            }
            else
            {
                file = new ms_modfile(argv[0], massesFile, false);
            }

            if (!file.isValid())
            {
                Console.WriteLine("There are errors. Cannot continue. The last error description:");
                Console.WriteLine(file.getLastErrorString());
                return;
            }

            int n = file.getNumberOfModifications();
            Console.WriteLine("There are {0} modifications configured:", n);

            // now get 'em all!
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(file.getModificationByNumber(i).getTitle());
            }

            // Now change Acetyl (K)
            ms_modification mod = file.getModificationByName("Acetyl (K)");
            mod.setHidden(true);
            file.updateModificationByName("Acetyl (K)", mod);

            // And delete Methyl
            file.deleteModificationByName("Methyl (R)");

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

            Console.WriteLine("There are now " + file.getNumberOfModifications() + " modifications configured.");
        }

    }
}

/*
Running the program as: 
config_modfile.exe c:\inetpub\mascot\config\mod_file
Will give the following output:

There are 1268 modifications configured:
2-dimethylsuccinyl (C)
2-monomethylsuccinyl (C)
2-nitrobenzyl (Y)
2-succinyl (C)
2HPG (R)
3-deoxyglucosone (R)
3-phosphoglyceryl (K)
3sulfo (N-term)
4-ONE (C)
4-ONE (H)
4-ONE (K)
4-ONE+Delta:H(-2)O(-1) (C)
4-ONE+Delta:H(-2)O(-1) (H)
4-ONE+Delta:H(-2)O(-1) (K)
4AcAllylGal (C)
a-type-ion (C-term)
AccQTag (K)
AccQTag (N-term)
Acetyl (C)
[...]
Xlink:B10621 (C)
Xlink:DMP (K)
Xlink:DMP (Protein N-term)
Xlink:DMP-de (K)
Xlink:DMP-de (Protein N-term)
Xlink:DMP-s (K)
Xlink:DMP-s (Protein N-term)
Xlink:DSS (K)
Xlink:DSS (Protein N-term)
Xlink:DST (K)
Xlink:DST (Protein N-term)
Xlink:DTSSP (K)
Xlink:DTSSP (Protein N-term)
Xlink:EGS (K)
Xlink:EGS (Protein N-term)
Xlink:EGScleaved (K)
Xlink:EGScleaved (Protein N-term)
Xlink:SMCC (C)
Xlink:SSD (K)
ZGB (K)
ZGB (N-term)
There are now 1267 modifications configured.
*/
