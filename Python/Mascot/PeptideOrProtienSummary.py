import msparser
import sys
import re

def main( filepath ) :  

    resfile = msparser.ms_mascotresfile( filepath )

    if checkErrors(resfile) :
        
        # The parameters passed to ms_peptidesummary or ms_proteinsummary determine
        # the type of grouping and the number of proteins and peptides displayed.
        # Default parameters can be returned using ms_mascotresfile::get_ms_mascotresults_params().
        # The return values from this function depend on the type of search,
        # and also on values in the mascot.dat configuration file if that is available.

        # You may need to change this path
        datfile = msparser.ms_datfile("../config/mascot.dat")

        # if the mascot.dat isn't available, use defaults
        mascotOptions = msparser.ms_mascotoptions()
        
        if (datfile.isValid()) :
            mascotOptions = datfile.getMascotOptions()
            
        (scriptName, 
        flags, 
        minProbability, 
        maxHitsToReport, 
        ignoreIonsScoreBelow, 
        minPepLenInPepSummary, 
        usePeptideSummary, 
        flags2) = resfile.get_ms_mascotresults_params(mascotOptions)
         
        bResult = False
        if (usePeptideSummary) :

            # For peptide summary
            #
            # Flags defined for hierarchical clustering algorithm:
            #     flags: ms_mascotresults::MSRES_CLUSTER_PROTEINS
            #            | ms_mascotresults::MSRES_SHOW_SUBSETS
            #            | ms_mascotresults::MSRES_MUDPIT_PROTEIN_SCORE;
            #     flags2: ms_peptidesummary::MSPEPSUM_USE_HOMOLOGY_THRESH;
            #
            # Flags defined for original simple parsimony algorithm:
            #     flags: ms_mascotresults::MSRES_GROUP_PROTEINS | ms_mascotresults::MSRES_SHOW_SUBSETS;

            print("-------------------------------------------------------------")
            print("---   Peptide summary report                              ---")
            print("-------------------------------------------------------------")
             
            bResult = show_results(resfile,
                                   usePeptideSummary,
                                   flags,
                                   minProbability,
                                   maxHitsToReport,
                                   ignoreIonsScoreBelow,
                                   minPepLenInPepSummary,
                                   flags2)
        else :
            # Show results from full protein summary, remove grouping
            flags &= ~msparser.ms_mascotresults.MSRES_GROUP_PROTEINS
            flags &= ~msparser.ms_mascotresults.MSRES_SHOW_SUBSETS
            
            print("-------------------------------------------------------------")
            print("---   Full Protein summary report                         ---")
            print("-------------------------------------------------------------")
            bResult = show_results(resfile,
                                   usePeptideSummary,
                                   flags,
                                   minProbability,
                                   maxHitsToReport,
                                   ignoreIonsScoreBelow,
                                   minPepLenInPepSummary,
                                   flags2)
                                   
            if (bResult and checkErrors(resfile)) :
                print("")
                
                # Show results from concise protein summary, add grouping
                flags |= msparser.ms_mascotresults.MSRES_GROUP_PROTEINS
                flags |= msparser.ms_mascotresults.MSRES_SHOW_SUBSETS
                
                print("-------------------------------------------------------------")
                print("---   Concise Protein summary report                      ---")
                print("-------------------------------------------------------------")
                bResult = show_results(resfile,
                                       usePeptideSummary,
                                       flags,
                                       minProbability,
                                       maxHitsToReport,
                                       ignoreIonsScoreBelow,
                                       minPepLenInPepSummary,
                                       flags2)

        if (bResult and checkErrors(resfile)) :
            returnValue = 0
             
        return returnValue


def show_results(resfile, usePeptideSummary, flags, minProteinProb, maxHits, minIonsScore, minPepLenInPepSummary, flags2) :

    if usePeptideSummary :
        results = msparser.ms_peptidesummary(
            resfile, flags, minProteinProb, maxHits, "", minIonsScore, minPepLenInPepSummary, "", flags2
            )
    else :
        results = msparser.ms_proteinsummary(
            resfile, flags, minProteinProb, maxHits
            )

    if not checkErrors(resfile) :
        return False
    
    family = 1
    hit  = 1
    prot = results.getHit(hit)

    while prot :
        accession   = prot.getAccession()
        description = results.getProteinDescription(accession)
        mass        = results.getProteinMass(accession)
        dbIdx       = prot.getDB()

        protein_hit = "Protein Hit %d" % hit
        if flags & msparser.ms_mascotresults.MSRES_CLUSTER_PROTEINS :
            protein_hit = protein_hit + "." + str(family)
        
        print protein_hit
        print("===================")
        print("Accession   : %s" % accession)
        print("Description : %s" % description)
        print("Score       : %s" % prot.getScore())
        print("Mass        : %s" % mass)
        print("Frame       : %s" % prot.getFrame())
        print("Coverage    : %s" % prot.getCoverage())
        print("RMS error   : %s" % prot.getRMSDeltas(results))
        print("Peptides    : %s" % prot.getNumDisplayPeptides())

        # Each protein has a number of peptides that matched - list them:
        num_peps = prot.getNumPeptides()

        for i in range(1, 1+ num_peps) :
            query = prot.getPeptideQuery(i)
            p     = prot.getPeptideP(i)

            isDupSameQuery = prot.getPeptideDuplicate(i) == msparser.ms_protein.DUPE_DuplicateSameQuery 
            if p != -1 and query != -1 and not isDupSameQuery :
                pep = results.getPeptide(query, p)
                if not pep:
                    continue

                displayPeptideInfo(
                    0, pep, results, 
                    prot.getPeptideDuplicate(i) == msparser.ms_protein.DUPE_Duplicate,
                    prot.getPeptideIsBold(i),
                    prot.getPeptideShowCheckbox(i)
                    )

        # Now display list of all proteins that contain a subset or a same set of the matching peptides

        if flags & msparser.ms_mascotresults.MSRES_GROUP_PROTEINS or flags & msparser.ms_mascotresults.MSRES_CLUSTER_PROTEINS :
            print("Proteins matching the same set of peptides:")

            i = 1
            similar_prot = results.getNextSimilarProteinOf(accession, dbIdx, 1)
            while similar_prot :
                similar_accession = similar_prot.getAccession()
                similar_dbIdx = similar_prot.getDB()
                if(flags & msparser.ms_mascotresults.MSRES_CLUSTER_PROTEINS and similar_dbIdx > 1) :
                    print(str(similar_dbIdx) + "::"),
                print(similar_accession + " - Total score:" + str(similar_prot.getScore())),
                print(" - Peptides matched:" + str(similar_prot.getNumDisplayPeptides()))
                i += 1
                similar_prot = results.getNextSimilarProteinOf(accession, dbIdx, i)
            
            if flags & msparser.ms_mascotresults.MSRES_SHOW_SUBSETS :
                print("Proteins matching a subset of these peptides:")

                i = 1
                subset_prot = results.getNextSubsetProteinOf(accession, dbIdx, 1)
                while subset_prot :
                    subset_accession = subset_prot.getAccession()
                    subset_dbIdx = subset_prot.getDB()
                    if(flags & msparser.ms_mascotresults.MSRES_CLUSTER_PROTEINS and subset_dbIdx > 1) :
                        print(str(subset_dbIdx) + "::"),
                    print(subset_accession + " - Total score:" + str(subset_prot.getScore())),
                    print(" - Peptides matched:" + str(subset_prot.getNumDisplayPeptides()))
                    
                    if(flags & msparser.ms_mascotresults.MSRES_CLUSTER_PROTEINS) :
                        j = 1
                        similar_subset_prot = results.getNextSimilarProteinOf(subset_accession, subset_dbIdx, j)
                        if similar_subset_prot :
                            print("  Proteins matching the same set of peptides for this subset:")
                        while similar_subset_prot :
                            similar_subset_accession = similar_subset_prot.getAccession()
                            similar_subset_dbIdx = similar_subset_prot.getDB()
                            print("  "),
                            if similar_subset_dbIdx > 1 :
                                print(str(similar_subset_dbIdx) + "::"),
                            print(similar_subset_accession + " - Total score:" + str(similar_subset_prot.getScore())), 
                            print(" Peptides matched:" + str(similar_subset_prot.getNumDisplayPeptides()))
                            j += 1
                            similar_subset_prot = results.getNextSimilarProteinOf(subset_accession, subset_dbIdx, j) 
                    
                    i += 1
                    subset_prot = results.getNextSubsetProteinOf(accession, dbIdx, i)

        if flags & msparser.ms_mascotresults.MSRES_CLUSTER_PROTEINS :           
            prot = results.getNextFamilyProtein(hit, family)
            family += 1 
            if not prot :
                hit += 1
                prot = results.getHit(hit)
                family = 1
        else :
            hit += 1
            prot = results.getHit(hit)
            
        print(" ")

    results.createUnassignedList(msparser.ms_mascotresults.SCORE)

    if results.getNumberOfUnassigned() :
        print("Unassigned list")
        print("---------------")

        for u in range(1, 1 + results.getNumberOfUnassigned()) :
            pep = results.getUnassigned(u)
            displayPeptideInfo(0, pep, results, 0, 1, 1)
        
    if usePeptideSummary :
        print(" ")
        displayYellowPopupInfo(results, 1)
        
    return True


def displayYellowPopupInfo(results, q) :
    """
    Shows the equivalent of the yellow popup box for given query
    - results is the results object
    - q is the query number
    """

    fmt = "%5s %5s %9s %7s %7s"
    print(fmt % ("Score", "Delta", "Hit", "Protein", "Peptide"))

    for p in range(1, 11) :
        pep = results.getPeptide(q, p)
        if not pep: continue

        seq = pep.getPeptideStr()
        if not seq: continue

        tmp = results.getProteinsWithThisPepMatch(q, p)

        (hit, protein) = ('', '')

        if tmp :
            hit, protein, morethan = re.search('(\d+):([^ ]*)[ ]*(.*)', tmp).groups()

            if morethan :
                hit += "+"

        print(fmt % (pep.getIonsScore(), pep.getDelta(), hit, protein, seq))

    p = 1
    print("Accessions that matched query %s rank %s :- %s" % (q, p, results.getProteinsWithThisPepMatch(q, p)))


def displayPeptideInfo(showFullDetails, p, results, isDuplicate, isBold, showCheckBox) :
    q = p.getQuery()

    if not showFullDetails :
        fmt = "%2s %4s %4d %11f %4d(%4d) %-20s %s%3.2f%s %3s"

        cb, bold = "--", "dim"
        if showCheckBox : cb = "CB" 
        if isBold : bold = "BOLD" 
        
        paren1, paren2 = "", ""
        if isDuplicate : paren1 = "(" ; paren2 = ")" 
        
        print(fmt % (
            cb,
            bold,
            q,
            p.getObserved(),
            p.getRank(),
            p.getPrettyRank(),
            p.getPeptideStr(),
            paren1,
            p.getIonsScore(),
            paren2,
            results.getReadableVarMods(q, p.getRank())
            ))

        return
    

    print("Peptide hit")

    if p.getAnyMatch() :
        fmt = "    %-12s: %s"
        print(fmt % ('Query'       , q))
        print(fmt % ('Rank'        , p.getRank()))
        print(fmt % ('Matched'     , p.getAnyMatch()))
        print(fmt % ('missedCleave', p.getMissedCleavages()))
        print(fmt % ('mrCalc'      , p.getMrCalc()))
        print(fmt % ('delta'       , p.getDelta()))
        print(fmt % ('observed'    , p.getObserved()))
        print(fmt % ('charge'      , p.getCharge()))
        print(fmt % ('mrExp'       , p.getMrExperimental()))
        print(fmt % ('ionsMatched' , p.getNumIonsMatched()))
        print(fmt % ('peptideStr'  , p.getPeptideStr()))
        print(fmt % ('peaksUsed1'  , p.getPeaksUsedFromIons1()))
        print(fmt % ('varModsStr'  , p.getVarModsStr()))
        print(fmt % ('readable mod', results.getReadableVarMods(q, p.getRank)))
        print(fmt % ('ionsScore'   , p.getIonsScore()))
        print(fmt % ('seriesUsedS' , p.getSeriesUsedStr()))
        print(fmt % ('peaksUsed2'  , p.getPeaksUsedFromIons2()))
        print(fmt % ('peaksUsed3'  , p.getPeaksUsedFromIons3()))
        print(fmt % ('idth, hth, p', ', '.join(
            results.getPeptideIdentityThreshold(q, 20),
            results.getHomologyThreshold(q, 20),
            results.getProbOfPepBeingRandomMatch(p.getIonsScore(), q)
            )))
        print(" ")
    else :
        print("    No match")

def checkErrors(resfile) :
    if resfile.getLastError() :
        for i in range(1, 1 + resfile.getNumberOfErrors()) :
            print("Error number: %s : %s" % (resfile.getErrorNumber(i), resfile.getErrorString(i)))
    
    #Call isValid before clearAllErrors, otherwise this method always returns true
    bIsValid = resfile.isValid()
    resfile.clearAllErrors()
    return bIsValid

if __name__ == "__main__" :
    sys.exit(main("data.dat"))
