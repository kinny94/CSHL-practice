/*
##############################################################################
# file: tools_aahelper.cs                                                    #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/tools_aahelper.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Collections.Generic;
using matrix_science.msparser;
using System.Text;


namespace MsParserExamples
{
    class tools_aahelper
    {
        public static int Main(string[] argv)
        {
            if (argv.Length < 2)
            {
                // We need an enzyme to build a list of peptides and mod_file file if we
                // want to apply any modifications
                Console.Error.WriteLine("Location of enzymes file and mod_file have to be specified as parameters");
                return 1;
            }

            ms_enzymefile enzymefile;
            if (!openEnzymefile(argv[0], out enzymefile))
            {
                return 1;
            }

            // Note: enzymefile *must* me kept in scope for as long as you use 'trypsin'.
            // See "Using the toolkit from Perl, Java, Python and C#" in the Mascot Parser
            // manual.
            ms_enzyme trypsin = enzymefile.getEnzymeByName("Trypsin");

            if (trypsin == null)
            {
                Console.Error.WriteLine("Cannot find 'Trypsin' in the enzyme file.  Cannot continue.");
                return 1;
            }

            ms_modfile modfile;
            if(!openModfile(argv[1], out modfile)) {
                return 1;
            }

            ms_aahelper aahelper = new ms_aahelper();

            // Note: both modfile and trypsin *must* be kept in score for as long as
            // you use aahelper.  See "Using the toolkit from Perl, Java, Python and C#" in the
            // Mascot Parser manual.
            aahelper.setMasses(modfile.getMassFile());
            aahelper.setEnzyme(trypsin);

            // Now we can generate peptides for a given protein.  This is
            // RL29_METTP (50S ribosomal protein L29P OS=Methanosaeta thermophila (strain
            // DSM 6194 / PT) GN=rpl29p PE=3 SV=1) from SwissProt 2010_09.
            string proteinStr = "MAIFRIDEIRNMSSEELEEELRKLEVELIRERGAVRAGGAPEKPGRIREIRRTIARMKTVQRERVRK";

            // No missed cleavages are allowed (third parameter)
            aahelper.startIteratePeptides(proteinStr, proteinStr.Length, 0);

            Console.WriteLine("List of peptides");

            while (aahelper.getNextPeptide())
            {
                int start = aahelper.getPepStart(), len = aahelper.getPepEnd() - aahelper.getPepStart() + 1;

                // getPepStart() returns one-based index
                string peptideStr = proteinStr.Substring(start - 1, len);
                Console.WriteLine(peptideStr);
            }

            Console.WriteLine("End of list\n");

            // Create a list of fixed modifications
            ms_modvector vecFixed = new ms_modvector();
            vecFixed.appendModification(modfile.getModificationByName("Phospho (Y)"));

            // Create a list of variable modifications
            ms_modvector vecVariable = new ms_modvector();
            vecVariable.appendModification(modfile.getModificationByName("Oxidation (M)"));
            vecVariable.appendModification(modfile.getModificationByName("Acetyl (N-term)"));

            // Note: Both vecFixed and vecVariable *must* be kept in scope for as long as you use
            // aahelper.  See "Using the toolkit from Perl, Java, Python and C#" in the
            // Mascot Parser manual
            aahelper.setAvailableModifications(vecFixed, vecVariable);

            // ms_aahelper can also contain errors that might happen when applying modifications,
            // for example when we have a conflict between two modifications (same residue or same
            // peptide end).
            if (!aahelper.isValid())
            {
                Console.WriteLine("Error while setting available modifications: {0}", aahelper.getLastErrorString());
                return 1;
            }

            // We will also need a separate error object for collecting peptide specific errors
            ms_errs err = new ms_errs();

            // Example of hot to call calcPeptideMZ().  It will often be more convenient to
            // create an ms_peptide instead, and the call getMrCalc() on that object
            vectori numThatMustBeModded = new vectori();
            numThatMustBeModded.Add(1); // 1 acetylNterm modification
            numThatMustBeModded.Add(1); // 1 site is oxidised

            double mr = aahelper.calcPeptideMZ(proteinStr,
                proteinStr.Length,
                1,
                10, // peptide ends (1-based)
                numThatMustBeModded,
                0,  // No charge - i.e. Mr
                MASS_TYPE.MASS_TYPE_MONO,
                err);

            if (!err.isValid())
            {
                Console.Error.WriteLine("Error while calculating peptide mass: {0}", err.getLastErrorString());
                // Don't need to halt as they are not fatal errors
                err.clearAllErrors();
            }
            else
            {
                Console.WriteLine("Peptide mass calculated using 'calcPeptideMZ' is {0:########.###}", mr);
            }

            // Create a peptide - which we can then fragment
            //
            // Specify which residues are modified and by whoch modification as it has to
            // correspond to a modification string:
            //
            // Nterm modification + 9 residues + Cterm modification
            vectori numModded = new vectori()
            {
                {2},    // N-term   - modified by 'Acetyl (N-term)'
                {1},    // M        - modified by 'Oxidation (M)'
                {0},    // A
                {0},    // I
                {0},    // F
                {0},    // R
                {0},    // I
                {0},    // D
                {0},    // E
                {0},    // I
                {0},    // R
                {0}     // C-Term
            };

            // We have to specify (or at least supply an empty vector) which neutral loss
            // value to use, in case there are more than one available for a modification
            vectori whichNL = new vectori() { 
                {0},    // N-term
                {1},    // M            - has 2 neutral losses.  Specify the first (-98)
                {0},    // A
                {0},    // I
                {0},    // F
                {0},    // R
                {0},    // I
                {0},    // D
                {0},    // E
                {0},    // I
                {0},    // R
                {0}     // C-term
            };

            ms_peptide peptide = aahelper.createPeptide(
                proteinStr,
                proteinStr.Length,
                1,
                10,             // end position
                numModded,      // modification string-like vector
                whichNL,        // which neutral loss to use
                0,              // no charge
                MASS_TYPE.MASS_TYPE_MONO,
                err);

            if (!err.isValid())
            {
                Console.Error.WriteLine("Error while creating a peptide: {0}", err.getLastErrorString());
                // Don't need to halt as they are not fatal errors
                err.clearAllErrors();
            }
            else
            {
                Console.WriteLine("\n\nPeptide has been created successfully: {0}", peptide.getPeptideStr());
            }

            // Keep a list of fragments from all series
            ms_fragmentvector allFragments = new ms_fragmentvector();

            ms_fragmentvector bions = fragmentPeptide(aahelper,
                peptide,
                (int) ms_fragmentationrules.FRAG_SERIES_TYPE.FRAG_B_SERIES,
                "b-ions series",
                false,      // singly charged ions only
                mr      // maximal fragment mass to return
                );

            // copyFrom() an only be used to populate the list for the first time
            allFragments.copyFrom(bions);

            ms_fragmentvector fragments = fragmentPeptide(aahelper,
                peptide,
                (int) ms_fragmentationrules.FRAG_SERIES_TYPE.FRAG_Y_SERIES,
                "y-ion series",
                false,              // singly charged ions only
                mr                  // maximal fragment mass to return
                );

            for (uint i = 0; i < fragments.getNumberOfFragments(); i++)
            {
                allFragments.appendFragment(fragments.getFragmentByNumber(i));
            }

            fragments = fragmentPeptide(aahelper,
                peptide,
                (int)ms_fragmentationrules.FRAG_SERIES_TYPE.FRAG_INTERNAL_YB,
                "internal yb-ion series",
                false,              // singly charged ions only
                700                  // maximal fragment mass to return
                );

            for(uint i = 0; i < fragments.getNumberOfFragments(); i++) {
                allFragments.appendFragment(fragments.getFragmentByNumber(i));
            }

            Console.WriteLine("Paste the following into a Mascot search query window to verify this output:");
            displayMascotTestSearch(vecFixed, vecVariable, trypsin, peptide.getMrCalc(),
                bions); // or you can use allFragments

            return 0;
        }

        // vecFixed contains a list of fixed mods applied to the peptide
        // vecVariable contains a list of variable mods applied to the peptide
        // enzyme is the enzyme used
        // me if the peptide Mr(calc)
        // fragments contains a list of b-ions from a peptide
        //
        // use this information to generate a test search that can be run on Mascot
        private static void displayMascotTestSearch(ms_modvector vecFixed, ms_modvector vecVariable, ms_enzyme enzyme, double mr, ms_fragmentvector fragments)
        {
            for (int i = 0; i < vecFixed.getNumberOfModifications(); i++)
            {
                Console.WriteLine("MODS={0}", vecFixed.getModificationByNumber(i).getTitle());
            }
            for (int i = 0; i < vecVariable.getNumberOfModifications(); i++)
            {
                Console.WriteLine("IT_MODS={0}", vecVariable.getModificationByNumber(i).getTitle());
            }

            Console.WriteLine("CHARGE=Mr");
            Console.WriteLine("CLE={0}", enzyme.getTitle());
            Console.Write("{0:#.###} ions(", mr);

            StringBuilder sbIons = new StringBuilder();
            for (uint i = 0; i < fragments.getNumberOfFragments(); i++)
            {
                if (i > 0) sbIons.Append(", ");
                sbIons.Append(string.Format("{0:0.###}", fragments.getFragmentByNumber(i).getMass()));
            }
            Console.Write(sbIons);

            Console.WriteLine(")");
        }

        private static ms_fragmentvector fragmentPeptide(ms_aahelper aahelper, ms_peptide peptide, int series, string seriesLabel, bool doubleCharged, double massMax)
        {
            ms_fragmentvector fragments = new ms_fragmentvector();
            ms_errs err = new ms_errs();

            aahelper.calcFragments(
                peptide,
                series,
                doubleCharged,
                100d,
                massMax,
                MASS_TYPE.MASS_TYPE_MONO,
                fragments,
                err);

            if (!err.isValid())
            {
                Console.Error.WriteLine("Error while creating peptide {0} fragments: {1}", seriesLabel, err.getLastErrorString());
                err.clearAllErrors();
            }
            Console.WriteLine("{0} fragments", seriesLabel);
            printFragmentsTable(fragments);
            return fragments;
        }

        private static void printFragmentsTable(ms_fragmentvector fragments)
        {
            Console.WriteLine("Number of fragments: {0}", fragments.getNumberOfFragments());
            string fmt = "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}";

            Console.WriteLine(fmt, "Col".PadLeft(5),
                "Start".PadLeft(5),
                "End".PadLeft(5),
                "Label".PadLeft(10),
                "Mass".PadLeft(10),
                "NL".PadLeft(10),
                "Name",
                "Immon",
                "Intern",
                "Reg".PadLeft(4));

            for (uint i = 0; i < fragments.getNumberOfFragments(); i++)
            {
                ms_fragment frag = fragments.getFragmentByNumber(i);
                Console.WriteLine(fmt,
                    frag.getColumn().ToString().PadLeft(5),
                    frag.getStart().ToString().PadLeft(5),
                    frag.getEnd().ToString().PadLeft(5),
                    frag.getLabel().PadLeft(10),
                    string.Format("{0:######0.00}", frag.getMass()).PadLeft(10),
                    string.Format("{0:######0.00}", frag.getNeutralLoss()).PadLeft(10),
                    frag.getSeriesName().PadLeft(4),
                    (frag.isImmonium() ? "1" : "").PadLeft(5),
                    (frag.isInternal() ? "1" : "").PadLeft(6),
                    (frag.isRegular() ? "1" : "").PadLeft(4));                
            }
            Console.WriteLine();
        }



        private static bool openModfile(string filename, out ms_modfile modfile)
        {
            // We can use the default masses in this example
            ms_masses masses = new ms_masses();
            modfile = new ms_modfile(filename, masses);
            if (!modfile.isValid())
            {
                Console.Error.WriteLine("Error while opening mod file: {0}", modfile.getLastErrorString());
                return false;
            }

            string[] aTargetMods = new string[]{"Oxidation (M)", "Acetyl (N-term)", "Phospho (Y)"};
            foreach(string mod in aTargetMods) {
                if (modfile.getModificationByName(mod) == null)
                {
                    Console.Error.WriteLine("Cannot find '{0}' in the mod file. Cannot continue.", mod);
                    return false;
                }
            }
            return true;
        }

        private static bool openEnzymefile(string filename, out ms_enzymefile enzymefile)
        {
            enzymefile = new ms_enzymefile(filename);
            if (!enzymefile.isValid())
            {
                Console.Error.WriteLine("Error while opening enzyme file: {0}", enzymefile.getLastErrorString());
                return false;
            }
            return true;
        }        
    }
}

/*
tools_aahelper.exe c:\inetpub\mascot\config\enzymes c:\inetpub\mascot\config\mod_file
Will give the following output:

List of peptides
M
MAIFR
AIFR
IDEIR
NMSSEELEEELR
K
LEVELIR
ER
GAVR
AGGAPEKPGR
IR
EIR
R
TIAR
MK
TVQR
ER
VR
K
End of list

Peptide mass calculated using 'calcPeptideMZ' is 1320.686


Peptide has been created successfully: MAIFRIDEIR
b-ions series fragments
Number of fragments: 9
  Col Start   End      Label       Mass         NL Name Immon Intern  Reg
    1     1    -1       b(1)     190.05       0.00    b                 1
    2     2    -1       b(2)     261.09       0.00    b                 1
    3     3    -1       b(3)     374.17       0.00    b                 1
    4     4    -1       b(4)     521.24       0.00    b                 1
    5     5    -1       b(5)     677.34       0.00    b                 1
    6     6    -1       b(6)     790.43       0.00    b                 1
    7     7    -1       b(7)     905.45       0.00    b                 1
    8     8    -1       b(8)    1034.50       0.00    b                 1
    9     9    -1       b(9)    1147.58       0.00    b                 1

y-ion series fragments
Number of fragments: 9
  Col Start   End      Label       Mass         NL Name Immon Intern  Reg
    9     9    -1       y(9)    1132.65       0.00    y                 1
    8     8    -1       y(8)    1061.61       0.00    y                 1
    7     7    -1       y(7)     948.53       0.00    y                 1
    6     6    -1       y(6)     801.46       0.00    y                 1
    5     5    -1       y(5)     645.36       0.00    y                 1
    4     4    -1       y(4)     532.27       0.00    y                 1
    3     3    -1       y(3)     417.25       0.00    y                 1
    2     2    -1       y(2)     288.20       0.00    y                 1
    1     1    -1       y(1)     175.12       0.00    y                 1

internal yb-ion series fragments
Number of fragments: 22
  Col Start   End      Label       Mass         NL Name Immon Intern  Reg
    2     2     3         AI     185.13       0.00   yb            1
    2     2     4        AIF     332.20       0.00   yb            1
    2     2     5       AIFR     488.30       0.00   yb            1
    2     2     6      AIFRI     601.38       0.00   yb            1
    3     3     4         IF     261.16       0.00   yb            1
    3     3     5        IFR     417.26       0.00   yb            1
    3     3     6       IFRI     530.34       0.00   yb            1
    3     3     7      IFRID     645.37       0.00   yb            1
    4     4     5         FR     304.18       0.00   yb            1
    4     4     6        FRI     417.26       0.00   yb            1
    4     4     7       FRID     532.29       0.00   yb            1
    4     4     8      FRIDE     661.33       0.00   yb            1
    5     5     6         RI     270.19       0.00   yb            1
    5     5     7        RID     385.22       0.00   yb            1
    5     5     8       RIDE     514.26       0.00   yb            1
    5     5     9      RIDEI     627.35       0.00   yb            1
    6     6     7         ID     229.12       0.00   yb            1
    6     6     8        IDE     358.16       0.00   yb            1
    6     6     9       IDEI     471.24       0.00   yb            1
    7     7     8         DE     245.08       0.00   yb            1
    7     7     9        DEI     358.16       0.00   yb            1
    8     8     9         EI     243.13       0.00   yb            1

Paste the following into a Mascot search query window to verify this output:
MODS=Phospho (Y)
IT_MODS=Oxidation (M)
IT_MODS=Acetyl (N-term)
CHARGE=Mr
CLE=Trypsin
1320.686 ions(190.053, 261.09, 374.174, 521.243, 677.344, 790.428, 905.455, 1034.498, 1147.582)
*/