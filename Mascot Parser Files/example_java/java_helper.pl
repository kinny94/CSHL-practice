#!/usr/bin/perl
##############################################################################
# file: java_helper.pl                                                       #
# 'msparser' toolkit                                                         #
# Test harness / example code                                                #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2005 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#
# Perl helper cgi script to run security_session.java
#

use strict;
use CGI ':standard';
my %param_hash;
my @param_name = param();
my $param_name;
my $param_hash;

my $thisScript = new CGI;

# identify the host OS
my $os = $^O;
my $jarSep = ";";
if ($os !~ /MSWin32/i) {
    $jarSep = ":";
}

foreach $param_name (@param_name){
  $param_hash{$param_name} =param($param_name);
  $param_hash{$param_name} =~s/\|/-pipe-/;
  $param_hash{$param_name} =~s/\(system\)/SYSTEM/;
  $param_hash{$param_name} =~tr/\(\)a-zA-Z0-9\t\@ _:\-\/\.//cd;   # remove any potentially dodgy characters 
}

my $command = "";
if ($os !~ /MSWin32/i) {
    $command = "LD_LIBRARY_PATH=\"\$LD_LIBRARY_PATH:.\"; export LD_LIBRARY_PATH; ";
}

$command .= "java -cp .".$jarSep."msparser.jar security_session ";

foreach $param_name (@param_name) {
    $command .= " ".$param_name." ".$param_hash{$param_name};
}

if (defined($thisScript->cookie(-name=>'MASCOT_SESSION'))) {
    my $cookieText = $thisScript->cookie(-name=>'MASCOT_SESSION');
    $cookieText = "null" if !$cookieText;
    $command .= " COOKIE_MASCOT_SESSION ".$cookieText
}

if (defined($ENV{'REMOTE_USER'})) {
    $command .= " ENV_ROMOTE_USER ".$ENV{'REMOTE_USER'};
}

system($command);