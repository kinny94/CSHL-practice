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

public class config_mascotdat {
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
            System.err.println("Location of mascot.dat has to be specified as a parameter");
            System.err.println("The location should either be the full path to the enzymes file");
            System.err.println("or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi");
            System.exit(0);
        }

        // A sessionID can optionally be passed as the second parameter
        // This will only be required if the 'file' is a URL
        ms_datfile file;
        if (argv.length > 1) 
        {
            ms_connection_settings cs = new ms_connection_settings();
            cs.setSessionID(argv[1]);
            file = new ms_datfile(argv[0], 0, cs);
        } 
        else 
        {
            file = new ms_datfile(argv[0]);
        }

        if (!file.isValid())
        {
            System.err.println("There are errors. Cannot continue. The last error description:");
            System.err.println(file.getLastErrorString());
            System.exit(0);
        }

        // retrieving Databases-section content
        ms_databases dbs = file.getDatabases();

        // check if there is actually a 'Databases' section in the file 
        if (dbs.isSectionAvailable()) 
        {
            int n = dbs.getNumberOfDatabases();
            System.out.println("There are " + n + " databases configured:");
            for(int i=0; i < n; i++) 
            {
                System.out.print(dbs.getDatabase(i).getName() + " : ");
                if (dbs.getDatabase(i).isActive()) 
                {
                    System.out.println("active");
                } 
                else 
                {
                    System.out.println("inactive");
                }
            }
        }
        else 
        {
            System.out.println("Databases-section is missing");
        }
        System.out.println("");

        // retrieving Parse-section content
        ms_parseoptions parseOptions = file.getParseOptions();

        // check if there is 'Parse' section in the file actually
        if (parseOptions.isSectionAvailable()) 
        {
            int n = parseOptions.getNumberOfParseRules();
            System.out.println("Parse rules configured:");
            for (int i=0; i < n; i++) 
            {
                // not all of them can be specified
                if (parseOptions.getParseRule(i).isAvailable()) 
                {
                    System.out.print("Rule_" + i + " : ");
                    System.out.println(parseOptions.getParseRule(i).getRuleStr());
                }
            }
        } 
        else 
        {
            System.out.println("Parse-section is missing");
        }
        System.out.println();
    
        // retrieving WWW-section content
        ms_wwwoptions wwwOptions = file.getWWWOptions();

        // check if there is 'WWW' section in the file actually
        if (wwwOptions.isSectionAvailable())
        {
            int n = wwwOptions.getNumberOfEntries();
            System.out.println("There are " + n + " sequence report sources configured:");
            for(int i=0; i < n; i++) 
            {
                System.out.print(wwwOptions.getEntry(i).getName() + "_");
                if (wwwOptions.getEntry(i).getType() == msparser.WWW_SEQ) 
                {
                    System.out.println("SEQ");
                } 
                else 
                {
                    System.out.println("REP");
                }
            }
        } 
        else 
        {
            System.out.println("WWW-section is missing");
        }
        System.out.println();

        // retrieving Taxonomy-sections
        System.out.println("Available taxonomy sources:");
        int maxtax =  file.getMaxTaxonomyRules();
        for (int taxind = 1; taxind <= maxtax; taxind++) 
        {
            // check whether a certain taxonomy section exists
            if (file.getTaxonomyRules(taxind) != null) 
            {
                System.out.print("TAXONOMY_" + taxind + " ");
                System.out.println(file.getTaxonomyRules(taxind).getIdentifier());
            }
        }
        System.out.println();

        // retrieving Cluster-section content
        ms_clusterparams clusterParams = file.getClusterParams();

        // check if there is 'Cluster' section in the file actually
        if (clusterParams.isSectionAvailable()) 
        {
            System.out.print("Cluster mode : ");
            if (clusterParams.isEnabled()) 
            {
                System.out.println("enabled");
            } 
            else 
            {
                System.out.println("disabled");
            }
        } 
        else 
        {
            System.out.println("Cluster-section is missing");
        }
        System.out.println();

        // retrieving Processor-section content
        ms_processoroptions procOptions = file.getProcessors();

        // check if there is actually a 'Processor' section in the file 
        if (procOptions.isSectionAvailable()) 
        {
            System.out.println(procOptions.getNumberOfProcessors()  + " CPU(s) configured");
        } 
        else 
        {
            System.out.println("Processor-section is missing");
        }
        System.out.println();

        // retrieving Options-section content
        ms_mascotoptions mascotOptions = file.getMascotOptions();

        // check if there is actually an 'Options' section in the file 
        if (mascotOptions.isSectionAvailable())
        {
            System.out.print("MascotCmdLine : ");
            System.out.println(mascotOptions.getMascotCmdLine());
        } 
        else 
        {
            System.out.println("Options-section is missing");
        }
        System.out.println();

        // retrieving Cron-section content
        ms_cronoptions cronOptions = file.getCronOptions();

        // check if there is actually a 'Cron' section in the file 
        if (cronOptions.isSectionAvailable())
        {
            if (cronOptions.isCronEnabled())
            {
                int n = cronOptions.getNumberOfCronJobs();
                System.out.println("There are " + n + " cron-jobs configured:");
                for (int i=0; i < n; i++)
                {
                    System.out.println(cronOptions.getCronJob(i).getCommandStr());
                }
            } 
            else 
            {
                System.out.println("Cron functionality is disabled");
            }
        } 
        else 
        {
            System.out.println("Cron-section is missing");
        }
        System.out.println();
    }
  
}

/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_mascotdat mascot.dat

There are 10 databases configured:
MSDB : active
NCBInr : active
EST_human : inactive
EST_mouse : inactive
EST_others : active
SwissProt : active
Trembl : inactive
IPI_human : inactive
IPI_mouse : inactive
yeastorfn : active

Parse rules configured:
Rule_1 : >owl[^ ]*|\([^ ]*\)
Rule_2 : >owl[^ ]*|[^ ]*[ ]\(.*\)
Rule_3 : >[A-Z][0-9];\([^ ]*\)
Rule_4 : >\([^ ]*\)
Rule_5 : >[^ ]* \(.*\)
Rule_6 : >\(gi|[0-9]*\)
Rule_7 : >[^ ]* \(.*\)
Rule_8 : \*\(.*\)>
Rule_9 : \*.*\(>[A-Z][0-9];.*\)
Rule_10 : \(LOCUS .*\)ORIGIN
Rule_11 : \(LOCUS .*\)
Rule_12 : >\([^ ]*\)
Rule_13 : >[^ ]* \(.*\)
Rule_14 : <pre>\(.*\)</pre>
Rule_15 : ^AC   \([^ ;]*\)
Rule_16 : \*.*\(ID   [A-Z0-9]*_[A-Z0-9]* .*\)
Rule_17 : >\([^ ]*\)
Rule_18 : >[^ ]* \(.*\)
Rule_19 : >[A-Z][0-9];\([^ ]*\)[ ]*
Rule_20 : >\(.*\)
Rule_21 : >IPI:\([^| .]*\)
Rule_22 : \*.*\(ID   IPI[0-9]* .*\)
Rule_23 : \(.*\)
Rule_24 : \*.*\(ID   [-A-Z0-9_].*\)
Rule_25 : >[^(]*.\([^)]*\)
Rule_26 : ^AC   \([^ ;]*\)
Rule_27 : \*.*\(AC   [A-Z0-9]*;.*\)

There are 19 sequence report sources configured:
NCBInr_SEQ
NCBInr_REP
EST_human_SEQ
EST_human_REP
EST_mouse_SEQ
EST_mouse_REP
EST_others_SEQ
EST_others_REP
SwissProt_SEQ
SwissProt_REP
Trembl_SEQ
Trembl_REP
MSDB_SEQ
MSDB_REP
IPI_human_SEQ
IPI_human_REP
IPI_mouse_SEQ
IPI_mouse_REP
yeastorfn_SEQ


Available taxonomy sources:
TAXONOMY_1 NCBI nr FASTA
TAXONOMY_2 OWL REF
TAXONOMY_3 Swiss-prot FASTA
TAXONOMY_4 NCBI dbEST FASTA
TAXONOMY_5 Swiss-prot DAT
TAXONOMY_6 MSDB REF (pre 20000621)
TAXONOMY_7 MSDB REF
TAXONOMY_8 NCBI nr FASTA using GI2TAXID
TAXONOMY_9 dbEST FASTA using GI2TAXID
TAXONOMY_10 EST_human FASTA with TaxID
TAXONOMY_11 EST_mouse FASTA with TaxID

Cluster mode : disabled

Processor-section is missing

MascotCmdLine : ../cgi/nph-mascot.exe

Cron functionality is disabled

*/