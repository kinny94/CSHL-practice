/*
 * ##############################################################################
 * # file: resfile_ms2quantitation.java                                         #
 * # Mascot Parser toolkit example code                                         #
 * ##############################################################################
 * # COPYRIGHT NOTICE                                                           #
 * # Copyright 1998-2013 Matrix Science Limited  All Rights Reserved.           #
 * #                                                                            #
 * ##############################################################################
 *
 */

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import matrix_science.msparser.ms_peptide_quant_key;
import matrix_science.msparser.ms_mascotresfile;
import matrix_science.msparser.ms_quant_method;
import matrix_science.msparser.ms_mascotoptions;
import matrix_science.msparser.ms_peptidesummary;
import matrix_science.msparser.ms_ms2quantitation;
import matrix_science.msparser.ms_peptide;
import matrix_science.msparser.ms_peptide_quant_ratio;
import matrix_science.msparser.ms_protein;
import matrix_science.msparser.ms_protein_quant_ratio;
import matrix_science.msparser.ms_quant_normalisation;
import matrix_science.msparser.ms_quant_outliers;
import matrix_science.msparser.ms_quant_quality;

/**
 *
 * @author patricke
 */
public class resfile_ms2quantitation {

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
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        if(args.length < 4) {
            System.err.println("Usage: <quantitation schema 1 filepath> <quantitation schema 2 filepath> <unimod schema filepath> <results file>");
            System.err.println("The location of schema files should be e.g.");
            System.err.println("  ../html/xmlns/schema/quantitation_1/quantitation_1.xsd");
            System.err.println("  ../html/xmlns/schema/quantitation_2/quantitation_2.xsd");
            System.err.println("  ../html/xmlns/schema/unimod_2/unimod_2.xsd");
            System.err.println("if running from the Mascot cgi directory.");
            System.exit(1);
        }

        String sQuantSchema1Filepath = args[0];
        String sQuantSchema2Filepath = args[1];
        String sUnimodSchemaFilepath = args[2];

        ms_mascotresfile oResfile = new ms_mascotresfile(args[3],1);
        if(!oResfile.isValid()) {
            System.err.println(oResfile.getLastErrorString());
            System.exit(1);
        }

        oResfile.setXMLschemaFilePath(ms_mascotresfile.XML_SCHEMA_QUANTITATION,
                "http://www.matrixscience.com/xmlns/schema/quantitation_2 "+sQuantSchema2Filepath+" http://www.matrixscience.com/xmlns/schema/quantitation_1 "+sQuantSchema1Filepath);

        oResfile.setXMLschemaFilePath(ms_mascotresfile.XML_SCHEMA_UNIMOD, "http://www.unimod.org/xmlns/schema/unimod_2 "+sUnimodSchemaFilepath);

        ms_quant_method oQMethod = new ms_quant_method();

        if(!oResfile.getQuantitationMethod(oQMethod)) {
            System.err.println(oResfile.getLastErrorString());
            System.exit(1);
        }

        // See section 'Multiple return values in Perl, Java and Python' in ms_parser help
        long[] iFlags = {0};
        double[] dMinProb = {0};
        int[] iMaxHits = {0};
        double[] dIgnoreIonsBelow = {0};
        long[] iMinPepLen = {0};
        boolean[] bUsePepSum = {false};
        long[] iFlags2 = {0};
        ms_mascotoptions oOptions = new ms_mascotoptions();

        oResfile.get_ms_mascotresults_params(oOptions, iFlags, dMinProb, iMaxHits, dIgnoreIonsBelow, iMinPepLen, bUsePepSum, iFlags2);

        if(!bUsePepSum[0]) {
            System.err.println("Results file cannot be used to open as a peptide summary");
            System.exit(1);
        }

        ms_peptidesummary oPepsum = new ms_peptidesummary(oResfile, (int) iFlags[0], dMinProb[0], iMaxHits[0], "", dIgnoreIonsBelow[0], (int) iMinPepLen[0], "", (int) iFlags2[0]);
        if(!oResfile.isValid()) {
            System.err.println(oResfile.getLastErrorString());
            System.exit(1);
        }

        // Quantitate the file using ms_ms2quantitation:
        ms_ms2quantitation oMs2Quant = new ms_ms2quantitation(oPepsum, oQMethod);

        if(!oMs2Quant.isValid()) {
            System.err.println(oMs2Quant.getLastErrorString());
            System.exit(1);
        }

        // Now we can inspect protein and peptide data in oMs2Quant
        // But first, dump the quantitation method parameters to see what we have:

        String[] aComps = new String[oQMethod.getNumberOfComponents()];
        for(int i = 0; i < oQMethod.getNumberOfComponents(); i++) {
            aComps[i] = oQMethod.getComponentByNumber(i).getName();
        }
        System.out.printf("Components: %s\n", Arrays.toString(aComps));

        String[] aRationames = new String[oQMethod.getNumberOfReportRatios()];
        for(int i = 0; i < oQMethod.getNumberOfReportRatios(); i++) {
            aRationames[i] = oQMethod.getReportRatioByNumber(i).getName();
        }
        System.out.printf("Report ratios: %s\n", Arrays.toString(aRationames));

        System.out.printf("Protein ratio type = %s\n", oQMethod.getProteinRatioType());
        System.out.printf("Min. number of peptides = %d\n", oQMethod.getMinNumPeptides());

        if (oQMethod.haveQuality()) {
            ms_quant_quality oQuality = oQMethod.getQuality();
            System.out.printf("Quality: min. precursor charge = %s\n", oQuality.getMinPrecursorCharge());
            System.out.printf("Quality: pep. threshold type = %s\n", oQuality.getPepThresholdType());
            System.out.printf("Quality: pep. threshold value = %s\n", oQuality.getPepThresholdValue());
        } else {
            System.out.print("Quality: no restrictions\n");
        }

        if (oQMethod.haveNormalisation()) {
            ms_quant_normalisation oNormalisation = oQMethod.getNormalisation();
            System.out.printf("Normalisation = %s\n", oNormalisation.getMethod());
        } else {
            System.out.print("Normalisation: none\n");
        }

        if (oQMethod.haveOutliers()) {
            ms_quant_outliers oOutliers = oQMethod.getOutliers();
            System.out.printf("Outliers = %s\n", oOutliers.getMethod());
        } else {
            System.out.print("Outliers: none\n");
        }
        System.out.println();

        // collect proteins we're interested in
        List<ms_protein> aProteins = new ArrayList<ms_protein>();
        for(int i = 1; i <= oPepsum.getNumberOfHits(); i++) {
            aProteins.add(oPepsum.getHit(i));

            int j = 0;
            ms_protein oProtein;
            while((oProtein = oPepsum.getNextFamilyProtein(i, ++j)) != null) {
                aProteins.add(oProtein);
            }
        }

        for(ms_protein oProtein : aProteins) {
            System.out.printf("%d::%s:\n", oProtein.getDB(), oProtein.getAccession());
            for(String sRationame : aRationames) {
                ms_protein_quant_ratio oRatio = oMs2Quant.getProteinRatio(oProtein.getAccession(), oProtein.getDB(), sRationame);
                if(oRatio.isMissing()) {
                    System.out.printf("%10s: ---\n", sRationame);
                } else {
                    System.out.printf("%10s = %10g, stderr = %10g, N = %d, normal-p = %6g, hypo-p = %6g\n",
                            sRationame, oRatio.getValue(), oRatio.getStandardError(),
                            oRatio.getSampleSize(), oRatio.getNormalityPvalue(),
                            oRatio.getHypothesisPvalue());
                }
            }
            System.out.println();
            StringBuilder sbPeptideRatioHeader = new StringBuilder();
            sbPeptideRatioHeader.append(String.format("%10s", "Peptide"));
            for(String sRationame : aRationames) {
                sbPeptideRatioHeader.append(" ");
                sbPeptideRatioHeader.append(String.format("%10s",sRationame));
            }
            System.out.println(sbPeptideRatioHeader);
            for(int i = 1; i <= oProtein.getNumPeptides(); i++) {
                if(oProtein.getPeptideDuplicate(i) == ms_protein.DUPE_DuplicateSameQuery) continue;
                int q = oProtein.getPeptideQuery(i);
                int p = oProtein.getPeptideP(i);

                ms_peptide oPeptide = oPepsum.getPeptide(q, p);
                if(oPeptide == null) continue;

                List<String> aValues = new ArrayList<String>();
                for(String sRationame : aRationames) {
                    // Note that the q,p arguments to ms_peptide_quant_key must be integers.
                    // Otherwise the wrong constructor is chosen. Here they are
                    // integers, as they are return values from Parser functions that
                    // return ints.
                    ms_peptide_quant_ratio oRatio = oMs2Quant.getPeptideRatio(new ms_peptide_quant_key(q,p), sRationame);

                    if(oRatio.isMissing()) {
                        aValues.add(String.format("%10s", "---"));
                        continue;
                    }

                    if(oRatio.isInfinite()) {
                        aValues.add(String.format("%10s", "---"));
                    } else {
                        aValues.add(String.format("%10g", oRatio.getValue()));
                    }
                }
                StringBuilder sbValues = new StringBuilder();
                for(String sValue : aValues) {
                    if(sbValues.length() > 0) sbValues.append(" ");
                    sbValues.append(sValue);
                }
                System.out.printf("%10s %s\n", String.format("q%d_p%d", q, p), sbValues);
            }
            System.out.println();
        }
        
        

    }

}
