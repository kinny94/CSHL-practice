/*
##############################################################################
# file: resfile_customquantitation.cs                                        #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/resfile_customquantitation.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Collections.Generic;
using matrix_science.msparser;

namespace MsParserExamples
{
    class resfile_customquantitation
    {
        public static int Main(string[] argv)
        {
            Dictionary<string, List<int[]>> proteinPeptideMap = new Dictionary<string, List<int[]>>()
            {
                {"ALBU_HUMAN", new List<int[]> {
                    {new int[] {1,1}}, 
                    {new int[] {2,1}}, 
                    {new int[] {3,1}}
                }},
                {"ALBU_BOVIN", new List<int[]> {
                    {new int[] {2,2}},
                    {new int[] {5,1}},
                    {new int[] {7,1}}
                }}
            };

            Dictionary<string, int> peptideIntensities = new Dictionary<string, int>
            {
                {"q1p1", 1000},
                {"q2p1", 500},
                {"q3p1", 750},
                {"q2p2", 750},
                {"q5p1", 2000},
                {"q7p1", 1000}
            };

            ms_quant_method qmethod = new ms_quant_method();

            ms_quant_average average = new ms_quant_average();
            average.setReferenceAccession("ALBU_HUMAN");
            average.setReferenceAmount("1.0");
            // We set the number of top N peptides to 2, so we expect the following
            // peptide intensities to be used:
            //   ALBU_HUMAN = 1000, 750
            //   ALBU_BOVIN = 2000, 1000
            average.setNumPeptides(2);

            ms_quant_protocol protocol = new ms_quant_protocol();
            protocol.setAverage(average);
            qmethod.setProtocol(protocol);

            // Since ms_customquantitation does not use the ratio definitions in the
            // quantitation method, we need not define them at all. Ratios are meaningless
            // in the Average protocol anyway, so we can use any name we want to denote
            // the intensity values.
            String rationame = "amount";
            setParams(qmethod, "none", "none", "average", 1);

            ms_customquantitation customquant = new ms_customquantitation(qmethod);
            if (!customquant.isValid())
            {
                Console.Error.WriteLine(customquant.getLastErrorString());
                return 1;
            }

            dumpWarnings(customquant.getErrorHandler());
            customquant.clearAllErrors();
            dumpQuantMethod(qmethod, new string[] {rationame});

            // Create the protein-peptide mapping
            foreach (string accession in proteinPeptideMap.Keys)
            {
                List<int[]> peptides = proteinPeptideMap[accession];
                foreach (int[] peptide in peptides)
                {
                    customquant.addPeptideQuantKey(accession, 0, new ms_peptide_quant_key(peptide[0], peptide[1]));
                }
            }

            // Add peptide intensities
            foreach (string qp in peptideIntensities.Keys)
            {
                double intensity = peptideIntensities[qp];
                int q, p;
                extractQP(qp, out q, out p);
                ms_peptide_quant_ratio ratio = new ms_peptide_quant_ratio(new ms_peptide_quant_key(q, p), rationame, intensity);
                customquant.addPeptideRatio(ratio);
            }
            Console.WriteLine();

            // print it all out immediately
            foreach (string accession in proteinPeptideMap.Keys)
            {
                Console.WriteLine(accession);
                List<int[]> aPeptides = proteinPeptideMap[accession];

                ms_protein_quant_ratio ratio = customquant.getProteinRatio(accession, 0, rationame);
                if (ratio.isMissing())
                {
                    Console.WriteLine("{0} ---", rationame);
                }
                else
                {
                    Console.WriteLine("{0} = {1}, stderr = {2}, n = {3} normal-p = {4}, hypo-p = {5}",
                                rationame, ratio.getValue(), ratio.getStandardError(),
                                ratio.getSampleSize(), ratio.getNormalityPvalue(),
                                ratio.getHypothesisPvalue()
                            );
                }

                Console.WriteLine("{0} {1}", "Peptide".PadRight(10), rationame.PadRight(10));
                foreach(int[] peptide in aPeptides) 
                {
                    ms_peptide_quant_ratio pepRatio = customquant.getPeptideRatio(new ms_peptide_quant_key(peptide[0], peptide[1]), rationame);
                    string ratioValue;
                    if (pepRatio.isMissing())
                    {
                        ratioValue = "---".PadLeft(10);
                    }
                    else if (pepRatio.isInfinite())
                    {
                        ratioValue = "###".PadLeft(10);
                    }
                    else
                    {
                        ratioValue = string.Format("{0:0.######}", pepRatio.getValue()).PadLeft(10);
                    }
                    Console.WriteLine("{0} {1}", string.Format("q{0}_p{1}", peptide[0], peptide[1]).PadLeft(10), ratioValue);
                }
                Console.WriteLine();

            }

            return 0;
        }

        private static void extractQP(string qp, out int q, out int p)
        {
            string[] bits = qp.Split('p');
            bits[0] = bits[0].Replace("q", "");
            q = Int32.Parse(bits[0]);
            p = Int32.Parse(bits[1]);
        }

        private static void dumpQuantMethod(ms_quant_method qmethod, string[] ratioNames)
        {
            {
                List<string> comps = new List<string>();
                for (int i = 0; i < qmethod.getNumberOfComponents(); i++)
                {
                    comps.Add(qmethod.getComponentByNumber(i).getName());
                }
                Console.WriteLine("Components: [{0}]", string.Join(", ", comps));                                
            }

            Console.WriteLine("Report ratios: [{0}]", string.Join(", ", ratioNames));
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

        private static void dumpWarnings(ms_errs ms_errs)
        {
            for (int i = 1; i <= ms_errs.getNumberOfErrors(); i++)
            {
                Console.Error.WriteLine("Warning: {0}", ms_errs.getErrorString(i));
            }
        }

        private static void setParams(ms_quant_method qmethod, string normalisationMethod, string outliersMethod, string proteinRatioType, int minNoPeptides)
        {
            ms_quant_quality quality = (qmethod.haveQuality()) ? qmethod.getQuality() : new ms_quant_quality();
            ms_quant_normalisation normalisation = (qmethod.haveNormalisation()) ? qmethod.getNormalisation() : new ms_quant_normalisation();
            ms_quant_outliers outliers = (qmethod.haveOutliers()) ? qmethod.getOutliers() : new ms_quant_outliers();

            normalisation.setMethod((string.IsNullOrEmpty(normalisationMethod)) ? "none" : normalisationMethod);
            outliers.setMethod((string.IsNullOrEmpty(outliersMethod)) ? "none" : outliersMethod);
            qmethod.setMinNumPeptides((minNoPeptides > 1) ? minNoPeptides : 2);
            qmethod.setProteinRatioType((string.IsNullOrEmpty(proteinRatioType)) ? "none" : proteinRatioType);
            quality.setMinPrecursorCharge(1);
            quality.setPepThresholdType("maximum expect");
            quality.setPepThresholdValue("0.05");

            qmethod.setQuality(quality);
            qmethod.setNormalisation(normalisation);
            qmethod.setOutliers(outliers);
        }
    }
}

/*
resfile_customquantitation.exe
Will give the following output:

Components: []
Report ratios: [amount]
Protein ratio type = average
Min. number of peptides = 2
Quality: min. precursor charge = 1
Quality: pep. threshold type = maximum expect
Quality: pep. threshold value = 0.05
Normalisation = none
Outliers = none

ALBU_HUMAN
amount = 721.124785153704, stderr = 1.2226920025227, n = 3 normal-p = 0.813227565666875, hypo-p = 0.000932101414662601
Peptide    amount
     q1_p1       1000
     q2_p1        500
     q3_p1        750

ALBU_BOVIN
amount = 1144.71424255333, stderr = 1.33788997067188, n = 3 normal-p = 0.552542164299708, hypo-p = 0.00170392414333331
Peptide    amount
     q2_p2        750
     q5_p1       2000
     q7_p1       1000
*/
