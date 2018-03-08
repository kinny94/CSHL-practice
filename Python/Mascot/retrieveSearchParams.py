import msparser
import sys

def main( filepath ) :

    resfile = msparser.ms_mascotresfile( filepath )

    if resfile.isValid() :
        searchParameters(resfile)


def searchParameters(resfile) :
    """
    Display parameters from the ms_searchparams object.
    The values come from the parameters and the masses sections of the file.
    """

    params = resfile.params()
    fmt = "%-20s: %s"

    print("Search parameters from ms_searchparams")
    print("=========================================")
    print(fmt % ("License"             , params.getLICENSE()))
    print(fmt % ("Search title"        , params.getCOM()))
    print(fmt % ("SEG mass"            , params.getSEG()))
    print(fmt % ("Peptide tol"         , params.getTOL()))
    print(fmt % ("Peptide tol units"   , params.getTOLU()))
    print(fmt % ("Fragment tol"        , params.getITOL()))
    print(fmt % ("Fragment tol units"  , params.getITOLU()))
    print(fmt % ("Missed cleavages"    , params.getPFA()))
    print(fmt % ("Database"            , params.getDB()))
    print(fmt % ("Static mods"         , params.getMODS()))
    print(fmt % ("Average/monoisotopic", params.getMASS()))
    print(fmt % ("Enzyme"              , params.getCLE()))
    print(fmt % ("Raw data file name"  , params.getFILENAME()))
    print(fmt % ("Input data"          , params.getQUE()))
    print(fmt % ("Type of search"      , params.getSEARCH()))
    print(fmt % ("User name"           , params.getUSERNAME()))
    print(fmt % ("User email"          , params.getUSEREMAIL()))
    print(fmt % ("Charge state"        , params.getCHARGE()))
    print(fmt % ("Repeat search file"  , params.getINTERMEDIATE()))
    print(fmt % ("Num hits to display" , params.getREPORT()))
    print(fmt % ("Show overview"       , params.getOVERVIEW()))
    print(fmt % ("Data file format"    , params.getFORMAT()))
    print(fmt % ("Form version"        , params.getFORMVER()))
    print(fmt % ("Variable mods"       , params.getIT_MODS()))

    for i in range(12) :
        print(fmt % ( "User%02d" % i, params.getUSERField(i)))
    
    print(fmt % ("Precursor mass"      , params.getPRECURSOR()))
    print(fmt % ("Taxonomy filter"     , params.getTAXONOMY()))
    print(fmt % ("Type of report"      , params.getREPTYPE()))
    print(fmt % ("Accessions to search", params.getACCESSION()))
    print(fmt % ("Subcluster used"     , params.getSUBCLUSTER()))
    print(fmt % ("ICAT search?"        , params.getICAT()))
    print(fmt % ("Instrument type"     , params.getINSTRUMENT()))
    print(fmt % ("Error tolerant?"     , params.getERRORTOLERANT()))
    print(fmt % ("Rules (ions series)" , params.getRULES()))

    for ch in range(ord('A'), 1 + ord('Z')) :
        print(fmt % ("Residue " + chr(ch), params.getResidueMass(chr(ch))))
    
    print(fmt % ("C terminus mass" , params.getCTermMass()))
    print(fmt % ("N terminus mass" , params.getNTermMass()))
    print(fmt % ("Mass of hydrogen", params.getHydrogenMass()))
    print(fmt % ("Mass of oxygen"  , params.getOxygenMass()))
    print(fmt % ("Mass of carbon"  , params.getCarbonMass()))
    print(fmt % ("Mass of nitrogen", params.getNitrogenMass()))
    print(fmt % ("Mass of electron", params.getElectronMass()))

    i = 1
    while params.getVarModsName(i) :
        print(fmt % ("Variable mod name"   , params.getVarModsName(i)))
        print(fmt % ("Variable mod delta"  , params.getVarModsDelta(i)))
        print(fmt % ("Variable mod neutral", params.getVarModsNeutralLoss(i)))
        i += 1

    print(" ")


if __name__ == "__main__" :
    sys.exit(main("data.dat"))
