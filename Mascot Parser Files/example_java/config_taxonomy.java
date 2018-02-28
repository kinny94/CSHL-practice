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

public class config_taxonomy 
{
    static 
    {
        try 
        {
            System.loadLibrary("msparserj");
        } 
        catch (UnsatisfiedLinkError e) 
        {
            System.err.println("Native code library failed to load. "
                + "Is msparserj.dll on the path?\n" + e);
            System.exit(0);
        }
    }

    public static void main(String argv[]) 
    {
        if (argv.length < 1) 
        {
            System.out.println("The location of taxonomy file has to be specified as a parameter");
            System.out.println("The location should either be the full path to the taxonomy file");
            System.out.println("or a URL to a Mascot server - e.g. http://mascot-server/mascot/cgi");
            System.exit(0);
        }

        // A sessionID can optionally be passed as the second parameter
        // This will only be required if the 'file' is a URL
        ms_taxonomyfile file;
        if (argv.length > 1) 
        {
            ms_connection_settings cs = new ms_connection_settings();
            cs.setSessionID(argv[1]);
            file = new ms_taxonomyfile(argv[0], cs);
        } 
        else 
        {
            file = new ms_taxonomyfile(argv[0]);
        }

        if (!file.isValid())
        {
            System.out.println("There are errors. Cannot continue. The last error description:");
            System.out.println(file.getLastErrorString());;
            System.exit(0);
        }

        int n = file.getNumberOfEntries();
        System.out.println("There are " + n + " taxonomy choice entries configured:");

        // now get'em all!
        for (int i=0; i < n; i++) 
        {
            System.out.println(file.getEntryByNumber(i).getTitle());
            System.out.print("Include: ");
            for (int j=0; j < file.getEntryByNumber(i).getNumberOfIncludeTaxonomies(); j++) 
            {
                if (j > 0) 
                {
                    System.out.print(",");
                }
                System.out.print(file.getEntryByNumber(i).getIncludeTaxonomy(j));
            }
            System.out.println();
            System.out.print("Exclude: ");
            for (int j=0; j < file.getEntryByNumber(i).getNumberOfExcludeTaxonomies(); j++) 
            {
                if (j > 0) 
                {
                    System.out.print(",");
                }
                System.out.print(file.getEntryByNumber(i).getExcludeTaxonomy(j));
            }
            System.out.println();

        }
  
    }
}
/*
will give the output: 



C:>java -classpath .;../java/msparser.jar config_taxonomy taxonomy

There are 64 taxonomy choice entries configured:
All entries
Include: 1
Exclude: 0
. . Archaea (Archaeobacteria)
Include: 2157
Exclude:
. . Eukaryota (eucaryotes)
Include: 2759
Exclude:
. . . . Alveolata (alveolates)
Include: 33630
Exclude:
. . . . . . Plasmodium falciparum (malaria parasite)
Include: 5833
Exclude:
. . . . . . Other Alveolata
Include: 33630
Exclude: 5833

...

*/