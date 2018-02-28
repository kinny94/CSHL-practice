/*
##############################################################################
# file: config_mascotdat.cs                                                  #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_mascotdat.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/29 10:06:27 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class config_mascotdat
    {
        public static void Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 1)
            {
                Console.Error.WriteLine(@"The location of the mascot.dat has to be specified as a parameter
The location should either be the full path to the mascot.dat file
or a URL to a Mascot server - e.g. http://mascot-searver/mascot/cgi
A Mascot security sessionID can optionally be passed as a second 
parameter");
                    return;
            }

            // A sessionID can optionally be passed as the second parameter
            // This will only be required if the 'file' is a URL
            ms_datfile file;
            if (argv.Length > 1)
            {
                ms_connection_settings cs = new ms_connection_settings();
                cs.setProxyServerType(ms_connection_settings.PROXY_TYPE.PROXY_TYPE_AUTO);
                cs.setSessionID(argv[1]);
                file = new ms_datfile(argv[0], 0, cs);
            } else file = new ms_datfile(argv[0]);

            if (!file.isValid())
            {
                Console.Error.WriteLine("There are errors. Cannot continue. The last error description:");
                Console.Error.WriteLine(file.getLastErrorString());
                return;
            }

            // retrieving Databases-section content
            ms_databases dbs = file.getDatabases();

            // check if there is actually a 'Databases' section in the file
            if (dbs.isSectionAvailable())
            {
                int n = dbs.getNumberOfDatabases();
                Console.WriteLine("There are {0} databases configured:", n);
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine("{0} : {1}", dbs.getDatabase(i).getName(), dbs.getDatabase(i).isActive() ? "active" : "inactive");
                }
            }
            else
            {
                Console.WriteLine("Databases-section is missing");
            }
            Console.WriteLine();

            // retrieving Parse-section content
            ms_parseoptions parseOptions = file.getParseOptions();

            // check if there is actually a 'Parse' section in the file
            if (parseOptions.isSectionAvailable())
            {
                int n = parseOptions.getNumberOfParseRules();
                Console.WriteLine("Parse rules configured:");
                for (int i = 0; i < n; i++)
                {
                    // not all of them can be specified
                    if (parseOptions.getParseRule(i).isAvailable())
                    {
                        Console.WriteLine("Rule_{0} : {1}", i, parseOptions.getParseRule(i).getRuleStr());
                    }
                }
            }
            else
            {
                Console.WriteLine("Parse-section is missing");
            }
            Console.WriteLine();

            // retrieving WWW-section content
            ms_wwwoptions wwwOptions = file.getWWWOptions();

            // check if there is actually a 'WWWW' section in the file
            if (wwwOptions.isSectionAvailable())
            {
                int n = wwwOptions.getNumberOfEntries();
                Console.WriteLine("There are {0} sequence report sources configured:", n);
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine("{0}_{1}", wwwOptions.getEntry(i).getName(), 
                        wwwOptions.getEntry(i).getType() == WWW_TYPE.WWW_SEQ ? "SEQ" : "REP");
                }
            }
            else
            {
                Console.WriteLine("WWW-section is missing");
            }
            Console.WriteLine();

            // retrieving Taxonomy-sections
            Console.WriteLine("Available taxonomy sources:");
            int maxTax = file.getMaxTaxonomyRules();
            for (int taxInd = 1; taxInd <= maxTax; taxInd++)
            {
                // check whether a certain taxonomy section exists
                if (file.getTaxonomyRules(taxInd) != null)
                {
                    Console.WriteLine("TAXONOMY_{0} {1}", taxInd, file.getTaxonomyRules(taxInd).getIdentifier());
                }
            }
            Console.WriteLine();

            // retrieving Cluster-section content
            ms_clusterparams clusterParams = file.getClusterParams();

            // Check if there is a 'Cluster' section available
            if (clusterParams.isSectionAvailable())
            {
                Console.WriteLine("Cluster mode : {0}", clusterParams.isEnabled() ? "enabled" : "disabled");
            }
            else
            {
                Console.WriteLine("Cluster-section is missing");
            }
            Console.WriteLine();

            // retrieving Processor-section content
            ms_processoroptions procOptions = file.getProcessors();

            // check if the 'Processor' section is available
            if (procOptions.isSectionAvailable())
            {
                Console.WriteLine("{0} CPU(s) configured", procOptions.getNumberOfProcessors());
            }
            else
            {
                Console.WriteLine("Processor-section is missing");
            }
            Console.WriteLine();

            // retrieving Options-section content
            ms_mascotoptions mascotOptions = file.getMascotOptions();

            // check if there is actually an 'Options' section in the file
            if (mascotOptions.isSectionAvailable())
            {
                Console.WriteLine("MascotCmdLine : {0}", mascotOptions.getMascotCmdLine());
            }
            else
            {
                Console.WriteLine("Options-section is missing");
            }
            Console.WriteLine();

            // retrieving Cron-section content
            ms_cronoptions cronOptions = file.getCronOptions();
            if (cronOptions.isCronEnabled())
            {
                int n = cronOptions.getNumberOfCronJobs();
                Console.WriteLine("There are {0} cron-jobs configured:", n);
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine(cronOptions.getCronJob(i).getCommandStr());
                }
            }
            else
            {
                Console.WriteLine("Cron functionality is disabled");
            }
            Console.WriteLine();
        }
    }
}

/*
Running the program as

config_mascotdat.exe c:\inetpub\mascot\config\mascot.dat

Will give the output:

There are 18 databases configured:
B_subtilis : inactive
Bug_11795 : inactive
contaminants : active
cRAP : active
Human_NoIsoforms : active
IPI-human : active
IPI_human : active
IPI_human_decoy : active
Mixed_M_2013-02-07_TruncWithReverse4 : active
PRIDE_PXD0000001 : active
Proteome_EBV_B95.8_GenBank : inactive
SwissProt : active
UniProt_Arabidopsis : active
UniProt_Chlamydomonas_reinhardtii : active
UniProt_Human : active
UniProt_Papillomaviridae : active
UniProt_Pig : active
UniProt_Rat : active

Parse rules configured:
Rule_1 : >[^|]*|[ABCDEFGHIJKLMNOPQRSTUVWXYZ]\([^ |]*\)
Rule_2 : >[^ ]* [^ ]* \(.*\)
Rule_3 : \*\(.*\)>
Rule_4 : >\(gi|[0-9]*\)
Rule_5 : >[^ ]* \(.*\)
Rule_6 : >\([^ ]*\)
Rule_7 : >[^ ]* \(.*\)
Rule_8 : \*\(.*\)>
Rule_9 : >..|\([^|]*\)
Rule_10 : \(.*\)
Rule_11 : >..|[ABCDEFGHIJKLMNOPQRSTUVWXYZ]\([^|]*\)
Rule_12 : >IPI:\([^| ]*\)
Rule_13 : ^ID   \([^ .]*\)
Rule_14 : \*\(.*\)>
Rule_15 : \*.*\(ID   IPI[0-9.]* .*\)
Rule_16 : >IPI:\([^| .]*\)
Rule_17 : ^ID   \([^ .]*\)
Rule_18 : >[^ ]* \(.*\)
Rule_19 : \*\(.*\)>
Rule_20 : \*.*\(ID   IPI[0-9.]* .*\)
Rule_21 : >\([^|]*\)
Rule_22 : >\([^ ]*\)
Rule_23 : >\(.*\)
Rule_24 : >..|[^|]*|\([^ ]*\)
Rule_25 : ^ID   \([^ ]*\)
Rule_26 : >[^ ]* \(.*\)
Rule_27 : \*\(.*\)>
Rule_28 : \*.*\(ID   [-A-Z0-9_].*\)
Rule_29 : human  *release  *\([0-9.][0-9.]*\)
Rule_30 : Swiss-Prot  *.*Release  *\([0-9._][0-9._]*\)
Rule_31 : >IPI:\([^| .]*\)
Rule_32 : >[^|]*|\\w\([^ |]*\)
Rule_33 : >..|\\w\([^|]*\)
Rule_34 : human  *release  *\([0-9.][0-9.]*\)

There are 27 sequence report sources configured:
B_subtilis_SEQ
Bug_11795_SEQ
contaminants_SEQ
cRAP_SEQ
Human_NoIsoforms_SEQ
IPI-human_SEQ
IPI_human_SEQ
IPI_human_decoy_SEQ
Mixed_M_2013-02-07_TruncWithReverse4_SEQ
PRIDE_PXD0000001_SEQ
Proteome_EBV_B95.8_GenBank_SEQ
SwissProt_SEQ
UniProt_Arabidopsis_SEQ
UniProt_Chlamydomonas_reinhardtii_SEQ
UniProt_Human_SEQ
UniProt_Papillomaviridae_SEQ
UniProt_Pig_SEQ
UniProt_Rat_SEQ
IPI-human_REP
IPI_human_REP
SwissProt_REP
UniProt_Arabidopsis_REP
UniProt_Chlamydomonas_reinhardtii_REP
UniProt_Human_REP
UniProt_Papillomaviridae_REP
UniProt_Pig_REP
UniProt_Rat_REP

Available taxonomy sources:
TAXONOMY_1 All human with TaxID 9606
TAXONOMY_2 SwissProt FASTA
TAXONOMY_3 All human with TaxID 9606
TAXONOMY_4 All mouse with TaxID 10090
TAXONOMY_5 EMBL EST Fasta
TAXONOMY_6 HUPO PSI PEFF Format
TAXONOMY_7 MSDB REF
TAXONOMY_8 NCBI nucleotide FASTA using GI2TAXID
TAXONOMY_9 NCBI protein FASTA using GI2TAXID
TAXONOMY_10 OWL REF
TAXONOMY_11 Obsolete_1
TAXONOMY_12 Obsolete_2
TAXONOMY_13 Obsolete_3
TAXONOMY_14 Swiss-prot DAT
TAXONOMY_15 SwissProt FASTA
TAXONOMY_16 UniRef Fasta

Cluster mode : disabled

Processor-section is missing

MascotCmdLine : ../cgi/nph-mascot.exe

There are 5 cron-jobs configured:
C:\Perl64\bin\perl.exe C:/inetpub/mascot/bin/dbman_add_task.pl SwissProt
C:\Perl64\bin\perl.exe C:/inetpub/mascot/bin/dbman_add_task.pl UniProt_Chlamydomonas_reinhardtii
C:\Perl64\bin\perl.exe C:/inetpub/mascot/bin/dbman_add_task.pl UniProt_Rat
C:\Perl64\bin\perl.exe C:/inetpub/mascot/bin/dbman_add_task.pl UniProt_Pig
C:\Perl64\bin\perl.exe C:/inetpub/mascot/bin/dbman_process_tasks.pl
*/
