@echo off
REM ##############################################################################
REM # file: build_all.cmd                                                        #
REM # 'msparser' toolkit                                                         #
REM # Builds all the examples in the example_csharp directory                    #
REM ##############################################################################
REM # COPYRIGHT NOTICE                                                           #
REM # Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
REM #                                                                            #
REM ##############################################################################
REM #    $Source: /vol/cvsroot/parser/examples/test_csharp/build_all.cmd,v $ #
REM #     $Author: davidc $ #
REM #       $Date: 2015/09/16 15:45:39 $ #
REM #   $Revision: 1.2 $ #
REM # $NoKeywords::                                                            $ #
REM ##############################################################################

for %%f IN (*.cs) do call csc.exe /r:..\bin\matrix_science.msparser.dll %%f