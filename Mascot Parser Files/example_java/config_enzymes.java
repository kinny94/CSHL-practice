/*
##############################################################################
# file: config_quantitation.java                                             #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2003 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_searchi $ #
#     $Author: yuryr $ #
#       $Date: 2006/02/07 19:44:48 $ #
#   $Revision: 1.1 $ #
# $NoKeywords::                                                            $ #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class config_enzymes {
    static {
        try {
            System.loadLibrary("msparserj");
        } catch (UnsatisfiedLinkError e) {
            System.err.println("Native code library failed to load. "
                               + "Is msparserj.dll on the path?\n" + e);
            System.exit(0);
        }
    }

    public static void main(String argv[]) 
    {
        // ----- Object creation -----
        if(argv.length < 1) {
            System.out.println("The location of enzymes file has to be specified as a parameter");
            System.out.println("The location should either be the full path to the enzymes file");
            System.out.println("or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi");
            System.exit(0);
        }

        // A sessionID can optionally be passed as the second parameter
        // This will only be required if the 'file' is a URL
        ms_enzymefile file;
        if (argv.length > 1) 
        {
            ms_connection_settings cs = new ms_connection_settings();
            cs.setSessionID(argv[1]);
            file = new ms_enzymefile(argv[0], cs);
        } 
        else 
        {
            file = new ms_enzymefile(argv[0]);
        }

        if (!file.isValid())
        {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }

        // how many do we have in total?
        int n = file.getNumberOfEnzymes();
        System.out.println("There are " + n + " enzymes definitions available");

        //now get them all!
        for (int i=0; i < n ; i++) 
        {
            ms_enzyme enzyme = file.getEnzymeByNumber(i);
            System.out.print(enzyme.getTitle() + ": ");
            for (int c=0; c < enzyme.getNumberOfCutters(); c++) 
            {
                if (enzyme.getCutterType(c) == ms_enzyme.NTERM_CUTTER) 
                {
                    System.out.print("nTerm - ");
                } 
                else 
                {
                      System.out.print("cTerm - ");
                }
                System.out.print(enzyme.getCleave(c) + "!");
                System.out.print(enzyme.getRestrict(c));
                System.out.print("; ");
            }
            System.out.println("");
        }

        // Now try updating the first one in the list to semi-specific
        ms_enzyme enzyme = file.getEnzymeByNumber(0);
        enzyme.setSemiSpecific(true);
        file.updateEnzymeByNumber(0, enzyme);

        // And delete V8-DE
        file.deleteEnzymeByName("V8-DE");

        // Finally, save the file under a new name
        file.setFileName(argv[0] + ".new");
        file.save_file();

        System.out.println("There are now " + file.getNumberOfEnzymes() + " enzymes definitions available");
    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_enzymes enzymes
There are 19 enzymes definitions available
Trypsin: cTerm - KR!P;
Arg-C: cTerm - R!P;
Asp-N: nTerm - BD!;
Asp-N_ambic: nTerm - DE!;
Chymotrypsin: cTerm - FLWY!P;
CNBr: cTerm - M!;
CNBr+Trypsin: cTerm - M!; cTerm - KR!P;
Formic_acid: cTerm - D!;
Lys-C: cTerm - K!P;
Lys-C/P: cTerm - K!;
PepsinA: cTerm - FL!;
Tryp-CNBr: cTerm - KMR!P;
TrypChymo: cTerm - FKLRWY!P;
Trypsin/P: cTerm - KR!;
V8-DE: cTerm - BDEZ!P;
V8-E: cTerm - EZ!P;
semiTrypsin: cTerm - KR!P;
LysC+AspN: nTerm - BD!; cTerm - K!P;
None: cTerm - KR!P;
There are now 18 enzymes definitions available

*/