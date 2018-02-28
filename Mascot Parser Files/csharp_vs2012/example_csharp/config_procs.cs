/*
##############################################################################
# file: config_modprocs.cs                                                   #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_procs.cs,v $                                #
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
    public class config_procs
    {
        public static void Main(string[] argv)
        {
            // Don't check for Linux hyperthreading
            ms_processors cpus = new ms_processors(false, 1);

            if (!cpus.isValid())
            {
                Console.WriteLine("There are errors.  Cannot continue.  The last error description:");
                Console.WriteLine(cpus.getLastErrorString());
                return;
            }
            Console.WriteLine("Number of CPUs available on the system: {0}", cpus.getNumOnSystem());
            
        }
    }
}

/*
Running the program as
config_procs.exe
On a dual core Microsoft Windows system with hyperthreading will give the following output:

Number of CPUs available on the system: 4
*/