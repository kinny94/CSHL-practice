/*
##############################################################################
# file: config_quantitation.java                                             #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2003 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_searchi  #
#     Author: yuryr  #
#       Date: 2006/02/06 17:53:44  #
#   Revision: 1.1  #
# NoKeywords::                                                             #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class config_masses {
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
        if (argv.length < 1) 
        {
            System.out.println("The location of masses file has to be specified as a parameter");
            System.out.println("The location should either be the full path to the masses file");
            System.out.println("or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi");
            System.exit(0);
        }

        // A sessionID can optionally be passed as the second parameter
        // This will only be required if the 'file' is a URL
        ms_masses file;
        if (argv.length > 1) 
        {
            ms_connection_settings cs = new ms_connection_settings();
            cs.setSessionID(argv[1]);
            file = new ms_masses(argv[0], cs);
        } 
        else 
        {
            file = new ms_masses(argv[0]);
        }

        // unlike the other configuration files, it is possible to use ms_masses
        // without specifying the filename - in which case it defaults to '../config/masses'\n";

        if (!file.isValid())
        {

            System.out.println("Error when reading file: " + file.getFileName());
            checkErrorHandler(file);
            System.exit(0);
        }


        System.out.println("Name of the file: " + file.getFileName());
        System.out.println("-------------------------------------------------------------");
        System.out.println("---   Content of masses file                              ---");
        System.out.println("-------------------------------------------------------------");
        System.out.println("Amino acid masses (monoisotopic and average)");

        // we need this object for outputting floating point numbers
        String doubleFormat = "#####.#####";
        java.text.DecimalFormat formatted = new java.text.DecimalFormat(doubleFormat);

        char res;
        for(res='A'; res <= 'Z'; res++)
        {
            System.out.print(res + ":");
            System.out.print(formatted.format(file.getResidueMass(msparser.MASS_TYPE_MONO, res)));
            System.out.print("," + formatted.format(file.getResidueMass(msparser.MASS_TYPE_AVE, res)));
            System.out.println();
        }  

        System.out.println();
        System.out.println("# Atomic masses used for terminus values");

        System.out.println("HYDROGEN:" 
            + formatted.format(file.getHydrogenMass(msparser.MASS_TYPE_MONO)) 
            + ","
            + formatted.format(file.getHydrogenMass(msparser.MASS_TYPE_AVE)));

        System.out.println("CARBON:"
            + formatted.format(file.getCarbonMass(msparser.MASS_TYPE_MONO))
        + "," 
        + formatted.format(file.getCarbonMass(msparser.MASS_TYPE_AVE)));

        System.out.println("NITROGEN:"
            + formatted.format(file.getNitrogenMass(msparser.MASS_TYPE_MONO))
        + ","
        + formatted.format(file.getNitrogenMass(msparser.MASS_TYPE_AVE)));

        System.out.println("OXYGEN:"
            + formatted.format(file.getOxygenMass(msparser.MASS_TYPE_MONO))
        + ","
        + formatted.format(file.getOxygenMass(msparser.MASS_TYPE_AVE)));

        System.out.println("ELECTRON:"
            + formatted.format(file.getElectronMass()));

    }

    static void checkErrorHandler(ms_masses file)
    {
        System.out.println("Testing the error handling...            ");
        System.out.println("=========================================");
        System.out.println("Error string: " + file.getLastErrorString());
        ms_errs errObject = file.getErrorHandler();
        int numErrors = errObject.getNumberOfErrors();
        System.out.println("Number of errors: " + numErrors);
        int i;
        for(i=1; i <= numErrors; i++)
        {
            System.out.println("Error number " + i + ": " + errObject.getErrorString(i));
        }

        file.clearAllErrors();
        System.out.println("Cleared all errors - should have no errors left: " + file.getLastErrorString());
    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_masses masses

Name of the file: ../config/masses
-------------------------------------------------------------
---   Content of masses file                              ---
-------------------------------------------------------------
Amino acid masses (monoisotopic and average)
A:71.03711,71.03711
B:114.53493,114.53493
C:103.00919,103.00919
D:115.02694,115.02694
E:129.04259,129.04259
F:147.06841,147.06841
G:57.02146,57.02146
H:137.05891,137.05891
I:113.08406,113.08406
J:0.00000,0.00000
K:128.09496,128.09496
L:113.08406,113.08406
M:131.04049,131.04049
N:114.04293,114.04293
O:0.00000,0.00000
P:97.05276,97.05276
Q:128.05858,128.05858
R:156.10111,156.10111
S:87.03203,87.03203
T:101.04768,101.04768
U:150.95364,150.95364
V:99.06841,99.06841
W:186.07931,186.07931
X:111.00000,111.00000
Y:163.06333,163.06333
Z:128.55059,128.55059

# Atomic masses used for terminus values
HYDROGEN:1.00783,1.00783
CARBON:12.00000,12.00000
NITROGEN:14.00307,14.00307
OXYGEN:15.99491,15.99491
ELECTRON:0.0005490

*/