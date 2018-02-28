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
#    Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_searchi  #
#     Author: yuryr  #
#       Date: 2006/02/06 17:53:44  #
#   Revision: 1.1  #
# NoKeywords::                                                             #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class config_modfile {
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
        if (argv.length < 1) 
        {
            System.out.println("The location of mod_file has to be specified as a parameter");
            System.out.println("The location should either be the full path to the mod_file");
            System.out.println("or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi");
            System.exit(0);
        }

        // we need ms_masses class instance
        // in practice, it has to be also read from a disc file
        // but, for simplicity, we will use default masses
        ms_masses massesFile = new ms_masses();


        // A sessionID can optionally be passed as the second parameter
        // This will only be required if the 'file' is a URL
        ms_modfile file;
        if (argv.length > 1) 
        {
            ms_connection_settings cs = new ms_connection_settings();
            cs.setSessionID(argv[1]);
            file = new ms_modfile(argv[0], massesFile, false, cs);
        } 
        else 
        {
            file = new ms_modfile(argv[0], massesFile, false);
        }

        if (!file.isValid()) 
        {
            System.out.println("There are errors. Cannot continue. The last error description:");
            System.out.println(file.getLastErrorString());
            System.exit(0);
        }

        int n = file.getNumberOfModifications();
        System.out.println("There are " + n + " modifications configured:");

        // now get'em all!
        for (int i=0; i < n; i++) 
        {
            System.out.println(file.getModificationByNumber(i).getTitle());
        }

        // Now change Acetyl (K)
        ms_modification mod = file.getModificationByName("Acetyl (K)");
        mod.setHidden(true);
        file.updateModificationByName("Acetyl (K)", mod);

        // And delete Methyl (R)
        file.deleteModificationByName("Methyl (R)");

        // Finally, save the file under a new name
        file.setFileName(argv[0] + ".new");
        file.save_file();

        System.out.println("There are now " + file.getNumberOfModifications() + " modifications configured:");

    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_modfile mod_file

There are 12 modifications configured:
Acetyl (N-term)
Acetyl (K)
Acetyl/Methyl (K)
Propionyl/Methyl (K)
Methyl (R)
Dimethyl (R)
Succininyl (K)
Succininyl+Methyl (K)
Biotinylated (N-term)
Biotinylated (K)
Carbamidomethyl (C)
Carbamyl (N-term)
...
*/