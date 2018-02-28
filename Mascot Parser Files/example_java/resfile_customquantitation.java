/*
 * ##############################################################################
 * # file: resfile_customquantitation                                           #
 * # Mascot Parser toolkit example code                                         #
 * ##############################################################################
 * # COPYRIGHT NOTICE                                                           #
 * # Copyright 1998-2013 Matrix Science Limited  All Rights Reserved.           #
 * #                                                                            #
 * ##############################################################################
 */

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Set;
import matrix_science.msparser.ms_customquantitation;
import matrix_science.msparser.ms_errs;
import matrix_science.msparser.ms_peptide_quant_key;
import matrix_science.msparser.ms_peptide_quant_ratio;
import matrix_science.msparser.ms_protein_quant_ratio;
import matrix_science.msparser.ms_quant_average;
import matrix_science.msparser.ms_quant_method;
import matrix_science.msparser.ms_quant_normalisation;
import matrix_science.msparser.ms_quant_outliers;
import matrix_science.msparser.ms_quant_protocol;
import matrix_science.msparser.ms_quant_quality;

/**
 *
 * @author patricke
 */
public class resfile_customquantitation {

    static {
        try {
            System.loadLibrary("msparserj");
        } catch (UnsatisfiedLinkError e) {
            System.err.println("Native code library failed to load. "
                               + "Is msparserj.dll on the path?\n" + e);
            System.exit(0);
        }
    }

    /**
     * # This example uses fictitious data to illustrate how ms_customquantitation can
     * # be used with the Average protocol. In a real situation peptide intensities
     * # and the protein-peptide mapping would come from some external source.
     */
    public static void main(String[] args) {
        HashMap<String, List<Integer[]>> hmProteinPeptideMapping = new HashMap<String, List<Integer[]>>();
        ArrayList<Integer[]> aAlbuHuman = new ArrayList<Integer[]>();
        aAlbuHuman.add(new Integer[]{1,1});
        aAlbuHuman.add(new Integer[]{2,1});
        aAlbuHuman.add(new Integer[]{3,1});
        hmProteinPeptideMapping.put("ALBU_HUMAN", aAlbuHuman);
        ArrayList<Integer[]> aAlbuBovin = new ArrayList<Integer[]>();
        aAlbuBovin.add(new Integer[]{2,2});
        aAlbuBovin.add(new Integer[]{5,1});
        aAlbuBovin.add(new Integer[]{7,1});
        hmProteinPeptideMapping.put("ALBU_BOVIN", aAlbuBovin);

        HashMap<String, Double> hmPeptideIntensities = new HashMap<String,Double>();
        hmPeptideIntensities.put("q1p1",1000d);
        hmPeptideIntensities.put("q2p1",500d);
        hmPeptideIntensities.put("q3p1",750d);
        hmPeptideIntensities.put("q2p2",750d);
        hmPeptideIntensities.put("q5p1",2000d);
        hmPeptideIntensities.put("q7p1",1000d);

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

        if(!customquant.isValid()) {
            System.err.println(customquant.getLastErrorString());
            System.exit(1);
        }

        dumpWarnings(customquant.getErrorHandler());
        customquant.clearAllErrors();
        dumpQuantMethod(qmethod, new String[] {rationame});

        // create the protein-peptide mapping
        Set<String> aProteinAccessions = hmProteinPeptideMapping.keySet();
        for(String sAccession : aProteinAccessions) {
            List<Integer[]> aPeptides = hmProteinPeptideMapping.get(sAccession);
            for(Integer[] aQP : aPeptides) {
                customquant.addProteinPeptideKey(sAccession, 0, new ms_peptide_quant_key(aQP[0], aQP[1]));
            }
        }

        // add peptide intensities
        Set<String> aPeptideQP = hmPeptideIntensities.keySet();
        for(String qp : aPeptideQP) {
            double dIntensity = hmPeptideIntensities.get(qp);
            int[] aQP = extractQP(qp);
            ms_peptide_quant_ratio ratio = new ms_peptide_quant_ratio(new ms_peptide_quant_key(aQP[0], aQP[1]), rationame, dIntensity);
            customquant.addPeptideRatio(ratio);
        }

        System.out.println();

        // print is all out immediately
        for(String sAccession : aProteinAccessions) {
            System.out.printf("%s\n", sAccession);
            List<Integer[]> aPeptides = hmProteinPeptideMapping.get(sAccession);

            ms_protein_quant_ratio ratio = customquant.getProteinRatio(sAccession, 0, rationame);

            if(ratio.isMissing()) {
                System.out.printf("%10s: ---", rationame);
            } else {
                System.out.printf("%10s = %10g, stderr = %10g, N = %d, normal-p = %6g, hypo-p = %6g\n", rationame, ratio.getValue(), ratio.getStandardError(), ratio.getSampleSize(), ratio.getNormalityPvalue(), ratio.getHypothesisPvalue());
            }
            System.out.println();
            StringBuilder sbPeptideRatioHeader = new StringBuilder();
            sbPeptideRatioHeader.append(String.format("%10s", "Peptide"));
            sbPeptideRatioHeader.append(" ");
            sbPeptideRatioHeader.append(String.format("%10s",rationame));
            System.out.println(sbPeptideRatioHeader);

            for(Integer[] aQP : aPeptides) {
                List<String> aValues = new ArrayList<String>();
                ms_peptide_quant_ratio oRatio = customquant.getPeptideRatio(new ms_peptide_quant_key(aQP[0],aQP[1]), rationame);

                if(oRatio.isMissing()) {
                    aValues.add(String.format("%10s", "---"));
                }
                if(oRatio.isInfinite()) {
                    aValues.add(String.format("%10s", "---"));
                } else {
                    aValues.add(String.format("%10g", oRatio.getValue()));
                }

                StringBuilder sbValues = new StringBuilder();
                for(String sValue : aValues) {
                    if(sbValues.length() > 0) sbValues.append(" ");
                    sbValues.append(sValue);
                }
                System.out.printf("%10s %s\n", String.format("q%d_p%d", aQP[0], aQP[1]), sbValues);
            }

            System.out.println();
        }
    }

    private static void setParams(ms_quant_method qmethod, String normalisation, String outliers, String proteinRatioType, int iMinNoPeptides) {
        ms_quant_quality oQuality = null;
        if(qmethod.haveQuality()) {
            oQuality = qmethod.getQuality();
        } else {
            oQuality = new ms_quant_quality();
        }

        ms_quant_normalisation oNormalisation = null;

        if(qmethod.haveNormalisation()) {
            oNormalisation = qmethod.getNormalisation();
        } else {
            oNormalisation = new ms_quant_normalisation();
        }

        ms_quant_outliers oOutliers = null;

        if(qmethod.haveOutliers()) {
            oOutliers = qmethod.getOutliers();
        } else {
            oOutliers = new ms_quant_outliers();
        }

        if(normalisation == null || normalisation.length() == 0) normalisation = "none";
        if(outliers == null || outliers.length() == 0) outliers = "none";
        if(iMinNoPeptides <= 0) iMinNoPeptides = 2;
        if(proteinRatioType == null || proteinRatioType.length() == 0) proteinRatioType="Average";

        oNormalisation.setMethod(normalisation);
        oOutliers.setMethod(outliers);
        qmethod.setMinNumPeptides(iMinNoPeptides);
        qmethod.setProteinRatioType(proteinRatioType);
        oQuality.setMinPrecursorCharge(1);
        oQuality.setPepThresholdType("maximum expect");
        oQuality.setPepThresholdValue("0.05");

        qmethod.setQuality(oQuality);
        qmethod.setNormalisation(oNormalisation);
        qmethod.setOutliers(oOutliers);
    }

    private static void dumpWarnings(ms_errs errorHandler) {
        for(int i = 1; i <= errorHandler.getNumberOfErrors(); i++) {
            System.out.println("Warning: "+errorHandler.getErrorString(i));
        }
    }

    private static void dumpQuantMethod(ms_quant_method qmethod, String[] aRatioNames) {
        String[] aComps = new String[qmethod.getNumberOfComponents()];
        for(int i = 0; i < qmethod.getNumberOfComponents(); i++) {
            aComps[i] = qmethod.getComponentByNumber(i).getName();
        }
        System.out.printf("Components: [%s]\n", Arrays.toString(aComps));

        System.out.printf("Report ratios: [%s]\n", Arrays.toString(aRatioNames));

        System.out.printf("Protein ratio type = %s\n", qmethod.getProteinRatioType());
        System.out.printf("Min. number of peptides = %d\n", qmethod.getMinNumPeptides());

        if(qmethod.haveQuality()) {
            ms_quant_quality quality = qmethod.getQuality();
            System.out.printf("Quality: min. precursor charge = %s\n", quality.getMinPrecursorCharge());
            System.out.printf("Quality: pep. threshold type = %s\n", quality.getPepThresholdType());
            System.out.printf("Quality: pep. threshold value = %s\n", quality.getPepThresholdValue());
        } else {
            System.out.print("Quality: no restrictions\n");
        }

        if(qmethod.haveNormalisation()) {
            System.out.printf("Normalisation = %s\n", qmethod.getNormalisation().getMethod());
        } else {
            System.out.print("Normalisation: none\n");
        }

        if(qmethod.haveOutliers()) {
            System.out.printf("Outliers = %s\n", qmethod.getOutliers().getMethod());
        } else {
            System.out.print("Outliers: none\n");
        }
    }

    private static int[] extractQP(String qp) {
        String[] aBits = qp.split("p");
        aBits[0] = aBits[0].replace("q", "");
        return new int[] {Integer.parseInt(aBits[0]), Integer.parseInt(aBits[1])};
    }



}
