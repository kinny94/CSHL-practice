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
#    $Archive:: /MowseBranches/ms_mascotresfile_1.2/test_java/test_searchi $ #
#     $Author: yuryr $ #
#       $Date: 2006/10/16 11:52:33 $ #
#   $Revision: 1.4 $ #
# $NoKeywords::                                                            $ #
##############################################################################
*/

import java.util.Date;
import matrix_science.msparser.*;

public class config_quantitation {
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
        if(argv.length < 2) {
            System.out.println("Must specify results quantitation.xml location as the first parameter and schema path as the second.");
            System.exit(0);
        }

        ms_quant_configfile file = new ms_quant_configfile(argv[0], argv[1]);
        if ( !file.isValid() )
        {
            System.out.println("Error number: "+file.getLastError());
            System.out.println("Error string: "+file.getLastErrorString());
            System.exit(0);
        }

        System.out.print("There are " + file.getNumberOfMethods());
        System.out.println(" methods available");
        for (int i=0; i < file.getNumberOfMethods(); i++)
        {
            System.out.println(file.getMethodByNumber(i).getName());
        }

        // now try to retrieve typeinfo from the schema file
        ms_xml_schema schemaFile = new ms_xml_schema();
        schemaFile.setFileName(argv[1]);
        schemaFile.read_file();
        if ( !schemaFile.isValid() )
        {
            System.out.println("Errors while reading schema-file. The last error description:");
            System.out.println("Error string: " + schemaFile.getLastErrorString());
            System.exit(0);
        }

        // validate the original document
        String strValidationErrors = file.validateDocument();
        if ( (strValidationErrors != null) && (strValidationErrors.length() > 0) )
        {
            System.out.println("Initial document validation errors: " + strValidationErrors);
        }

        // the document can be validated programmatically
        // but it is not schema-validation!!!
        for(int i = 0; i < file.getNumberOfMethods(); i++)
        {
            // Shallow validation: we check only presence of required attributes and elements
            ms_quant_method pMethod = file.getMethodByNumber(i);
            String strErrors = pMethod.validateShallow(schemaFile); // pass the schema object as a parameter
            if ( (strErrors != null) && (strErrors.length() > 0) )
            {
                System.out.println("Method " + pMethod.getName());
                System.out.println("Shallow validation errors: " + strErrors);
                System.out.println();
            }

            // Deep validation: we check attributes and elements recursively applying basic constraints
            strErrors = pMethod.validateDeep(schemaFile);
            if ( (strErrors != null) && (strErrors.length() > 0) )
            {
                System.out.println("Method " + pMethod.getName());
                System.out.println("Deep validation errors: " + strErrors);
                System.out.println();
            }
        }

        // create a new invalid document
        ms_quant_configfile configFileInvalid = new ms_quant_configfile();

        // copy the first method from the original file (method "null")
        ms_quant_method myMethodInvalid = new ms_quant_method(file.getMethodByNumber(0));
        myMethodInvalid.setName(""); // invalid name with less than allowed string length

        // one way to validate an object
        // 1. shallow validation - only presence of attributes and elements is checked
        String strErr1 = myMethodInvalid.validateShallow(schemaFile);
        System.out.println("Shallow validation of a method: " + strErr1);

        // 2. deep validation - data types of attributes and elements are checked and a base type too
        String strErr2 = myMethodInvalid.validateDeep(schemaFile);
        System.out.println("Deep validation of a method: " + strErr2);

        // another way to validate an object - by using schema-object (not schema-document!!!)
        // the result is absolutely equivalent to the previous call
        String strErr3 = schemaFile.validateComplexObject(myMethodInvalid, true); // "true" for deep validation
        System.out.println("Deep validation using schema-object: " + strErr3);

        // one can validate an attribute separately
        // also methods validateSimpleInteger(), validateSimpleDouble() and validateSimpleBool() can be used
        String strErr4 = schemaFile.validateSimpleString(myMethodInvalid.getName(), // attribute value
            myMethodInvalid.getNameSchemaType()); // attribute data type

        System.out.println("Attribute validation: " + strErr4);

        // and finally, the whole document validation
        configFileInvalid.appendMethod(myMethodInvalid);
        String errMethod = configFileInvalid.validateDocument();
        System.out.println("Document validation errors: " + errMethod);

        // however, the config-file object itself stays valid
        if ( !configFileInvalid.isValid() )
        {
            System.out.println("Errors in config-file object. The last error description:");
            System.out.println(configFileInvalid.getLastErrorString());
        }

        // file is going to be saved even if its invalid
        // but the errors will be stored in the config-file object and the object will become invalid
        configFileInvalid.setFileName("quantitation_temp.xml"); // give it another name
        configFileInvalid.save_file();
        if ( !configFileInvalid.isValid() )
        {
            System.out.println("Errors in config-file object. The last error description:");
            System.out.println(configFileInvalid.getLastErrorString());
        }
    }
  
}

/*
will give the output: 

C:>java -classpath .;../java/msparser.jar config_quantitation quantitation.xml quantitation_1.xsd
There are 10 methods available
None
iTRAQ
ICAT C+9
ICPL C+6 pre-digest
ICPL C+6 post-digest
SILAC K+6 R+10
18O corrected, single scan
18O simple, single scan
SILAC K+6 R+4 multiplex
15N Metabolic
Shallow validation of a method: 
Deep validation of a method: Attribute 'name' -> String is shorter than minLength-limit
Deep validation using schema-object: Attribute 'name' -> String is shorter than minLength-limit
Attribute validation: String is shorter than minLength-limit
Document validation errors: XML library failure with error: quantitation.xml: line 4, col 240 - Datatype error: Type:InvalidDatatypeValueException, Message:Value '' with length '0' is less than minimum length facet of '1' .
Errors in config-file object. The last error description:
Failed to save quantitation configuration file

*/