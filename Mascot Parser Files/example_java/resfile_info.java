/*
##############################################################################
# file: resfile_info.java                                                    #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2003 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_searchi $ #
#     $Author: davidc $ #
#       $Date: 2004/12/23 14:27:35 $ #
#   $Revision: 1.2 $ #
# $NoKeywords::                                                            $ #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class resfile_info {
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
	        searchInformation(file);
        } else {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }
    }
  
      
  
    /**
     * searchInformation
     * Display parameters from the ms mascotfile object.  The functions
     * anyPMF, anyMSMSm andSQ should normally be used in preference to
     * isPMF etc because some people submit MSMS through the sequence query
     * window etc.
     **/
  
    private static void searchInformation(ms_mascotresfile file) {
        Date searchDate = null;                       // Java.util.date object
        int seconds = file.getDate();                 // get date of search in seconds since midnight GMT Jan 1st 1970
        long milliseconds = (long) seconds * 1000;    // convert to milliseconds
        searchDate=new Date(milliseconds);            // create the date object

        System.out.println("Search information from ms_mascotresfile");
        System.out.println("========================================");

        System.out.println("Number of queries   : "+file.getNumQueries());
        System.out.println("Number of hits      : "+file.getNumHits());
        System.out.println("Number of sequences : "+file.getNumSeqs());
        System.out.println("Sequences after tax : "+file.getNumSeqsAfterTax());
        System.out.println("Number of residues  : "+(int)file.getNumResidues());
        System.out.println("Execution time      : "+file.getExecTime());
        System.out.println("Date (seconds)      : "+file.getDate());


        // make java output identical to perl output
        System.out.print("Date                : "); 
        String toPerlDate = searchDate.toString();
        if(toPerlDate.indexOf("GMT ") > -1) {
            String beforeGMT = toPerlDate.substring(0,toPerlDate.indexOf("GMT"));
            String afterGMT = toPerlDate.substring(toPerlDate.indexOf("GMT ")+4,toPerlDate.length());
            System.out.println(beforeGMT+afterGMT);
        } else {
            System.out.println(searchDate);
        }

        System.out.println("Mascot version      : "+file.getMascotVer());
        System.out.println("Fasta version       : "+file.getFastaVer());
        // make java output identical to perl output

        System.out.println("Is PMF?             : "+toBinary(file.isPMF()));
        System.out.println("Is MSMS?            : "+toBinary(file.isMSMS()));
        System.out.println("Is SQ?              : "+toBinary(file.isSQ()));
        System.out.println("Is Error tolerant   : "+toBinary(file.isErrorTolerant()));
        System.out.println("Any PMF?            : "+toBinary(file.anyPMF()));
        System.out.println("Any MSMS?           : "+toBinary(file.anyMSMS()));
        System.out.println("Any SQ?             : "+toBinary(file.anySQ()));
        System.out.println("Any peptides section: "+toBinary(file.doesSectionExist(ms_mascotresfile.SEC_PEPTIDES)));
        System.out.println("Any peptide matches : "+toBinary(file.anyPeptideSummaryMatches()));
        System.out.println();
    }


    /**
     * toBinary(boolean bool)
     * returns 0 for false, 1 for true
     **/

    private static int toBinary(boolean bool) {
        if(bool) return 1;
        return 0;
    }
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar resfile_info F981123.dat
Search information from ms_mascotresfile
========================================
Number of queries   : 4
Number of hits      : 50
Number of sequences : 820227
Sequences after tax : 820227
Number of residues  : 255696937
Execution time      : 14
Date (seconds)      : 1014568786
Date                : Sun Feb 24 16:39:46 2002
Mascot version      : 1.7.17
Fasta version       : MSDB_20020121.fasta
Is PMF?             : 0
Is MSMS?            : 1
Is SQ?              : 0
Is Error tolerant   : 0
Any PMF?            : 0
Any MSMS?           : 1
Any SQ?             : 0
Any peptides section: 1
Any peptide matches : 1


*/