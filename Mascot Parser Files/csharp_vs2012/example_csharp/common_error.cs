/*
##############################################################################
# file: common_error.cs                                                      #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/common_error.cs,v $                                #
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
    class common_error
    {
        static void Main(string[] argv)
        {
            ms_datfile file = new ms_datfile();
            file.setFileName((argv.Length > 0 && argv[0].Length > 0) ? argv[0] : "wrong_name.txt");
            file.read_file();

            if (file.isValid())
            {
                Console.WriteLine("The file has been read and parsed successfully.  Congratulations!");
            }
            else
            {
                checkErrorHandler(file);
            }
        }

        private static void checkErrorHandler(ms_datfile file)
        {
            Console.WriteLine("Last error description                   ");
            Console.WriteLine("=========================================");
            Console.WriteLine("Error: {0}", file.getLastErrorString());
            Console.WriteLine("=========================================");
            Console.WriteLine("Testing the error handling...            ");
            Console.WriteLine("=========================================");
            
            ms_errs err = file.getErrorHandler();
            for (int i = 1; i <= err.getNumberOfErrors(); i++)
            {
                Console.WriteLine("Error number: {0} ({1} times) : {2}", err.getErrorNumber(i), err.getErrorRepeats(i) + 1, err.getErrorString(i));
            }
            Console.WriteLine();
            file.clearAllErrors();
        }
    }
}

/*
common_error.exe
Will give the following output:

Last error description
=========================================
Error: Cannot find Mascot configuration file 'wrong_name.txt'.
=========================================
Testing the error handling...
=========================================
Error number: 1537 (1 times) : Cannot find Mascot configuration file 'wrong_name.txt'.
*/
