import sys
import time
import msparser

def main( filepath ):

    resfile = msparser.ms_mascotresfile( filepath )

    if( resfile.isValid() ):
            searchInformation( resfile )
            return 0

    print "Failure"
    return -1

def searchInformation( resfile ):

    """
    Display parameters from the ms_mascotresfile object. The functions
    anyPMF, anyMSMS, anySQ should normally be used in preference to isPMF etc
    because some people submit MSMS though the sequence query window etc.
    """

    fmt = "%20s: %s"

    print("Search information from ms_mascotresfile")
    print("========================================")
    print(fmt % ("Number of queries"   , resfile.getNumQueries()))
    print(fmt % ("Number of hits"      , resfile.getNumHits()))
    print(fmt % ("Number of sequences" , resfile.getNumSeqs()))
    print(fmt % ("Sequences after tax" , resfile.getNumSeqsAfterTax()))
    print(fmt % ("Number of residues"  , resfile.getNumResidues()))
    print(fmt % ("Execution time"      , resfile.getExecTime()))
    print(fmt % ("Date (seconds)"      , resfile.getDate()))

    date = time.localtime(resfile.getDate())
    Wdays = "Mon Tue Wed Thu Fri Sat Sun".split(" ")
    Mons = "Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec".split(" ")

    print(fmt %( "Date", "%s %s %d %02d:%02d:%02d %d" % (
        Wdays[date.tm_wday],
        Mons[date.tm_mon - 1],
        date.tm_mday,
        date.tm_hour,
        date.tm_min,
        date.tm_sec,
        date.tm_year
        )))

    print(fmt % ("Mascot version"       , resfile.getMascotVer()))
    print(fmt % ("Fasta version"        , resfile.getFastaVer()))
    print(fmt % ("Is PMF?"              , resfile.isPMF()))
    print(fmt % ("Is MSMS?"             , resfile.isMSMS()))
    print(fmt % ("Is SQ?"               , resfile.isSQ()))
    print(fmt % ("Is Error tolerant"    , resfile.isErrorTolerant()))
    print(fmt % ("Any PMF?"             , resfile.anyPMF()))
    print(fmt % ("Any MSMS?"            , resfile.anyMSMS()))
    print(fmt % ("Any SQ?"              , resfile.anySQ()))
    print(fmt % ("Any peptides section" , resfile.doesSectionExist(msparser.ms_mascotresfile.SEC_PEPTIDES)))
    print(fmt % ("Any peptide matches"  , resfile.anyPeptideSummaryMatches()))

    print(" ")


if __name__ == "__main__":
    sys.exit(main("data.dat"))
