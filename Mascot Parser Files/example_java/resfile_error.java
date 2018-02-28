/*
##############################################################################
# file: resfile_error.java                                                   #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2004 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_errorha $ #
#     $Author: davidc $ #
#       $Date: 2004/12/23 14:27:35 $ #
#   $Revision: 1.2 $ #
# $NoKeywords::                                                            $ #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class resfile_error {
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
            System.out.println("Must specify results filename as parameter");
            System.exit(0);
        }

        ms_mascotresfile file = new ms_mascotresfile(argv[0], 0, "");
        if (file.isValid()) {
	        checkErrorHandler(file);
        } else {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }
    }
  
      
  
  /**
   * checkErrorHandler
   * ms_mascotresfile
   * Calls a couple of functions with invalid arguments
   **/
  
    private static void checkErrorHandler(ms_mascotresfile file) {
        int numberOfQueries;
        int loop;
        int loopTwo;

        System.out.println("Testing the error handling...            ");
        System.out.println("=========================================");

        numberOfQueries = file.getNumQueries();
        file.getObservedCharge(numberOfQueries+40);                // should fail
        System.out.println("Error number: "+file.getLastError());
        System.out.println("Error string: "+file.getLastErrorString());

        file.clearAllErrors();
        System.out.print("Cleared all errors - should have no errors left: "+file.getNumberOfErrors());
        System.out.println(" errors left\n");

        for(loop=1; loop <= 20; loop++) {
            file.getObservedCharge(numberOfQueries+loop);          // should fail
        }

        // Now, the best way, print out all errors.
        System.out.println("More errors added - there are now "+file.getNumberOfErrors()+" errors");

        for(loopTwo=1; loopTwo <= file.getNumberOfErrors(); loopTwo++) {
            System.out.print("Error number: "+file.getErrorNumber(loopTwo));
            System.out.print(" : "+file.getErrorString(loopTwo));
            System.out.println("");
        }

        System.out.println("");
        file.clearAllErrors();  
    }   
}

/*


will give the output: 



C:>java -classpath .;../java/msparser.jar test_errorhandler F981123.dat
Testing the error handling...
=========================================
Error number: 4
Error string: Query out of range. In function getObservedCharge. Request query 44, num queries: 4
Cleared all errors - should have no errors left: 0 errors left

More errors added - there are now 2 errors
Error number: 4 : Query out of range. In function getObservedCharge. Request query 5, num queries: 4
Error number: 4 : Query out of range. In function getObservedCharge. Request query 6, num queries: 4 (Error repeated 19 times)

*/
