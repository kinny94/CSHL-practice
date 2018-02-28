/*
##############################################################################
# file: tools_stats.cs                                                       #
# 'msparser' toolkit example code                                            #
##############################################################################
# COPYRIGHT NOTICE                                                           #
# Copyright 1998-2015 Matrix Science Limited  All Rights Reserved.           #
#                                                                            #
##############################################################################
#    $Source: /vol/cvsroot/parser/examples/test_csharp/tools_stats.cs,v $                                #
#     $Author: patricke $                                                    #
#       $Date: 2015/07/31 14:38:19 $                                         #
#   $Revision: 1.2 $                                                         #
# $NoKeywords::                                                            $ #
##############################################################################
*/

using System;
using System.Collections.Generic;
using matrix_science.msparser;

namespace MsParserExamples
{
    class tools_stats
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("Binomial coefficients for sample size 6:");

            for (int i = 0; i <= 6; i++)
            {
                Console.WriteLine("\t6 choose {0} = {1}", i, ms_quant_stats.binomialCoefficient(6, (uint) i));
            }

            Console.WriteLine("First 10 values of the Poisson (lambda = 1) distribution");
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine("\tPoisson(lambda 1, k {0}) = {1}", i, ms_quant_stats.poissonDensity(1, (uint) i));
            }

            Console.WriteLine("Common values of the standard normal distribution");
            foreach (double t in (new double[] {0.001, 0.01, 0.025, 0.05, 0.1, 0.5, 0.9, 0.95, 0.975, 0.99, 0.999})) {
                // Note that normalCriticalValue() takes and argument between 0.5 and 1.0;
                // that is, it returns only the right tail critical values
                double critval;
                if (t < 0.5)
                {
                    critval = -ms_quant_stats.normalCriticalValue(1d - t);
                }
                else
                {
                    critval = ms_quant_stats.normalCriticalValue(t);
                }
                Console.WriteLine("\tNormal-1({0}) = {1}", t, critval);
            }

            foreach (double t in (new double[] {-3.09, -2.32, -1.96, -1.64, -1.28, 0, 1.28, 1.64, 1.96, 2.32, 3.09}))
            {
                // Note that normalCumulativeProbability() takes and argument between 0 and 4.09; that is
                // it reports only the right tail probability
                double p;
                if (t < 0)
                {
                    p = 0.5 - ms_quant_stats.normalCumulativeProbability(-t);
                }
                else
                {
                    p = 0.5 + ms_quant_stats.normalCumulativeProbability(t);
                }
                Console.WriteLine("\tNormal(x <= {0}) = {1:0.######}", t, p);
            }

            Console.WriteLine("Common values of the chi-squared distribution at 10 degrees of freedom:");
            
            // Note the significance levels the chi-square functions support
            foreach (double t in (new double[] {0.001, 0.01, 0.025, 0.05, 0.1})) {
                double lowcrit = ms_quant_stats.chisqLowerCriticalValue(10, t);
                double upcrit = ms_quant_stats.chisqUpperCriticalValue(10, t);
                Console.WriteLine("\tchi-sq(df 10, sigLevel {0}) = <{1}, {2}>", t, lowcrit, upcrit);
            }

            Console.WriteLine("Common values of the Student's t distribution at 10 degrees of freedom:");

            // Note the significance levels the Student's t function supports.
            foreach (double t in (new double[] {0.001, 0.005, 0.01, 0.025, 0.05, 0.1})) 
            {
                Console.WriteLine("\tt(df 10, sigLevel {0}) = {1}", t, ms_quant_stats.studentsCriticalValue(10, t));
            }

            Console.WriteLine("Common values of the F distribution at 10 and 1 degrees of freedom:");

            // Note the significance levels the F function supports
            foreach(double t in (new double[] {0.01, 0.05, 0.1})) 
            {
                Console.WriteLine("\tF(df1 10, df2 1, sifLevel {0}) = {1}", t, ms_quant_stats.snedecorsCriticalValue(10, 1, t));
            }

            // see Sample struct below
            List<Sample> samples = new List<Sample>();

            // uniform data generated with
            // perl -e 'print join "\n", map {+ rand() } 0 .. 0'
            samples.Add(new Sample("uniform(0,1)",
                        new double[] { 0.400313346021907, 0.635154745166318, 0.795328049493161, 0.465079196585926, 0.709667388012424 ,
                        0.76161797692474, 0.617230566355229, 0.282583560369936, 0.251706165020014, 0.2962014901575},
                        0.5));

            // Normal (5, 2.5) data generated in R with
            // > rnorm(10, 5, 2.5)
            samples.Add(new Sample("normal(5, 2.5)",
                new double[] { 3.1476833, 9.2061900, 2.6079399, 4.4361054, 6.4133568, 4.3745899, 3.5588195, 3.0542840, 0.6643241,4.3697818},
                5));

            // Beta (0.5, 0.5) data generated in R with
            // > rbeta(10, 0.5, 0.5)
            samples.Add(new Sample("beta(0.5, 0.5)",
                new double[] { 0.754152463, 0.008992998, 0.221637853, 0.426146895, 0.656421042, 0.224818243, 0.022050868, 0.539143737 ,
                0.877643550 ,0.128030530 },
                0.5));

            Console.WriteLine();
            foreach (Sample sample in samples)
            {
                vectord vec = sample.Data;
                // Note: vectord does not provided a ToArray method as standard.  The ToArray method used
                // here is an extension implemented in the VectordExtensionMethods class (see below)
                Console.WriteLine("Sample '{0}' = [{1}]", sample.SampleName, string.Join(", ", vec.ToArray()));

                // The sorted sample vector is needed for the Shapiro-Wilk W test
                // it also makes finsing the sample median fast
                vectord vecSorted = sample.SortedData;

                // Auxiliary helper functions:
                Console.WriteLine("\tSum = {0:#.#####}", ms_quant_stats.sum(vec));
                Console.WriteLine("\tSum of squares = {0:#.#####}", ms_quant_stats.sumsq(vec));

                // Note: vectord does not provided a ToArray method as standard.  The ToArray method used
                // here is an extension implemented in the VectordExtensionMethods class (see below)
                Console.WriteLine("\tLog transformed = [{0}]", string.Join(", ", ms_quant_stats.logTransform(vec).ToArray()));
                Console.WriteLine("\tExp transformed = [{0}]", string.Join(", ", ms_quant_stats.expTransform(vec).ToArray()));

                // Basic sample statistics
                double xbar = ms_quant_stats.arithmeticMean(vec);
                double stdev = ms_quant_stats.arithmeticStandardDeviation(vec, xbar);
                double median = ms_quant_stats.sortedMedian(vecSorted);
                // If the vector isn't sorted:
                // double median = ms_quant_stats.unsortedMedian(vec);

                double geombar = ms_quant_stats.geometricMean(vec);
                double geomdev = ms_quant_stats.geometricStandardDeviation(vec, xbar);

                Console.WriteLine("\tArithmetic stdev = {0}", stdev);
                Console.WriteLine("\tArithmetic mean = {0}", xbar);
                Console.WriteLine("\tStandard error of mean = {0}", ms_quant_stats.arithmeticStandardErrorOfMean(stdev, (uint)vec.Count));
                Console.WriteLine("\tMedian = {0}", median);
                Console.WriteLine("\tStandard error of median = {0}", ms_quant_stats.arithmeticStandardErrorOfMedian(stdev, (uint)vec.Count));
                Console.WriteLine("\tGeometric mean = {0}", geombar);
                Console.WriteLine("\tGeometric stdev = {0}", geomdev);

                // Basic distribution tests for normally distributed data:
                Console.WriteLine("\tHypothesis: distribution is N({0}, sigma^2) (mean test). p-value = {1:0.#####}", sample.PopulationMean,
                    ms_quant_stats.calculateNormalMeanPvalue(sample.PopulationMean, xbar, stdev, (uint)vec.Count));

                Console.WriteLine("\tHypothesis: distribution is N({0}, sigma^2) (median test). p-value = {1:0.#####}", sample.PopulationMean,
                    ms_quant_stats.calculateNormalMedianPvalue(sample.PopulationMean, median, stdev, (uint)vec.Count));

                // (We would expect the normal sample to have the highest p-value, i.e.
                // least evidence against the hypothesis, while the uniform and beta
                // distributions should have fairly low p-values.  However, sample size is
                // so small that the tests are not very reliable)

                {
                    double W, p;
                    bool ok = ms_quant_stats.testShapiroWilkW(vecSorted, out W, out p);
                    Console.WriteLine("\tShapiro-Wilk ok = {0}, W = {1}, p-value = {2}", ok, W != 0 ? W.ToString() : "(none)",
                        p != 0 ? p.ToString() : "(none)");
                }

                // outlier detection can be done even when the above tests indicate that the
                // sample is not normally distributed - after outlier removal, the sample may
                // be normally distributed!
                {
                    uint numL, numH;
                    ms_quant_stats.detectOutliers(vecSorted, "auto", 0.05, out numL, out numH);

                    // numL and numH are the number of elements at each end of vecSorted
                    // which seem to be outliers.  That is, elements between 0 and numL-1, and
                    // vecSorted.Count-numH to vecSorted.Count-1.

                    Console.WriteLine("\tOutlier detection('auto',0.05) = <{0}, {1}>", numL,
                        numH);
                }

                Console.WriteLine();
            }

        }

        

    }

    
    public static class VectordExtensionMethods
    {
        // vectord doesn't provide a ToArray method, instead providing a CopyTo method for
        // populating an existing double array.  This extension class adds a
        // ToArray helper method to vectord
        public static double[] ToArray(this vectord vector)
        {
            double[] data = new double[vector.Count];
            vector.CopyTo(data);
            return data;
        }
    }

    struct Sample
    {
        private string Name;
        private double[] data;
        private double PopMean;

        public Sample(string name, double[] data, double popmean)
        {
            this.Name = name;
            this.data = data;
            this.PopMean = popmean;
        }

        public string SampleName
        {
            get
            {
                return Name;
            }
        }

        public double PopulationMean
        {
            get
            {
                return PopMean;
            }
        }

        public vectord Data
        {
            get
            {
                return new vectord(data);
            }
        }

        public vectord SortedData
        {
            get
            {
                // maintain the underlying data array unsorted
                double[] dataCopy = new double[data.Length];
                this.Data.CopyTo(dataCopy);
                Array.Sort(dataCopy);
                return new vectord(dataCopy);
            }
        }
    }
}

/*
tools_stats.exe
Will give the following output

Binomial coefficients for sample size 6:
	6 choose 0 = 1
	6 choose 1 = 6
	6 choose 2 = 15
	6 choose 3 = 20
	6 choose 4 = 15
	6 choose 5 = 6
	6 choose 6 = 1
First 10 values of the Poisson (lambda = 1) distribution
	Poisson(lambda 1, k 1) = 0.367879441171442
	Poisson(lambda 1, k 2) = 0.183939720585721
	Poisson(lambda 1, k 3) = 0.0613132401952405
	Poisson(lambda 1, k 4) = 0.0153283100488101
	Poisson(lambda 1, k 5) = 0.00306566200976203
	Poisson(lambda 1, k 6) = 0.00051094366829367
	Poisson(lambda 1, k 7) = 7.29919526133814E-05
	Poisson(lambda 1, k 8) = 9.12399407667265E-06
	Poisson(lambda 1, k 9) = 1.0137771196303E-06
	Poisson(lambda 1, k 10) = 1.01377711963029E-07
Common values of the standard normal distribution
	Normal-1(0.001) = -3.09
	Normal-1(0.01) = -2.318
	Normal-1(0.025) = -1.959
	Normal-1(0.05) = -1.642
	Normal-1(0.1) = -1.282
	Normal-1(0.5) = 0
	Normal-1(0.9) = 1.282
	Normal-1(0.95) = 1.642
	Normal-1(0.975) = 1.959
	Normal-1(0.99) = 2.318
	Normal-1(0.999) = 3.09
	Normal(x <= -3.09) = 0.001
	Normal(x <= -2.32) = 0.00554
	Normal(x <= -1.96) = 0.025
	Normal(x <= -1.64) = 0.0505
	Normal(x <= -1.28) = 0.10027
	Normal(x <= 0) = 0.5
	Normal(x <= 1.28) = 0.89973
	Normal(x <= 1.64) = 0.9495
	Normal(x <= 1.96) = 0.975
	Normal(x <= 2.32) = 0.99446
	Normal(x <= 3.09) = 0.999
Common values of the chi-squared distribution at 10 degrees of freedom:
	chi-sq(df 10, sigLevel 0.001) = <1.479, 29.588>
	chi-sq(df 10, sigLevel 0.01) = <2.558, 23.209>
	chi-sq(df 10, sigLevel 0.025) = <3.247, 20.483>
	chi-sq(df 10, sigLevel 0.05) = <3.94, 18.307>
	chi-sq(df 10, sigLevel 0.1) = <4.865, 15.987>
Common values of the Student's t distribution at 10 degrees of freedom:
	t(df 10, sigLevel 0.001) = 4.143
	t(df 10, sigLevel 0.005) = 3.169
	t(df 10, sigLevel 0.01) = 2.764
	t(df 10, sigLevel 0.025) = 2.228
	t(df 10, sigLevel 0.05) = 1.812
	t(df 10, sigLevel 0.1) = 1.372
Common values of the F distribution at 10 and 1 degrees of freedom:
	F(df1 10, df2 1, sifLevel 0.01) = 6055.85
	F(df1 10, df2 1, sifLevel 0.05) = 241.882
	F(df1 10, df2 1, sifLevel 0.1) = 60.195

Sample 'uniform(0,1)' = [0.400313346021907, 0.635154745166318, 0.795328049493161, 0.465079196585926, 0.709667388012424, 0.76161797692474, 0.617230566355229, 0.282583560369936, 0.251706165020014, 0.2962014901575]
	Sum = 5.21488
	Sum of squares = 3.10813
	Log transformed = [-0.915507673489646, -0.4538866166025, -0.229000608568736, -0.765547572658224, -0.342958886300134, -0.272310191628172, -0.482512635488649, -1.26378098321243, -1.37949288361674, -1.21671534624448]
	Exp transformed = [1.49229222822126, 1.88731417160841, 2.2151675622189, 1.59214027596837, 2.03331484127, 2.14173870243289, 1.85378698617621, 1.32655261769313, 1.28621804590024, 1.34474108162913]
	Arithmetic stdev = 0.207799545425621
	Arithmetic mean = 0.521488248410716
	Standard error of mean = 0.0657119860292586
	Median = 0.541154881470578
	Standard error of median = 0.0773298651592315
	Geometric mean = 0.480863737139541
	Geometric stdev = 1.56255472576704
	Hypothesis: distribution is N(0.5, sigma^2) (mean test). p-value = 0.75114
	Hypothesis: distribution is N(0.5, sigma^2) (median test). p-value = 0.60747
	Shapiro-Wilk ok = True, W = 0.904955259787869, p-value = 0.248103833545881
	Outlier detection('auto',0.05) = <0, 0>

Sample 'normal(5, 2.5)' = [3.1476833, 9.20619, 2.6079399, 4.4361054, 6.4133568, 4.3745899, 3.5588195, 3.054284, 0.6643241, 4.3697818]
	Sum = 41.83307
	Sum of squares = 222.94057
	Log transformed = [1.14666672193647, 2.21987608389596, 0.958560599320351, 1.48977682935013, 1.85838281560642, 1.47581277827312, 1.26942888874853, 1.11654519526921, -0.408985146179298, 1.47471307651077]
	Exp transformed = [23.2820644952793, 9958.58228914508, 13.5710642846326, 84.4454192814535, 609.93768731081, 79.4072679533238, 35.1217114826069, 21.2059965963608, 1.94317668425472, 79.0263862606734]
	Arithmetic stdev = 2.30795630958355
	Arithmetic mean = 4.18330747
	Standard error of mean = 0.729839867844071
	Median = 3.96430065
	Standard error of median = 0.858875556478903
	Geometric mean = 3.52569572037875
	Geometric stdev = 2.04835540793366
	Hypothesis: distribution is N(5, sigma^2) (mean test). p-value = 0.29211
	Hypothesis: distribution is N(5, sigma^2) (median test). p-value = 0.25861
	Shapiro-Wilk ok = True, W = 0.917418368639979, p-value = 0.335925333025062
	Outlier detection('auto',0.05) = <0, 0>

Sample 'beta(0.5, 0.5)' = [0.754152463, 0.008992998, 0.221637853, 0.426146895, 0.656421042, 0.224818243, 0.022050868, 0.539143737, 0.87764355, 0.12803053]
	Sum = 3.85904
	Sum of squares = 2.3588
	Log transformed = [-0.28216072584468, -4.71130900444498, -1.50671052190953, -0.852971168207217, -0.420952863607245, -1.49246301212003, -3.8144033127847, -0.617773070154802, -0.130514747277869, -2.05548652787785]
	Exp transformed = [2.12580905780365, 1.00903355649617, 1.24811929335097, 1.53134570563969, 1.92788017044885, 1.2520951184563, 1.02229578528984, 1.71453813789789, 2.40522523023951, 1.13658770218992]
	Arithmetic stdev = 0.310837187272829
	Arithmetic mean = 0.3859038179
	Standard error of mean = 0.0982953493262443
	Median = 0.325482569
	Standard error of median = 0.115673967087124
	Geometric mean = 0.204236938593202
	Geometric stdev = 5.41293770078999
	Hypothesis: distribution is N(0.5, sigma^2) (mean test). p-value = 0.27561
	Hypothesis: distribution is N(0.5, sigma^2) (median test). p-value = 0.16565
	Shapiro-Wilk ok = True, W = 0.930482521074392, p-value = 0.452655082151213
	Outlier detection('auto',0.05) = <0, 0>


*/
