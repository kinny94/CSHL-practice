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

public class config_procs {
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
        // Don't check for Linux hyperthreading
        ms_processors cpus = new ms_processors(false, 1);

        if (!cpus.isValid()) 
        {
            System.out.println("There are errors. Cannot continue. The last error description:");
            System.out.println(cpus.getLastErrorString());
            System.exit(0);
        }

        System.out.println("Number of CPUS available on the system: " + cpus.getNumOnSystem());
    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_procs

Number of CPUS available on the system: 2

*/