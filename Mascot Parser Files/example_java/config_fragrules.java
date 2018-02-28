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

public class config_fragrules {
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
            System.out.println("The location of 'fragmentation_rules' file has to be specified as a parameter");
            System.out.println("The location should either be the full path to the fragmentation_rules file");
            System.out.println("or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi");
            System.exit(0);
        }

        // A sessionID can optionally be passed as the second parameter
        // This will only be required if the 'file' is a URL
        ms_fragrulesfile file;
        if (argv.length > 1) 
        {
            ms_connection_settings cs = new ms_connection_settings();
            cs.setSessionID(argv[1]);
            file = new ms_fragrulesfile(argv[0], cs);
        } 
        else 
        {
            file = new ms_fragrulesfile(argv[0]);
        }

        if (!file.isValid())
        {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }

        int n = file.getNumberOfInstruments();
        System.out.println(n + " instruments are configured:");

        for(int i=0; i < n; i++) 
        {
            System.out.println(file.getInstrumentName(i));
        }

        // Now change ESI-QUAD-TOF
        ms_fragmentationrules instrument = file.getInstrumentByName("ESI-QUAD-TOF");
        instrument.setSeriesUsed(23, true);  // Add v series
        file.updateInstrumentByName("ESI-QUAD-TOF", instrument);

        // And delete MALDI-QIT-TOF
        file.deleteInstrumentByName("MALDI-QIT-TOF");

        // Finally, save the file under a new name
        file.setFileName(argv[0] + ".new");
        file.save_file();

        System.out.println("There are now " + file.getNumberOfInstruments() + " instruments configured.");
    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_fragrules fragmentation_rules

12 instruments are configured:
Default
ESI-QUAD-TOF
MALDI-TOF-PSD
ESI-TRAP
ESI-QUAD
ESI-FTICR
MALDI-TOF-TOF
ESI-4SECTOR
FTMS-ECD
MALDI-QUAD-TOF
MALDI-QIT-TOF
ALL
There are now 11 instruments configured.

*/