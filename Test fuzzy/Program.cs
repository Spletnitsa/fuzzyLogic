using System.Globalization;

namespace Test_fuzzy
{
    internal class Program
    {
        public struct FuzzySet
        {
            public string name { get; }
            public double lowerRange { get; }
            public double upperRange { get; }
            public List<FuzzySubset> MFs { get; }

            public FuzzySet(string nameOfSet, double lower, double upper, List<FuzzySubset> listOfSubset)
            {
                name = nameOfSet;
                lowerRange = lower;
                upperRange = upper;
                MFs = listOfSubset;
            }
        }

        public struct FuzzySubset
        {
            public string name { get; }
            public double S1 { get; }
            public double S2 { get; }
            public double S3 { get; }
            public List<double> objects { get; }
            public List<double> membershipDegree { get; init; }

            public FuzzySubset(string nameOfSubset, double S1, double S2, double S3, double[] listOfObjects)
            {
                name = nameOfSubset;
                this.S1 = S1;
                this.S2 = S2;
                this.S3 = S3;
                objects = new List<double>();
                objects.AddRange(listOfObjects);
                membershipDegree = new List<double>();
            }
        }

        public struct Rule
        {
            public string A { get; }
            public string B { get; }

            public Rule(string IFrule, string THENrule)
            {
                A = IFrule;
                B = THENrule;
            }
        }

        public class FuzzyLog
        {
            private FuzzySet input;
            private FuzzySet output;
            private List<Rule> rules;

            public FuzzyLog(FuzzySet input, FuzzySet output, List<Rule>rules)
            {
                this.input = input;
                this.output = output;
                this.rules = rules;
            }

            public double[] membershipDegreeFunc(FuzzySubset subset)
            {
                double xIncreasing = subset.S2 - subset.S1;
                double xDecreasing = subset.S3 - subset.S2;
                double increasingValuePrice = xIncreasing / 10;
                double decreasingValuePrice = xDecreasing / 10;
                double[] membershipDegree = new double[subset.objects.Count];
                double value;

                for (int i = 0; i < subset.objects.Count; i++)
                {
                    if (subset.objects[i] < subset.S2)
                    {
                        value = (subset.objects[i] - subset.S1) / increasingValuePrice / 10;
                        membershipDegree[i] = value;
                    }
                    else if(subset.objects[i] > subset.S2)
                    {
                        value = 1 - ((subset.objects[i] - subset.S2) / decreasingValuePrice / 10);
                        membershipDegree[i] = value;
                    }
                    else
                        membershipDegree[i] = 1;
                }

                return membershipDegree;
            }
        }

        static void Main(string[] args)
        {
            List<FuzzySubset> subsets1 = new List<FuzzySubset>();

            double[] smallSubsetObjects = new double[] { 10, 15, 25, 40 };
            FuzzySubset small = new FuzzySubset("Малое", 0, 25, 45, smallSubsetObjects);

            double[] bellowAvgSubsetObjects = new double[] { 30, 45, 60, 75 };
            FuzzySubset bellowAvg = new FuzzySubset("Ниже среднего", 25, 45, 75, bellowAvgSubsetObjects);

            double[] midleSubsetObjects = new double[] { 50, 60, 80, 110 };
            FuzzySubset midle = new FuzzySubset("Среднее", 50, 80, 125, midleSubsetObjects);

            double[] aboveAvgSubsetObjects = new double[] { 85, 150, 180, 225 };
            FuzzySubset aboveAvg = new FuzzySubset("Выше среднего", 80, 150, 230, aboveAvgSubsetObjects);

            double[] highSubsetObjects = new double[] { 135, 200, 230, 290 };
            FuzzySubset high = new FuzzySubset("Высокое", 130, 250, 300, highSubsetObjects);

            subsets1.AddRange(new[] { small, bellowAvg, midle, aboveAvg, high});

            FuzzySet input = new FuzzySet("Потребляемая мощность электроэнергии(кВт*час)", 0, 250, subsets1);


            List<FuzzySubset> subsets2 = new List<FuzzySubset>();

            double[] lowSubsetObjects = new double[] { 70, 100, 170, 330 };
            FuzzySubset low = new FuzzySubset("Низкая", 0, 150, 350, lowSubsetObjects);

            double[] bellowNormSubsetObjects = new double[] { 210, 270, 355, 570 };
            FuzzySubset bellowNorm = new FuzzySubset("Ниже нормы", 200, 350, 600, bellowNormSubsetObjects);

            double[] normSubsetObjects = new double[] { 390, 500, 660, 930 };
            FuzzySubset norm = new FuzzySubset("Норма", 350, 600, 1000, normSubsetObjects);

            double[] aboveNormSubsetObjects = new double[] { 670, 800, 1250, 1500 };
            FuzzySubset aboveNorm = new FuzzySubset("Выше нормы", 650, 1000, 1650, aboveNormSubsetObjects);

            double[] highCostSubsetObjects = new double[] { 1400, 1500, 1670, 1790 };
            FuzzySubset highCost = new FuzzySubset("Высокая", 1100, 1500, 1800, highCostSubsetObjects);

            subsets1.AddRange(new[] { low, bellowNorm, norm, aboveNorm, highCost });

            FuzzySet output = new FuzzySet("Стоимость(руб)", 0, 1500, subsets2);

            List<Rule> rules = new List<Rule>();
            Rule rule1 = new Rule("Малое", "Низкая");
            Rule rule2 = new Rule("Ниже среднего", "Ниже нормы");
            Rule rule3 = new Rule("Среднее", "Норма");
            Rule rule4 = new Rule("Выше среднего", "Выше нормы");
            Rule rule5 = new Rule("Высокое", "Высокая");

            rules.AddRange(new[] { rule1, rule2, rule3, rule4, rule5 });

            FuzzyLog fuzzy1 = new FuzzyLog(input, output, rules);

            for (int i = 0; i < input.MFs.Count; i++)
            {
                Console.WriteLine(fuzzy1.membershipDegreeFunc(input.MFs[i])[0]);
            }
        }
    }
}