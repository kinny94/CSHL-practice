/*
##############################################################################
# file: resfile_params.java                                                  #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2003 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_searchp $ #
#     $Author: davidc $ #
#       $Date: 2006/07/07 16:16:42 $ #
#   $Revision: 1.3 $ #
# $NoKeywords::                                                            $ #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class resfile_params {
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
	        searchParameters(file);
        } else {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }
    }
  
      
    /**
     * searchParameters
     * Display parameters from the ms searchparams object
     * The values come from the parameters and the masses sections of the file
     **/
  
    private static void searchParameters(ms_mascotresfile file) {
        int i;
        char ch;
        ms_searchparams p = file.params();

        System.out.println("Search parameters from ms_searchparams");
        System.out.println("=========================================");

        System.out.println("License             : "+p.getLICENSE());
        System.out.println("Search title        : "+p.getCOM());
        System.out.println("SEG mass            : "+p.getSEG());
        System.out.println("Peptide tol         : "+p.getTOL());
        System.out.println("Peptide tol units   : "+p.getTOLU());
        System.out.println("Fragment tol        : "+p.getITOL());
        System.out.println("Fragment tol units  : "+p.getITOLU());
        System.out.println("Missed cleavages    : "+p.getPFA());
        System.out.println("Database            : "+p.getDB());
        System.out.println("Static mods         : "+p.getMODS());
        System.out.println("Average/monoisotopic: "+p.getMASS());
        System.out.println("Enzyme              : "+p.getCLE());
        System.out.println("Raw data file name  : "+p.getFILENAME());
        System.out.println("Input data          : "+p.getQUE());
        System.out.println("Type of search      : "+p.getSEARCH());
        System.out.println("User name           : "+p.getUSERNAME());
        System.out.println("User email          : "+p.getUSEREMAIL());
        System.out.println("Charge state        : "+p.getCHARGE());
        System.out.println("Repeat search file  : "+p.getINTERMEDIATE());
        System.out.println("Num hits to display : "+p.getREPORT());
        System.out.println("Show overview       : "+toBinary(p.getOVERVIEW()));
        System.out.println("Data file format    : "+p.getFORMAT());
        System.out.println("Form version        : "+p.getFORMVER());
        System.out.println("Variable mods       : "+p.getIT_MODS());

        // java lacks a printf equivalent by default
        for(i=0; i <=12; i++) {
            String output = "User";
            if(i < 10) output += "0";
            output += i+"              : "+p.getUSERField(i);
            System.out.println(output);
        }

        System.out.println("Precursor mass      : "+roundWholeNumber(p.getPRECURSOR()));
        System.out.println("Taxonomy filter     : "+p.getTAXONOMY());
        System.out.println("Type of report      : "+p.getREPTYPE());
        System.out.println("Accessions to search: "+p.getACCESSION());
        System.out.println("Subcluster used     : "+p.getSUBCLUSTER());
        System.out.println("ICAT search?        : "+toBinary(p.getICAT()));
        System.out.println("Instrument type     : "+p.getINSTRUMENT());
        System.out.println("Error tolerant?     : "+toBinary(p.getERRORTOLERANT()));
        System.out.println("Rules (ions series) : "+p.getRULES());
        System.out.println("Quantitation method : "+p.getQUANTITATION());
        System.out.println("Peptide isotope err : "+p.getPEP_ISOTOPE_ERROR());
        System.out.println("Decoy database      : "+p.getDECOY());

        for(ch='A'; ch <= 'Z'; ch++) {
            System.out.println("Residue "+ch+"           : "+roundWholeNumber(p.getResidueMass(ch)));
        }

        System.out.println("C terminus mass     : "+roundWholeNumber(p.getCTermMass()));
        System.out.println("N terminus mass     : "+roundWholeNumber(p.getNTermMass()));
        System.out.println("Mass of hydrogen    : "+roundWholeNumber(p.getHydrogenMass()));
        System.out.println("Mass of oxygen      : "+roundWholeNumber(p.getOxygenMass()));
        System.out.println("Mass of carbon      : "+roundWholeNumber(p.getCarbonMass()));
        System.out.println("Mass of nitrogen    : "+roundWholeNumber(p.getNitrogenMass()));
        System.out.println("Mass of electron    : "+roundWholeNumber(p.getElectronMass()));

        i=1;

        while(!(p.getVarModsName(i).equals(""))) {
            System.out.println("Variable mod name   : "+p.getVarModsName(i));
            System.out.println("Variable mod delta  : "+roundWholeNumber(p.getVarModsDelta(i)));
            System.out.println("Variable mod neutral: "+roundWholeNumber(p.getVarModsNeutralLoss(i)));
            i++;
        }
        System.out.println("\n");    
    }
  


    /**
     * toBinary(boolean bool)
     * returns 0 for false, 1 for true
     **/

    private static int toBinary(boolean bool) {
        if(bool) return 1;
        return 0;
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

}

/*


will give the output: 



C:>java -classpath .;../java/msparser.jar resfile_params F981123.dat
Search parameters from ms_searchparams
=========================================
License             :
Search title        : A few peptides from an LCMS run
SEG mass            : -1
Peptide tol         : 2.0
Peptide tol units   : Da
Fragment tol        : 1.0
Fragment tol units  : Da
Missed cleavages    : 1
Database            : MSDB
Static mods         : Oxidation (M),SMA (K),SMA (N-term)
Average/monoisotopic: Monoisotopic
Enzyme              : Trypsin
Raw data file name  : U:\\Mascot test data\\TSQ\\dyckall_1.asc
Input data          :
Type of search      : MIS
User name           : Cat R. Piller
User email          : crp@brassica.com
Charge state        : 2+
Repeat search file  :
Num hits to display : 5
Show overview       : 0
Data file format    : Finnigan (.ASC)
Form version        : 1.01
Variable mods       :
User00              :
User01              :
User02              :
User03              :
User04              :
User05              :
User06              :
User07              :
User08              :
User09              :
User10              :
User11              :
User12              :
Precursor mass      : 0
Taxonomy filter     : All entries
Type of report      : peptide
Accessions to search:
Subcluster used     : -1
ICAT search?        : 0
Instrument type     : ESI-QUAD-TOF
Error tolerant?     : 0
Rules (ions series) : 1,2,8,9,10,13,14,15
Quantitation method : ICAT
Peptide isotope err : 2
Decoy database      : 1
Residue A           : 71.03711
Residue B           : 114.53493
Residue C           : 103.00919
Residue D           : 115.02694
Residue E           : 129.04259
Residue F           : 147.06841
Residue G           : 57.02146
Residue H           : 137.05891
Residue I           : 113.08406
Residue J           : 0
Residue K           : 255.15829
Residue L           : 113.08406
Residue M           : 147.03541
Residue N           : 114.04293
Residue O           : 0
Residue P           : 97.05276
Residue Q           : 128.05858
Residue R           : 156.10111
Residue S           : 87.03203
Residue T           : 101.04768
Residue U           : 0
Residue V           : 99.06841
Residue W           : 186.07931
Residue X           : 111
Residue Y           : 163.06333
Residue Z           : 128.55059
C terminus mass     : 17.002735
N terminus mass     : 128.07116
Mass of hydrogen    : 1.007825
Mass of oxygen      : 15.99491
Mass of carbon      : 12
Mass of nitrogen    : 14.00307
Mass of electron    : 0


*/
