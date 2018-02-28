/*
##############################################################################
# file: resfile_summary.cs                                                   #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2016 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/resfile_summary.cs,v $#
#     $Author: francoisr $                                                   #
#       $Date: 2016/06/23 14:20:39 $                                         #
#   $Revision: 1.1 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Text.RegularExpressions;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class resfile_summary
    {
        public static int Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                Console.WriteLine("Must specify results filename as parameter");
                return 1;
            }

            int returnValue = 1; //failure

            ms_mascotresfile file = new ms_mascotresfile(argv[0], 0, "");

            if (checkErrors(file))
            {
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

                uint flags, flags2, minPepLenInPepSummary;
                int maxHitsToReport;
                double minProbability, ignoreIonsScoreBelow;
                bool usePeptideSummary;

                string scriptName = file.get_ms_mascotresults_params(
                                        mascotOptions,
                                        out flags,
                                        out minProbability,
                                        out maxHitsToReport,
                                        out ignoreIonsScoreBelow,
                                        out minPepLenInPepSummary,
                                        out usePeptideSummary,
                                        out flags2);

                bool bResult;
                if (usePeptideSummary)
                {
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

                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine("---   Peptide summary report                              ---");
                    Console.WriteLine("-------------------------------------------------------------");

                    bResult = showResults(file,
                                          usePeptideSummary,
                                          flags,
                                          minProbability,
                                          maxHitsToReport,
                                          ignoreIonsScoreBelow,
                                          (int)minPepLenInPepSummary,
                                          flags2);
                }
                else
                {
                    // Show results from full protein summary, remove grouping
                    flags &= ~(uint)ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS;
                    flags &= ~(uint)ms_mascotresults.FLAGS.MSRES_SHOW_SUBSETS;

                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine("---   Full Protein summary report                         ---");
                    Console.WriteLine("-------------------------------------------------------------");

                    bResult = showResults(file,
                                          usePeptideSummary,
                                          flags,
                                          minProbability,
                                          maxHitsToReport,
                                          ignoreIonsScoreBelow,
                                          (int)minPepLenInPepSummary,
                                          flags2);

                    if (bResult && checkErrors(file))
                    {
                        // Show results from concise protein summary, add grouping
                        flags |= (uint)ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS;
                        flags |= (uint)ms_mascotresults.FLAGS.MSRES_SHOW_SUBSETS;

                        Console.WriteLine("-------------------------------------------------------------");
                        Console.WriteLine("---   Concise Protein summary report                      ---");
                        Console.WriteLine("-------------------------------------------------------------");

                        bResult = showResults(file,
                                              usePeptideSummary,
                                              flags,
                                              minProbability,
                                              maxHitsToReport,
                                              ignoreIonsScoreBelow,
                                              (int)minPepLenInPepSummary,
                                              flags2);
                    }
                }

                if (bResult && checkErrors(file))
                {
                    returnValue = 0; //success
                }
            }

            //ms_mascotresults has a reference on ms_mascotresfile, therefore the GC must not delete it too early
            GC.KeepAlive(file);

            return returnValue;
        }

        private static bool showResults(ms_mascotresfile file,
                                        bool pepSum,
                                        uint flags,
                                        double minProteinProb,
                                        int maxHits,
                                        double minIonsScore,
                                        int minPepLenInPepSummary,
                                        uint flags2)
        {            
            int hit, num_peps, i, query, p, u, family, dbIdx;
            string accession, description;
            double mass;
            ms_mascotresults results;
            ms_protein prot;
            ms_peptide pep;

            if (pepSum)
            {
                results = new ms_peptidesummary(file,
                    flags,
                    minProteinProb,
                    maxHits,
                    "", //unigene file
                    minIonsScore,
                    minPepLenInPepSummary,
                    null,
                    flags2);
            }
            else
            {
                results = new ms_proteinsummary(file,
                    flags,
                    minProteinProb,
                    maxHits,
                    null,
                    null);
            }

            if (!checkErrors(file))
            {
                return false;
            }

            Console.WriteLine("{0} acid database", ((results.isNA()) ? "Nucleic" : "Amino"));

            family = 1;
            // get hit one
            hit = 1;
            prot = results.getHit(hit);

            while (hit <= results.getNumberOfHits())
            {
                accession = prot.getAccession();
                dbIdx = prot.getDB();
                description = results.getProteinDescription(accession);
                mass = results.getProteinMass(accession);

                if ((flags & (uint)ms_mascotresults.FLAGS.MSRES_CLUSTER_PROTEINS) > 0)
                {
                    Console.WriteLine("Protein Hit {0}.{1}\n===================", hit, family);
                }
                else
                {
                    Console.WriteLine("Protein Hit {0}\n===================", hit);
                }
                Console.WriteLine("Accession   : {0}", accession);
                Console.WriteLine("Description : {0}", description);
                Console.WriteLine("Score       : {0:#}", prot.getScore());
                Console.WriteLine("Mass        : {0}", mass);
                Console.WriteLine("Frame       : {0}", prot.getFrame());
                Console.WriteLine("Coverage    : {0}", prot.getCoverage());
                Console.WriteLine("RMS error   : {0}", prot.getRMSDeltas(results));
                Console.WriteLine("Peptides    : {0}", prot.getNumDisplayPeptides());

                // Each protein has a number of peptides that matched - list them
                num_peps = prot.getNumPeptides();
                for (i = 1; i <= num_peps; i++)
                {
                    query = prot.getPeptideQuery(i);
                    p = prot.getPeptideP(i);
                    if (p != -1 && query != -1 && prot.getPeptideDuplicate(i) != ms_protein.DUPLICATE.DUPE_DuplicateSameQuery)
                    {
                        pep = results.getPeptide(query, p);
                        if (pep != null)
                        {
                            displayPeptideInfo(false, pep, results, prot.getPeptideDuplicate(i) == ms_protein.DUPLICATE.DUPE_Duplicate, 
                                prot.getPeptideIsBold(i), prot.getPeptideShowCheckbox(i));
                        }
                    }
                }

                // now display list of all proteins that contained subsets or an identical list of the matching peptides

                if ((flags & (uint) ms_mascotresults.FLAGS.MSRES_GROUP_PROTEINS) > 0 ||
                    (flags & (uint)ms_mascotresults.FLAGS.MSRES_CLUSTER_PROTEINS) > 0)
                {
                    Console.WriteLine("Proteins matching the same set of peptides:");
                    i = 1;
                    while ((prot = results.getNextSimilarProteinOf(accession, dbIdx, i)) != null)
                    {
                        string similar_accession = prot.getAccession();
                        int similar_dbIdx = prot.getDB();
                        if ((flags & (uint)ms_mascotresults.FLAGS.MSRES_CLUSTER_PROTEINS) > 0 && similar_dbIdx > 1)
                        {
                            Console.Write("{0}::", similar_dbIdx);
                        }
                        Console.WriteLine("{0} Total score: {1:#}   Peptides matched: {2}", similar_accession, 
                                          prot.getScore(), prot.getNumDisplayPeptides());
                        i++;
                    }
                    if ((flags & ((uint) ms_mascotresults.FLAGS.MSRES_SHOW_SUBSETS)) > 0)
                    {
                        Console.WriteLine("Proteins matching a subset of these peptides:");
                        i = 1;
                        while ((prot = results.getNextSubsetProteinOf(accession, dbIdx, i)) != null)
                        {
                            string subset_accession = prot.getAccession();
                            int subset_dbIdx = prot.getDB();
                            if ((flags & (uint)ms_mascotresults.FLAGS.MSRES_CLUSTER_PROTEINS) > 0 && subset_dbIdx > 1)
                            {
                                Console.Write("{0}::", subset_dbIdx);
                            }
                            Console.WriteLine("{0} Total score: {1:#}   Peptides matched: {2}", subset_accession,
                                              prot.getScore(), prot.getNumDisplayPeptides());
                                              
                            if ((flags & (uint)ms_mascotresults.FLAGS.MSRES_CLUSTER_PROTEINS) > 0)
                            {
                                int j = 1;
                                if (results.getNextSimilarProteinOf(subset_accession, subset_dbIdx, j) != null)
                                {
                                    Console.WriteLine("  Proteins matching the same set of peptides for this subset:");
                                }
                                ms_protein similar_subset_prot;
                                while ((similar_subset_prot = 
                                        results.getNextSimilarProteinOf(subset_accession, subset_dbIdx, j)) != null)
                                {
                                    string similar_subset_accession = similar_subset_prot.getAccession();
                                    int similar_subset_dbIdx        = similar_subset_prot.getDB();
                                    Console.Write("  ");
                                    if (similar_subset_dbIdx > 1)
                                    {
                                        Console.Write("{0}::", similar_subset_dbIdx);
                                    }
                                    Console.WriteLine("{0} Total score: {1:#}   Peptides matched: {2}",
                                                      similar_subset_accession, similar_subset_prot.getScore(), 
                                                      similar_subset_prot.getNumDisplayPeptides());
                                    j++;
                                }
                            }
                                              
                            i++;
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
                
                if ((flags & (uint)ms_mascotresults.FLAGS.MSRES_CLUSTER_PROTEINS) > 0)
                {
                    prot = results.getNextFamilyProtein(hit, family++);
                    
                    if(prot == null)
                    {
                        hit++;
                        prot = results.getHit(hit);
                        family = 1;
                    }
                }
                else
                {
                    hit++;
                    prot = results.getHit(hit);
                }
            }

            results.createUnassignedList(ms_mascotresults.sortBy.SCORE);
            if (results.getNumberOfUnassigned() > 0)
            {
                Console.WriteLine("Unassigned list");
                Console.WriteLine("---------------");
                for (u = 1; u <= results.getNumberOfUnassigned(); u++)
                {
                    pep = results.getUnassigned(u);
                    displayPeptideInfo(false, pep, results, false, true, true);
                }
            }

            if (pepSum) displayYellowPopupInfo(results, 1);

            return true;
        } //method

        private static void displayYellowPopupInfo(ms_mascotresults results, int queryNo)
        {
            int p;
            ms_peptide pep;
            String seq, tmp, hit, protein, morethan = "";
            Regex regex = new Regex(@"(\d):([^ ]*)[ ]*(.*)");

            Console.WriteLine("Score Delta      Hit Protein Peptide");

            for (p = 1; p <= 10; p++)
            {
                if ((pep = results.getPeptide(queryNo, p)) != null)
                {
                    seq = pep.getPeptideStr();
                    if (seq.Length > 0)
                    {
                        tmp = results.getProteinsWithThisPepMatch(queryNo, p, true);
                        hit = "   ";
                        protein = "       ";
                        morethan = "";
                        if (tmp.Length > 0)
                        {
                            if (regex.IsMatch(tmp))
                            {
                                Match OneMatch = regex.Match(tmp);
                                if (OneMatch.Success)
                                {
                                    int groupCount = OneMatch.Groups.Count;
                                    if (groupCount >= 2) hit = OneMatch.Groups[1].Value;
                                    if (groupCount >= 3) protein = OneMatch.Groups[2].Value;
                                    if (groupCount >= 4) morethan = OneMatch.Groups[3].Value;
                                }
                            }
                            if (morethan.Length > 0)
                            {
                                hit = hit+"+";
                            }

                        }
                        Console.WriteLine("{0}  {1}  {2}  {3}  {4}", pep.getIonsScore(), pep.getDelta(), hit, protein, pep.getPeptideStr());                            
                    }
                }
            }
            p = 1;
            Console.WriteLine("Accessions that matched query {0} rank {1}:- {2}", queryNo, p, results.getProteinsWithThisPepMatch(queryNo, p, false));
        } //method

        private static void displayPeptideInfo(bool showFullDetails, ms_peptide p, ms_mascotresults r, bool isDuplicate, bool isBold, bool showCheckBox)
        {
            int q = p.getQuery();

            if (showFullDetails)
            {
                Console.WriteLine("Peptide hit");
                if (p.getAnyMatch())
                {
                    Console.WriteLine("  Query       : {0}", q);
                    Console.WriteLine("  Rank        : {0}", p.getRank());
                    Console.WriteLine("  Matched     : {0}", p.getAnyMatch());
                    Console.WriteLine("  missedCleave: {0}", p.getMissedCleavages());
                    Console.WriteLine("  mrCalc      : {0}", p.getMrCalc());
                    Console.WriteLine("  delta       : {0}", p.getDelta());
                    Console.WriteLine("  observed    : {0}", p.getObserved());
                    Console.WriteLine("  charge      : {0}", p.getCharge());
                    Console.WriteLine("  mrExp       : {0}", p.getMrExperimental());
                    Console.WriteLine("  ionsMatched : {0}", p.getNumIonsMatched());
                    Console.WriteLine("  peptideStr  : {0}", p.getPeptideStr());
                    Console.WriteLine("  peaksUsed1  : {0}", p.getPeaksUsedFromIons1());
                    Console.WriteLine("  varModsStr  : {0}", p.getVarModsStr());
                    Console.WriteLine("  readable mod: {0}", r.getReadableVarMods(q, p.getRank(), 2));
                    Console.WriteLine("  ionsScore   : {0}", p.getIonsScore());
                    Console.WriteLine("  seriesUsedS : {0}", p.getSeriesUsedStr());
                    Console.WriteLine("  peaksUsed2  : {0}", p.getPeaksUsedFromIons2());
                    Console.WriteLine("  peaksUsed3  : {0}", p.getPeaksUsedFromIons3());
                    Console.WriteLine("  idth, hth, p: {0},  {1},  {2}", r.getPeptideIdentityThreshold(q, 20), 
                        r.getHomologyThreshold(q, 20), r.getProbOfPepBeingRandomMatch(p.getIonsScore(), q));
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("  No match");
                }
            }
            else
            {
                if (showCheckBox) Console.Write("CB ");
                else Console.Write("-- ");
                if (isBold) Console.Write("BOLD ");
                else Console.Write("dim  ");

                Console.Write(q.ToString().PadLeft(4));
                Console.Write(" ");
                double observed = p.getObserved();
                Console.Write(" ");
                Console.Write(string.Format("{0}", observed.ToString("0.000000")).PadLeft(11));
                Console.Write(" ");
                Console.Write(p.getRank().ToString().PadLeft(4));
                Console.Write("({0})", p.getPrettyRank().ToString().PadLeft(4));
                Console.Write(" ");
                Console.Write(p.getPeptideStr().ToString().PadLeft(20));
                Console.Write(" ");

                double score = p.getIonsScore();
                Console.Write(string.Format("{0}{1}{2}", (isDuplicate) ? "(" : "", score.ToString("0.00"), (isDuplicate) ? ")" : "").PadLeft(8));
                Console.Write(" ");
                if (r.getReadableVarMods(q, p.getRank(), 2).Length == 0) Console.WriteLine("  0");
                else
                {
                    string let = r.getReadableVarMods(q, p.getRank(), 2).Substring(0, 1);
                    int iLet;
                    if (int.TryParse(let, out iLet))
                    {
                        Console.WriteLine("  {0}", iLet);
                    }
                    else Console.WriteLine("  0");
                }
            }
        } //method

        private static bool checkErrors(ms_mascotresfile file)
        {
            for (int loop = 1; loop <= file.getNumberOfErrors(); loop++)
            {
                Console.WriteLine("Error number: {0} : {1}", file.getErrorNumber(loop), file.getErrorString(loop));
            }

            // Call isValid before clearAllErrors, otherwise this method always returns true
            bool isValid = file.isValid();
            file.clearAllErrors();

            return isValid;
        } //method

    } //class
} //namespace

/*
resfile_summary.exe c:\inetpub\mascot\data\F981123.dat
Will give the following output:

-------------------------------------------------------------
---   Peptide summary report                              ---
-------------------------------------------------------------
Amino acid database
Protein Hit 1
===================
Accession   : CH60_HUMAN
Description : 60 kDa heat shock protein, mitochondrial OS=Homo sapiens GN=HSPD1
PE=1 SV=2
Score       : 1225
Mass        : 61016.38
Frame       : 0
Coverage    : 283
RMS error   : 30.4200726378481
Peptides    : 31
CB BOLD   11   417.182190    1(   1)             APGFGDNR    45.35   0
CB BOLD   12   422.743286    1(   1)             VGEVIVTK    45.74   0
CB BOLD   13   430.732788    1(   1)             IPAMTIAK    36.27   0
CB BOLD   15   451.249908    1(   1)            LSDGVAVLK    51.95   0
CB BOLD   16   456.780609    1(   1)            VGLQVVAVK    59.00   0
CB BOLD   21   480.744690    1(   1)            VTDALNATR    45.33   0
CB BOLD   24   595.785522    1(   1)          EIGNIISDAMK  (56.55)   0
CB BOLD   25   603.771973    1(   1)          EIGNIISDAMK    59.52   0
CB BOLD   26   608.309875    1(   1)         NAGVEGSLIVEK    73.21   0
CB BOLD   27   617.285706    1(   1)         VGGTSDVEVNEK    80.63   0
CB BOLD   31   672.837524    1(   1)         TVIIEQSWGSPK    64.38   0
CB BOLD   34   714.888428    1(   1)       GVMLAVDAVIAELK  (64.52)   0
CB BOLD   35   714.893799    1(   1)       GVMLAVDAVIAELK  (72.61)   0
CB BOLD   36   722.884888    1(   1)       GVMLAVDAVIAELK    75.19   0
CB BOLD   37   722.893372    1(   1)       GVMLAVDAVIAELK  (72.57)   0
CB BOLD   39   752.864319    1(   1)        TLNDELEIIEGMK    89.56   0
CB BOLD   40   760.846130    1(   1)        TLNDELEIIEGMK  (88.82)   0
CB BOLD   45   640.328125    1(   1)   ISSIQSIVPALEIANAHR   101.50   0
CB BOLD   46   960.032715    1(   1)   ISSIQSIVPALEIANAHR  (87.34)   0
CB BOLD   48  1019.510620    1(   1)    IQEIIEQLDVTTSEYEK    52.42   0
CB BOLD   51  1057.053711    1(   1) ALMLQGVDLLADAVAVTMGPK   115.78   0
CB BOLD   52  1065.039917    1(   1) ALMLQGVDLLADAVAVTMGPK  (71.79)   0
CB BOLD   53  1065.062256    1(   1) ALMLQGVDLLADAVAVTMGPK  (26.17)   0
CB BOLD   54  1073.047729    1(   1) ALMLQGVDLLADAVAVTMGPK  (92.82)   2
CB BOLD   58   789.106201    1(   1) KPLVIIAEDVDGEALSTLVLNR  (55.53)   0
CB BOLD   59  1183.156982    1(   1) KPLVIIAEDVDGEALSTLVLNR  (65.46)   0
CB BOLD   60   789.109375    1(   1) KPLVIIAEDVDGEALSTLVLNR    94.59   0
CB BOLD   61   828.123779    1(   1) TALLDAAGVASLLTTAEVVVTEIPK  (26.50)   0
CB BOLD   62   828.132202    1(   1) TALLDAAGVASLLTTAEVVVTEIPK    47.53   0
CB BOLD   64   854.058777    1(   1) LVQDVANNTNEEAGDGTTTATVLAR    75.15   0
CB BOLD   65  1038.503052    1(   1) DMAIATGGAVFGEEGLTLNLEDVQPHDLGK    13.05   0

Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:
CH60_PONAB Total score: 1008   Peptides matched: 25
CH60_CRIGR Total score: 951   Peptides matched: 23
CH60_MOUSE Total score: 951   Peptides matched: 23
CH60_RAT Total score: 951   Peptides matched: 23
CH60_BOVIN Total score: 918   Peptides matched: 22
CH60_CHICK Total score: 876   Peptides matched: 19
CH60_MESAU Total score: 664   Peptides matched: 18
CH60C_DROME Total score: 121   Peptides matched: 2
CH60C_ARATH Total score: 91   Peptides matched: 2
HSP60_CANAL Total score: 45   Peptides matched: 1
HSP60_EMENI Total score: 45   Peptides matched: 1
HSP60_PARBA Total score: 45   Peptides matched: 1
HSP60_YEAST Total score: 45   Peptides matched: 1
CH602_VIBPA Total score: 45   Peptides matched: 1
CH602_VIBVU Total score: 45   Peptides matched: 1
CH602_VIBVY Total score: 45   Peptides matched: 1
CH60_EUGGR Total score: 45   Peptides matched: 1


Protein Hit 2
===================
Accession   : CH60_DROME
Description : 60 kDa heat shock protein, mitochondrial OS=Drosophila melanogaste
r GN=Hsp60 PE=1 SV=3
Score       : 174
Mass        : 60770.89
Frame       : 0
Coverage    : 67
RMS error   : 29.5905072791318
Peptides    : 4
-- dim    11   417.182190    1(   1)             APGFGDNR    45.35   0
-- dim    27   617.285706    2(   2)         VGGSSEVEVNEK    41.69   0
-- dim    59  1183.156982    2(   2) KPLVIIAEDIDGEALSTLVVNR    12.20   0
-- dim    64   854.058777    1(   1) LVQDVANNTNEEAGDGTTTATVLAR    75.15   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:
HSP60_SCHPO Total score: 87   Peptides matched: 2


Protein Hit 3
===================
Accession   : CH60_CAEEL
Description : Chaperonin homolog Hsp-60, mitochondrial OS=Caenorhabditis elegans
 GN=hsp-60 PE=1 SV=2
Score       : 135
Mass        : 60063.75
Frame       : 0
Coverage    : 21
RMS error   : 36.5383063193603
Peptides    : 3
-- dim    11   417.182190    1(   1)             APGFGDNR    45.35   0
-- dim    39   752.864319    2(   1)        TLNDELELIEGMK    89.56   0
-- dim    40   760.846130    2(   1)        TLNDELELIEGMK  (88.82)   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:


Protein Hit 4
===================
Accession   : CH60_STRM5
Description : 60 kDa chaperonin OS=Stenotrophomonas maltophilia (strain R551-3)
GN=groL PE=3 SV=1
Score       : 42
Mass        : 57311.84
Frame       : 0
Coverage    : 9
RMS error   : 76.92376960617
Peptides    : 1
-- dim    16   456.780609    2(   2)            GIVKVVAVK    42.20   0
Proteins matching the same set of peptides:
CH60_STRMK Total score: 42   Peptides matched: 1
CH60_XANAC Total score: 42   Peptides matched: 1
CH60_XANC5 Total score: 42   Peptides matched: 1
CH60_XANC8 Total score: 42   Peptides matched: 1
CH60_XANCB Total score: 42   Peptides matched: 1
CH60_XANCH Total score: 42   Peptides matched: 1
CH60_XANCP Total score: 42   Peptides matched: 1
CH60_XANOM Total score: 42   Peptides matched: 1
CH60_XANOP Total score: 42   Peptides matched: 1
CH60_XANOR Total score: 42   Peptides matched: 1
Proteins matching a subset of these peptides:


Protein Hit 5
===================
Accession   : NMDE4_HUMAN
Description :
Score       : 37
Mass        : 0
Frame       : 0
Coverage    : 10
RMS error   : 9.41906700790969
Peptides    : 1
-- dim    16   456.780609    3(   3)           VAAGVAVVAR    37.24   0
Proteins matching the same set of peptides:
NMDE4_MOUSE Total score: 37   Peptides matched: 1
NMDE4_RAT Total score: 36   Peptides matched: 1
Proteins matching a subset of these peptides:


Protein Hit 6
===================
Accession   : YF81_THET2
Description :
Score       : 35
Mass        : 0
Frame       : 0
Coverage    : 9
RMS error   : 37.0214184966023
Peptides    : 1
-- dim    16   456.780609    4(   4)            VAQVLGVVK    34.76   0
Proteins matching the same set of peptides:
Y1944_THET8 Total score: 35   Peptides matched: 1
Proteins matching a subset of these peptides:


Protein Hit 7
===================
Accession   : F4ST_FLACH
Description :
Score       : 34
Mass        : 0
Frame       : 0
Coverage    : 9
RMS error   : 87.8815544838649
Peptides    : 1
-- dim    15   451.249908    2(   2)            LSATGLVLK    33.85   0
Proteins matching the same set of peptides:
Proteins matching a subset of these peptides:


Protein Hit 8
===================
Accession   : ZN711_HUMAN
Description : Zinc finger protein 711 OS=Homo sapiens GN=ZNF711 PE=1 SV=2
Score       : 31
Mass        : 86190.24
Frame       : 0
Coverage    : 13
RMS error   : 69.4028633218151
Peptides    : 1
CB BOLD   33   714.364929    1(   1)        EASPLSSNKLILR    30.84   0
Proteins matching the same set of peptides:
ZN711_MOUSE Total score: 31   Peptides matched: 1
Proteins matching a subset of these peptides:


Unassigned list
---------------
CB BOLD   14   442.228302    1(   1)             IAIRGLLK    28.47   0
CB BOLD   22  1101.536621    1(   1)          ITVSTSGLVPK    14.40   0
CB BOLD    9   747.396179    1(   1)              DAEDVAK    13.59   0
CB BOLD   23  1101.621704    1(   1)           QLLMVAGVDR    12.04   0
CB BOLD    8   714.372498    1(   1)              LAPAQSK    10.69   0
CB BOLD    4   662.275574    1(   1)              AGNAVCK     9.73   0
CB BOLD   30   663.837891    1(   1)          AQLLEINEKLR     9.54   0
CB BOLD   57   747.036072    1(   1)  AMWRLVDEMVQDGFPTTAR     9.05   0
CB BOLD   55  1099.094727    1(   1) LNAEAVRTLLSANGQKPSEAK     8.05   0
CB BOLD   29   642.353577    1(   1)       VVGVAGQGASALVR     7.91   0
CB BOLD    6   673.349487    1(   1)              AVLGGTR     7.59   0
CB BOLD   28   642.352600    1(   1)         KNVSVSQGPDPR     7.22   0
CB BOLD   38   749.383972    1(   1)       QSTMQRSAAGTSTR     7.10   0
CB BOLD   50  1048.561523    1(   1)   ALDEILEYQNYPVVCAKK     5.70   0
CB BOLD   49  1020.987915    1(   1) VEPPGDKTLKPGPGAHSPEK     5.49   0
CB BOLD   19   932.364380    1(   1)              MHNLMDR     4.61   0
CB BOLD   20   933.499023    1(   1)             SRDPGMVR     3.21   0
CB BOLD   47   665.009583    1(   1)     QTLQVFKYYLMDENGK     2.25   0
CB BOLD   41   886.405884    1(   1)    AGLELADSPVTPEMLGR     1.93   0
CB BOLD   18   930.703003    1(   1)             ASRAQLER     1.60   0
CB BOLD    7   711.364685    1(   1)              GGAHEIK     1.34   0
CB BOLD   32   711.370728    1(   1)        DKAIGYTSCGNHR     1.20   0
CB BOLD   17   930.683105    1(   1)             KIQAEITK     1.00   0
CB BOLD   44   949.550720    1(   1)    LAARWLAEHPHAPSSVR     0.76   0
CB BOLD   43   933.003784    1(   1)    ISATCVPSAVQKWFAEK     0.70   0
CB BOLD    1   498.272888    1(   1)                          0.00   0
CB BOLD    2   500.256012    1(   1)                          0.00   0
CB BOLD    3   575.558411    1(   1)                          0.00   0
CB BOLD    5   662.417175    1(   1)                          0.00   0
CB BOLD   10   747.412476    1(   1)                          0.00   0
CB BOLD   42   932.460815    1(   1)                          0.00   0
CB BOLD   56  1119.045166    1(   1)                          0.00   0
CB BOLD   63   832.798584    1(   1)                          0.00   0
CB BOLD   66  1113.894653    1(   1)                          0.00   0
CB BOLD   67  1116.177490    1(   1)                          0.00   0
Score Delta      Hit Protein Peptide
Accessions that matched query 97 rank 1:-
*/
