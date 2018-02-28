/*
##############################################################################
# file: tools_tinycdb.cs                                                     #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/tools_tinycdb.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.IO;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class tools_tinycdb
    {
        public static void Main(string[] argv)
        {
            if (argv.Length < 1)
            {
                Console.WriteLine("The location of the source file for this application (tools_tinycdb.cs) should be specified as a parameter");
                return;
            }
            string version = "1.1";
            // this should return the current executable name and full path
            string sourceFile = System.IO.Path.GetFileName((new tools_tinycdb()).GetType().Assembly.Location);
            string cdbname = sourceFile + ".cdb";
            // this section just creates the object.  it doesn't open a file
            // the 'source file' is (unusually!) the executable for this class, meaning
            // that if this file is changed, the index will be re-created
            ms_tinycdb cdb = new ms_tinycdb(cdbname, version, sourceFile);

            if (!cdb.isValid())
            {
                showErrors(cdb);
                return;
            }

            if (!cdb.openIndexFile(true))
            {
                if (cdb.isPossibleToCreate())
                {
                    Console.WriteLine("Creating file because: {0}", cdb.getLastErrorString());
                    cdb.prepareToCreate();
                    cdb.saveValueForKey("Hello", "world");

                    // it is unusual to use the same key for 2 values, but it can be done:
                    cdb.saveValueForKey("HELP", "me");
                    cdb.saveValueForKey("HELP", "me too");

                    // Example of saving a file offset using the binary offset into the file
                    // with the saveFileOffsetForKey() function.  This example uses the
                    // C# source file as the input
                    // and saves an offset to THIS line!                    
                    using (StreamReader fh = new StreamReader(argv[0]))
                    {
                        long _pos = -1;
                        string line;
                        while ((line = fh.ReadLine()) != null)
                        {
                            if (line.Contains("// and saves an offset to THIS line!"))
                            {
                                break;
                            }
                            // fetch the current position in the file
                            _pos = GetActualPosition(fh);
                        }                        
                        cdb.saveFileOffsetForKey("Line", _pos);                        
                    }

                    cdb.finishCreate();
                }
            }
            if (cdb.isValid())
            {
                Console.WriteLine("Retrieving value for Hello      : {0}", cdb.getValueFromKey("Hello"));
                Console.WriteLine("Retrieving first value for HELP : {0}", cdb.getValueFromKey("HELP", 0));
                Console.WriteLine("Retrieving second value for HELP: {0}", cdb.getValueFromKey("HELP", 1));

                // check saved position index
                long pos = cdb.getFileOffsetFromKey("Line");
                if (pos != -1)
                {
                    using (FileStream fs = new FileStream(argv[0], FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(pos, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            Console.WriteLine("Testing file offset. Offset {0}: {1}", pos, sr.ReadLine());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Target line was not found during .cdb file creation");
                }
            }
            else
            {
                showErrors(cdb);
            }

        }

        // Finds the current position in the underlying Stream for the passed StreamReader
        // See http://stackoverflow.com/questions/5404267/streamreader-and-seeking
        private static long GetActualPosition(StreamReader reader)
        {
            // The current buffer of decoded characters
            char[] charBuffer = (char[])reader.GetType().InvokeMember("charBuffer"
                , System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField
                , null, reader, null);

            // The current position in the buffer of decoded characters
            int charPos = (int)reader.GetType().InvokeMember("charPos"
                , System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField
                , null, reader, null);

            // The number of bytes that the already-read characters need when encoded.
            int numReadBytes = reader.CurrentEncoding.GetByteCount(charBuffer, 0, charPos);

            // The number of encoded bytes that are in the current buffer
            int byteLen = (int)reader.GetType().InvokeMember("byteLen"
                , System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField
                , null, reader, null);

            return reader.BaseStream.Position - byteLen + numReadBytes;
        }

        private static void showErrors(ms_tinycdb cdb)
        {
            ms_errs err = cdb.getErrorHandler();
            for (int i = 1; i <= err.getNumberOfErrors(); i++)
            {
                Console.WriteLine("Error number: {0} ({1} times) : {2}", err.getErrorNumber(i), err.getErrorRepeats(i) + 1, err.getErrorString(i));
            }
        }
    }
}
/*
tools_tinycdb.exe .\tools_tinycdb.cs
Will give the following output:

Creating file because: Cache file tools_tinycdb.exe.cdb is missing or cannot be opened
Retrieving value for Hello      : world
Retrieving first value for HELP : me
Retrieving second value for HELP: me too
Testing file offset. Offset 3060:                     // and saves an offset to THIS line!
*/