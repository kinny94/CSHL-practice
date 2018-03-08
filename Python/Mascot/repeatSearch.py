
import msparser
import sys
import subprocess
import re

def main( filepath ) :

    return repeatSearch( filepath )


def repeatSearch(filename) :
    resfile = msparser.ms_mascotresfile(filename)

    if not resfile.isValid() :
        print("Cannot open results file %s : %s" % (filename, resfile.getLastErrorString()))
        return 1

    s = []

    s.append("----12345")
    s.append('Content-Disposition: form-data; name="QUE"')
    s.append('')

    # Parameters section
    sec_params = msparser.ms_mascotresfile.SEC_PARAMETERS
    count = 1
    key = resfile.enumerateSectionKeys(sec_params, count)

    while len(key) > 0 :
        val = resfile.getSectionValueStr(sec_params, key)

        # To search against a different database, add 'and key ne "DB"'
        if len(val) > 0 and key != "INTERMEDIATE" and key != "RULES" and key != "INTERNALS" and key != "SEARCH" :
            s.append("%s=%s" % (key, val))

        count += 1
        key = resfile.enumerateSectionKeys(sec_params, count)

    # To search against a different DB add e.g.
    # s.append("DB=MY_DB")

    # Most flexible to repeat each search as a 'sequence' search.
    s.append("SEARCH=SQ")

    # For ms-ms data, tell nph-mascot where to find the ions data
    s.append("INTERMEDIATE=" + filename)

    # Now the repeat search data.
    for q in range(1, 1 + resfile.getNumQueries()) :
        s.append(resfile.getRepeatSearchString(q))

    # Terminating line for MIME format file
    s.append("----12345--")

    # Start nph-mascot.exe, and redirect the output to tmp.txt.
    try :
        tmp = open("tmp.txt", "w")
    
    except IOError as err:
        errno, strerror = err.args
        print("Cannot open tmp.txt for writing: %s" % strerror)
        return 1
    # Replace 'nph-mascot.exe' with './nph-mascot.exe' in Unix.

    try :
        proc = subprocess.Popen(
                ['./nph-mascot.exe', '4', '-commandline'], 
                stdin=subprocess.PIPE,
                stdout=tmp
                )
    except OSError as e :
        print("Cannot run nph-mascot.exe: %s" % e)
        return 1

    for line in s :
        proc.stdin.write(line.encode('utf-8'))
        proc.stdin.write("\n".encode('utf-8'))
        

    proc.stdin.close()
    proc.wait()

    tmp.close()

    try :
        tmp = open("tmp.txt", "r")
    except IOError as err:
        errno, strerror = err.args
        print("Cannot open tmp.txt for reading: %s" % strerror)
        return 1
        
    while True :
        line = tmp.readline()

        if len(line) == 0 : break

        if re.match('.*SUCCESS.*', line) :
            # Next line contains the results file name
            line = tmp.readline()

            compareResults(resfile, line.rstrip('\n'))
            continue

        if re.match('.*ERROR.*', line) :
            print("Search failed:", line.rstrip('\n'))
            # Print details of error messages

            while len(line) != 0 :
                line = tmp.readline()
                print(line.rstrip('\n'))

            break


def compareResults(originalSearch, repeatedSearchFileName) :
    repeatedSearch = msparser.ms_mascotresfile(repeatedSearchFileName)
    anyReport = 0

    if not repeatedSearch.isValid() :
        print("Invalid repeat search: %s" % repeatedSearch.getLastErrorString())
        return

    if originalSearch.anyPMF() :
        # Use protein summary
        originalResults = msparser.ms_proteinsummary(originalSearch)
        repeatedResults = msparser.ms_proteinsummary(repeatedSearch)

        originalProt = originalResults.getHit(1)
        repeatedProt = repeatedResults.getHit(1)

        if originalProt and repeatedProt :
            diff = repeatedProt.getScore() - originalProt.getScore()

            if diff > 10.0 :
                print("Protein score is %d higher for search" % diff)
                print("%s than %s " % (originalSearch.getFileName(), repeatedSearchFileName))
                

                anyReport = 1
    else :
        #  Use peptide summary
        originalResults = msparser.ms_peptidesummary(originalSearch)
        repeatedResults = msparser.ms_peptidesummary(repeatedSearch)

        # Compare peptide scores
        for q in range(1, 1 + originalSearch.getNumQueries()) :
            pepOriginal = originalResults.getPeptide(q, 1)
            pepRepeated = repeatedResults.getPeptide(q, 1)
            diff = pepRepeated.getIonsScore() - pepOriginal.getIonsScore()

            if diff > 10.0 :
                print("Query %d has score %d higher for search %s than %s" % (q, diff, originalSearch.getFileName(), repeatedSearchFileName))
                

                anyReport = 1

    if not anyReport :
        print("Similar results for %s and %s" % (originalSearch.getFileName(), repeatedSearchFileName))
        


def usage() :
    print("""
Usage: repeat_search.py <results file>

Given an mascot results file name, repeat the search against the same data.

This program must be run in the Mascot Server cgi directory.
""")

if __name__ == "__main__" :
    sys.exit(main("data.dat"))