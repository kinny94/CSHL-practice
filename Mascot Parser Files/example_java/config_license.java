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

public class config_license {
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
            System.out.println("Location of 'mascot.license'-file has to be specified as a parameter");
            System.exit(0);
        }

        ms_license file = new ms_license(argv[0]);
        if (!file.isValid())
        {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }

        if (file.isLicenseValid()) 
        {
            System.out.println("Mascot license is still valid. Other information:");
            System.out.println(file.getLicenseString());
        } 
        else 
        {
            System.out.println("Mascot license has either expired or been corrupted");
        }
    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_license mascot.license

Mascot license has either expired or been corrupted

*/