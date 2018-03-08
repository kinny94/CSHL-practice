import msparser
import sys
import os.path

def main( filepath ) :

    createMGF( filepath )


def createMGF(filename) :
    resfile = msparser.ms_mascotresfile(filename)

    if not resfile.isValid() :
        print("Cannot open results file %s : %s" % (filename, resfile.getLastErrorString()))
        return
    
    output_filename = filename + '.mgf'

    if os.path.isfile(output_filename) :
        print("%s already exists; will not overwrite" % output_filename)
        return
    
    try :
        fh = open(output_filename, "w") 
        
    except IOError as err:
        errno, strerror = err.args
        print("Cannot open output_filename for writing: I/O error(%d): %s" % (
                errno, strerror
                ))
        sys.exit(1)

    for q in range(1, 1 + resfile.getNumQueries()) :
        inp_query = msparser.ms_inputquery(resfile, q)

        if inp_query.getNumberOfPeaks(1) == 0 :
            # PMF - just the mass
            fh.write("%f\n" % resfile.getObservedMass(q))
            continue

        fh.write("BEGIN_IONS\n");
        fh.write("PEPMASS=%f\n" % resfile.getObservedMass(q))

        if resfile.getObservedCharge(q) > 0 :
            fh.write("CHARGE=%f+\n" % resfile.getObservedCharge(q))
        else :
            fh.write("CHARGE=Mr\n")

        if inp_query.getStringTitle(1) :
            fh.write("TITLE=%s\n" % inp_query.getStringTitle(1))
        
        for i in range(1, 1 + inp_query.getNumberOfPeaks(1)) :
            fh.write("%f %f\n" % (inp_query.getPeakMass(1, i), inp_query.getPeakIntensity(1, i)))
        
        fh.write("END IONS\n")

    fh.close()
    

def usage() :
    print("""
Usage: create_mgf.py <results file>

Given a mascot results file name, create an MGF file. The MGF file
will be named <results file>.mgf in the same directory where <results file>
is located.
""")

if __name__ == "__main__" :
    sys.exit(main("data.dat"))