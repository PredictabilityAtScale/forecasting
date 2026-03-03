using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;
using System.Xml.Linq;

using FocusedObjective.Common;

namespace FocusedObjective.Distributions
{
    public static class DistributionHelper
    {

        public static double[] ConvertDistributionArguments(string argumentString, int minimumArgsNeeded, XElement errors = null, XElement source = null)
        {
            double[] empty = new double[0];

            if (string.IsNullOrEmpty(argumentString))
            {
                if (errors != null && minimumArgsNeeded > 0)
                    Helper.AddError(errors, ErrorSeverityEnum.Warning, 37, string.Format(Strings.Error37, minimumArgsNeeded), source);
 
                return empty;
            }

            string[] argsAsStrings = argumentString.Trim().Split(new char[] { ',', '|' });

            if (errors != null && argsAsStrings.Length < minimumArgsNeeded)
            {
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Warning, 37, string.Format(Strings.Error37, minimumArgsNeeded), source);
                return empty;
            }

            double temp = 0.0;
            int index = 0;

            var args = from arg in argsAsStrings
                       let valid = double.TryParse(arg, out temp)
                       select new { valid = valid, value = temp, index = index++ };

            var firstError = args.FirstOrDefault(a => a.valid == false);
            if (firstError != null)
            {
                if (errors != null)
                    FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Warning, 38, string.Format(Strings.Error38, firstError.index , "'" + argumentString + "'"), source);
                
                return empty;
            }

            return args.Select(a => a.value).ToArray();
         }

        public static Generator CreateGenerator(string generator)
        {
            Generator gen;

            switch (generator.Trim().ToLower())
            {
                case "mt19937":
                    gen = new MT19937Generator();
                    break;
                case "xordshift128":
                    gen = new XorShift128Generator();
                    break;
                case "alf":
                    gen = new ALFGenerator();
                    break;
                default:
                    gen = new ALFGenerator();
                    break;
            }

            return gen;
        }

        public static Distribution CreateDistribution(DistributionData data, XElement errors = null)
        {
            Distribution result = null;

            Generator gen = DistributionHelper.CreateGenerator(data.Generator);

            switch (data.Shape.ToLower())
            {
                case "beta":
                    result = createBetaDistribution(data, gen, errors);
                    break;

                case "betaprime":
                    result = createBetaPrimeDistribution(data, gen, errors);
                    break;

                case "cauchy":
                    result = createCauchyDistribution(data, gen, errors);
                    break;

                case "chi":
                    result = createChiDistribution(data, gen, errors);
                    break;

                case "chisquare":
                    result = createChiSquareDistribution(data, gen, errors);
                    break;

                case "erlang":
                    result = createErlangDistribution(data, gen, errors);
                    break;

                case "exponential":
                    result = createExponentialDistribution(data, gen, errors);
                    break;

                case "fishersnedecor":
                    result = createFisherSnedecorDistribution(data, gen, errors);
                    break;

                case "fishertippett":
                    result = createFisherTippettDistribution(data, gen, errors);
                    break;

                case "fromsamples":
                    result = createFromSamplesDistribution(data, gen, errors);
                    break;

                case "gamma":
                    result = createGammaDistribution(data, gen, errors);
                    break;

                case "laplace":
                    result = createLaplaceDistribution(data, gen, errors);
                    break;
                
                case "lognormal":
                    result = createLogNormalDistribution(data, gen, errors);
                    break;
                
                case "normal":
                    result = createNormalDistribution(data, gen, errors);
                    break;

                case "pareto":
                    result = createParetoDistribution(data, gen, errors);
                    break;

                case "power":
                    result = createPowerDistribution(data, gen, errors);
                    break;

                case "rayleigh" :
                     result = createRayleighDistribution(data, gen, errors);
                     break;

                case "studentst":
                     result = createStudentsTDistribution(data, gen, errors);
                     break;
                
                case "triangle":
                     result = createTriangleDistribution(data, gen, errors);
                     break;

                case "weibull":
                     result = createWeibullDistribution(data, gen, errors);
                     break;
                
                case "markovchain":
                     result = createMarkovChainDistribution(data, gen, errors);
                     break;

                case "custom":
                     // to support the old custom which is now discrete
                     var testDist = new CustomRangeDistribution();
                     if (testDist.IsValidData(data.Parameters))
                         result = createCustomRangeDistribution(data, gen, errors);
                     else
                         result = createDiscreteDistribution(data, gen, errors);
                     break;

                case "discrete": 
                    result = createDiscreteDistribution(data, gen, errors);
                     break;

                case "customrange":
                     result = createCustomRangeDistribution(data, gen, errors);
                     break;
                
                case "sip":
                     result = createSIPDistribution(data, gen, errors);
                     break;

                case "model":
                case "fromModel":
                     result = createModelDistribution(data, gen, errors);
                     break;

                default: 
                    // uniform
                     result = createContinuousUniformDistribution(data, gen, errors);
                    break;
            }

            if (result != null)
                result.Data = data;

            return result;
        }



        public static DistributionData CreateDefaultWeibull(double low, double high)
        {
            DistributionData data = new DistributionData();

            data.LowBound = low;
            data.HighBound = high;
            data.Location = low;
            data.Shape = "weibull";
            data.Parameters = string.Format("{0},{1}",1.5, (high-low)/3.2) ;
            data.NumberType = DistributionNumberType.Double;

            return data;
        }


        private static Distribution createBetaDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                BetaDistribution dist = new BetaDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidBeta(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Beta", "alpha, beta", "http://en.wikipedia.org/wiki/Beta_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createBetaPrimeDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                BetaPrimeDistribution dist = new BetaPrimeDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidBeta(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "BetaPrime", "alpha, beta", "http://en.wikipedia.org/wiki/Beta_prime_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createCauchyDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                CauchyDistribution dist = new CauchyDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidGamma(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Cauchy", "alpha, gamma", "http://en.wikipedia.org/wiki/Cauchy_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createChiDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 1, errors);

            if (args.Length == 1)
            {
                ChiDistribution dist = new ChiDistribution(gen, (int)Math.Round(args[0], 0));

                if (dist.IsValidAlpha((int)Math.Round(args[0],0)))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Chi", "degrees of freedom (k)", "http://en.wikipedia.org/wiki/Chi_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createChiSquareDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 1, errors);

            if (args.Length == 1)
            {
                ChiSquareDistribution dist = new ChiSquareDistribution(gen, (int)Math.Round(args[0], 0));

                if (dist.IsValidAlpha((int)Math.Round(args[0], 0)))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "ChiSquare", "degrees of freedom (k)", "http://en.wikipedia.org/wiki/Chi-square_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createErlangDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                ErlangDistribution dist = new ErlangDistribution(gen, (int)Math.Round(args[0], 0), args[1]);

                if (dist.IsValidAlpha((int)Math.Round(args[0], 0)) && dist.IsValidLambda(args[1])) 
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Erlang", "alpha, lambda", "http://en.wikipedia.org/wiki/Erlang_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createExponentialDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 1, errors);

            if (args.Length == 1)
            {
                ExponentialDistribution dist = new ExponentialDistribution(gen, args[0]);

                if (dist.IsValidLambda(args[0]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Exponential", "lambda", "http://en.wikipedia.org/wiki/Exponential_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createFisherSnedecorDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                FisherSnedecorDistribution dist = new FisherSnedecorDistribution(gen, (int)Math.Round(args[0], 0), (int)Math.Round(args[1], 0));

                if (dist.IsValidAlpha((int)Math.Round(args[0], 0)) && dist.IsValidBeta((int)Math.Round(args[1], 0)))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Fisher-Snedecor", "alpha, beta", "http://en.wikipedia.org/wiki/F-distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createFisherTippettDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                FisherTippettDistribution dist = new FisherTippettDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidMu(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Fisher-Tippett", "alpha, mu", "http://en.wikipedia.org/wiki/Laplace_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createGammaDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                GammaDistribution dist = new GammaDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidTheta(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Gamma", "alpha, theta", "http://en.wikipedia.org/wiki/Gamma_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createLaplaceDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                LaplaceDistribution dist = new LaplaceDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidMu(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Laplace", "alpha, mu", "http://en.wikipedia.org/wiki/Laplace_distribution"),
                    data.Source);

            return result;
        }
        
        private static Distribution createNormalDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null; 

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                NormalDistribution dist = new NormalDistribution(gen, args[0], args[1]);
                                
                if (dist.IsValidMu(args[0]) && dist.IsValidSigma(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Normal", "mean (mu), standard deviation (sigma)", "http://en.wikipedia.org/wiki/Normal_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createParetoDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                ParetoDistribution dist = new ParetoDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidBeta(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Pareto", "alpha, beta", "http://en.wikipedia.org/wiki/Pareto_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createPowerDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                ParetoDistribution dist = new ParetoDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidBeta(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Power", "alpha, beta", "http://www.xycoon.com/power.htm"),
                    data.Source);

            return result;
        }

        private static Distribution createLogNormalDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;
            
            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                LognormalDistribution dist = new LognormalDistribution(gen, args[0], args[1]);

                if (dist.IsValidMu(args[0]) && dist.IsValidSigma(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "LogNormal", "mean (mu), standard deviation (sigma)", "http://en.wikipedia.org/wiki/Log-normal_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createTriangleDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 3, errors);

            if (args.Length == 3)
            {
                TriangularDistribution dist = new TriangularDistribution(gen, args[0], args[1], args[2]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidBeta(args[1]) && dist.IsValidGamma(args[2]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Triangular", "lowest value (alpha), highest value (beta), mean (gamma)", "http://en.wikipedia.org/wiki/Triangular_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createWeibullDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                WeibullDistribution dist = new WeibullDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidLambda(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Weibull", "alpha, lambda", "http://en.wikipedia.org/wiki/Weibull_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createRayleighDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 1, errors);

            if (args.Length == 1)
            {
                RayleighDistribution dist = new RayleighDistribution(gen, args[0]);

                if (dist.IsValidSigma(args[0]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Rayleigh", "sigma", "http://en.wikipedia.org/wiki/Rayleigh_distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createStudentsTDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 1, errors);

            if (args.Length == 1)
            {
                StudentsTDistribution dist = new StudentsTDistribution(gen, (int)Math.Round(args[0], 0));

                if (dist.IsValidNu((int)Math.Round(args[0], 0)))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Students T", "nu", "http://en.wikipedia.org/wiki/Student%27s_t-distribution"),
                    data.Source);

            return result;
        }

        private static Distribution createMarkovChainDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 1, errors);

            if (args.Length == 1 && !string.IsNullOrEmpty(data.Data))
            {
                MarkovChainDistribution dist = new MarkovChainDistribution(gen);

                dist.Window = (int)Math.Round(args[0]);
                dist.Count = data.Count; 
                dist.Samples = data.Data;

                if (dist.IsValidWindow((int)Math.Round(args[0])) && dist.IsValidSamples(data.Data))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "markovChain", "window size (window)"),
                    data.Source);

            return result;
        }

        private static Distribution createFromSamplesDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            if (!string.IsNullOrEmpty(data.Data))
            {
                FromSamplesDistribution dist = new FromSamplesDistribution(gen);
                dist.Samples = data.Data;
                dist.Count = data.Count;

                if (dist.IsValidSamples(data.Data) && dist.IsValidCount(data.Count))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 56,
                    string.Format(Strings.Error56, "fromSamples", data.Name),
                    data.Source);

            return result;
        }

        private static Distribution createDiscreteDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            if (!string.IsNullOrEmpty(data.Parameters))
            {
                CustomDiscreteDistribution dist = new CustomDiscreteDistribution(gen);
                dist.Count = data.Count;
                dist.DataString = data.Parameters;

                if (dist.IsValidData(data.Parameters) && dist.IsValidCount(data.Count))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "custom", "value,pct,value,pct...(the pct values must sum to 1.0, for example 0.5 = 50%, 0.25 = 25%)", ""),
                    data.Source);

            return result;
        }

        private static Distribution createCustomRangeDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            if (!string.IsNullOrEmpty(data.Parameters))
            {
                CustomRangeDistribution dist = new CustomRangeDistribution(gen);
                dist.Count = data.Count;
                dist.DataString = data.Parameters;

                if (dist.IsValidData(data.Parameters) && dist.IsValidCount(data.Count))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "customrange", "low-bound value,high-bound value, pct,low-bound value,high-bound value, pct,...(the pct values must sum to 1.0, for example 0.5 = 50%, 0.25 = 25%)", ""),
                    data.Source);

            return result;
        }

        private static Distribution createSIPDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            SIPDistribution dist = new SIPDistribution(gen);

            if (dist.IsValidSIPString(data.Data))
            {
                dist.SIPString = data.Data;
                result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "SIP", "A full SIP XML element as per the SIP v2 standard, with at least one value", "http://probabilitymanagement.org/standards.html"),
                    data.Source);

            return result;
        }

        private static Distribution createModelDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            ModelDistribution dist = new ModelDistribution(gen);

            if (dist.IsValidPath(data.Path))
            {
                dist.Path = data.Path;
                result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "Model", "Required the 'path' attribute be set to a valid and accessible model file.", ""),
                    data.Source);

            return result;
        }

        private static Distribution createContinuousUniformDistribution(DistributionData data, Generator gen, XElement errors = null)
        {
            Distribution result = null;

            double[] args = ConvertDistributionArguments(data.Parameters, 2, errors);

            if (args.Length == 2)
            {
                ContinuousUniformDistribution dist = new ContinuousUniformDistribution(gen, args[0], args[1]);

                if (dist.IsValidAlpha(args[0]) && dist.IsValidBeta(args[1]))
                    result = dist;
            }

            if (errors != null && result == null)
                FocusedObjective.Common.Helper.AddError(errors, ErrorSeverityEnum.Error, 40,
                    string.Format(Strings.Error40, "uniform", "minimum value (alpha), maximim value (beta)", "http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29"),
                    data.Source);

            return result;
        }

        public static Distribution Combine(Distribution dist1, Distribution dist2, int numberOfRandomNumbers = 1000)
        {
            if (dist1 == null && dist2 == null)
                return null;

            if (dist1 == null)
                return dist2;

            if (dist2 == null)
                return dist1;

            List<double> randomNumbers = new List<double>(numberOfRandomNumbers * 2);

            for (int i = 0; i < numberOfRandomNumbers; i++)
            {
                randomNumbers.Add(dist1.GetNextDoubleForDistribution());
                randomNumbers.Add(dist2.GetNextDoubleForDistribution());
            }

            FocusedObjective.Simulation.StatisticResults<double> stats =
                new FocusedObjective.Simulation.StatisticResults<double>(randomNumbers);

            return DistributionFromXML(stats.AsXML("stats"));
        }

        public static Distribution DistributionFromXML(XElement xml)
        {
            var distXML = xml.Element("distribution");

            if (distXML != null)
            {
                DistributionData data = new DistributionData();

                data.Name = distXML.Attribute("name").Value;
                data.Shape = distXML.Attribute("shape").Value;
                data.NumberType = distXML.Attribute("numberType").Value == "double" ? DistributionNumberType.Double : DistributionNumberType.Integer;
                data.Parameters = distXML.Attribute("parameters").Value;

                return FocusedObjective.Distributions.DistributionHelper.CreateDistribution(data);
            }
            else
            {
                return null;
            }
        }
    }
}
