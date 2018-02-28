/*
##############################################################################
# file: tools_quant_helper.cs                                                #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/tools_quant_helper.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.IO;
using System.Collections.Generic;
using matrix_science.msparser;

namespace MsParserExamples

{
    
    class tools_quant_helper

    {

        private const string QUANT_SCHEMA = @"http://www.matrixscience.com/xmlns/schema/quantitation_2 ../html/xmlns/schema/quantitation_2/quantitation_2.xsd http://www.matrixscience.com/xmlns/schema/quantitation_1 ../html/xmlns/schema/quantitation_1/quantitation_1.xsd", UNIMOD_SCHEMA = @"http://www.unimod.org/xmlns/schema/unimod_2 ../html/xmlns/schema/unimod_2/unimod_2.xsd";


        public static int Main(string[] argv)
        {
            if (argv.Length == 0)
            {
                Console.Error.WriteLine("Usage: {0} <quantitation results.dat>", Path.GetFileName((new tools_quant_helper()).GetType().Assembly.Location));
                return 1;
            }

            ms_mascotresfile resfile = new ms_mascotresfile(argv[0]);

            if (!resfile.isValid())
            {
                Console.Error.WriteLine(resfile.getLastErrorString());
                return 1;
            }

            // the details of loading the quantitation method and peptide summary object
            // are not relevant to this example (see methods at the end of the file for
            // implementation).
            ms_quant_configfile quantConfigFile;
            ms_quant_method qmethod;
            ms_peptidesummary pepSum;
            if (!loadMethod(resfile, out quantConfigFile, out qmethod)) return 1;
            ms_umod_configfile umodfile = null;
            if (!loadUmodConfigFile(resfile, out umodfile)) return 1;
            if (!loadPeptideSummary(resfile, out pepSum)) return 1;            

            ms_quant_helper quantHelper = new ms_quant_helper(pepSum, qmethod, umodfile);
            if (!quantHelper.isValid())
            {
                Console.WriteLine("ms_quant_helper is not value: {0}", quantHelper.getLastErrorString());
                return 1;
            }
            List<ms_protein> proteins = pullProteinsFrom(pepSum);

            Console.WriteLine("File {0} uses {1} and has {2} family proteins", argv[0], qmethod.getName(), proteins.Count);

            dumpQuantMethod(qmethod);
            Console.WriteLine();

            Dictionary<int, string> peptideQuantStr = new Dictionary<int, string>()
            {
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_EXCLUDED_FIXEDMOD, "PEPTIDE_HAS_EXCLUDED_FIXEDMOD"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_EXCLUDED_LOCAL_FIXEDMOD, "PEPTIDE_HAS_EXCLUDED_LOCAL_FIXEDMOD"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_EXCLUDED_LOCAL_VARMOD, "PEPTIDE_HAS_EXCLUDED_LOCAL_VARMOD"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_EXCLUDED_VARMOD, "PEPTIDE_HAS_EXCLUDED_VARMOD"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_NO_REQUIRED_FIXEDMOD, "PEPTIDE_HAS_NO_REQUIRED_FIXEDMOD"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_NO_REQUIRED_VARMOD, "PEPTIDE_HAS_NO_REQUIRED_VARMOD"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_HAS_UNMODIFIED_SITE, "PEPTIDE_HAS_UNMODIFIED_SITE"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_IS_QUANTIFIABLE, "PEPTIDE_IS_QUANTIFIABLE"},
                {(int) ms_quant_helper.PEPTIDE_QUANTIFIABILITY.PEPTIDE_QUANTIFIABILITY_UNAVAILABLE, "PEPTIDE_QUANTIFIABILITY_UNAVAILABLE"}
            };

            Dictionary<int, string> peptideQualityStr = new Dictionary<int, string>()
            {
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_EXPECT_ABOVE_THRESHOLD, "PEPTIDE_EXPECT_ABOVE_THRESHOLD"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_CHARGE_BELOW_PRECURSOR_MIN, "PEPTIDE_CHARGE_BELOW_PRECURSOR_MIN"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_HAS_NO_EXCLUSIVE_MODS, "PEPTIDE_HAS_NO_EXCLUSIVE_MODS"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_NOT_UNIQUE, "PEPTIDE_NOT_UNIQUE"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_QUALITY_IS_OK, "PEPTIDE_QUALITY_IS_OK"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_QUALITY_UNAVAILABLE, "PEPTIDE_QUALITY_UNAVAILABLE"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_SCORE_BELOW_HOMOLOGY_THR, "PEPTIDE_SCORE_BELOW_HOMOLOGY_THR"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_SCORE_BELOW_IDENTITY_THR, "PEPTIDE_SCORE_BELOW_IDENTITY_THR"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_SCORE_BELOW_IDENTITY_THR_NOHOM, "PEPTIDE_SCORE_BELOW_IDENTITY_THR_NOHOM"},
                {(int) ms_quant_helper.PEPTIDE_QUALITY.PEPTIDE_SCORE_BELOW_SCORE_THR, "PEPTIDE_SCORE_BELOW_SCORE_THR"}
            };

            foreach (ms_protein protein in proteins)
            {
                Console.WriteLine("Protein {0}::{1}", protein.getDB(), protein.getAccession());
                for (int i = 1; i <= protein.getNumPeptides(); i++)
                {
                    int q = protein.getPeptideQuery(i), p = protein.getPeptideP(i);
                    ms_peptide peptide = pepSum.getPeptide(q, p);
                    if (peptide == null) continue;

                    // Each peptide can be tested for two things:
                    // a) is it quantifiable?
                    // b) is it of high enough quality for quantification
                    //
                    // The two parameters are defined in the quantitation method object
                    // The two tests are orthogonal: the peptide need not pass test (a)
                    // in order to pass test (b), and vice versa.  Normally, for
                    // quantitation purposes, you can ignore peptides which fail either test,
                    // so you can continue straight to the next peptide if test (a) fails

                    // Test (a):
                    {
                        string reason;
                        int ok = quantHelper.isPeptideQuantifiable(q, p, protein, i, out reason);
                        Console.WriteLine("\tq{0}_p{1} quantifiable? {2} ({3})", q, p, peptideQuantStr[ok], (string.IsNullOrEmpty(reason) ? "" : reason));
                    }

                    // Test (b):
                    {
                        string reason;
                        int ok = quantHelper.isPeptideQualityOK(q, p, out reason);
                        Console.WriteLine("\tq{0}_p{1} quality? {2} ({3})", q, p, peptideQualityStr[ok], (string.IsNullOrEmpty(reason) ? "" : reason));
                    }

                }
            }

            pepSum.getNumberOfHits();
            resfile._params();

            return 0;
        }

        private static void dumpQuantMethod(ms_quant_method qmethod)
        {
            {
                List<string> comps = new List<string>();
                for (int i = 0; i < qmethod.getNumberOfComponents(); i++)
                {
                    comps.Add(qmethod.getComponentByNumber(i).getName());
                }
                Console.WriteLine("Components: [{0}]", string.Join(", ", comps));
            }

            Console.WriteLine("Protein ratio type = {0}", qmethod.getProteinRatioType());
            Console.WriteLine("Min. number of peptides = {0}", qmethod.getMinNumPeptides());

            if (qmethod.haveQuality())
            {
                var q = qmethod.getQuality();
                Console.WriteLine("Quality: min. precursor charge = {0}", q.getMinPrecursorCharge());
                Console.WriteLine("Quality: pep. threshold type = {0}", q.getPepThresholdType());
                Console.WriteLine("Quality: pep. threshold value = {0}", q.getPepThresholdValue());
            }
            else
            {
                Console.WriteLine("Quality: no restrictions");
            }

            if (qmethod.haveNormalisation())
            {
                var q = qmethod.getNormalisation();
                Console.WriteLine("Normalisation = {0}", q.getMethod());
            }
            else
            {
                Console.WriteLine("Normalisation: none");
            }

            if (qmethod.haveOutliers())
            {
                var q = qmethod.getOutliers();
                Console.WriteLine("Outliers = {0}", q.getMethod());
            }
            else
            {
                Console.WriteLine("Outliers: none");
            }
        }

        private static List<ms_protein> pullProteinsFrom(ms_peptidesummary pepsum)
        {
            List<ms_protein> proteins = new List<ms_protein>();
            for (int i = 1; i <= pepsum.getNumberOfHits(); i++)
            {
                ms_protein hit = pepsum.getHit(i);
                proteins.Add(hit);

                int j = 0;
                ms_protein protein;
                while((protein = pepsum.getNextFamilyProtein(i, ++j)) != null) {
                    proteins.Add(protein);
                }
            }
            return proteins;
        }

        private static bool loadPeptideSummary(ms_mascotresfile resfile, out ms_peptidesummary pepSum)//, out ms_peptidesummary pepsum)
        {
            // set pepsum to null so we can return out with false if the method fails
            pepSum = null;

            ms_mascotoptions opts = new ms_mascotoptions();

            uint flags, flags2, minpeplen;
            int maxhits;
            double minprob, iisb;
            bool usePepsum;
            resfile.get_ms_mascotresults_params(opts, out flags, out minprob, out maxhits, out iisb, out minpeplen, out usePepsum, out flags2);

            if (!usePepsum)
            {
                Console.Error.WriteLine("Results file cannot be opened as a peptide summary");
                return false;
            }

            pepSum = new ms_peptidesummary(resfile, flags, minprob, maxhits, "", iisb, (int) minpeplen, "", flags2);
            if (!resfile.isValid())
            {
                Console.Error.WriteLine(resfile.getLastErrorString());
                return false;
            }
            return true;
        }

        private static bool loadUmodConfigFile(ms_mascotresfile resfile, out ms_umod_configfile umodfile)
        {
            umodfile = new ms_umod_configfile();
            umodfile.setSchemaFileName(UNIMOD_SCHEMA);
            if (!resfile.getUnimod(umodfile))
            {
                Console.Error.WriteLine("Result file does not have a Unimod section");
                return false;
            }
            if (!umodfile.isValid())
            {
                Console.Error.WriteLine("Unimod file is not value: {0}", umodfile.getLastErrorString());
                return false;
            }

            string str = umodfile.validateDocument();
            if (str.Length > 0)
            {
                Console.Error.WriteLine("Unimod file does not validate: {0}", str);
                return false;
            }
            return true;
        }

        private static bool loadMethod(ms_mascotresfile resfile, out ms_quant_configfile quantConfigFile, out ms_quant_method qmethod)
        {
            // set the out values to null so we can return false without defining them
            quantConfigFile = null;
            qmethod = null;
            string quantMethodName = resfile._params().getQUANTITATION();
            if (quantMethodName == null || quantMethodName.Length == 0 || quantMethodName.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("File has no quantitation method");
                return false;
            }
            quantConfigFile = new ms_quant_configfile();
            quantConfigFile.setSchemaFileName(QUANT_SCHEMA);
            
            if (!resfile.getQuantitation(quantConfigFile))
            {
                Console.Error.WriteLine("File has no quantitation method: {0}", resfile.getLastErrorString());
                return false;
            }

            if (!quantConfigFile.isValid())
            {
                Console.Error.WriteLine("Quantitation file is not valid: {0}", quantConfigFile.getLastErrorString());
                return false;
            }

            string str = quantConfigFile.validateDocument();
            if (str.Length > 0)
            {
                Console.Error.WriteLine("Quantitation file does not validate: {0}", str);
                return false;
            }

            qmethod = quantConfigFile.getMethodByName(quantMethodName);

            if (qmethod == null)
            {
                Console.Error.WriteLine("Quantitation file has no method called: '{0}'", quantMethodName);
                return false;
            }
            return true;
        }
    }
}

/*
tools_quant_helper.exe ..\data\F981133.dat
Will give the following output:

File ..\data\F981133.dat uses SILAC K+6 R+6 multiplex and has 22 family proteins
Components: [light, heavy]
Protein ratio type = weighted
Min. number of peptides = 2
Quality: min. precursor charge = 1
Quality: pep. threshold type = at least homology
Quality: pep. threshold value = 0.05
Normalisation: none
Outliers = auto

Protein 1::K2C1_PANTR
	q18_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q18_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q28_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q28_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q33_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q33_p2 quality? PEPTIDE_QUALITY_IS_OK ()
	q38_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q38_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q39_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q39_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q40_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q40_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q55_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q55_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::TRYP_PIG
	q1_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q1_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q2_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q2_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q3_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q3_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q9_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q9_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q72_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q72_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q73_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q73_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q74_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q74_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q75_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q75_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q76_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q76_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q77_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q77_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q78_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q78_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q81_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q81_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q82_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q82_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q90_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q90_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q91_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q91_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::IGG2B_MOUSE
	q12_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q12_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q57_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q57_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q58_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q58_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q62_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q62_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q66_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q66_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::ALBU_BOVIN
	q16_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q16_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q30_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q30_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q46_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q46_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q49_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q49_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q50_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q50_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::ENPL_MOUSE
	q4_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q4_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q19_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q19_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q20_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q20_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q24_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q24_p2 quality? PEPTIDE_QUALITY_IS_OK ()
	q26_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q26_p2 quality? PEPTIDE_QUALITY_IS_OK ()
	q41_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q41_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q42_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q42_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::NUCL_MOUSE
	q5_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q5_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q6_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q6_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q7_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q7_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q8_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q8_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q44_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q44_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q45_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q45_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q92_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q92_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q93_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q93_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::EPHB2_HUMAN
	q13_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q13_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q21_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q21_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q53_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q53_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q88_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q88_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::K2C1_RAT
	q33_p4 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q33_p4 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q38_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q38_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q39_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q39_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q40_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q40_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::K2C75_BOVIN
	q28_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q28_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q38_p4 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q38_p4 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q40_p6 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q40_p6 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::HNRPU_HUMAN
	q32_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q32_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q34_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q34_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q51_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q51_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q52_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q52_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::SFPQ_HUMAN
	q14_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q14_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q15_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q15_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q22_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q22_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q69_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q69_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q70_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q70_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::CAPR1_MOUSE
	q23_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q23_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q36_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q36_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q37_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q37_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::K2C1B_HUMAN
	q18_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q18_p2 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q38_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q38_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q40_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q40_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q53_p3 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q53_p3 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::ENPL_ARATH
	q24_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q24_p2 quality? PEPTIDE_QUALITY_IS_OK ()
	q26_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q26_p2 quality? PEPTIDE_QUALITY_IS_OK ()
	q41_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q41_p2 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q42_p10 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q42_p10 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q67_p10 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q67_p10 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::VIME_CRIGR
	q17_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q17_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q35_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q35_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::BCAR1_MOUSE
	q10_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q10_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q11_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q11_p1 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q63_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q63_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::HTPG_ALKEH
	q24_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q24_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q26_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q26_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q67_p9 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q67_p9 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
Protein 1::FAK1_MOUSE
	q29_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q29_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::HTPG_BDEBA
	q24_p3 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q24_p3 quality? PEPTIDE_QUALITY_IS_OK ()
	q26_p3 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q26_p3 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::K2C8_MOUSE
	q33_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q33_p1 quality? PEPTIDE_QUALITY_IS_OK ()
Protein 1::TRY1_RAT
	q72_p10 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q72_p10 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q73_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q73_p2 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q74_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q74_p2 quality? PEPTIDE_QUALITY_IS_OK ()
	q75_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q75_p2 quality? PEPTIDE_SCORE_BELOW_IDENTITY_THR_NOHOM (Peptide score is below identity threshold (no homology threshold))
	q76_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q76_p2 quality? PEPTIDE_SCORE_BELOW_IDENTITY_THR_NOHOM (Peptide score is below identity threshold (no homology threshold))
	q78_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q78_p2 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q81_p4 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q81_p4 quality? PEPTIDE_SCORE_BELOW_HOMOLOGY_THR (Peptide score is below homology threshold)
	q82_p2 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q82_p2 quality? PEPTIDE_SCORE_BELOW_IDENTITY_THR_NOHOM (Peptide score is below identity threshold (no homology threshold))
Protein 1::IGKC_MOUSE
	q47_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q47_p1 quality? PEPTIDE_QUALITY_IS_OK ()
	q48_p1 quantifiable? PEPTIDE_IS_QUANTIFIABLE ()
	q48_p1 quality? PEPTIDE_QUALITY_IS_OK ()
*/
