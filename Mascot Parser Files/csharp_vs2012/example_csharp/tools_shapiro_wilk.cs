/*
##############################################################################
# file: tools_shapiro_wilk.cs                                                #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/tools_shapiro_wilk.cs,v $                                #
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
    class tools_shapiro_wilk
    {
        public static void Main(string[] argv)
        {
            ms_shapiro_wilk sw = new ms_shapiro_wilk();
            sw.appendSampleValue(-0.1420);
            sw.appendSampleValue(-0.1184);
            sw.appendSampleValue(-0.0583);
            sw.appendSampleValue(-0.0402);
            sw.appendSampleValue(-0.0363);
            sw.appendSampleValue(-0.0363);
            sw.appendSampleValue(-0.0348);
            sw.appendSampleValue(-0.0333);
            sw.appendSampleValue(-0.0222);
            sw.appendSampleValue(-0.0139);
            sw.appendSampleValue(-0.0114);
            sw.appendSampleValue(-0.0042);
            sw.appendSampleValue(-0.0036);
            sw.appendSampleValue(-0.0026);
            sw.appendSampleValue( 0.0000);
            sw.appendSampleValue( 0.0016);
            sw.appendSampleValue( 0.0058);
            sw.appendSampleValue( 0.0077);
            sw.appendSampleValue( 0.0084);
            sw.appendSampleValue( 0.0102);
            sw.appendSampleValue( 0.0132);
            sw.appendSampleValue( 0.0172);
            sw.appendSampleValue( 0.0180);
            sw.appendSampleValue( 0.0191);
            sw.appendSampleValue( 0.0194);
            sw.appendSampleValue( 0.0200);
            sw.appendSampleValue( 0.0246);
            sw.appendSampleValue( 0.0293);
            sw.appendSampleValue( 0.0533);
            sw.appendSampleValue( 0.0987);

            sw.calculate(30, 30, 15);
            string format = "{0} W = {1:0.######} and P = {2:0.######}{3}";
            Console.WriteLine(format, "Test should give", 0.892184, 0.0054, "");
            Console.WriteLine(format, "Results:", sw.getResult(), sw.getPValue(), "; error code = " + sw.getErrorCode());
        }
    }
}

/*
tools_shapiro_wilk.exe
Will give the following output:
 
Test should give W = 0.892184 and P = 0.0054
Results: W = 0.892184 and P = 0.005437; error code = 0
*/