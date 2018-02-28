/*
##############################################################################
# file: config_quantitation.cs                                               #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/config_quantitation.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using matrix_science.msparser;

namespace MsParserExamples
{
    public class config_quantitation
    {
        public static void Main(string[] argv)
        {
            // ----- Object creation -----
            if (argv.Length < 2)
            {
                Console.WriteLine("You must specify quantitation.xml location as the first parameter and the schema path as the second.");
                return;
            }

            ms_quant_configfile file = new ms_quant_configfile(argv[0], argv[1]);
            if (!file.isValid())
            {
                Console.WriteLine("Error number: {0}", file.getLastError());
                Console.WriteLine("Error string: {0}", file.getLastErrorString());
                return;
            }

            Console.WriteLine("There are {0} methods available", file.getNumberOfMethods());
            for (int i = 0; i < file.getNumberOfMethods(); i++)
            {
                Console.WriteLine(file.getMethodByNumber(i).getName());
            }

            // now try to retrieve typeinfo from the schema file
            ms_xml_schema schemaFile = new ms_xml_schema();
            schemaFile.setFileName(argv[1]);
            schemaFile.read_file();
            if (!schemaFile.isValid())
            {
                Console.WriteLine("Error while reading schema-file.  The last error description:");
                Console.WriteLine("Error string: {0}", schemaFile.getLastErrorString());
                return;
            }

            // validate the original document
            string strValidationErrors = file.validateDocument();
            if (strValidationErrors != null && strValidationErrors.Length > 0)
            {
                Console.WriteLine("Initial document validation errors: {0}", strValidationErrors);
            }

            // the document can be validated programmatically
            // but it is not schema-validation
            for (int i = 0; i < file.getNumberOfMethods(); i++)
            {
                // Shallow validation: we check on presence of required attributes and elements
                ms_quant_method pMethod = file.getMethodByNumber(i);
                String strErrors = pMethod.validateShallow(schemaFile);  // pass the schema object as a parameter
                if (strErrors != null && strErrors.Length > 0)
                {
                    Console.WriteLine("Method {0}", pMethod.getName());
                    Console.WriteLine("Shallow validation errors: {0}", strErrors);
                    Console.WriteLine();
                }

                // Deep validation: we check attributes and elements recursively applying basic constrains
                strErrors = pMethod.validateDeep(schemaFile);
                if (strErrors != null && strErrors.Length > 0)
                {
                    Console.WriteLine("Method {0}", pMethod.getName());
                    Console.WriteLine("Deep validation errors: {0}", strErrors);
                    Console.WriteLine();
                }
                    
            }

            // create a new invalid document
            ms_quant_configfile configFileInvalid = new ms_quant_configfile();

            // copy the first method from original file
            ms_quant_method myMethodInvalid = new ms_quant_method(file.getMethodByNumber(0));
            myMethodInvalid.setName("");  // invalid name with less than allowed string length

            // one way to validate an object
            // 1. shallow validation - only presence of attributes and elements is checked
            string strErr1 = myMethodInvalid.validateShallow(schemaFile);
            Console.WriteLine("Shallow validation of a method: {0}", strErr1);

            // 2. deep validation  data type of attributes and elements are checked and a base type too
            string strErr2 = myMethodInvalid.validateDeep(schemaFile);
            Console.WriteLine("Deep validation of a method: {0}", strErr2);

            // another way to validate an object - by using schema-object (but not schema-document)
            // the result is absolutely equivalent to the previous call
            string strErr3 = schemaFile.validateComplexObject(myMethodInvalid, true);   // true for deep validation
            Console.WriteLine("Deep validation using schema-object: {0}", strErr3);

            // one can validate an attribute separately
            // also methods validateSimpleInteger(), validateSimpleDouble() and validateSimpleBool() can be used
            string strErr4 = schemaFile.validateSimpleString(myMethodInvalid.getName(), // attribute value,
                myMethodInvalid.getNameSchemaType()); // attribute data type
            Console.WriteLine("Attribute validation: {0}", strErr4);

            // and finally, the whole document validation
            configFileInvalid.appendMethod(myMethodInvalid);
            string errMethod = configFileInvalid.validateDocument();
            Console.WriteLine("Document validation errors: {0}", errMethod);

            // however, the config-file object itself stays valid
            if (!configFileInvalid.isValid())
            {
                Console.WriteLine("Errors in config-file object.  The last error description:");
                Console.WriteLine(configFileInvalid.getLastErrorString());
            }

            // file is going to be saved even if it is invalid
            // but the errors will be stored in the config-file object and the object will become invalid
            configFileInvalid.setFileName("quantitation_temp.xml"); // give it another name
            configFileInvalid.save_file();
            if (!configFileInvalid.isValid())
            {
                Console.WriteLine("Errors in config-file object.  The last error description:");
                Console.WriteLine(configFileInvalid.getLastErrorString());
            }
        }
    }
}

/*
config_quantitation.exe c:\inetpub\mascot\config\quantitation.xml c:\inetpub\mascot\html\xmlns\schema\quantitation_2\quantitation_2.xsd
Will give the output (depending on your configuration):

There are 32 methods available
None
iTRAQ 4plex
iTRAQ 4plex (protein)
iTRAQ 8plex
TMT 6plex
TMT 2plex
TMT 10plex
DiLeu 4plex
18O multiplex
SILAC K+6 R+6 multiplex
IPTL (Succinyl and IMID) multiplex
ICPL duplex pre-digest [MD]
ICPL duplex post-digest [MD]
ICPL triplex pre-digest [MD]
ICPL quadruplex pre-digest [MD]
18O corrected [MD]
15N Metabolic [MD]
15N + 13C Metabolic [MD]
SILAC K+6 R+10 [MD]
SILAC K+6 R+10 Arg-Pro [MD]
SILAC K+6 R+6 [MD]
SILAC R+6 R+10 [MD]
SILAC K+8 R+10 [MD]
SILAC K+4 K+8 R+6 R+10 [MD]
ICAT ABI Cleavable [MD]
ICAT D8 [MD]
Dimethylation [MD]
NBS Shimadzu [MD]
Acetylation [MD]
Label-free [MD]
Average [MD]
iTRAQ 8plex PXD001253
Shallow validation of a method:
Deep validation of a method: Attribute 'name' -> String is shorter than minLengt
h-limit
Deep validation using schema-object: Attribute 'name' -> String is shorter than
minLength-limit
Attribute validation: String is shorter than minLength-limit
Document validation errors:
*/
