/*
##############################################################################
# file: resfile_summary.java                                                 #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2016 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
# $Source: /vol/cvsroot/parser/examples/test_java/resfile_summary.java,v $   #
#     $Author: francoisr $                                                   #
#       $Date: 2016/07/19 14:22:55 $                                         #
#   $Revision: 1.3 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

// Note - uses java.util.regex - some lines will need to be commented out
// before this will work on JDK versions older than 1.4.  There are marked 
// in the code

import java.util.Date;
import matrix_science.msparser.*;
import java.util.regex.*;           // comment out if using version of JDK prior to 1.4
import java.io.File;

public class resfile_summary {
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
        int returnValue = -1;
        
        // ----- Object creation -----
        if (argv.length < 1) {
            System.out.println("Must specify results filename as parameter");
            System.exit(returnValue);
        }

        ms_mascotresfile file = new ms_mascotresfile(argv[0]);
        if (checkErrors(file)) {

            /*
             * The parameters passed to ms_peptidesummary or ms_proteinsummary determine
             * the type of grouping and the number of proteins and peptides displayed.
             * Default parameters can be returned using ms_mascotresfile::get_ms_mascotresults_params().
             * The return values from this function depend on the type of search,
             * and also on values in the mascot.dat configuration file if that is available.
             */

            ms_datfile datfile = new ms_datfile("../config/mascot.dat"); //You may need to change this path

            //if the mascot.dat isn't available, use defaults
            ms_mascotoptions mascotOptions = new ms_mascotoptions();
            
            if (datfile.isValid())
            {
                mascotOptions = datfile.getMascotOptions();
            }

            boolean[]   usePeptideSummary = {false};
            long[]      flags = {0};
            double[]    minProbability = {0};
            int[]       maxHitsToReport = {0};
            double[]    ignoreIonsScoreBelow = {0};
            long[]      minPepLenInPepSummary = {0};
            long[]      flags2 = {0};

            String scriptName = file.get_ms_mascotresults_params(
                mascotOptions,
                flags,
                minProbability,
                maxHitsToReport,
                ignoreIonsScoreBelow,
                minPepLenInPepSummary,
                usePeptideSummary,
                flags2
            );

            boolean bResult;
            if (usePeptideSummary[0]) {
                /*
                 * For peptide summary
                 *
                 * Flags defined for hierarchical clustering algorithm:
                 *     flags: ms_mascotresults::MSRES_CLUSTER_PROTEINS
                 *            | ms_mascotresults::MSRES_SHOW_SUBSETS
                 *            | ms_mascotresults::MSRES_MUDPIT_PROTEIN_SCORE;
                 *     flags2: ms_peptidesummary::MSPEPSUM_USE_HOMOLOGY_THRESH;
                 *
                 * Flags defined for original simple parsimony algorithm:
                 *     flags: ms_mascotresults::MSRES_GROUP_PROTEINS | ms_mascotresults::MSRES_SHOW_SUBSETS;
                 */

                System.out.println("-------------------------------------------------------------");
                System.out.println("---   Peptide summary report                              ---");
                System.out.println("-------------------------------------------------------------");

                bResult = showResults(file,
                        usePeptideSummary[0],
                        (int)flags[0],
                        minProbability[0],
                        maxHitsToReport[0],
                        "", // Unigene file name
                        ignoreIonsScoreBelow[0],
                        (int)minPepLenInPepSummary[0],
                        (int)flags2[0]
                        );

            } else {
                
                // Show results from full protein summary, remove grouping
                flags[0] &= ~ms_mascotresults.MSRES_GROUP_PROTEINS;
                flags[0] &= ~ms_mascotresults.MSRES_SHOW_SUBSETS;
                
                System.out.println("-------------------------------------------------------------");
                System.out.println("---   Full Protein summary report                         ---");
                System.out.println("-------------------------------------------------------------");

                bResult = showResults(file,
                        usePeptideSummary[0],
                        (int)flags[0],
                        minProbability[0],
                        maxHitsToReport[0],
                        "", // Unigene file name
                        ignoreIonsScoreBelow[0],
                        (int)minPepLenInPepSummary[0],
                        (int)flags2[0]
                        );
                
                if(bResult && checkErrors(file)) {
                    System.out.println();
                    
                    // Show results from concise protein summary, add grouping
                    flags[0] |= ms_mascotresults.MSRES_GROUP_PROTEINS;
                    flags[0] |= ms_mascotresults.MSRES_SHOW_SUBSETS;
                    
                    System.out.println("-------------------------------------------------------------");
                    System.out.println("---   Concise Protein summary report                      ---");
                    System.out.println("-------------------------------------------------------------");

                    bResult = showResults(file,
                            usePeptideSummary[0],
                            (int)flags[0],
                            minProbability[0],
                            maxHitsToReport[0],
                            "", // Unigene file name
                            ignoreIonsScoreBelow[0],
                            (int)minPepLenInPepSummary[0],
                            (int)flags2[0]
                            );
                }
            }
            
            if (bResult && checkErrors(file)) {
                returnValue = 0;
            }
        }
        
        System.exit(returnValue);
    }
    
  /**
   * showResults(
   * ms_mascotresfile file,
   * boolean pepSum,            - true for peptide summary, false for protein
   * int flags,                 - the flags for display
   * double minProteinProb,     - the minimum protein probability to display
   * int maxHits,               - the maximum number of hits to display
   * String unigeneFile,        - the path to a unigene file
   * double minIonsScore        - the minimum ions score to use
   * int minPepLenInPepSummary  - the minPepLenInPepSummary to use
   * int flags2                 - the flags of enum MSPEPSUM
   * )
   * 
   **/
  
    private static boolean showResults(ms_mascotresfile file, 
                                    boolean pepSum, 
                                    int flags, 
                                    double minProteinProb, 
                                    int maxHits,
                                    String unigeneFile,
                                    double minIonsScore, 
                                    int minPepLenInPepSummary,
                                    int flags2){
  
        ms_mascotresults results=null;
        int family;
        int hit;
        ms_protein prot;
        String accession;
        String description;
        double mass;
        int dbIdx;
        int num_peps;
        int i;
        int query;
        int p;
        int u;
        ms_peptide pep;

        if(pepSum) {
            results = new ms_peptidesummary(file,
                                            flags,
                                            minProteinProb,
                                            maxHits,
                                            unigeneFile,
                                            minIonsScore,
                                            minPepLenInPepSummary, 
                                            null,
                                            flags2);
        } else {
            results = new ms_proteinsummary(file,
                                            flags,
                                            minProteinProb,
                                            maxHits,
                                            null, 
                                            null);
        }
      
        //Stop there is any fatal error occurred
        if (!checkErrors(file))
        {
            return false;
        }
      
        if (results.isNA())
            System.out.println("Nucleic acid database\n");
        else
            System.out.println("Amino acid database ");

      
        // get hit one
        family = 1;
        hit = 1;
        prot = results.getHit(hit);

        while(hit <= results.getNumberOfHits()) {
            accession     =   prot.getAccession();
            description   =   results.getProteinDescription(accession);
            mass          =   results.getProteinMass(accession);
            dbIdx         =   prot.getDB();
            
            String protein_hit = "Protein Hit " + hit;
            if((flags & ms_mascotresults.MSRES_CLUSTER_PROTEINS) == ms_mascotresults.MSRES_CLUSTER_PROTEINS) {
                protein_hit += "." + family;
            }
        
            System.out.println(protein_hit+"\n===================");
            System.out.println("Accession   : "+accession);
            System.out.println("Description : "+description);
            System.out.println("Score       : "+roundWholeNumber(prot.getScore()));
            System.out.println("Mass        : "+mass);
            System.out.println("Frame       : "+prot.getFrame());
            System.out.println("Coverage    : "+prot.getCoverage());
            System.out.println("RMS error   : "+prot.getRMSDeltas(results));
            System.out.println("Peptides    : "+prot.getNumDisplayPeptides());

            // Each protein has a number of peptides that matched - list them
            num_peps = prot.getNumPeptides();
            for(i=1; i <= num_peps; i++) {
                query = prot.getPeptideQuery(i);
                p     = prot.getPeptideP(i);
                if((p != -1) && (query != -1) && (prot.getPeptideDuplicate(i) !=
                ms_protein.DUPE_DuplicateSameQuery)) {
                    pep = results.getPeptide(query,p);
                    if(pep != null) {
                        displayPeptideInfo(false,pep,results,prot.getPeptideDuplicate(i)
                        == ms_protein.DUPE_Duplicate, prot.getPeptideIsBold(i),
                        prot.getPeptideShowCheckbox(i));
                    }
                }
            }
          
            // Now display list of all proteins that contain a subset or a same set of the matching peptides

            if((flags & ms_mascotresults.MSRES_GROUP_PROTEINS) == ms_mascotresults.MSRES_GROUP_PROTEINS
               || (flags & ms_mascotresults.MSRES_CLUSTER_PROTEINS) == ms_mascotresults.MSRES_CLUSTER_PROTEINS) {
                System.out.println("Proteins matching the same set of peptides:");
                i = 1;
           
                while((prot = results.getNextSimilarProteinOf(accession, dbIdx, i)) != null) {
                    String similar_accession = prot.getAccession();
                    int similar_dbIdx        = prot.getDB();
                    if((flags & ms_mascotresults.MSRES_CLUSTER_PROTEINS) == ms_mascotresults.MSRES_CLUSTER_PROTEINS
                        && similar_dbIdx > 1) {
                        System.out.print(similar_dbIdx + "::");
                    }
                    System.out.print(similar_accession + " - Total score: " + roundWholeNumber(prot.getScore()));
                    System.out.println(" - Peptides matched: " + prot.getNumDisplayPeptides());
                    i++;
                }
            
                if((flags & ms_mascotresults.MSRES_SHOW_SUBSETS) == ms_mascotresults.MSRES_SHOW_SUBSETS) {
                    System.out.println("Proteins matching a subset of these peptides:");
                    i = 1;
                    while((prot = results.getNextSubsetProteinOf(accession, dbIdx, i)) != null) {
                        String subset_accession = prot.getAccession();
                        int subset_dbIdx     = prot.getDB();
                        if((flags & ms_mascotresults.MSRES_CLUSTER_PROTEINS) == ms_mascotresults.MSRES_CLUSTER_PROTEINS
                            && subset_dbIdx > 1) {
                            System.out.print(subset_dbIdx + "::");
                        }
                        System.out.print(subset_accession + " - Total score: " + roundWholeNumber(prot.getScore()));
                        System.out.println(" - Peptides matched: " + prot.getNumDisplayPeptides());
                        
                        if((flags & ms_mascotresults.MSRES_CLUSTER_PROTEINS) == ms_mascotresults.MSRES_CLUSTER_PROTEINS) {
                            int j = 1;
                            if(results.getNextSimilarProteinOf(subset_accession, subset_dbIdx, j) != null) {
                                System.out.println("  Proteins matching the same set of peptides for this subset:");
                            }
                            ms_protein similar_subset_prot;
                            while((similar_subset_prot = results.getNextSimilarProteinOf(subset_accession, subset_dbIdx, j)) != null) {
                                String similar_subset_accession = similar_subset_prot.getAccession();
                                int similar_subset_dbIdx = similar_subset_prot.getDB();
                                System.out.print("  ");
                                if(similar_subset_dbIdx > 1) {
                                    System.out.print(similar_subset_dbIdx + "::");
                                }
                                System.out.print(similar_subset_accession + " - Total score: " + roundWholeNumber(similar_subset_prot.getScore()));
                                System.out.println(" - Peptides matched: " + similar_subset_prot.getNumDisplayPeptides());
                                j++;
                            }
                        }
                        
                        i++;
                    }
                }
            }
            
            if((flags & ms_mascotresults.MSRES_CLUSTER_PROTEINS) == ms_mascotresults.MSRES_CLUSTER_PROTEINS) {
                prot = results.getNextFamilyProtein(hit, family++);
                if(prot == null) {
                    hit++;
                    prot = results.getHit(hit);
                    family = 1;
                }
            } else {
                hit++;
                prot = results.getHit(hit);
            }

            System.out.println("\n");  
        }
        results.createUnassignedList(ms_mascotresults.SCORE);
        if(results.getNumberOfUnassigned() > 0) {
            System.out.println("Unassigned list");
            System.out.println("---------------");
            for(u=1; u <= results.getNumberOfUnassigned(); u++) {
                pep = results.getUnassigned(u);
                displayPeptideInfo(false, pep, results, false, true, true);
            }
        }
     
        if(pepSum) displayYellowPopupInfo(results,97);
        
        return true;
    }

    
    /**
     * displayYellowPopupInfo (ms_mascotresults results, int queryNo)
     * Shows the equivalent of the yellow popup box for given query
     **/
  
    private static void displayYellowPopupInfo (ms_mascotresults results, int queryNo) {
      
        int p;
        ms_peptide pep;
        String seq;
        String tmp;
        String hit;
        String protein;
        String morethan="";

        System.out.println("Score Delta      Hit Protein Peptide");

        for(p=1; p <= 10; p++) {
            pep = results.getPeptide(queryNo,p);
            if(pep != null) {
                seq = pep.getPeptideStr();
                if(!(seq.equals(""))) {
                    tmp = results.getProteinsWithThisPepMatch(queryNo,p,true);
                    hit = "   ";
                    protein = "       ";
                  
                    // comment from here
                  
                    if(!(tmp.equals(""))) {
                        // uses Java 1.4 regex
                        String regex = "(\\d):([^ ]*)[ ]*(.*)";
                        Pattern pat = Pattern.compile(regex);
                        Matcher m = pat.matcher(tmp);
                      
                        if(m.matches()) {
                            int groupCount = m.groupCount();
                            if (groupCount >= 1) hit = m.group(1);
                            if (groupCount >= 2) protein = m.group(2);
                            if (groupCount >= 3) morethan = m.group(3);
                        }    
                        if(!(morethan.equals(""))) {
                            hit = hit+"+";
                        }
                    }
                  
                    // to here out if using versions of java prior to 1.4
                    System.out.print(pep.getIonsScore()+ "  "+ pep.getDelta()+ "  ");
                    System.out.println(hit+ "  "+ protein+ " "+ pep.getPeptideStr());
                }
            }
        }
        p=1;
        System.out.println("Accessions that matched query "+ queryNo+ " rank "
        + p+ ":- "+results.getProteinsWithThisPepMatch(queryNo,p, false));
    }
  
  
    /**
     * displayPeptideInfo (
     * boolean showFullDetails,
     * ms_peptide p,
     * ms_mascotresults r,
     * boolean isDuplicate,
     * boolean isBold,
     * boolean showCheckBox
     **/
  
    private static void displayPeptideInfo(boolean showFullDetails, ms_peptide p,
                                           ms_mascotresults r, 
                                           boolean isDuplicate, 
                                           boolean isBold, 
                                           boolean showCheckBox) {
        int q;
      
        q=p.getQuery();
      
        if(showFullDetails) {
            System.out.println("Peptide hit");
            if(p.getAnyMatch()) {
                System.out.println("  Query       : "+q);
                System.out.println("  Rank        : "+p.getRank());
                System.out.println("  Matched     : "+p.getAnyMatch());
                System.out.println("  missedCleave: "+p.getMissedCleavages());
                System.out.println("  mrCalc      : "+p.getMrCalc());
                System.out.println("  delta       : "+p.getDelta());
                System.out.println("  observed    : "+p.getObserved());
                System.out.println("  charge      : "+p.getCharge());
                System.out.println("  mrExp       : "+p.getMrExperimental());
                System.out.println("  ionsMatched : "+p.getNumIonsMatched());
                System.out.println("  peptideStr  : "+p.getPeptideStr());
                System.out.println("  peaksUsed1  : "+p.getPeaksUsedFromIons1());
                System.out.println("  varModsStr  : "+p.getVarModsStr());
                System.out.println("  readable mod: "+r.getReadableVarMods(q, p.getRank(),2));
                System.out.println("  ionsScore   : "+p.getIonsScore());
                System.out.println("  seriesUsedS : "+p.getSeriesUsedStr());
                System.out.println("  peaksUsed2  : "+p.getPeaksUsedFromIons2());
                System.out.println("  peaksUsed3  : "+p.getPeaksUsedFromIons3());
                System.out.print("  idth, hth, p: "  +r.getPeptideIdentityThreshold(q,20));
                System.out.print(",  "               +r.getHomologyThreshold(q,20));
                System.out.println(",  "             +r.getProbOfPepBeingRandomMatch(p.getIonsScore(),q));
                System.out.println();
            } else {
                System.out.println("  No match");
            }
        } else {
            if(showCheckBox) System.out.print("CB ");
            else System.out.print("-- ");
            if(isBold) System.out.print("BOLD ");
            else System.out.print("dim  ");
              
            // java has no printf :
            String query = ""+q;
            String observed = ""+p.getObserved();
            System.out.print(padout(query,4," "));
            if(observed.indexOf(".")>-1) {
                String one = observed.substring(0,observed.indexOf("."));
                String two = observed.substring(observed.indexOf(".")+1,observed.length());
                two = padend(two,6,"0");
                observed = one+"."+two;
            }
            System.out.print(padout(observed,11," "));

            System.out.print(padout(""+p.getRank(),4," "));
            System.out.print("("+padout(""+p.getPrettyRank(),4," ")+")");
            System.out.print(padout(p.getPeptideStr(),20," "));

            double score = p.getIonsScore();
            String scoreString = ""+score;

            if(scoreString.indexOf(".")>-1) {
                String one = scoreString.substring(0,scoreString.indexOf("."));
                String two = scoreString.substring(scoreString.indexOf(".")+1,
                                                   scoreString.length());
                if(isDuplicate) {
                    System.out.print("("+padout(one,3,"")+"."+padend(two,2,"0")+")");
                } else {
                    System.out.print(" "+padout(one,3,"")+"."+padend(two,2,"0")+" ");
                }
            } else {
                System.out.println(scoreString);
            }

            // perl seems to take 1st letter of the readable mod, pad with 2 spaces
            // and ignore (set to 0) any letters, so this does the same (quick & nasty)
            if(r.getReadableVarMods(q,p.getRank(),2).equals("")) System.out.println("  0");
            else {
                String let = r.getReadableVarMods(q,p.getRank(),2).substring(0,1);
                try {
                    int intLet = Integer.parseInt(let);
                    System.out.println("  "+let);
                } catch (NumberFormatException notNumber) {
                    System.out.println("  0");
                }
            }
        }
    }
  
  
    /**
     * padout (String toPrint, int length, String padding)
     * pads out the string toPrint to the passed length
     **/
  
    private static String padout(String toPrint, int length,String padding) {
    
        String returnValue="";
    
        if(toPrint.length() < length) {
            for(int queryLoop = 1; queryLoop <= length - toPrint.length(); queryLoop++) {
                returnValue += padding;
            }   
            returnValue += toPrint;
        }
        else returnValue = toPrint;
   
        return returnValue;
    }
  
    /**
     * padend (String toPrint, int length, String padding)
     * pads out the string toPrint to the passed length at the end of the string
     **/
  
    private static String padend(String toPrint, int length,String padding) {
    
        String returnValue=toPrint;
    
        if(toPrint.length() < length) {
            for(int queryLoop = 1; queryLoop <= length - toPrint.length(); queryLoop++) {
                returnValue += padding;
            }   
        }
        return returnValue;
    }
  
    /**
     * roundWholeNumber(double toRound)
     * remove .0 from whole numbers
     * 
     **/
  
    private static String roundWholeNumber (double toRound) {
        String myInt = ""+toRound;
        myInt += "\n";
        if(myInt.indexOf(".0\n")>-1) {
            return myInt.substring(0,myInt.indexOf(".0\n"));
        }
        return ""+toRound;
    }
  
    /**
     * checkErrors(ms_mascotresfile file)
     * check if any error was raised after processing the file
     * 
     **/
    
    private static boolean checkErrors(ms_mascotresfile file) {
        if((file.getLastError())>0) {
            for(int loopErrors=1; loopErrors <= file.getNumberOfErrors(); loopErrors++) {
                System.out.print("Error number: "+file.getErrorNumber(loopErrors));
                System.out.print(" : "+file.getErrorString(loopErrors));
                System.out.println("");
            }
        }
        
        //Call isValid before clearAllErrors, otherwise this method always returns true
        boolean bIsValid = file.isValid();
        file.clearAllErrors();
        
        return bIsValid;
    }
}

/*

will give the output: 


C:>java -classpath .;../java/msparser.jar resfile_summary F981123.dat
-------------------------------------------------------------
---   Peptide summary report                              ---
-------------------------------------------------------------
Amino acid database
Protein Hit 1
===================
Accession   : Q9XZJ2
Description : HEAT SHOCK PROTEIN 70.- Crassostrea gigas (Pacific oyster).
Score       : 286.47705382849546
Mass        : 79480.13
Frame       : 0
Coverage    : 57
RMS error   : 274.58772054111273
Peptides    : 4
CB BOLD    1 671.900000   1(   1)        DAGTISGLNVLR 95.12   0
CB BOLD    2 808.300000   1(   1)       TTPSYVAFTDTER 74.55   0
CB BOLD    3 973.900000   1(   1)     NQVAMNPNNTIFDAK 89.84   0
-- BOLD    41084.900000   3(   3)   IINEPTAAAIAYGLDKK 30.07   0
Proteins matching the same set of peptides:
Q94805 Total score: 283.3741076569909   Peptides matched: 4
Proteins matching a subset of these peptides:
AAF09496 Total score: 211.92705382849545  Peptides matched: 3
Q9I9Q5 Total score: 199.74  Peptides matched: 3
Q24789 Total score: 199.74  Peptides matched: 3
BAB72169 Total score: 199.74  Peptides matched: 3
Q9W6Y1 Total score: 196.63705382849543  Peptides matched: 3
AAB93665 Total score: 196.63705382849543  Peptides matched: 3
Q95V47 Total score: 194.45999999999998  Peptides matched: 3
Q91993 Total score: 193.5341076569909  Peptides matched: 3
T45476 Total score: 191.35705382849545  Peptides matched: 3
O01948 Total score: 169.67000000000002  Peptides matched: 2
O97150 Total score: 166.56705382849543  Peptides matched: 2
Q9NJ92 Total score: 125.19  Peptides matched: 2
Q9NJ93 Total score: 125.19  Peptides matched: 2
Q9PUF9 Total score: 125.19  Peptides matched: 2
S21175 Total score: 125.19  Peptides matched: 2
B44261 Total score: 125.19  Peptides matched: 2
T45473 Total score: 125.19  Peptides matched: 2
AAA64872 Total score: 125.19  Peptides matched: 2
P81159 Total score: 125.19  Peptides matched: 2
JC4610 Total score: 122.08705382849546  Peptides matched: 2
O73788 Total score: 122.08705382849544  Peptides matched: 2
1ATR Total score: 104.62  Peptides matched: 2
1ATS Total score: 104.62  Peptides matched: 2
1BA0 Total score: 104.62  Peptides matched: 2
1BA1 Total score: 104.62  Peptides matched: 2
1BUPA Total score: 104.62  Peptides matched: 2
1HSC Total score: 104.62  Peptides matched: 2
1KAX Total score: 104.62  Peptides matched: 2
1KAY Total score: 104.62  Peptides matched: 2
1KAZ Total score: 104.62  Peptides matched: 2
1NGC Total score: 104.62  Peptides matched: 2
1NGD Total score: 104.62  Peptides matched: 2
1NGE Total score: 104.62  Peptides matched: 2
1NGF Total score: 104.62  Peptides matched: 2
1NGG Total score: 104.62  Peptides matched: 2
1NGH Total score: 104.62  Peptides matched: 2
1NGI Total score: 104.62  Peptides matched: 2
2BUPA Total score: 104.62  Peptides matched: 2
HHKW7A Total score: 104.62  Peptides matched: 2
HHUM7B Total score: 104.62  Peptides matched: 2
Q9GPK0 Total score: 104.62  Peptides matched: 2
Q9GRE8 Total score: 104.62  Peptides matched: 2
Q9H3R6 Total score: 104.62  Peptides matched: 2
Q9M4E6 Total score: 104.62  Peptides matched: 2
Q9M6R1 Total score: 104.62  Peptides matched: 2
Q9NCC8 Total score: 104.62  Peptides matched: 2
Q9NGK9 Total score: 104.62  Peptides matched: 2
Q9NJB7 Total score: 104.62  Peptides matched: 2
Q9SAU8 Total score: 104.62  Peptides matched: 2
Q9TW75 Total score: 104.62  Peptides matched: 2
Q9U545 Total score: 104.62  Peptides matched: 2
Q9UAE9 Total score: 104.62  Peptides matched: 2
Q95LN9 Total score: 104.62  Peptides matched: 2
PQ0138 Total score: 104.62  Peptides matched: 2
JC2215 Total score: 104.62  Peptides matched: 2
JC4786 Total score: 104.62  Peptides matched: 2
JC5642 Total score: 104.62  Peptides matched: 2
S07197 Total score: 104.62  Peptides matched: 2
S11456 Total score: 104.62  Peptides matched: 2
T21394 Total score: 104.62  Peptides matched: 2
A25646 Total score: 104.62  Peptides matched: 2
S27004 Total score: 104.62  Peptides matched: 2
Q27031 Total score: 104.62  Peptides matched: 2
A27077 Total score: 104.62  Peptides matched: 2
AAA30130 Total score: 104.62  Peptides matched: 2
AAA30569 Total score: 104.62  Peptides matched: 2
S31716 Total score: 104.62  Peptides matched: 2
AAA33009 Total score: 104.62  Peptides matched: 2
A35922 Total score: 104.62  Peptides matched: 2
S36753 Total score: 104.62  Peptides matched: 2
S37165 Total score: 104.62  Peptides matched: 2
Q43532 Total score: 104.62  Peptides matched: 2
A44985 Total score: 104.62  Peptides matched: 2
T45478 Total score: 104.62  Peptides matched: 2
T45479 Total score: 104.62  Peptides matched: 2
A45635 Total score: 104.62  Peptides matched: 2
A45935 Total score: 104.62  Peptides matched: 2
A48439 Total score: 104.62  Peptides matched: 2
O61003 Total score: 104.62  Peptides matched: 2
O61004 Total score: 104.62  Peptides matched: 2
O73885 Total score: 104.62  Peptides matched: 2
O76274 Total score: 104.62  Peptides matched: 2
P79984 Total score: 104.62  Peptides matched: 2
CAA81523 Total score: 104.62  Peptides matched: 2
Q93146 Total score: 104.62  Peptides matched: 2
Q93147 Total score: 104.62  Peptides matched: 2
Q94614 Total score: 104.62  Peptides matched: 2
O96541 Total score: 104.62  Peptides matched: 2
O96911 Total score: 104.62  Peptides matched: 2
Q9TUG3 Total score: 101.51705382849545  Peptides matched: 2
Q9TX61 Total score: 101.51705382849545  Peptides matched: 2
HS74_YEAST Total score: 101.51705382849545  Peptides matched: 2
Q99KD7 Total score: 101.51705382849545  Peptides matched: 2
S08211 Total score: 101.51705382849545  Peptides matched: 2
S10859 Total score: 101.51705382849545  Peptides matched: 2
AAD11466 Total score: 101.51705382849545  Peptides matched: 2
CAA33735 Total score: 101.51705382849545  Peptides matched: 2
B36590 Total score: 101.51705382849545  Peptides matched: 2
AAA37859 Total score: 101.51705382849545  Peptides matched: 2
A55719 Total score: 101.51705382849545  Peptides matched: 2
B86295 Total score: 101.51705382849545  Peptides matched: 2
AAA49563 Total score: 95.12  Peptides matched: 1
1HJOA Total score: 74.55  Peptides matched: 1
1NGA Total score: 74.55  Peptides matched: 1
1QQMA Total score: 74.55  Peptides matched: 1
1QQNA Total score: 74.55  Peptides matched: 1
1QQOA Total score: 74.55  Peptides matched: 1
Q9I8F9 Total score: 74.55  Peptides matched: 1
Q9IAC1 Total score: 74.55  Peptides matched: 1
Q9LU13 Total score: 74.55  Peptides matched: 1
Q9N1U2 Total score: 74.55  Peptides matched: 1
Q9NFT1 Total score: 74.55  Peptides matched: 1
Q9QWJ5 Total score: 74.55  Peptides matched: 1
HS71_SCHPO Total score: 74.55  Peptides matched: 1
JH0095 Total score: 74.55  Peptides matched: 1
JU0164 Total score: 74.55  Peptides matched: 1
PC1156 Total score: 74.55  Peptides matched: 1
AAA03451 Total score: 74.55  Peptides matched: 1
AAB03704 Total score: 74.55  Peptides matched: 1
S09036 Total score: 74.55  Peptides matched: 1
AAA17441 Total score: 74.55  Peptides matched: 1
O18474 Total score: 74.55  Peptides matched: 1
T19211 Total score: 74.55  Peptides matched: 1
AAD21815 Total score: 74.55  Peptides matched: 1
AAD21816 Total score: 74.55  Peptides matched: 1
T22169 Total score: 74.55  Peptides matched: 1
S25585 Total score: 74.55  Peptides matched: 1
A25773 Total score: 74.55  Peptides matched: 1
Q26357 Total score: 74.55  Peptides matched: 1
Q26936 Total score: 74.55  Peptides matched: 1
Q26937 Total score: 74.55  Peptides matched: 1
A29160 Total score: 74.55  Peptides matched: 1
S31766 Total score: 74.55  Peptides matched: 1
AAL34314 Total score: 74.55  Peptides matched: 1
S37394 Total score: 74.55  Peptides matched: 1
I37564 Total score: 74.55  Peptides matched: 1
T41121 Total score: 74.55  Peptides matched: 1
T43724 Total score: 74.55  Peptides matched: 1
T45468 Total score: 74.55  Peptides matched: 1
A45871 Total score: 74.55  Peptides matched: 1
A48872 Total score: 74.55  Peptides matched: 1
A49242 Total score: 74.55  Peptides matched: 1
I51129 Total score: 74.55  Peptides matched: 1
CAA52328 Total score: 74.55  Peptides matched: 1
S53357 Total score: 74.55  Peptides matched: 1
I54542 Total score: 74.55  Peptides matched: 1
AAA57233 Total score: 74.55  Peptides matched: 1
Q63256 Total score: 74.55  Peptides matched: 1
Q63718 Total score: 74.55  Peptides matched: 1
S67431 Total score: 74.55  Peptides matched: 1
BAB72167 Total score: 74.55  Peptides matched: 1
BAB72168 Total score: 74.55  Peptides matched: 1
BAB72233 Total score: 74.55  Peptides matched: 1
O73922 Total score: 74.55  Peptides matched: 1
AAA78276 Total score: 74.55  Peptides matched: 1
BAB78505 Total score: 74.55  Peptides matched: 1
Q98896 Total score: 74.55  Peptides matched: 1
Q98899 Total score: 74.55  Peptides matched: 1
Q98900 Total score: 74.55  Peptides matched: 1
Q98901 Total score: 74.55  Peptides matched: 1
E980236 Total score: 74.55  Peptides matched: 1
Q9UVM0 Total score: 71.44705382849544  Peptides matched: 1
JQ1515 Total score: 71.44705382849544  Peptides matched: 1
PC7036 Total score: 71.44705382849544  Peptides matched: 1
HHBYA1 Total score: 30.07  Peptides matched: 1
HS7E_SPIOL Total score: 30.07  Peptides matched: 1
Q9ATB8 Total score: 30.07  Peptides matched: 1
Q9DC27 Total score: 30.07  Peptides matched: 1
Q9FSY7 Total score: 30.07  Peptides matched: 1
Q9GPM5 Total score: 30.07  Peptides matched: 1
Q9GTX3 Total score: 30.07  Peptides matched: 1
Q9LHA8 Total score: 30.07  Peptides matched: 1
Q9M4E7 Total score: 30.07  Peptides matched: 1
Q9M4E8 Total score: 30.07  Peptides matched: 1
Q9NAX9 Total score: 30.07  Peptides matched: 1
Q9NCC9 Total score: 30.07  Peptides matched: 1
Q9NCD0 Total score: 30.07  Peptides matched: 1
Q9NCD1 Total score: 30.07  Peptides matched: 1
Q9NCD2 Total score: 30.07  Peptides matched: 1
Q9NCD3 Total score: 30.07  Peptides matched: 1
Q9NCD4 Total score: 30.07  Peptides matched: 1
Q9NCD5 Total score: 30.07  Peptides matched: 1
Q9NCD6 Total score: 30.07  Peptides matched: 1
Q9NCD7 Total score: 30.07  Peptides matched: 1
Q9NCD8 Total score: 30.07  Peptides matched: 1
Q9NCD9 Total score: 30.07  Peptides matched: 1
Q9NCE0 Total score: 30.07  Peptides matched: 1
Q9NHB4 Total score: 30.07  Peptides matched: 1
Q9P8E0 Total score: 30.07  Peptides matched: 1
Q9U540 Total score: 30.07  Peptides matched: 1
Q9ZS55 Total score: 30.07  Peptides matched: 1
HS71_CANAL Total score: 30.07  Peptides matched: 1
HS71_PICAN Total score: 30.07  Peptides matched: 1
HS71_YEAST Total score: 30.07  Peptides matched: 1
HS72_PICAN Total score: 30.07  Peptides matched: 1
HS72_YEAST Total score: 30.07  Peptides matched: 1
Q96IS6 Total score: 30.07  Peptides matched: 1
Q96W30 Total score: 30.07  Peptides matched: 1
Q943K7 Total score: 30.07  Peptides matched: 1
Q961X6 Total score: 30.07  Peptides matched: 1
JQ0966 Total score: 30.07  Peptides matched: 1
CAA02784 Total score: 30.07  Peptides matched: 1
S03250 Total score: 30.07  Peptides matched: 1
T03581 Total score: 30.07  Peptides matched: 1
T04078 Total score: 30.07  Peptides matched: 1
T04080 Total score: 30.07  Peptides matched: 1
Q05829 Total score: 30.07  Peptides matched: 1
T06358 Total score: 30.07  Peptides matched: 1
AAB06397 Total score: 30.07  Peptides matched: 1
T06598 Total score: 30.07  Peptides matched: 1
JC7132 Total score: 30.07  Peptides matched: 1
BAB08435 Total score: 30.07  Peptides matched: 1
BAA13948 Total score: 30.07  Peptides matched: 1
S14949 Total score: 30.07  Peptides matched: 1
S14950 Total score: 30.07  Peptides matched: 1
T15513 Total score: 30.07  Peptides matched: 1
Q17310 Total score: 30.07  Peptides matched: 1
S18181 Total score: 30.07  Peptides matched: 1
S20139 Total score: 30.07  Peptides matched: 1
S21879 Total score: 30.07  Peptides matched: 1
S21880 Total score: 30.07  Peptides matched: 1
Q23712 Total score: 30.07  Peptides matched: 1
S24782 Total score: 30.07  Peptides matched: 1
Q24895 Total score: 30.07  Peptides matched: 1
Q24928 Total score: 30.07  Peptides matched: 1
Q24952 Total score: 30.07  Peptides matched: 1
A25089 Total score: 30.07  Peptides matched: 1
Q27078 Total score: 30.07  Peptides matched: 1
Q27121 Total score: 30.07  Peptides matched: 1
Q27146 Total score: 30.07  Peptides matched: 1
Q27147 Total score: 30.07  Peptides matched: 1
CAA27330 Total score: 30.07  Peptides matched: 1
AAA28075 Total score: 30.07  Peptides matched: 1
CAA28976 Total score: 30.07  Peptides matched: 1
CAA31393 Total score: 30.07  Peptides matched: 1
CAA31663 Total score: 30.07  Peptides matched: 1
A32475 Total score: 30.07  Peptides matched: 1
T34037 Total score: 30.07  Peptides matched: 1
AAA34139 Total score: 30.07  Peptides matched: 1
Q40924 Total score: 30.07  Peptides matched: 1
S41372 Total score: 30.07  Peptides matched: 1
S42488 Total score: 30.07  Peptides matched: 1
A42582 Total score: 30.07  Peptides matched: 1
S44168 Total score: 30.07  Peptides matched: 1
D44261 Total score: 30.07  Peptides matched: 1
O45038 Total score: 30.07  Peptides matched: 1
T45298 Total score: 30.07  Peptides matched: 1
T45474 Total score: 30.07  Peptides matched: 1
T45475 Total score: 30.07  Peptides matched: 1
T45477 Total score: 30.07  Peptides matched: 1
T45517 Total score: 30.07  Peptides matched: 1
T45522 Total score: 30.07  Peptides matched: 1
S46302 Total score: 30.07  Peptides matched: 1
T46574 Total score: 30.07  Peptides matched: 1
T46650 Total score: 30.07  Peptides matched: 1
T48270 Total score: 30.07  Peptides matched: 1
T48271 Total score: 30.07  Peptides matched: 1
A48469 Total score: 30.07  Peptides matched: 1
O48563 Total score: 30.07  Peptides matched: 1
S49303 Total score: 30.07  Peptides matched: 1
S51682 Total score: 30.07  Peptides matched: 1
S51712 Total score: 30.07  Peptides matched: 1
AAB53051 Total score: 30.07  Peptides matched: 1
S53126 Total score: 30.07  Peptides matched: 1
CAA54419 Total score: 30.07  Peptides matched: 1
O62564 Total score: 30.07  Peptides matched: 1
AAB63968 Total score: 30.07  Peptides matched: 1
AAA65099 Total score: 30.07  Peptides matched: 1
O65171 Total score: 30.07  Peptides matched: 1
S71171 Total score: 30.07  Peptides matched: 1
O76306 Total score: 30.07  Peptides matched: 1
CAC84456 Total score: 30.07  Peptides matched: 1
AAF88019 Total score: 30.07  Peptides matched: 1
D90093 Total score: 30.07  Peptides matched: 1
P90655 Total score: 30.07  Peptides matched: 1
Q91624 Total score: 30.07  Peptides matched: 1
P91738 Total score: 30.07  Peptides matched: 1
AAA92743 Total score: 30.07  Peptides matched: 1
O93935 Total score: 30.07  Peptides matched: 1
O94104 Total score: 30.07  Peptides matched: 1
O94106 Total score: 30.07  Peptides matched: 1
Q94439 Total score: 30.07  Peptides matched: 1
H96605 Total score: 30.07  Peptides matched: 1
P97966 Total score: 30.07  Peptides matched: 1
AAA99920 Total score: 30.07  Peptides matched: 1
Q9NCC5 Total score: 26.967053828495448  Peptides matched: 1
Q9NCC6 Total score: 26.967053828495448  Peptides matched: 1
Q9NCC7 Total score: 26.967053828495448  Peptides matched: 1
AAC05418 Total score: 26.967053828495448  Peptides matched: 1
AAB08760 Total score: 26.967053828495448  Peptides matched: 1
AAF23276 Total score: 26.967053828495448  Peptides matched: 1
Q91688 Total score: 26.967053828495448  Peptides matched: 1
P93937 Total score: 26.967053828495448  Peptides matched: 1


Protein Hit 2
===================
Accession   : S14992
Description : dnaK-type molecular chaperone hsp70 - soybean
Score       : 166.04
Mass        : 78349.66
Frame       : 0
Coverage    : 45
RMS error   : 316.0424369822863
Peptides    : 3
-- dim     2 808.300000   1(   1)       TTPSYVAFTDTER 74.55   0
-- dim     3 973.900000   2(   2)     NQVAMNPQNTVFDAK 61.42   0
-- dim     41084.900000   3(   3)   IINEPTAAAIAYGLDKK 30.07   0
Proteins matching the same set of peptides:
S53498 Total score: 166.04   Peptides matched: 3
Proteins matching a subset of these peptides:
Q9R2A1 Total score: 135.97  Peptides matched: 2
HHXL70 Total score: 135.97  Peptides matched: 2
Q96QC9 Total score: 135.97  Peptides matched: 2
S21365 Total score: 135.97  Peptides matched: 2
S41415 Total score: 135.97  Peptides matched: 2
B45871 Total score: 135.97  Peptides matched: 2
I49761 Total score: 135.97  Peptides matched: 2
AAA74906 Total score: 135.97  Peptides matched: 2
O75634 Total score: 135.97  Peptides matched: 2
O88686 Total score: 135.97  Peptides matched: 2
Q17228 Total score: 61.42  Peptides matched: 1
Q17289 Total score: 61.42  Peptides matched: 1
Q27379 Total score: 61.42  Peptides matched: 1
AAA27868 Total score: 61.42  Peptides matched: 1
A34041 Total score: 58.317053828495446  Peptides matched: 1


Protein Hit 3
===================
Accession   : A36333
Description : dnaK-type molecular chaperone Hsc70-4 - fruit fly (Drosophila melanogaster)
Score       : 147.45
Mass        : 77814.51
Frame       : 0
Coverage    : 45
RMS error   : 464.59967503590167
Peptides    : 3
-- dim     2 808.300000   1(   1)       TTPSYVAFTDTER 74.55   0
-- dim     3 973.900000   3(   3)     NQVAMNPTQTIFDAK 42.83   0
-- dim     41084.900000   3(   3)   IINEPTAAAIAYGLDKK 30.07   0
Proteins matching the same set of peptides:
AAF55150 Total score: 147.45   Peptides matched: 3
Proteins matching a subset of these peptides:


Protein Hit 4
===================
Accession   : A03309
Description : dnaK-type molecular chaperone Dsim/Hsc70-1 - fruit fly (Drosophila simulans) (fragments)
Score       : 147.03
Mass        : 25558.88
Frame       : 0
Coverage    : 28
RMS error   : 365.7556240028131
Peptides    : 2
-- dim     2 808.300000   3(   3)       TTPSYVAFTESER 57.19   0
-- dim     3 973.900000   1(   1)     NQVAMNPNNTIFDAK 89.84   0
Proteins matching the same set of peptides:
AAA28632 Total score: 147.03   Peptides matched: 2
JN0668 Total score: 143.92705382849545   Peptides matched: 2
Proteins matching a subset of these peptides:
Q9BIS1 Total score: 57.19  Peptides matched: 1
Q9BIS3 Total score: 57.19  Peptides matched: 1
B03309 Total score: 57.19  Peptides matched: 1


Protein Hit 5
===================
Accession   : Q95PU3
Description : HEAT SHOCK PROTEIN (HSP70).- Euplotes crassus.
Score       : 113.66
Mass        : 80429.66
Frame       : 0
Coverage    : 30
RMS error   : 407.38524706491955
Peptides    : 2
-- dim     2 808.300000   1(   1)       TTPSYVAFTDTER 74.55   0
CB dim     41084.900000   1(   1)   IIIEPTAAAIAYGLDKK 39.11   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:


Protein Hit 6
===================
Accession   : 1NGB
Description : Heat-shock cognate 70kd protein (44kd atpase n-terminal Fragment) (EC 3.6.1.3) mutant with glu 175 replace
d by Gln (e175q) - bovine
Score       : 109.89
Mass        : 45220.22
Frame       : 0
Coverage    : 30
RMS error   : 411.19901615189326
Peptides    : 2
-- dim     2 808.300000   1(   1)       TTPSYVAFTDTER 74.55   0
-- dim     41084.900000   2(   2)   IINQPTAAAIAYGLDKK 35.34   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:


Protein Hit 7
===================
Accession   : O61226
Description :
Score       : 90.71000000000001
Mass        : 0.0
Frame       : 0
Coverage    : 30
RMS error   : 804.0968914838121
Peptides    : 2
-- dim     2 808.300000   2(   2)       TTPSYVAFTNTER 60.64   0
-- dim     41084.900000   3(   3)   IINEPTAAAIAYGLDKK 30.07   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:


Protein Hit 8
===================
Accession   : P87036
Description :
Score       : 81.27000000000001
Mass        : 0.0
Frame       : 0
Coverage    : 29
RMS error   : 570.361400922183
Peptides    : 2
-- dim     1 671.900000   3(   3)        DAGTIAGLEVLR 51.20   0
-- dim     41084.900000   3(   3)   IINEPTAAAIAYGLDKK 30.07   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:
AG2111 Total score: 51.2  Peptides matched: 1
S06158 Total score: 51.2  Peptides matched: 1
A25398 Total score: 51.2  Peptides matched: 1
AAA30204 Total score: 51.2  Peptides matched: 1
CAB59514 Total score: 51.2  Peptides matched: 1
S74372 Total score: 51.2  Peptides matched: 1
O76958 Total score: 51.2  Peptides matched: 1
Q9GYW3 Total score: 48.09705382849545  Peptides matched: 1
Q9HG01 Total score: 48.09705382849545  Peptides matched: 1
S11448 Total score: 48.09705382849545  Peptides matched: 1
CAA36551 Total score: 48.09705382849545  Peptides matched: 1
A45515 Total score: 48.09705382849545  Peptides matched: 1
S52727 Total score: 48.09705382849545  Peptides matched: 1
Q94594 Total score: 48.09705382849545  Peptides matched: 1


Score Delta      Hit Protein Peptide
Accessions that matched query 97 rank 1:-

*/
