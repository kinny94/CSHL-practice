/*
##############################################################################
# file: tools_treecluster.cs                                                 #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/tools_treecluster.cs,v $                                #
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
    public class tools_treecluster
    {
        public static void Main(string[] argv)
        {
            int rows = 11;
            int cols = 5;
            ms_treecluster tc = new ms_treecluster(rows, cols);

            var data = new vectord();
            var mask = new vectori();

            data.Add(1.623866862); data.Add(-0.052894948); data.Add(1.182692298); data.Add(2.298658316); data.Add(1.13422094);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(0 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.156396617); data.Add(0.0000000000); data.Add(0.521050737); data.Add(1.544979883); data.Add(0.65718266);
            mask.Add(1);           mask.Add(0);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(1 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.523561956); data.Add(-0.017417053); data.Add(1.168000125); data.Add(2.459693903); data.Add(1.308011315);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(2 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.55875743);  data.Add(-0.241270432); data.Add(0.440420721); data.Add(2.427337989); data.Add(1.043344505);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(3 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.449957484); data.Add(-0.169744676); data.Add(0.867896464); data.Add(2.418999465); data.Add(1.171206827);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(4 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.171206827); data.Add(0.0000000000); data.Add(0.854394678); data.Add(2.075532631); data.Add(0.950095094);
            mask.Add(1);           mask.Add(0);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(5 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.361768359); data.Add(-0.120294234); data.Add(0.992043276); data.Add(2.238786860); data.Add(1.090175950);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(6 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.781149852); data.Add(0.0028825090); data.Add(1.079975377); data.Add(2.464929601); data.Add(1.301002256);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(7 , data, mask); data.Clear(); mask.Clear();

            data.Add(-1.227692025);data.Add(-3.522840789); data.Add(-2.434402824);data.Add(-0.873027144);data.Add(-1.977099598);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(8 , data, mask); data.Clear(); mask.Clear();

            data.Add(1.272023189); data.Add(-0.535331733); data.Add(0.608809243); data.Add(2.004681156); data.Add(0.826192536);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1); 
            tc.setRow(9 , data, mask);
            data.Clear(); mask.Clear();

            data.Add(1.069014678); data.Add(-0.623709617); data.Add(0.412510571); data.Add(1.82822536);  data.Add(0.650764559);
            mask.Add(1);           mask.Add(1);            mask.Add(1);           mask.Add(1);           mask.Add(1);

            var weights = new vectord();
            var left = new vectori();
            var right = new vectori();
            var distance = new vectord();

            if (!tc.cluster(ms_treecluster.TREE_CLUSTER_DISTANCE.TCD_EUCLIDEAN, ms_mascotresults.TREE_CLUSTER_METHOD.TCM_PAIRWISE_AVERAGE,
                weights, left, right, distance))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Node\tleft\tright\tdistance\t");
                for (int i = 0; i < left.Count; i++)
                {
                    int _node = -1 - i;
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", _node, left[i], right[i], distance[i]);
                }
            }
        }
    }
}

/*
tools_treecluster.exe
Will give the following output:

Node    left    right   distance
-1      6       4       0.0129355369880599
-2      2       0       0.0135342339743674
-3      7       -2      0.0168450469179865
-4      5       9       0.0227119800299183
-5      -1      -3      0.0358259860933716
-6      -4      1       0.0924477619793738
-7      3       -5      0.098592161220467
-8      -6      -7      0.201808959296598
-9      10      -8      1.88084728645169
-10     8       -9      9.01668349698743
*/
