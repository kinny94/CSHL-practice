/*
##############################################################################
# file: config_masses.cs                                                     #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_masses.cs,v $                                #
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
    public class config_masses
    {
        public static void Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                Console.Error.WriteLine(@"The location of the masses file has to be specified as a parameter
The location should either be the full path to the masses file
or a URL to a Mascot server - e.g. http://mascot-searver/mascot/cgi
A Mascot security sessionID can optionally be passed as a second 
parameter");
                return;
            }

            // A sessionID can optionally be passed as the second parameter
            // This will only be required if the 'file' is a URL
            ms_masses file;
            if (argv.Length > 1)
            {
                ms_connection_settings cs = new ms_connection_settings();
                cs.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);
                cs.setSessionID(argv[1]);
                file = new ms_masses(argv[0], cs);
            }
            else
            {
                file = new ms_masses(argv[0]);
            }

            // unlike the other configuration files, it is possible to use ms_masses
            // without specifying the filename - in which case it defaults to '../config/masses'

            if (!file.isValid())
            {
                Console.Error.WriteLine("Error when reading file {0}", file.getFileName());
                checkErrorHandler(file);
                return;
            }

            Console.WriteLine("Name of the file: {0}", file.getFileName());
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("---   Content of masses file                              ---");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Amino acid masses (monoisotopic and average)");

            char res;
            for (res = 'A'; res <= 'Z'; res++)
            {
                Console.WriteLine("{0}:{1:#####.00000},{2:#####.00000}", res,
                    file.getResidueMass(MASS_TYPE.MASS_TYPE_MONO, res),
                    file.getResidueMass(MASS_TYPE.MASS_TYPE_AVE, res));
            }
            Console.WriteLine();
            Console.WriteLine("# Atomic masses used for terminus values");
            Console.WriteLine("HYDROGEN:{0:#####.00000},{1:#####.00000}",
                file.getHydrogenMass(MASS_TYPE.MASS_TYPE_MONO),
                file.getHydrogenMass(MASS_TYPE.MASS_TYPE_AVE));

            Console.WriteLine("CARBON:{0:#####.00000},{1:#####.00000}",
                file.getCarbonMass(MASS_TYPE.MASS_TYPE_MONO),
                file.getCarbonMass(MASS_TYPE.MASS_TYPE_AVE));

            Console.WriteLine("NITROGEN:{0:#####.00000},{1:#####.00000}",
                file.getNitrogenMass(MASS_TYPE.MASS_TYPE_MONO),
                file.getNitrogenMass(MASS_TYPE.MASS_TYPE_AVE));

            Console.WriteLine("OXYGEN:{0:#####.00000},{1:#####.00000}",
                file.getOxygenMass(MASS_TYPE.MASS_TYPE_MONO),
                file.getOxygenMass(MASS_TYPE.MASS_TYPE_AVE));

            Console.WriteLine("ELECTRON:{0:####0.00000}",
                file.getElectronMass());
        }

        private static void checkErrorHandler(ms_masses file)
        {
            Console.WriteLine("Testing the error handling...            ");
            Console.WriteLine("=========================================");
            Console.WriteLine("Error string: {0}", file.getLastErrorString());
            ms_errs errObject = file.getErrorHandler();
            int numErrors = errObject.getNumberOfErrors();
            Console.WriteLine("Number of errors: {0}", numErrors);
            for (int i = 1; i <= numErrors; i++)
            {
                Console.WriteLine("Error number {0}: {1}", i, errObject.getErrorString(i));
            }
            file.clearAllErrors();
            Console.WriteLine("Cleared all errors - should have no errors left: {0}", file.getLastErrorString());
        }
    }
}

/*
Running the program as

config_masses.exe c:\inetpub\mascot\config\masses

Will give the following output:

Name of the file: c:\inetpub\mascot\config\masses
-------------------------------------------------------------
---   Content of masses file                              ---
-------------------------------------------------------------
Amino acid masses (monoisotopic and average)
A:71.03711,71.07790
B:114.53494,114.59500
C:103.00919,103.14290
D:115.02694,115.08740
E:129.04259,129.11400
F:147.06841,147.17390
G:57.02146,57.05130
H:137.05891,137.13930
I:113.08406,113.15760
J:113.08406,113.15760
K:128.09496,128.17230
L:113.08406,113.15760
M:131.04049,131.19610
N:114.04293,114.10260
O:237.14773,237.29820
P:97.05276,97.11520
Q:128.05858,128.12920
R:156.10111,156.18570
S:87.03203,87.07730
T:101.04768,101.10390
U:150.95363,150.03790
V:99.06841,99.13110
W:186.07931,186.20990
X:111.00000,111.00000
Y:163.06333,163.17330
Z:128.55059,128.62160

# Atomic masses used for terminus values
HYDROGEN:1.00783,1.00794
CARBON:12.00000,12.01070
NITROGEN:14.00307,14.00670
OXYGEN:15.99491,15.99940
ELECTRON:0.00055
*/
