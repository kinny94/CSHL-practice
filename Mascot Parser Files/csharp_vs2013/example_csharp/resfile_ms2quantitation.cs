/*
##############################################################################
# file: resfile_ms2quantitaion.cs                                            #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/resfile_ms2quantitation.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using matrix_science.msparser;

namespace MsParserExamples
{
    class resfile_ms2quantitation
    {
        public static void Main(string[] argv)
        {
            if (argv.Length < 4)
            {
                Console.Error.WriteLine("Usage: {0} <quantitation schema 1 filepath> <quantitation schema 2 filepath> <unimod schema filepath> <results file>", Path.GetFileName((new resfile_ms2quantitation()).GetType().Assembly.Location));
                Console.Error.WriteLine(@"The location of schema files should be e.g.
  ../html/xmlns/schema/quantitation_1/quantitation_1.xsd
  ../html/xmlns/schema/quantitation_2/quantitation_2.xsd
  ../html/xmlns/schema/unimod_2/unimod_2.xsd
if running from the Mascot cgi directory.");
                return;
            }

            string sQuantSchema1Filepath = argv[0], sQuantSchema2Filepath = argv[1], sUnimodSchemaFilepath = argv[2], sResultFile = argv[3];

            ms_mascotresfile resfile = new ms_mascotresfile(sResultFile, 0);
            if (!resfile.isValid())
            {
                Console.Error.WriteLine(resfile.getLastErrorString());
                return;
            }

            resfile.setXMLschemaFilePath(ms_mascotresfile.XML_SCHEMA.XML_SCHEMA_QUANTITATION, "http://www.matrixscience.com/xmlns/schema/quantitation_2 " +
                sQuantSchema2Filepath + " http://www.matrixscience.com/xmlns/schema/quantitation_1 " + sQuantSchema1Filepath);
            resfile.setXMLschemaFilePath(ms_mascotresfile.XML_SCHEMA.XML_SCHEMA_UNIMOD, "http://www.unimod.org/xmlns/schema/unimod_2 " + sUnimodSchemaFilepath);

            ms_quant_method qMethod = new ms_quant_method();
            if (!resfile.getQuantitationMethod(qMethod))
            {
                Console.Error.WriteLine(resfile.getLastErrorString());
                return;
            }

            // calls to generate ms_peptidesummary object are not important for this example,
            // see loadPeptideSummary at the end of the file for details
            ms_peptidesummary pepsum;
            if (!loadPeptideSummary(resfile, out pepsum))
            {
                return;
            }
            // Quantitation the file using ms_ms2quantitation:
            ms_ms2quantitation ms2quant = new ms_ms2quantitation(pepsum, qMethod);

            if (!ms2quant.isValid())
            {
                Console.Error.WriteLine(ms2quant.getLastErrorString());
                return;
            }            
            
            if (ms2quant.hasIntensityNormalisation())
            {
                ms2quant.normaliseIntensities();
            }
            else if (ms2quant.hasPeptideRatioNormalisation())
            {
                ms2quant.normalisePeptideRatios();
            }

            // Update the quantitation method in case ms_ms2quantitation changed any
            // of the parameters:
            qMethod = ms2quant.getQuantitationMethod();

            dumpWarnings(ms2quant.getErrorHandler());
            ms2quant.clearAllErrors();

            // Now we can inspect protein and peptide data in ms2quant
            // But first, dump the quantitation method parameters we have:
            {
                string[] comps = new string[qMethod.getNumberOfComponents()];
                for (int i = 0; i < comps.Length; i++)
                {
                    comps[i] = qMethod.getComponentByNumber(i).getName();
                }
                Console.WriteLine("Components: [{0}]", string.Join(", ", comps));
            }

            string[] ratioNames = new string[qMethod.getNumberOfReportRatios()];
            for (int i = 0; i < ratioNames.Length; i++)
            {
                ratioNames[i] = qMethod.getReportRatioByNumber(i).getName();
            }
            Console.WriteLine("Report ratios [{0}]", string.Join(", ", ratioNames));

            Console.WriteLine("Report ratio type = {0}", qMethod.getProteinRatioType());
            Console.WriteLine("Min. number of peptides = {0}", qMethod.getMinNumPeptides());

            if (qMethod.haveQuality())
            {
                ms_quant_quality q = qMethod.getQuality();
                Console.WriteLine("Quality: min. precursor charge = {0}", q.getMinPrecursorCharge());
                Console.WriteLine("Quality: pep. threshold type = {0}", q.getPepThresholdType());
                Console.WriteLine("Quality: pep. threshold value = {0}", q.getPepThresholdValue());
            }
            else
            {
                Console.WriteLine("Quality: no restrictions");                
            }
            
            if (qMethod.haveNormalisation())
            {
                ms_quant_normalisation _n = qMethod.getNormalisation();
                Console.WriteLine("Normalisation = {0}", _n.getMethod());

                if (_n.haveProteins())
                {
                    ms_quant_normalisation_proteins _p = _n.getProteins();
                    StringBuilder sbJoin = new StringBuilder();
                    for (int i = 0; i < _p.getNumberOfProteins(); i++)
                    {
                        if (i > 0) sbJoin.Append(", ");
                        sbJoin.Append(_p.getProtein(i).getAccession());
                    }
                    Console.WriteLine(" - Accessions = {0}", sbJoin);
                }

                if (_n.havePeptides())
                {
                    ms_quant_normalisation_peptides _p = _n.getPeptides();
                    StringBuilder sbJoin = new StringBuilder();
                    for (int i = 0; i < _p.getNumberOfPeptides(); i++)
                    {
                        if (i > 0) sbJoin.Append(", ");
                        sbJoin.Append(_p.getPeptide(i).getSequence());
                    }
                    Console.WriteLine(" - Sequences = {0}", sbJoin);
                }

                double[] intensityComponentBases = new double[qMethod.getNumberOfComponents()];
                for (int i = 0; i < qMethod.getNumberOfComponents(); i++)
                {
                    intensityComponentBases[i] = ms2quant.getIntensityNormalisationBase(qMethod.getComponentByNumber(i).getName());
                }
                double[] ratioComponentBases = new double[ratioNames.Length];
                for (int i = 0; i < ratioComponentBases.Length; i++)
                {
                    ratioComponentBases[i] = ms2quant.getPeptideRatioNormalisationBase(ratioNames[i]);
                }

                Console.WriteLine("Intensity normalisation constants: [{0}]", string.Join(", ", intensityComponentBases));
                Console.WriteLine("Ratio normalisation constants: [{0}]", string.Join(", ", ratioComponentBases));
            }
            else
            {
                Console.WriteLine("Normalisation: none");
            }

            if (qMethod.haveOutliers())
            {
                Console.WriteLine("Outliers = {0}", qMethod.getOutliers().getMethod());
            }
            else
            {
                Console.WriteLine("Outliers: none");
            }
            Console.WriteLine();

            // Collect the proteins we're interested in
            List<ms_protein> proteins = new List<ms_protein>();
            for (int i = 1; i <= pepsum.getNumberOfHits(); i++)
            {
                proteins.Add(pepsum.getHit(i));
                int j = 0;
                ms_protein protein;
                while ((protein = pepsum.getNextFamilyProtein(i, ++j)) != null)
                {
                    proteins.Add(protein);
                }
            }

            foreach (ms_protein protein in proteins)
            {
                Console.WriteLine("{0}::{1}", protein.getDB(), protein.getAccession());

                foreach (string rationame in ratioNames)
                {
                    ms_protein_quant_ratio ratio = ms2quant.getProteinRatio(protein.getAccession(), protein.getDB(), rationame);
                    if (ratio.isMissing())
                    {
                        Console.WriteLine("{0}: ---", rationame.PadLeft(10));
                    }
                    else
                    {
                        Console.WriteLine("{0} = {1}, stderr = {2}, n = {3} normal-p = {4}, hypo-p = {5}",
                                rationame, ratio.getValue(), ratio.getStandardError(),
                                ratio.getSampleSize(), ratio.getNormalityPvalue(),
                                ratio.getHypothesisPvalue()
                            );
                    }
                    ratio.Dispose();
                    ratio = null;
                }
                Console.WriteLine();

                StringBuilder sbHeader = new StringBuilder();
                sbHeader.Append("Peptide".PadRight(11));
                foreach (string rationame in ratioNames)
                {
                    sbHeader.Append(" ").Append(rationame.PadRight(9));
                }
                Console.WriteLine(sbHeader);
                
                for (int i = 1; i <= protein.getNumPeptides(); i++)
                {
                    if (protein.getPeptideDuplicate(i) == ms_protein.DUPLICATE.DUPE_DuplicateSameQuery) continue;
                    int q = protein.getPeptideQuery(i), p = protein.getPeptideP(i);
                    ms_peptide peptide = pepsum.getPeptide(q, p);
                    if (peptide == null) continue;
                    List<string> aValues = new List<string>();
                    foreach (string rationame in ratioNames) 
                    {
                        ms_peptide_quant_ratio ratio = ms2quant.getPeptideRatio(new ms_peptide_quant_key(q, p), rationame);
                        if (ratio.isMissing())
                        {
                            aValues.Add("---".PadRight(9));
                            continue;
                        }
                        if (ratio.isInfinite())
                        {
                            aValues.Add("###".PadRight(9));
                        }
                        else
                        {
                            string sValue = ratio.getValue().ToString("0.####").PadRight(9);
                            aValues.Add(sValue);
                        }
                    }
                    Console.WriteLine("{0}  {1}", string.Format("q{0}_p{1}", q, p).PadRight(11), string.Join(" ", aValues));
                }
                Console.WriteLine();
            }

            pepsum.getNumberOfHits();
            resfile._params();

        }

        private static void dumpWarnings(ms_errs ms_errs)
        {
            for (int i = 0; i < ms_errs.getNumberOfErrors(); i++)
            {
                Console.WriteLine("Warning: {0}", ms_errs.getErrorString(i));
            }
        }

        private static bool loadPeptideSummary(ms_mascotresfile resfile, out ms_peptidesummary pepsum)
        {
            // set pepsum to null so we can return out with false if the method fails
            pepsum = null;

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

            pepsum = new ms_peptidesummary(resfile, flags, minprob, maxhits, "", iisb, (int)minpeplen, "", flags2);

            if (!resfile.isValid())
            {
                Console.Error.WriteLine(resfile.getLastErrorString());
                return false;
            }
            return true;
        }
    }
}

/*
 resfile_ms2quantitation.exe c:\inetpub\mascot\html\xmlns\schema\quantitation_1\quantitation_1.xsd c:\inetpub\mascot\html\xmlns\schema\quantitation_2\quantitation_2.xsd c:\inetpub\mascot\html\xmlns\schema\unimod_2\unimod_2.xsd c:\inetpub\mascot\data\F981133.dat

Will give the following output:

Warning:
Components: [light, heavy]
Report ratios [Heavy/Light]
Report ratio type = median
Min. number of peptides = 2
Quality: min. precursor charge = 1
Quality: pep. threshold type = at least homology
Quality: pep. threshold value = 0.05
Normalisation: none
Outliers = auto

1::K2C1_PANTR
Heavy/Light = 0.0579803721548197, stderr = 1.43947598358519, n = 5 normal-p = 0.636188020087278, hypo-p = 0.0014453988365255

Peptide     Heavy/Light
q18_p1       ---
q28_p1       0
q33_p2       0.153
q38_p1       0.0406
q39_p1       0.0309
q40_p1       0.1156
q55_p1       0.058

1::TRYP_PIG
Heavy/Light = 0.0953455094336426, stderr = 1.24484972786916, n = 10 normal-p = 0.325749316021252, hypo-p = 1.98308193710695E-06

Peptide     Heavy/Light
q1_p1        0
q2_p1        ---
q3_p1        0.1477
q9_p1        0
q72_p1       0.0829
q73_p1       0.0843
q74_p1       0.059
q75_p1       0.0552
q76_p1       0.0936
q77_p1       0.3288
q78_p1       0.2642
q81_p1       ---
q82_p1       0.1249
q90_p1       ---
q91_p1       0.0972

1::IGG2B_MOUSE
Heavy/Light: ---

Peptide     Heavy/Light
q12_p1       0
q57_p1       ---
q58_p1       ---
q62_p1       -0.0007
q66_p1       -0.0008

1::ALBU_BOVIN
Heavy/Light = 0.0213058637909053, stderr = 1.21840219076313, n = 2 normal-p = -1, hypo-p = 0.0326462021178481

Peptide     Heavy/Light
q16_p1       ---
q30_p1       0.0175
q46_p1       0.026
q49_p1       -0.0006
q50_p1       ---

1::ENPL_MOUSE
Heavy/Light = 0.826726169499555, stderr = 1.47145311768354, n = 3 normal-p = 0.429488582555117, hypo-p = 0.671039840711699

Peptide     Heavy/Light
q4_p1        ---
q19_p1       ---
q20_p1       ---
q24_p2       0.3551
q26_p2       1.0692
q41_p1       0.8267
q42_p1       ---

1::NUCL_MOUSE
Heavy/Light = 1.32117663975823, stderr = 1.26401425891289, n = 3 normal-p = 0.980416559320101, hypo-p = 0.356540636322033

Peptide     Heavy/Light
q5_p1        ---
q6_p1        ---
q7_p1        ---
q8_p1        ---
q44_p1       ---
q45_p1       0.937
q92_p1       1.8861
q93_p1       1.3212

1::EPHB2_HUMAN
Heavy/Light = 10.9654930264722, stderr = 1.03789418103362, n = 2 normal-p = -1,hypo-p = 0.00988679141070892

Peptide     Heavy/Light
q13_p1       10.5651
q21_p1       ---
q53_p1       11.381
q88_p1       ---

1::K2C1_RAT
Heavy/Light = 0.0405615762807822, stderr = 1.59441776859148, n = 3 normal-p = 0.374888347489863, hypo-p = 0.0205371306366633

Peptide     Heavy/Light
q33_p4       ---
q38_p1       0.0406
q39_p1       0.0309
q40_p1       0.1156

1::K2C75_BOVIN
Heavy/Light: ---

Peptide     Heavy/Light
q28_p1       0
q38_p4       ---
q40_p6       ---

1::HNRPU_HUMAN
Heavy/Light = 0.870877403713838, stderr = 1.12071669533565, n = 2 normal-p = -1, hypo-p = 0.438890343357515

Peptide     Heavy/Light
q32_p1       ---
q34_p1       ---
q51_p1       0.976
q52_p1       0.7771

1::SFPQ_HUMAN
Heavy/Light = 0.890580462682766, stderr = 1.58916835083167, n = 3 normal-p = 0.968098028626276, hypo-p = 0.825807022031107

Peptide     Heavy/Light
q14_p1       0.8906
q15_p1       ---
q22_p1       ---
q69_p1       0.4372
q70_p1       1.7429

1::CAPR1_MOUSE
Heavy/Light = 1.19967671408572, stderr = 1.06348879302472, n = 3 normal-p = 0.901608333526897, hypo-p = 0.0978334823884838

Peptide     Heavy/Light
q23_p1       1.0855
q36_p1       1.3043
q37_p1       1.1997

1::K2C1B_HUMAN
Heavy/Light = 0.0684746549583752, stderr = 1.68816553095394, n = 2 normal-p = -1, hypo-p = 0.122783146524372

Peptide     Heavy/Light
q18_p2       ---
q38_p1       0.0406
q40_p1       0.1156
q53_p3       ---

1::ENPL_ARATH
Heavy/Light = 0.616183752030204, stderr = 1.73514843004814, n = 2 normal-p = -1, hypo-p = 0.541069995173351

Peptide     Heavy/Light
q24_p2       0.3551
q26_p2       1.0692
q41_p2       ---
q42_p10      ---
q67_p10      ---

1::VIME_CRIGR
Heavy/Light: ---

Peptide     Heavy/Light
q17_p1       0.4569
q35_p1       ---

1::BCAR1_MOUSE
Heavy/Light: ---

Peptide     Heavy/Light
q10_p1       ---
q11_p1       ---
q63_p1       1.025

1::HTPG_ALKEH
Heavy/Light = 0.616183752030204, stderr = 1.73514843004814, n = 2 normal-p = -1, hypo-p = 0.541069995173351

Peptide     Heavy/Light
q24_p1       0.3551
q26_p1       1.0692
q67_p9       ---

1::FAK1_MOUSE
Heavy/Light: ---

Peptide     Heavy/Light
q29_p1       0.4906

1::HTPG_BDEBA
Heavy/Light = 0.616183752030204, stderr = 1.73514843004814, n = 2 normal-p = -1, hypo-p = 0.541069995173351

Peptide     Heavy/Light
q24_p3       0.3551
q26_p3       1.0692

1::K2C8_MOUSE
Heavy/Light: ---

Peptide     Heavy/Light
q33_p1       0.153

1::TRY1_RAT
Heavy/Light: ---

Peptide     Heavy/Light
q72_p10      ---
q73_p2       ---
q74_p2       0.2836
q75_p2       ---
q76_p2       ---
q78_p2       ---
q81_p4       ---
q82_p2       ---

1::IGKC_MOUSE
Heavy/Light: ---

Peptide     Heavy/Light
q47_p1       -0.0002
q48_p1       0
*/
