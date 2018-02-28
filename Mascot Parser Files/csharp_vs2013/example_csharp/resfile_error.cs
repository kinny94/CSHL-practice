/*
##############################################################################
# file: resfile_error.cs                                                     #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/resfile_error.cs,v $                                #
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
    public class resfile_error
    {
        public static void Main(string[] argv) 
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                Console.WriteLine("Must specify results filename as parameter");
                return;
            }

            ms_mascotresfile file = new ms_mascotresfile(argv[0], 0, "");
            if (file.isValid())
            {
                checkErrorHandler(file);
            }
            else
            {
                Console.WriteLine("Error number: {0}", file.getLastError());
                Console.WriteLine("Error string: {0}", file.getLastErrorString());
                return;
            }
        }

        private static void checkErrorHandler(ms_mascotresfile file)
        {
            int numberOfQueries, loop, loopTwo;

            Console.WriteLine("Testing the error handling...            ");
            Console.WriteLine("=========================================");

            numberOfQueries = file.getNumQueries();
            file.getObservedCharge(numberOfQueries + 40);                   // should fail
            Console.WriteLine("Error number: {0}", file.getLastError());
            Console.WriteLine("Error string: {0}", file.getLastErrorString());

            file.clearAllErrors();
            Console.WriteLine("Cleared all errors - should have no errors left: {0} errors left", file.getNumberOfErrors());

            for (loop = 1; loop <= 20; loop++)
            {
                file.getObservedCharge(numberOfQueries + loop);     // should fail
            }

            // Now, the best way, print out all errors
            Console.WriteLine("More errors added - there are now {0} errors", file.getNumberOfErrors());

            for (loopTwo = 1; loopTwo <= file.getNumberOfErrors(); loopTwo++)
            {
                Console.WriteLine("Error number: {0} : {1}", file.getErrorNumber(loopTwo), file.getErrorString(loopTwo));
            }

            Console.WriteLine();
            file.clearAllErrors();
        }

    }
}

/*

resfile_error.exe c:\inetpub\mascot\data\F001261.dat

Will give the following output:

Testing the error handling...
=========================================
Error number: 4
Error string: Query out of range. In function getObservedCharge. Request query 7
7368, num queries: 77328
Cleared all errors - should have no errors left: 0 errors left
More errors added - there are now 2 errors
Error number: 4 : Query out of range. In function getObservedCharge. Request query 77329, num queries: 77328
Error number: 4 : Query out of range. In function getObservedCharge. Request query 77330, num queries: 77328 (Error repeated 19 times)
*/