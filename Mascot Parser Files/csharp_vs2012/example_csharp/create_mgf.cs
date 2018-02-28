/*
##############################################################################
# file: create_mgf.cs                                                        #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/create_mgf.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.IO;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class create_mgf
    {
        public static void Main(string[] argv)
        {
            if (argv.Length < 1)
            {
                usage();
                return;
            }
            createMGF(argv[0]);
        }

        private static void createMGF(string filename)
        {
            ms_mascotresfile resfile = new ms_mascotresfile(filename);
            if (!resfile.isValid())
            {
                Console.WriteLine("Cannot open results file {0}: {1}", filename, resfile.getLastErrorString());
                return;
            }            

            string outputFilename = filename + ".mgf";
            // check if the mgf file already exists - do not overwrite if it does
            if (File.Exists(outputFilename))
            {
                Console.WriteLine("{0} already exists; will not overwrite", outputFilename);
                return;
            }

            // create the file
            

            using (TextWriter tw = new StreamWriter(outputFilename))
            {
                for (int q = 1; q <= resfile.getNumQueries(); q++)
                {
                    ms_inputquery inpQuery = new ms_inputquery(resfile, q);
                    if (inpQuery.getNumberOfPeaks(1) == 0)
                    {
                        // PMF - just the mass
                        tw.WriteLine(resfile.getObservedMass(q));
                        continue;
                    }

                    tw.WriteLine("BEGIN IONS");
                    tw.WriteLine("PEPMASS={0}", resfile.getObservedMass(q));

                    if (Math.Abs(resfile.getObservedCharge(q)) > 0)
                    {
                        tw.WriteLine("CHARGE={0}", resfile.getObservedCharge(q));
                    }
                    else
                    {
                        tw.WriteLine("CHARGE=Mr");
                    }

                    if (inpQuery.getStringTitle(true).Length > 0)
                    {
                        tw.WriteLine("TITLE={0}", inpQuery.getStringTitle(true));
                    }

                    for (int i = 1; i <= inpQuery.getNumberOfPeaks(1); i++)
                    {
                        tw.WriteLine("{0} {1}", inpQuery.getPeakMass(1, i), inpQuery.getPeakIntensity(1, i));
                    }

                    tw.WriteLine("END IONS");
                }
            }

        }

        private static void usage()
        {
            Console.WriteLine(@"Usage: create_mgf <results file>.

Given a mascot results file name, create an MGF file.  The MGF file
will be named <results file>.mgf in the same directory where <results file>
is located.");
        }
    }
}
