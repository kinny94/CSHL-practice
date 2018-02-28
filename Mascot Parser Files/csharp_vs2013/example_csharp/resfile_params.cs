/*
##############################################################################
# file: resfile_params.cs                                                    #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/resfile_params.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class resfile_params
    {
        public static void Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                Console.WriteLine("Must specify results filename as parameter");
                return;
            }

            ms_mascotresfile file = new ms_mascotresfile(argv[0], 0, "");
            if (file.isValid())
            {
                searchParameters(file);
            }
            else
            {
                Console.WriteLine("Error number: {0}", file.getLastError());
                Console.WriteLine("Error string: {0}", file.getLastErrorString());
                return;
            }

        }

        private static void searchParameters(ms_mascotresfile file)
        {
            int i;
            char ch;
            ms_searchparams p = file._params();

            Console.WriteLine("Search parameters from ms_searchparams");
            Console.WriteLine("=========================================");
            Console.WriteLine("License             : {0}", p.getLICENSE());
            Console.WriteLine("Search title        : {0}", p.getCOM());
            Console.WriteLine("SEG mass            : {0}", p.getSEG());
            Console.WriteLine("Peptide tol         : {0}", p.getTOL());
            Console.WriteLine("Peptide tol units   : {0}", p.getTOLU());
            Console.WriteLine("Fragment tol        : {0}", p.getITOL());
            Console.WriteLine("Fragment tol units  : {0}", p.getITOLU());
            Console.WriteLine("Missed cleavages    : {0}", p.getPFA());
            Console.WriteLine("Database            : {0}", p.getDB());
            Console.WriteLine("Static mods         : {0}", p.getMODS());
            Console.WriteLine("Average/monoisotopic: {0}", p.getMASS());
            Console.WriteLine("Enzyme              : {0}", p.getCLE());
            Console.WriteLine("Raw data file name  : {0}", p.getFILENAME());
            Console.WriteLine("Input data          : {0}", p.getQUE());
            Console.WriteLine("Type of search      : {0}", p.getSEARCH());
            Console.WriteLine("User name           : {0}", p.getUSERNAME());
            Console.WriteLine("User email          : {0}", p.getUSEREMAIL());
            Console.WriteLine("Charge state        : {0}", p.getCHARGE());
            Console.WriteLine("Repeat searhc file  : {0}", p.getINTERMEDIATE());
            Console.WriteLine("Num hits to display : {0}", p.getREPORT());
            Console.WriteLine("Show overview       : {0}", ((p.getOVERVIEW()) ? 1 : 0));
            Console.WriteLine("Data file format    : {0}", p.getFORMAT());
            Console.WriteLine("Form version        : {0}", p.getFORMVER());
            Console.WriteLine("Variable mods       : {0}", p.getIT_MODS());

            for (i = 0; i <= 12; i++)
            {
                Console.WriteLine("User{0}              : {1}", "" + i.ToString("D2"), p.getUSERField(i));
            }

            Console.WriteLine("Precursor mass      : {0:#}", p.getPRECURSOR());
            Console.WriteLine("Taxonomy filter     : {0}", p.getTAXONOMY());
            Console.WriteLine("Type of report      : {0}", p.getREPTYPE());
            Console.WriteLine("Accessions to search: {0}", p.getACCESSION());
            Console.WriteLine("Subcluster used     : {0}", p.getSUBCLUSTER());
            Console.WriteLine("ICAT search?        : {0}", ((p.getICAT()) ? 1 : 0));
            Console.WriteLine("Instrument type     : {0}", p.getINSTRUMENT());
            Console.WriteLine("Error tolerant?     : {0}", ((p.getERRORTOLERANT()) ? 1 : 0));
            Console.WriteLine("Rules (ions series) : {0}", p.getRULES());
            Console.WriteLine("Quantitation method : {0}", p.getQUANTITATION());
            Console.WriteLine("Peptide isotope err : {0}", p.getPEP_ISOTOPE_ERROR());
            Console.WriteLine("Decoy database      : {0}", p.getDECOY());

            for (ch = 'A'; ch <= 'Z'; ch++)
            {
                Console.WriteLine("Residue {0}           : {1:#}", ch, p.getResidueMass(ch));
            }

            Console.WriteLine("C terminus mass     : {0:#}", p.getCTermMass());
            Console.WriteLine("N terminus mass     : {0:#}", p.getNTermMass());
            Console.WriteLine("Mass of hydrogen    : {0:#}", p.getHydrogenMass());
            Console.WriteLine("Mass of oxygen      : {0:#}", p.getOxygenMass());
            Console.WriteLine("Mass of carbon      : {0:#}", p.getCarbonMass());
            Console.WriteLine("Mass of nitrogen    : {0:#}", p.getNitrogenMass());
            Console.WriteLine("Mass of electron    : {0:#}", p.getElectronMass());

            i = 1;

            while (p.getVarModsName(i).Length > 0)
            {
                Console.WriteLine("Variable mod name   : {0}", p.getVarModsName(i));
                Console.WriteLine("Variable mod delta  : {0:#}", p.getVarModsDelta(i));
                Console.WriteLine("Variable mod neutral: {0:#}", p.getVarModsNeutralLoss(i));
                i++;
            }
            Console.WriteLine();
        }
    }
}

/*
resfile_params.exe c:\inetpub\mascot\data\F981123.dat
Will give the following output:

Search parameters from ms_searchparams
=========================================
License             : Matrix Science  (ECUK-U3C2-5RS5-AAQL-H2X5)
Search title        : MS/MS Example
SEG mass            : -1
Peptide tol         : 0.2
Peptide tol units   : Da
Fragment tol        : 0.2
Fragment tol units  : Da
Missed cleavages    : 1
Database            : SwissProt
Static mods         :
Average/monoisotopic: Monoisotopic
Enzyme              : Trypsin
Raw data file name  :
Input data          :
Type of search      : MIS
User name           :
User email          :
Charge state        : 2+
Repeat searhc file  : ../data/F981123.dat
Num hits to display : 0
Show overview       : 0
Data file format    : Mascot generic
Form version        : 1.01
Variable mods       : Oxidation (M)
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
Precursor mass      :
Taxonomy filter     : All entries
Type of report      : Peptide
Accessions to search:
Subcluster used     : -1
ICAT search?        : 0
Instrument type     : ESI-QUAD-TOF
Error tolerant?     : 0
Rules (ions series) : 1,2,8,9,10,13,14,15
Quantitation method : None
Peptide isotope err : 0
Decoy database      : -1
Residue A           : 71
Residue B           : 115
Residue C           : 103
Residue D           : 115
Residue E           : 129
Residue F           : 147
Residue G           : 57
Residue H           : 137
Residue I           : 113
Residue J           : 113
Residue K           : 128
Residue L           : 113
Residue M           : 131
Residue N           : 114
Residue O           : 237
Residue P           : 97
Residue Q           : 128
Residue R           : 156
Residue S           : 87
Residue T           : 101
Residue U           : 151
Residue V           : 99
Residue W           : 186
Residue X           : 111
Residue Y           : 163
Residue Z           : 129
C terminus mass     : 17
N terminus mass     : 1
Mass of hydrogen    : 1
Mass of oxygen      : 16
Mass of carbon      : 12
Mass of nitrogen    : 14
Mass of electron    :
Variable mod name   : Oxidation (M)
Variable mod delta  : 16
Variable mod neutral:
*/