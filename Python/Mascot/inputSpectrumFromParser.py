import msparser
import sys

def main( filename ):

    resfile = msparser.ms_mascotresfile( filename )

    if resfile.isValid() :
        inputData(resfile)


def inputData(resfile) :
    # display input data via inputquery get functions
    for i in range(1, 2) :
        # replace range with (1, 1 + resfile.getNumQueries()) to get all input
        # data.

        print("Input data for query number %d" % i)
        print("=========================================")

        q = msparser.ms_inputquery(resfile, i)
        fmt = "    %-16s: %s"

        print(fmt % ("title"       , q.getStringTitle(1)))
        print(fmt % ("mass_min"    , q.getMassMin()))
        print(fmt % ("mass_max"    , q.getMassMax()))
        print(fmt % ("int_min"     , q.getIntMin()))
        print(fmt % ("int_max"     , q.getIntMax()))
        print(fmt % ("num_vals"    , q.getNumVals()))
        print(fmt % ("num_used1"   , q.getNumUsed()))
        print(fmt % ("ions1"       , q.getStringIons1()))
        print(fmt % ("ions2"       , q.getStringIons2()))
        print(fmt % ("ions3"       , q.getStringIons3()))
        print(fmt % ("peptol"      , q.getPepTol()))
        print(fmt % ("peptol units", q.getPepTolUnits()))
        print(fmt % ("peptol str"  , q.getPepTolString()))
        print(fmt % ("repeat srch" , resfile.getRepeatSearchString(i)))

        num_peaks = q.getNumberOfPeaks(1)
        for j in range (1, 1+ num_peaks) :
            print("%f, %f" % (q.getPeakMass(1, j), q.getPeakIntensity(1, j)))
                
    print(" ")



if __name__ == "__main__" :
    sys.exit(main("data.dat"))