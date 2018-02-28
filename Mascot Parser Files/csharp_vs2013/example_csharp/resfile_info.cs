/*
##############################################################################
# file: resfile_info.cs                                                      #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/resfile_info.cs,v $                                #
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
    public class resfile_info
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
                searchInformation(file);
            }
            else
            {
                Console.WriteLine("Error number: {0}", file.getLastError());
                Console.WriteLine("Error string: {0}", file.getLastErrorString());
                return;
            }

        }

        private static void searchInformation(ms_mascotresfile file)
        {
            DateTime searchDate;                        // C# DateTime struct
            int seconds = file.getDate();               // get date of search in seconds since midnight GMT Jan 1st 1970
            long milliseconds = (long) seconds * 1000;        // convert to milliseconds
            searchDate = (new DateTime(1970, 1, 1)).AddMilliseconds(milliseconds);    // create the DateTime struct.  .NET DateTime
                                                                                      // are intialised to 1/1/0001, so we need to reset to
                                                                                      // 1/1/1970

            Console.WriteLine("Search information from ms_mascotresfile");
            Console.WriteLine("========================================");

            Console.WriteLine("Number of queries    : {0}", file.getNumQueries());
            Console.WriteLine("Number of hits       : {0}", file.getNumHits());
            Console.WriteLine("Number of sequences  : {0}", file.getNumSeqs());
            Console.WriteLine("Sequences after tax  : {0}", file.getNumSeqsAfterTax());
            Console.WriteLine("Number of residues   : {0}", (int) file.getNumResidues());
            Console.WriteLine("Execution time       : {0}", file.getExecTime());
            Console.WriteLine("Date (seconds)       : {0}", file.getDate());
            Console.WriteLine("Date                 : {0}", searchDate.ToString("ddd MMM dd hh:mm:ss yyyy")); // Change output format to match Perl
            Console.WriteLine("Mascot version       : {0}", file.getMascotVer());
            Console.WriteLine("Fasta version        : {0}", file.getFastaVer());

            // make C# output identical to perl output
            Console.WriteLine("Is PMF?              : {0}", toBinary(file.isPMF()));
            Console.WriteLine("Is MSMS?             : {0}", toBinary(file.isMSMS()));
            Console.WriteLine("Is SQ?               : {0}", toBinary(file.isSQ()));
            Console.WriteLine("Is Error tolerant    : {0}", toBinary(file.isErrorTolerant()));
            Console.WriteLine("Any PMF?             : {0}", toBinary(file.anyPMF()));
            Console.WriteLine("Any MSMS?            : {0}", toBinary(file.anyMSMS()));
            Console.WriteLine("Any SQ?              : {0}", toBinary(file.anySQ()));

            // C++ enum converted to C# enum, so we need to use the enum name when referencing the
            // required section constant
            Console.WriteLine("Any peptides section : {0}", toBinary(file.doesSectionExist(ms_mascotresfile.section.SEC_PEPTIDES)));
            Console.WriteLine("Any peptide matches  : {0}", toBinary(file.anyPeptideSummaryMatches()));
            Console.WriteLine();

        }

        public static int toBinary(bool b)
        {
            return (b) ? 1 : 0;
        }
    }
}

/*
resfile_info.exe c:\inetpub\mascot\data\F981118.dat
Will give the following output:

Search information from ms_mascotresfile
========================================
Number of queries    : 6
Number of hits       : 50
Number of sequences  : 257964
Sequences after tax  : 15720
Number of residues   : 93947433
Execution time       : 8
Date (seconds)       : 1171893592
Date                 : Mon Feb 19 01:59:52 2007
Mascot version       : 2.1.119
Fasta version        : SwissProt_51.6.fasta
Is PMF?              : 1
Is MSMS?             : 0
Is SQ?               : 0
Is Error tolerant    : 0
Any PMF?             : 1
Any MSMS?            : 0
Any SQ?              : 0
Any peptides section : 0
Any peptide matches  : 0
*/