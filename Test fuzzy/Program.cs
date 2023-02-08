using System.Globalization;
using System.Net.Http.Headers;

namespace Test_fuzzy
{
        public struct FuzzySet
        {
            public string Name { get; }
            public double LowerRange { get; }
            public double UpperRange { get; }
            public List<FuzzySubset> MFs { get; }

            public FuzzySet(string nameOfSet, double lower, double upper, List<FuzzySubset> listOfSubset)
            {
                Name = nameOfSet;
                LowerRange = lower;
                UpperRange = upper;
                MFs = listOfSubset;
            }
        }

        public struct FuzzySubset
        {
            public string Name { get; }
            public double S1 { get; }
            public double S2 { get; }
            public double S3 { get; }
            public List<double> Objects { get; }
            public List<double> MembershipDegree { get; init; }

            public FuzzySubset(string nameOfSubset, double S1, double S2, double S3, double[] listOfObjects)
            {
                Name = nameOfSubset;
                this.S1 = S1;
                this.S2 = S2;
                this.S3 = S3;
                Objects = new List<double>();
                Objects.AddRange(listOfObjects);
                MembershipDegree = new List<double>();
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
            public FuzzySet Input { get; }
            public FuzzySet Output { get; }
            public List<Rule> Rules { get; }


            public FuzzyLog(FuzzySet input, FuzzySet output, List<Rule>rules)
            {
                this.Input = input;
                this.Output = output;
                this.Rules = rules;

                MembershipDegreeFunc();
            }

            private void MembershipDegreeFunc()
            {
                int iteration = 0;
                FuzzySet set = new FuzzySet();

                while (iteration < 2)
                {
                    if (iteration == 0)
                    {
                        set = Input;
                    }
                    else if (iteration == 1)
                    {
                        set = Output;
                    }

                    for (int i = 0; i < set.MFs.Count; i++)
                    {
                        FuzzySubset subset = set.MFs[i];

                        double xIncreasing = subset.S2 - subset.S1;
                        double xDecreasing = subset.S3 - subset.S2;
                        double increasingValuePrice = xIncreasing / 10;
                        double decreasingValuePrice = xDecreasing / 10;
                        double[] degrees = new double[subset.Objects.Count];
                        double value;

                        for (int j = 0; j < subset.Objects.Count; j++)
                        {
                            if (subset.Objects[j] < subset.S2)
                            {
                                value = (subset.Objects[j] - subset.S1) / increasingValuePrice / 10;
                                degrees[j] = value;
                            }
                            else if(subset.Objects[j] > subset.S2)
                            {
                                value = 1 - ((subset.Objects[j] - subset.S2) / decreasingValuePrice / 10);
                                degrees[j] = value;
                            }
                            else
                                degrees[j] = 1;
                        }

                        if (iteration == 0)
                        {
                            Input.MFs[i].MembershipDegree.AddRange(degrees);
                        }
                        else if (iteration == 1)
                        {
                            Output.MFs[i].MembershipDegree.AddRange(degrees);
                        }
                    }

                    iteration++;
                }
            }

            private double[] ArraysMinValues(double[] implicationArray1, double[] implicationArray2)
            {
                double[] minValue = new double[implicationArray1.Length];

                for (int i = 0; i < implicationArray1.Length; i++)
                {
                    minValue[i] = Math.Min(implicationArray1[i], implicationArray2[i]);
                }

                return minValue;
            }

            private double[] ColumnToArray(double[][] matrix, int rowNumber)
            {
                double[] array = new double[matrix[rowNumber].Length];

                for (int i = 0; i < matrix[rowNumber].Length; i++)
                {
                    array[i] = matrix[i][rowNumber];
                }

                return array;
            }

            public double[][] Implication(int numberRule)
            {
                int lengthA = 0;
                int lengthB = 0;
                int indexA = 0;
                int indexB = 0;

                for (int i = 0; i < Input.MFs.Count; i++)
                {
                    if(Rules[numberRule].A == Input.MFs[i].Name)
                    {
                        lengthA = Input.MFs[i].Objects.Count;
                        indexA = i;
                        break;
                    }
                }

                for (int i = 0; i < Output.MFs.Count; i++)
                {
                    if (Rules[numberRule].B == Output.MFs[i].Name)
                    {
                        lengthB = Output.MFs[i].Objects.Count;
                        indexB = i;
                        break;
                    }
                }

                double[][] matrix = new double[lengthA][];

                for (int i = 0; i < lengthA; i++)
                {
                    matrix[i] = new double[lengthB];
                }

                for (int i = 0; i < matrix.Length; i++)
                {
                    for (int j = 0; j < matrix[i].Length; j++)
                    {
                        matrix[i][j] = Math.Round(Math.Min(Input.MFs[indexA].MembershipDegree[i], Output.MFs[indexB].MembershipDegree[j]),
                            2, MidpointRounding.AwayFromZero);
                    }
                }

                return matrix;
            }

            public double[][] Сonvolution(double[][] implicationMatrix1, double[][] implicationMatrix2)
            {
                double[][] matrix = new double[implicationMatrix1.Length][];

                for (int i = 0; i < matrix.Length; i++)
                {
                    matrix[i] = new double[implicationMatrix2[i].Length];
                }

                for (int i = 0; i < matrix.Length; i++)
                {
                    for (int j = 0; j < matrix[i].Length; j++)
                    {
                        matrix[i][j] = ArraysMinValues(implicationMatrix1[i], ColumnToArray(implicationMatrix2, j)).Max();
                    }
                }

                return matrix;
            }

            public double[][] RuleChange(double[][] implicationMatrix)
            {
                double[][] transposedMatrix = new double[implicationMatrix.Length][];
                for (int i = 0; i < transposedMatrix.Length; i++)
                {
                    transposedMatrix[i] = new double[implicationMatrix[i].Length];
                }

                for (int i = 0; i < implicationMatrix.Length; i++)
                {
                    transposedMatrix[i] = ColumnToArray(implicationMatrix, i);
                }

                return transposedMatrix;
            }

            public double[] Addition(string setName, int subsetNumber)
            {
                double[] toReverse;
                List<double> listHandler;

                switch (setName)
                {
                    case "Потребляемая мощность электроэнергии(кВт*час)":
                        toReverse = new double[Input.MFs[subsetNumber].MembershipDegree.Count];
                        listHandler = Input.MFs[subsetNumber].MembershipDegree;
                        break;
                    case "Стоимость(руб)":
                        toReverse = new double[Output.MFs[subsetNumber].MembershipDegree.Count];
                        listHandler = Output.MFs[subsetNumber].MembershipDegree;
                        break;
                    default:
                        Console.WriteLine("Нет такого множества");
                        toReverse = new double[0];
                        return toReverse;
                }

                for (int i = 0; i < toReverse.Length; i++)
                {
                    toReverse[i] = 1 - listHandler[i];
                }

                return toReverse;
            }

        }
    internal class Program
    {
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

            subsets2.AddRange(new[] { low, bellowNorm, norm, aboveNorm, highCost });

            FuzzySet output = new FuzzySet("Стоимость(руб)", 0, 1500, subsets2);

            List<Rule> rules = new List<Rule>();
            Rule rule1 = new Rule("Малое", "Низкая");
            Rule rule2 = new Rule("Ниже среднего", "Ниже нормы");
            Rule rule3 = new Rule("Среднее", "Норма");
            Rule rule4 = new Rule("Выше среднего", "Выше нормы");
            Rule rule5 = new Rule("Высокое", "Высокая");

            rules.AddRange(new[] { rule1, rule2, rule3, rule4, rule5 });

            FuzzyLog fuzzy1 = new FuzzyLog(input, output, rules);

            double[][] mtx;

            Console.WriteLine("Операция импликации:");

            mtx = fuzzy1.Implication(0);

            for (int i = 0; i < mtx.Length; i++)
            {
                for (int j = 0; j < mtx[i].Length; j++)
                {
                    Console.Write(mtx[i][j] + "\t");
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            Console.WriteLine("Операция свертки:");

            mtx = fuzzy1.Сonvolution(fuzzy1.Implication(0), fuzzy1.Implication(1));

            for (int i = 0; i < mtx.Length; i++)
            {
                for (int j = 0; j < mtx[i].Length; j++)
                {
                    Console.Write(mtx[i][j] + "\t");
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            Console.WriteLine("Операция изменения праивл:");

            mtx = fuzzy1.RuleChange(fuzzy1.Implication(0));

            for (int i = 0; i < mtx.Length; i++)
            {
                for (int j = 0; j < mtx[i].Length; j++)
                {
                    Console.Write(mtx[i][j] + "\t");
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            double[] test;

            test = fuzzy1.Addition("Потребляемая мощность электроэнергии(кВт*час)", 1);

            Console.WriteLine("Значения после операции дополнения: ");
            for (int i = 0; i < test.Length; i++)
            {
                Console.Write(test[i] + "\t");
            }

            Console.WriteLine();

            Console.WriteLine("Изначальные значения: ");
            for (int i = 0; i < fuzzy1.Input.MFs[1].MembershipDegree.Count; i++)
            {
                Console.Write(fuzzy1.Input.MFs[1].MembershipDegree[i] + "\t");
            }
        }
    }
}