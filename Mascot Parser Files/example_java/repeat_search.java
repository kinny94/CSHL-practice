/*
##############################################################################
# file: repeat_search.java                                                   #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2003 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/repeat_searc $ #
#     $Author: davidc $ #
#       $Date: 2007/03/08 16:24:22 $ #
#   $Revision: 1.2 $ #
# $NoKeywords::                                                            $ #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;
import java.io.*;

public class repeat_search {
    static {
        try {
            System.loadLibrary("msparserj");
        } catch (UnsatisfiedLinkError e) {
            System.err.println("Native code library failed to load.\n" + e);
            System.exit(0);
        }
    }

    public static void main(String argv[]) {
        // ----- Object creation -----
        if(argv.length < 1) {
            usage();
            System.exit(0);
        }
        repeatSearch(argv[0]);
    }


    private static boolean repeatSearch(String filename) {
        boolean success = false;

        ms_mascotresfile file = new ms_mascotresfile(filename, 0, "");
        if (file.isValid()) {
            String s;     // Build up a MIME format string with all parameters
            s = "----12345\n";
            s += "Content-Disposition: form-data; name=\"QUE\"";
            s += "\n\n";

            // Parameters section
            int count = 1;
            String key = file.enumerateSectionKeys(ms_mascotresfile.SEC_PARAMETERS, 
                                                   count);
            while (key.length() != 0)
            {
                String val = file.getSectionValueStr(ms_mascotresfile.SEC_PARAMETERS, 
                                                     key);
                // To search against a different database, add && key != "DB"
                if ((val.length()) != 0 && !key.equals("INTERMEDIATE") && !key.equals("RULES") && !key.equals("INTERNALS") && !key.equals("SEARCH"))
                    s += key+ "=" +val+ "\n";
                key = file.enumerateSectionKeys(ms_mascotresfile.SEC_PARAMETERS, ++count);

            }
            // To search against a different DB add: s += "DB=MY_DB\n";
            s += "DB=repeat\n";

            // Most flexible to repeat each search as a 'sequence' search.
            s += "SEARCH=SQ\n";

            // For ms-ms data, tell nph-mascot where to find the ions data
            s += "INTERMEDIATE=" +filename+ "\n";

            // Now the repeat search data
            for (int q=1; q <= file.getNumQueries(); q++) {
                s += file.getRepeatSearchString(q) + "\n";
            }

            s += "----12345--\n"; // terminating line for MIME format file
            System.out.println(s);

            // Start nph-mascot.exe, and redirect the output to tmp.txt
            // Note that for Unix, you may need to use ./nph-mascot.exe
            try {
                String runTimeCommand;
                runTimeCommand = "./nph-mascot.exe 4 -commandline > tmp.txt";
                Process p = Runtime.getRuntime().exec(runTimeCommand);
                OutputStream output = p.getOutputStream();
                BufferedWriter bwrite = new BufferedWriter(new OutputStreamWriter(output));

                InputStream input = p.getInputStream();
                BufferedReader bread = new BufferedReader(new InputStreamReader(input));

                bwrite.write(s);
                bwrite.close();
                int returnValue = p.waitFor();

                String buf = null;
                while((buf = bread.readLine()) != null){
                    if (buf.indexOf("SUCCESS") != -1) {
                        if ((buf = bread.readLine()) != null){
                            System.out.println(buf);
                            compareResults(file, buf);
                            success = true;
                        }
                    } else if (buf.indexOf("ERROR") != -1) {
                        System.out.println("Search failed: " +buf);
                        while ((buf = bread.readLine()) != null) {
                            System.out.println(buf);
                        }
                    }
                }
                bread.close();

            }
            catch(java.io.IOException e){
                e.printStackTrace();
            }
            catch(java.lang.InterruptedException e){
                e.printStackTrace();
            }
        }
        else
        {
            System.out.println("Cannot open results file " +filename); 
            System.out.println(file.getLastErrorString() + "\n");
        }
        return success;
    }

    private static void compareResults(ms_mascotresfile originalSearch, 
                                       String repeatedSearchFileName)
    {
        ms_mascotresfile repeatedSearch = new ms_mascotresfile(repeatedSearchFileName, 0, "");
        boolean anyReport = false;

        if (repeatedSearch.isValid()) {
            if (originalSearch.anyPMF()) {
                // Use protein summary
                ms_proteinsummary originalResults 
                    = new ms_proteinsummary(originalSearch, 
                                            ms_mascotresults.MSRES_GROUP_PROTEINS,
                                            0, 1, null, null);
                ms_proteinsummary repeatedResults 
                    = new ms_proteinsummary(repeatedSearch,
                                            ms_mascotresults.MSRES_GROUP_PROTEINS,
                                            0, 1, null, null);

                ms_protein  originalProt = originalResults.getHit(1);
                ms_protein  repeatedProt = repeatedResults.getHit(1);
                if (originalProt != null && repeatedProt != null) {
                    double diff = repeatedProt.getScore() - originalProt.getScore();
                    if (diff > 10.0) {
                        System.out.println("Protein score is " 
                                            + diff
                                            + " higher for search " 
                                            +  originalSearch.getFileName()
                                            + " than "
                                            + repeatedSearchFileName
                                            + "\n");
                        anyReport = true;
                    }
                }
            } else {
                // Use peptide summary
                ms_peptidesummary originalResults 
                    = new ms_peptidesummary(originalSearch,
                                            ms_mascotresults.MSRES_GROUP_PROTEINS,
                                            0, 1, null, 0, 0, null);

                ms_peptidesummary repeatedResults 
                    = new ms_peptidesummary(repeatedSearch,
                                            ms_mascotresults.MSRES_GROUP_PROTEINS,
                                            0, 1, null, 0, 0, null);

                // Compare peptide scores
                for (int q=1; q <= originalSearch.getNumQueries(); q++) {
                    ms_peptide pepOriginal = originalResults.getPeptide(q, 1);
                    ms_peptide pepRepeated = repeatedResults.getPeptide(q, 1);
                    if (pepOriginal != null && pepRepeated != null) {
                        double diff = pepRepeated.getIonsScore() 
                                    - pepOriginal.getIonsScore();
                        if (diff > 10.0) {
                            System.out.println("Query " 
                                                + q
                                                + "has score " + diff
                                                + " higher for search "
                                                + originalSearch.getFileName()
                                                + " than "
                                                + repeatedSearchFileName
                                                + "\n");

                            anyReport = true;
                        }
                    }
                }
            }
            if (!anyReport) {
                System.out.println("Similar results for "
                                   + originalSearch.getFileName()
                                   + " and "
                                   + repeatedSearchFileName
                                   + "\n");
            }
        } else {
            System.out.println("Invalid repeat search " 
                               + repeatedSearch.getLastErrorString());
        }
    }


    private static void usage()
    {
        System.out.println("Usage: java repeat_search <results_file> ");
        System.out.println("Given an mascot results file name, repeat the search ");
        System.out.println("against the same data");
        System.out.println("   results_file is a full path to a results file");
        System.out.println("The program must be run from the mascot cgi directory");
    }
}

