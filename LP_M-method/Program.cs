using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace LP_M_method
{
    public class Fraction
    {
        public Int64 Numerator;
        public Int64 Denominator;
        public Fraction(double f, Int64 MaximumDenominator = 4096)
        {
            /* Translated from the C version. */
            /*  a: continued fraction coefficients. */
            Int64 a;
            var h = new Int64[3] { 0, 1, 0 };
            var k = new Int64[3] { 1, 0, 0 };
            Int64 x, d, n = 1;
            int i, neg = 0;

            if (MaximumDenominator <= 1)
            {
                Denominator = 1;
                Numerator = (Int64)f;
                return;
            }

            if (f < 0) { neg = 1; f = -f; }

            while (f != Math.Floor(f)) { n <<= 1; f *= 2; }
            d = (Int64)f;

            /* continued fraction and check denominator each step */
            for (i = 0; i < 64; i++)
            {
                a = (n != 0) ? d / n : 0;
                if ((i != 0) && (a == 0)) break;

                x = d; d = n; n = x % n;

                x = a;
                if (k[1] * a + k[0] >= MaximumDenominator)
                {
                    x = (MaximumDenominator - k[0]) / k[1];
                    if (x * 2 >= a || k[1] >= MaximumDenominator)
                        i = 65;
                    else
                        break;
                }

                h[2] = x * h[1] + h[0]; h[0] = h[1]; h[1] = h[2];
                k[2] = x * k[1] + k[0]; k[0] = k[1]; k[1] = k[2];
            }
            Denominator = k[1];
            Numerator = neg != 0 ? -h[1] : h[1];
        }
        public override string ToString()
        {
            if (Denominator == 1)
                return string.Format("{0}", Numerator);
            else
                return string.Format("({0} / {1})", Numerator, Denominator);
        }
    }

    enum FunctionLimit
    {
        Min,
        Max
    }

    class LpTask
    {
        public int[] FunctionZ { get; set; }
        public int[] FunctionM { get; set; }
        public FunctionLimit Limit { get; set; }
        public double[,] System { get; set; }
        public double[,] SimplexTable { get; set; }
        public int SystemBasisOffset { get; set; }
    }

    class Program
    {
        public static void PrintMatrix(double[,] matrix)
        {
            var matrixRows = matrix.GetLength(0);
            var matrixCols = matrix.GetLength(1);

            Console.WriteLine("System:");

            for (var currentRow = 0; currentRow < matrixRows; currentRow++)
            {
                for (var currentCol = 0; currentCol < matrixCols; currentCol++)
                    Console.Write("[{0,5:F2}]", matrix[currentRow, currentCol]);
                Console.Write('\n');
            }
            Console.Write('\n');
        }

        public static void PrintSystem(double[,] matrix)
        {
            var matrixRows = matrix.GetLength(0);
            var matrixCols = matrix.GetLength(1);

            Console.WriteLine("System:");

            for (var currentRow = 0; currentRow < matrixRows; currentRow++)
            {
                for (var currentCol = 0; currentCol < matrixCols; currentCol++)
                    if (currentCol < matrixCols - 1)
                    {
                        Console.Write("{0}[x{1}] ", matrix[currentRow, currentCol], currentCol + 1);
                    }
                    else
                    {
                        Console.Write("= {0}", matrix[currentRow, currentCol]);
                    }
                Console.Write('\n');
            }
            Console.Write('\n');
        }

        public static void PrintSimplexTable(double[,] simplexTable)
        {
            var simplexTableRows = simplexTable.GetLength(0);
            var simplexTableCols = simplexTable.GetLength(1);

            Console.WriteLine("Simplex Table:");

            //Print Top Headers
            Console.Write(new string("B.V.").PadRight(7));
            Console.Write("|     1     |");
            for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                Console.Write(new string($"[x{currentCol}]").PadRight(13));
            Console.Write('\n');

            //Print Basis variables
            for (var currentRow = 0; currentRow < simplexTableRows - 2; currentRow++)
            {
                //Print B.V.
                Console.Write("|      |");

                //Print 1
                //Console.Write("{0,7:F2}|", simplexTable[currentRow, 0]);
                Console.Write("{0,11:F2}|", new Fraction(simplexTable[currentRow, 0], simplexTable[currentRow, 0] >= 2 ? 65536 : 4096));

                //Print Xi
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                    Console.Write("[{0,11:F2}]", new Fraction(simplexTable[currentRow, currentCol], simplexTable[currentRow, currentCol] >= 2 ? 65536 : 4096));
                    //Console.Write("[{0,7:F2}]", simplexTable[currentRow, currentCol]);
                Console.Write('\n');
            }
            Console.Write('\n');

            //Print Z row
            {
                //Print B.V.
                Console.Write(new string("Z").PadRight(7));
                //Print 1
                //Console.Write("|{0,7:F2}|", simplexTable[simplexTableRows - 2, 0]);
                Console.Write("|{0,11:F2}|", new Fraction(simplexTable[simplexTableRows - 2, 0], simplexTable[simplexTableRows - 2, 0] >= 2 ? 65536 : 4096));
                //Print Xi
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                    Console.Write("[{0,11:F2}]", new Fraction(simplexTable[simplexTableRows - 2, currentCol], simplexTable[simplexTableRows - 2, currentCol] >= 2 ? 65536 : 4096));
                    //Console.Write("[{0,7:F2}]", simplexTable[simplexTableRows - 2, currentCol]);
                Console.Write('\n');
            }

            //Print M row
            {
                //Print B.V.
                Console.Write(new string("M").PadRight(7));
                //Print 1
                //Console.Write("|{0,7:F2}|", simplexTable[simplexTableRows - 1, 0]);
                Console.Write("|{0,11:F2}|", new Fraction(simplexTable[simplexTableRows - 1, 0], simplexTable[simplexTableRows - 1, 0] >= 2 ? 65536 : 4096));
                //Print Xi
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                    Console.Write("[{0,11:F2}]", new Fraction(simplexTable[simplexTableRows - 1, currentCol], simplexTable[simplexTableRows - 1, currentCol] >= 2 ? 65536 : 4096));
                    //Console.Write("[{0,7:F2}]", simplexTable[simplexTableRows - 1, currentCol]);
                Console.Write('\n');
            }
        }

        public static void PrintFunctionZ(int[] functionZ, FunctionLimit limit)
        {
            var functionZCols = functionZ.GetLength(0);

            Console.WriteLine("FunctionZ:");
            Console.Write("Z = ");

            for (var currentCol = 0; currentCol < functionZCols; currentCol++)
                Console.Write("{0}[x{1}] ", functionZ[currentCol], currentCol + 1);
            Console.Write(limit == FunctionLimit.Min ? "-> min" : "-> max");

            Console.Write("\n\n");
        }

        public static void PrintFunctionZM(int[] functionZ, FunctionLimit limit, int[] functionM)
        {
            var functionZCols = functionZ.GetLength(0);
            var functionMCols = functionM.GetLength(0);

            Console.WriteLine("FunctionZM:");
            Console.Write("Z = ");

            for (var currentCol = 0; currentCol < functionZCols; currentCol++)
                Console.Write("{0}[x{1}] ", functionZ[currentCol], currentCol + 1);
            Console.Write(limit == FunctionLimit.Min ? "+ M( " : "- M( ");

            for (var currentCol = 0; currentCol < functionMCols; currentCol++)
                Console.Write("{0}[x{1}] ", functionM[currentCol], functionZCols + currentCol + 1);

            Console.Write(limit == FunctionLimit.Min ? ") -> min" : ") -> max");

            Console.Write("\n\n");
        }

        public static List<int> GetBasisRowsForSystem(double[,] matrix, int basisCheckOffsetCol)
        {
            var matrixRows = matrix.GetLength(0);
            var matrixCols = matrix.GetLength(1) - 1;

            var basisRows = new List<int>();

            for (var currentCol = basisCheckOffsetCol; currentCol < matrixCols; currentCol++)
            {
                int oneCount = 0;
                int zeroCount = 0;
                int basisRow = -1;
                for (var currentRow = 0; currentRow < matrixRows; currentRow++)
                {
                    if (matrix[currentRow, currentCol] == 0)
                        zeroCount++;
                    else if (matrix[currentRow, currentCol] == 1)
                    {
                        oneCount++;
                        basisRow = currentRow;
                    }
                }

                //Console.WriteLine("Zeros: {0}, Ones: {1}, Matrix Rows: {2}", zeroCount, oneCount, matrixRows);

                if (zeroCount == matrixRows - 1 && oneCount == 1 && basisRow > -1)
                    basisRows.Add(basisRow);
            }

            return basisRows;
        }

        public static List<int> GetBasisColsForSimplexTable(double[,] simplexTable)
        {
            var simplexTableRows = simplexTable.GetLength(0);
            var simplexTableCols = simplexTable.GetLength(1);

            var basisRows = new List<int>();

            for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
            {
                int oneCount = 0;
                int zeroCount = 0;
                int basisCol = -1;
                for (var currentRow = 0; currentRow < simplexTableRows; currentRow++)
                {
                    if (simplexTable[currentRow, currentCol] == 0)
                        zeroCount++;
                    else if (simplexTable[currentRow, currentCol] == 1)
                    {
                        oneCount++;
                        basisCol = currentCol;
                    }
                }

                //Console.WriteLine("Zeros: {0}, Ones: {1}, Matrix Rows: {2}", zeroCount, oneCount, matrixRows);

                if (zeroCount == simplexTableRows - 1 && oneCount == 1 && basisCol > -1)
                    basisRows.Add(basisCol);
            }

            return basisRows;
        }

        public static bool SimplexStepM(ref double[,] simplexTable, FunctionLimit limit)
        {
            var simplexTableRows = simplexTable.GetLength(0);
            var simplexTableCols = simplexTable.GetLength(1);

            if (limit == FunctionLimit.Max)
            {
                var isNotOptimal = false;
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 1, currentCol] < 0)
                        isNotOptimal = true;
                }

                if (!isNotOptimal)
                    return true;

                int stepRow = -1;
                int stepCol = -1;
                double stepColMin = Double.MaxValue;
                double stepRowMin = Double.MaxValue;

                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 1, currentCol] < stepColMin)
                    {
                        stepColMin = simplexTable[simplexTableRows - 1, currentCol];
                        stepCol = currentCol;
                    }
                }

                var simplexCoefs = new double[simplexTableRows - 2];
                var simplexCoefsRows = simplexCoefs.GetLength(0);

                Console.WriteLine("Co:");
                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (Math.Abs(simplexTable[currentRow, stepCol]) < 0.001 || simplexTable[currentRow, stepCol] < 0)
                        simplexCoefs[currentRow] = double.MaxValue;
                    else
                        simplexCoefs[currentRow] = Math.Abs(simplexTable[currentRow, 0] / simplexTable[currentRow, stepCol]);

                    Console.WriteLine("{0} / {1} = {2}", simplexTable[currentRow, 0], simplexTable[currentRow, stepCol], simplexCoefs[currentRow]);
                }

                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (simplexCoefs[currentRow] < stepRowMin)
                    {
                        stepRowMin = simplexCoefs[currentRow];
                        stepRow = currentRow;
                    }
                }

                PrintSimplexTable(simplexTable);
                Console.WriteLine("Step Column: {0}", stepCol);
                Console.WriteLine("Step Row: {0}", stepRow + 1);

                //Optimize Simplex Matrix
                {
                    var elementRow = stepRow;
                    var elementCol = stepCol;
                    var element = simplexTable[elementRow, elementCol];

                    //Calculate other elements
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                        {
                            for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                            {
                                if (currentCol != elementCol)
                                {
                                    //Console.WriteLine($"{simplexMatrix[currentRow, currentCol]}[{currentRow}{currentCol}] -= ({simplexMatrix[elementRow, currentCol]}[{elementRow}{currentCol}] * {simplexMatrix[currentRow, elementCol]}[{currentRow}{elementCol}]) / {element}[{elementRow}{elementCol}]");
                                    simplexTable[currentRow, currentCol] -= (simplexTable[elementRow, currentCol] * simplexTable[currentRow, elementCol]) / element;
                                }
                            }
                        }
                    }

                    //Divide current line by element
                    for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                    {
                        simplexTable[elementRow, currentCol] /= element;
                    }

                    //Zerofill current column but element
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                            simplexTable[currentRow, elementCol] = 0;
                    }
                }
            }
            else
            {
                var isNotOptimal = false;
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 1, currentCol] > 0)
                        isNotOptimal = true;
                }

                if (!isNotOptimal)
                    return true;

                int stepRow = -1;
                int stepCol = -1;
                double stepColMax = Double.MinValue;
                double stepRowMin = Double.MaxValue;

                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 1, currentCol] > stepColMax)
                    {
                        stepColMax = simplexTable[simplexTableRows - 1, currentCol];
                        stepCol = currentCol;
                    }
                }

                var simplexCoefs = new double[simplexTableRows - 2];
                var simplexCoefsRows = simplexCoefs.GetLength(0);

                Console.WriteLine("Co:");
                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (Math.Abs(simplexTable[currentRow, stepCol]) < 0.001 || simplexTable[currentRow, stepCol] < 0)
                        simplexCoefs[currentRow] = double.MaxValue;
                    else
                        simplexCoefs[currentRow] = Math.Abs(simplexTable[currentRow, 0] / simplexTable[currentRow, stepCol]);

                    Console.WriteLine("{0} / {1} = {2}", simplexTable[currentRow, 0], simplexTable[currentRow, stepCol], simplexCoefs[currentRow]);
                }

                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (simplexCoefs[currentRow] < stepRowMin)
                    {
                        stepRowMin = simplexCoefs[currentRow];
                        stepRow = currentRow;
                    }
                }

                PrintSimplexTable(simplexTable);
                Console.WriteLine("Step Column: {0}", stepCol);
                Console.WriteLine("Step Row: {0}", stepRow + 1);

                //Optimize Simplex Matrix
                {
                    var elementRow = stepRow;
                    var elementCol = stepCol;
                    var element = simplexTable[elementRow, elementCol];

                    //Calculate other elements
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                        {
                            for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                            {
                                if (currentCol != elementCol)
                                {
                                    //Console.WriteLine($"{simplexMatrix[currentRow, currentCol]}[{currentRow}{currentCol}] -= ({simplexMatrix[elementRow, currentCol]}[{elementRow}{currentCol}] * {simplexMatrix[currentRow, elementCol]}[{currentRow}{elementCol}]) / {element}[{elementRow}{elementCol}]");
                                    simplexTable[currentRow, currentCol] -= (simplexTable[elementRow, currentCol] * simplexTable[currentRow, elementCol]) / element;
                                }
                            }
                        }
                    }

                    //Divide current line by element
                    for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                    {
                        simplexTable[elementRow, currentCol] /= element;
                    }

                    //Zerofill current column but element
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                            simplexTable[currentRow, elementCol] = 0;
                    }
                }
            }

            return false;

            throw new NotImplementedException();
        }

        public static bool SimplexStepZ(ref double[,] simplexTable, FunctionLimit limit)
        {
            var simplexTableRows = simplexTable.GetLength(0);
            var simplexTableCols = simplexTable.GetLength(1);

            if (limit == FunctionLimit.Max)
            {
                var isNotOptimal = false;
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 2, currentCol] < 0 && simplexTable[simplexTableRows - 1, currentCol] == 0)
                        isNotOptimal = true;
                }

                if (!isNotOptimal)
                    return true;

                int stepRow = -1;
                int stepCol = -1;
                double stepColMax = Double.MinValue;
                double stepRowMin = Double.MaxValue;

                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 2, currentCol] > stepColMax && simplexTable[simplexTableRows - 2, currentCol] < 0 && simplexTable[simplexTableRows - 1, currentCol] == 0)
                    {
                        stepColMax = simplexTable[simplexTableRows - 2, currentCol];
                        stepCol = currentCol;
                    }
                }

                var simplexCoefs = new double[simplexTableRows - 2];
                var simplexCoefsRows = simplexCoefs.GetLength(0);

                Console.WriteLine("Co:");
                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (Math.Abs(simplexTable[currentRow, stepCol]) < 0.001 || simplexTable[currentRow, stepCol] < 0)
                        simplexCoefs[currentRow] = double.MaxValue;
                    else
                        simplexCoefs[currentRow] = Math.Abs(simplexTable[currentRow, 0] / simplexTable[currentRow, stepCol]);

                    Console.WriteLine("{0} / {1} = {2}", simplexTable[currentRow, 0], simplexTable[currentRow, stepCol], simplexCoefs[currentRow]);
                }

                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (simplexCoefs[currentRow] < stepRowMin)
                    {
                        stepRowMin = simplexCoefs[currentRow];
                        stepRow = currentRow;
                    }
                }

                PrintSimplexTable(simplexTable);
                Console.WriteLine("Step Column: {0}", stepCol);
                Console.WriteLine("Step Row: {0}", stepRow + 1);

                //Optimize Simplex Matrix
                {
                    var elementRow = stepRow;
                    var elementCol = stepCol;
                    var element = simplexTable[elementRow, elementCol];

                    //Calculate other elements
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                        {
                            for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                            {
                                if (currentCol != elementCol)
                                {
                                    //Console.WriteLine($"{simplexMatrix[currentRow, currentCol]}[{currentRow}{currentCol}] -= ({simplexMatrix[elementRow, currentCol]}[{elementRow}{currentCol}] * {simplexMatrix[currentRow, elementCol]}[{currentRow}{elementCol}]) / {element}[{elementRow}{elementCol}]");
                                    simplexTable[currentRow, currentCol] -= (simplexTable[elementRow, currentCol] * simplexTable[currentRow, elementCol]) / element;
                                }
                            }
                        }
                    }

                    //Divide current line by element
                    for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                    {
                        simplexTable[elementRow, currentCol] /= element;
                    }

                    //Zerofill current column but element
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                            simplexTable[currentRow, elementCol] = 0;
                    }
                }
            }
            else
            {
                var isNotOptimal = false;
                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 2, currentCol] > 0 && simplexTable[simplexTableRows - 1, currentCol] == 0)
                        isNotOptimal = true;
                }

                if (!isNotOptimal)
                    return true;

                int stepRow = -1;
                int stepCol = -1;
                double stepColMin = Double.MaxValue;
                double stepRowMin = Double.MaxValue;

                for (var currentCol = 1; currentCol < simplexTableCols; currentCol++)
                {
                    if (simplexTable[simplexTableRows - 2, currentCol] < stepColMin && simplexTable[simplexTableRows - 2, currentCol] > 0 && simplexTable[simplexTableRows - 1, currentCol] == 0)
                    {
                        stepColMin = simplexTable[simplexTableRows - 2, currentCol];
                        stepCol = currentCol;
                    }
                }

                var simplexCoefs = new double[simplexTableRows - 2];
                var simplexCoefsRows = simplexCoefs.GetLength(0);

                Console.WriteLine("Co:");
                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (Math.Abs(simplexTable[currentRow, stepCol]) < 0.001 || simplexTable[currentRow, stepCol] < 0)
                        simplexCoefs[currentRow] = double.MaxValue;
                    else
                        simplexCoefs[currentRow] = Math.Abs(simplexTable[currentRow, 0] / simplexTable[currentRow, stepCol]);

                    Console.WriteLine("{0} / {1} = {2}", simplexTable[currentRow, 0], simplexTable[currentRow, stepCol], simplexCoefs[currentRow]);
                }

                for (var currentRow = 0; currentRow < simplexCoefsRows; currentRow++)
                {
                    if (simplexCoefs[currentRow] < stepRowMin)
                    {
                        stepRowMin = simplexCoefs[currentRow];
                        stepRow = currentRow;
                    }
                }

                PrintSimplexTable(simplexTable);
                Console.WriteLine("Step Column: {0}", stepCol);
                Console.WriteLine("Step Row: {0}", stepRow + 1);

                //Optimize Simplex Matrix
                {
                    var elementRow = stepRow;
                    var elementCol = stepCol;
                    var element = simplexTable[elementRow, elementCol];

                    //Calculate other elements
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                        {
                            for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                            {
                                if (currentCol != elementCol)
                                {
                                    //Console.WriteLine($"{simplexMatrix[currentRow, currentCol]}[{currentRow}{currentCol}] -= ({simplexMatrix[elementRow, currentCol]}[{elementRow}{currentCol}] * {simplexMatrix[currentRow, elementCol]}[{currentRow}{elementCol}]) / {element}[{elementRow}{elementCol}]");
                                    simplexTable[currentRow, currentCol] -= (simplexTable[elementRow, currentCol] * simplexTable[currentRow, elementCol]) / element;
                                }
                            }
                        }
                    }

                    //Divide current line by element
                    for (int currentCol = 0; currentCol < simplexTableCols; currentCol++)
                    {
                        simplexTable[elementRow, currentCol] /= element;
                    }

                    //Zerofill current column but element
                    for (int currentRow = 0; currentRow < simplexTableRows; currentRow++)
                    {
                        if (currentRow != elementRow)
                            simplexTable[currentRow, elementCol] = 0;
                    }
                }
            }

            return false;

            throw new NotImplementedException();
        }

        public static void OptimizeSimplexTable(ref double[,] simplexTable)
        {
            var simplexTableRows = simplexTable.GetLength(0);
            var simplexTableCols = simplexTable.GetLength(1);

            for (var currentRow = 0; currentRow < simplexTableRows; currentRow++)
            {
                for (var currentCol = 0; currentCol < simplexTableCols; currentCol++)
                {
                    if (Math.Abs(simplexTable[currentRow, currentCol]) < 0.001)
                    {
                        simplexTable[currentRow, currentCol] = 0.0;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            //NUMBER 3
            // var myTask = new LpTask
            // {
            //     FunctionZ = new[] { 2, -1, -1, -1 },
            //     Limit = FunctionLimit.Max,
            //     System = new double[,]
            //     {
            //         { 1, 1, 2, -1, 2 },
            //         { 2, 1, -3, 1, 6 },
            //         { 1, 1, 1, 1, 7 }
            //     },
            //     SystemBasisOffset = 0
            // };

            //NUMBER 4
            // var myTask = new LpTask
            // {
            //     FunctionZ = new[] { 1, 1, -1, -2, 0 },
            //     Limit = FunctionLimit.Min,
            //     System = new double[,]
            //     {
            //         { -1, 2, 0, -1, 0, 3 },
            //         { 0, 0, 1, -2, 0, 2 },
            //         { 0, 3, 0, -1, 1, 5 }
            //     },
            //     SystemBasisOffset = 4
            // };

            //NUMBER 5
            // var myTask = new LpTask
            // {
            //     FunctionZ = new[] { -1, 2, -3 },
            //     Limit = FunctionLimit.Max,
            //     System = new double[,]
            //     {
            //         { -2, 1, 3, 2 },
            //         { 2, 3, 4, 1 }
            //     },
            //     SystemBasisOffset = 0
            // };

            //NUMBER 6
            // var myTask = new LpTask
            // {
            //     FunctionZ = new[] { 1,5,3,3 },
            //     Limit = FunctionLimit.Max,
            //     System = new double[,]
            //     {
            //         {4,4,3,-3,12},
            //         {2,7,5,6,66}
            //     },
            //     SystemBasisOffset = 0
            // };

            // COURSE WORK
            var myTask = new LpTask
            {
                FunctionZ = new[] { 1, 5, 0, 0, 0 },
                Limit = FunctionLimit.Min,
                System = new double[,]
                {
                    { 4, 1, -1, 0, 0, 9 },
                    { 3, 2, 0, -1, 0, 13 },
                    { 2, 5, 0, 0, -1, 6 }
                },
                SystemBasisOffset = 0
            };

            if (myTask.Limit == FunctionLimit.Max)
            {
                PrintFunctionZ(myTask.FunctionZ, myTask.Limit);
                PrintSystem(myTask.System);

                var basisRows = GetBasisRowsForSystem(myTask.System, myTask.SystemBasisOffset); //.ForEach(x => Console.WriteLine("{0}", x))

                var matrixTableRows = myTask.System.GetLength(0);
                var matrixTableCols = myTask.System.GetLength(1);
                var simplexTableRows = matrixTableRows;
                var simplexTableCols = matrixTableCols;
                var simplexTableAddCols = simplexTableRows - basisRows.Count;

                simplexTableCols += simplexTableAddCols;

                myTask.SimplexTable = new double[simplexTableRows + 2, simplexTableCols];

                //Copy System to Simplex Table
                for (var currentRow = 0; currentRow < myTask.System.GetLength(0); currentRow++)
                    for (var currentCol = 0; currentCol < myTask.System.GetLength(1) - 1; currentCol++)
                        myTask.SimplexTable[currentRow, currentCol + 1] = myTask.System[currentRow, currentCol];

                for (var currentRow = 0; currentRow < myTask.System.GetLength(0); currentRow++)
                    myTask.SimplexTable[currentRow, 0] = myTask.System[currentRow, matrixTableCols - 1];

                //Add remaining basis variables
                var basisRowsNew = new List<int>();
                for (var currentRow = 0; currentRow < matrixTableRows; currentRow++)
                    basisRowsNew.Add(currentRow);

                basisRows.ForEach(x => basisRowsNew.Remove(x));
                var basisRowsNewCount = basisRowsNew.Count;
                //basisRowsNew.ForEach(x => Console.WriteLine("{0}", x));

                for (var currentCol = simplexTableCols - simplexTableAddCols; currentCol < simplexTableCols; currentCol++)
                {
                    var currentRow = basisRowsNew.FirstOrDefault();
                    myTask.SimplexTable[currentRow, currentCol] = 1;
                    basisRowsNew.Remove(currentRow);
                }

                //Compute Z row & M row
                myTask.FunctionM = new int[basisRowsNewCount];
                for (var currentCol = 0; currentCol < myTask.FunctionM.GetLength(0); currentCol++)
                    myTask.FunctionM[currentCol] = 1;

                var basisCols = GetBasisColsForSimplexTable(myTask.SimplexTable); //.ForEach(x => Console.WriteLine("{0}", x));

                //Create Z sum matrix
                double[,] functionZMatrix = new double[simplexTableCols - simplexTableAddCols - 1, simplexTableCols];
                var functionZMatrixHeight = functionZMatrix.GetLength(0);
                var functionZMatrixWidth = functionZMatrix.GetLength(1);
                for (int currentRowCol = 0; currentRowCol < functionZMatrixHeight; currentRowCol++)
                {
                    functionZMatrix[currentRowCol, currentRowCol + 1] = 1;
                }

                //Create M sum matrix
                double[,] functionMMatrix = new double[simplexTableAddCols, simplexTableCols];
                var functionMMatrixHeight = functionMMatrix.GetLength(0);
                var functionMMatrixWidth = functionMMatrix.GetLength(1);

                basisCols.ForEach(x =>
                {
                    if (x <= functionZMatrixHeight)
                    {
                        for (var currentRow = 0; currentRow < functionZMatrixHeight; currentRow++)
                        {
                            functionZMatrix[currentRow, 0] = myTask.SimplexTable[currentRow, 0];

                            for (var currentCol = 1; currentCol < functionMMatrixWidth - simplexTableAddCols; currentCol++)
                            {
                                if (currentCol == x)
                                    continue;
                                functionZMatrix[currentRow, currentCol] = myTask.SimplexTable[currentRow, currentCol];
                            }
                        }
                    }
                    else
                    {
                        for (var currentRow = 0; currentRow < functionMMatrixHeight; currentRow++)
                        {
                            functionMMatrix[currentRow, 0] = myTask.SimplexTable[currentRow, 0];

                            for (var currentCol = 1; currentCol < functionMMatrixWidth - simplexTableAddCols; currentCol++)
                            {
                                if (currentCol == x)
                                    continue;
                                if (myTask.Limit == FunctionLimit.Max)
                                    functionMMatrix[currentRow, currentCol] = -myTask.SimplexTable[currentRow, currentCol];
                                else
                                    functionMMatrix[currentRow, currentCol] = myTask.SimplexTable[currentRow, currentCol];
                            }
                        }
                    }
                });

                simplexTableRows = myTask.SimplexTable.GetLength(0);

                //Sum-up functionZMatrix and move to SimplexTable
                {
                    for (var currentRow = 0; currentRow < functionZMatrixHeight; currentRow++)
                    {
                        for (var currentCol = 0; currentCol < functionZMatrixWidth - simplexTableAddCols - 1; currentCol++)
                        {
                            functionZMatrix[currentRow, currentCol + 1] *= myTask.FunctionZ[currentCol];
                        }
                    }

                    for (var currentCol = 0; currentCol < functionZMatrixWidth; currentCol++)
                    {
                        for (var currentRow = 0; currentRow < functionZMatrixHeight; currentRow++)
                        {
                            myTask.SimplexTable[simplexTableRows - 2, currentCol] += functionZMatrix[currentRow, currentCol];
                        }
                    }

                    //if (myTask.Limit == FunctionLimit.Max)
                    for (var currentCol = 0; currentCol < functionZMatrixWidth; currentCol++)
                    {
                        myTask.SimplexTable[simplexTableRows - 2, currentCol] *= -1;
                    }
                }

                //Sum-up functionMMatrix and move to SimplexTable
                {
                    for (var currentCol = 0; currentCol < functionMMatrixWidth; currentCol++)
                    {
                        for (var currentRow = 0; currentRow < functionMMatrixHeight; currentRow++)
                        {
                            myTask.SimplexTable[simplexTableRows - 1, currentCol] += functionMMatrix[currentRow, currentCol];
                        }
                    }

                    //if (myTask.Limit == FunctionLimit.Max)
                    myTask.SimplexTable[simplexTableRows - 1, 0] *= -1;
                }

                // PrintMatrix(functionZMatrix);
                // PrintMatrix(functionMMatrix);

                var myTaskSimplexTable = myTask.SimplexTable;

                //Optimize SimplexTable to remove -0.00
                OptimizeSimplexTable(ref myTaskSimplexTable);

                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("<--- [Constructed Simplex-M Task] --->");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write('\n');
                Console.Write('\n');

                PrintFunctionZM(myTask.FunctionZ, myTask.Limit, myTask.FunctionM);
                PrintSimplexTable(myTask.SimplexTable);

                //Do simplex steps

                var shouldRun = true;
                var simplexStepCount = 1;
                while (shouldRun)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("<--- [Simplex-M Task: Step {0}] --->", simplexStepCount++);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write('\n');
                    Console.Write('\n');

                    shouldRun = !SimplexStepM(ref myTaskSimplexTable, myTask.Limit);
                    OptimizeSimplexTable(ref myTaskSimplexTable);
                }

                var shouldRunSimplexZ = true;
                while (shouldRunSimplexZ)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("<--- [Simplex-M Task: Step {0}] --->", simplexStepCount++ - 1);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write('\n');
                    Console.Write('\n');
                
                    shouldRunSimplexZ = !SimplexStepZ(ref myTaskSimplexTable, myTask.Limit);
                    OptimizeSimplexTable(ref myTaskSimplexTable);
                }

                PrintSimplexTable(myTaskSimplexTable);
            }
            else
            {
                PrintFunctionZ(myTask.FunctionZ, myTask.Limit);
                PrintSystem(myTask.System);

                var basisRows = GetBasisRowsForSystem(myTask.System, myTask.SystemBasisOffset); //.ForEach(x => Console.WriteLine("{0}", x))

                var matrixTableRows = myTask.System.GetLength(0);
                var matrixTableCols = myTask.System.GetLength(1);
                var simplexTableRows = matrixTableRows;
                var simplexTableCols = matrixTableCols;
                var simplexTableAddCols = simplexTableRows - basisRows.Count;

                simplexTableCols += simplexTableAddCols;

                myTask.SimplexTable = new double[simplexTableRows + 2, simplexTableCols];

                //Copy System to Simplex Table
                for (var currentRow = 0; currentRow < myTask.System.GetLength(0); currentRow++)
                    for (var currentCol = 0; currentCol < myTask.System.GetLength(1) - 1; currentCol++)
                        myTask.SimplexTable[currentRow, currentCol + 1] = myTask.System[currentRow, currentCol];

                for (var currentRow = 0; currentRow < myTask.System.GetLength(0); currentRow++)
                    myTask.SimplexTable[currentRow, 0] = myTask.System[currentRow, matrixTableCols - 1];

                //Add remaining basis variables
                var basisRowsNew = new List<int>();
                for (var currentRow = 0; currentRow < matrixTableRows; currentRow++)
                    basisRowsNew.Add(currentRow);

                basisRows.ForEach(x => basisRowsNew.Remove(x));
                var basisRowsNewCount = basisRowsNew.Count;
                //basisRowsNew.ForEach(x => Console.WriteLine("{0}", x));

                for (var currentCol = simplexTableCols - simplexTableAddCols; currentCol < simplexTableCols; currentCol++)
                {
                    var currentRow = basisRowsNew.FirstOrDefault();
                    myTask.SimplexTable[currentRow, currentCol] = 1;
                    basisRowsNew.Remove(currentRow);
                }



                //Compute Z row & M row
                myTask.FunctionM = new int[basisRowsNewCount];
                for (var currentCol = 0; currentCol < myTask.FunctionM.GetLength(0); currentCol++)
                    myTask.FunctionM[currentCol] = 1;

                var basisCols = GetBasisColsForSimplexTable(myTask.SimplexTable); //.ForEach(x => Console.WriteLine("{0}", x));

                //Create Z sum matrix
                double[,] functionZMatrix = new double[simplexTableCols - simplexTableAddCols - 1, simplexTableCols];
                var functionZMatrixHeight = functionZMatrix.GetLength(0);
                var functionZMatrixWidth = functionZMatrix.GetLength(1);
                for (int currentRowCol = 0; currentRowCol < functionZMatrixHeight; currentRowCol++)
                {
                    functionZMatrix[currentRowCol, currentRowCol + 1] = 1;
                }

                //Create M sum matrix
                double[,] functionMMatrix = new double[simplexTableAddCols, simplexTableCols];
                var functionMMatrixHeight = functionMMatrix.GetLength(0);
                var functionMMatrixWidth = functionMMatrix.GetLength(1);

                basisCols.ForEach(x =>
                {
                    if (x <= functionZMatrixHeight)
                    {
                        // for (var currentRow = 0; currentRow < functionZMatrixHeight; currentRow++)
                        // {
                        //     functionZMatrix[currentRow, 0] = myTask.SimplexTable[currentRow, 0];
                        //
                        //     for (var currentCol = 1; currentCol < functionMMatrixWidth - simplexTableAddCols; currentCol++)
                        //     {
                        //         if (currentCol == x)
                        //             continue;
                        //         functionZMatrix[currentRow, currentCol] = myTask.SimplexTable[currentRow, currentCol];
                        //     }
                        // }
                    }
                    else
                    {
                        for (var currentRow = 0; currentRow < functionMMatrixHeight; currentRow++)
                        {
                            functionMMatrix[currentRow, 0] = myTask.SimplexTable[currentRow, 0];

                            for (var currentCol = 1; currentCol < functionMMatrixWidth - simplexTableAddCols; currentCol++)
                            {
                                if (currentCol == x)
                                    continue;
                                if (myTask.Limit == FunctionLimit.Max)
                                    functionMMatrix[currentRow, currentCol] = -myTask.SimplexTable[currentRow, currentCol];
                                else
                                    functionMMatrix[currentRow, currentCol] = myTask.SimplexTable[currentRow, currentCol];
                            }
                        }
                    }
                });

                // basisCols.ForEach(x => Console.WriteLine("{0}", x));
                //
                // Console.WriteLine("---functionMMatrix---");
                //
                // PrintMatrix(functionMMatrix);
                //
                // Console.WriteLine("---functionZMatrix---");
                //
                // PrintMatrix(functionZMatrix);

                simplexTableRows = myTask.SimplexTable.GetLength(0);

                //Sum-up functionZMatrix and move to SimplexTable
                {
                    for (var currentRow = 0; currentRow < functionZMatrixHeight; currentRow++)
                    {
                        for (var currentCol = 0; currentCol < functionZMatrixWidth - simplexTableAddCols - 1; currentCol++)
                        {
                            functionZMatrix[currentRow, currentCol + 1] *= myTask.FunctionZ[currentCol];
                        }
                    }

                    for (var currentCol = 0; currentCol < functionZMatrixWidth; currentCol++)
                    {
                        for (var currentRow = 0; currentRow < functionZMatrixHeight; currentRow++)
                        {
                            myTask.SimplexTable[simplexTableRows - 2, currentCol] += functionZMatrix[currentRow, currentCol];
                        }
                    }

                    //if (myTask.Limit == FunctionLimit.Max)
                    for (var currentCol = 0; currentCol < functionZMatrixWidth; currentCol++)
                    {
                        myTask.SimplexTable[simplexTableRows - 2, currentCol] *= -1;
                    }
                }

                //Sum-up functionMMatrix and move to SimplexTable
                {
                    for (var currentCol = 0; currentCol < functionMMatrixWidth; currentCol++)
                    {
                        for (var currentRow = 0; currentRow < functionMMatrixHeight; currentRow++)
                        {
                            myTask.SimplexTable[simplexTableRows - 1, currentCol] += functionMMatrix[currentRow, currentCol];
                        }
                    }
                }

                // PrintMatrix(functionZMatrix);
                // PrintMatrix(functionMMatrix);

                var myTaskSimplexTable = myTask.SimplexTable;

                //Optimize SimplexTable to remove -0.00
                OptimizeSimplexTable(ref myTaskSimplexTable);

                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("<--- [Constructed Simplex-M Task] --->");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write('\n');
                Console.Write('\n');

                PrintFunctionZM(myTask.FunctionZ, myTask.Limit, myTask.FunctionM);
                PrintSimplexTable(myTask.SimplexTable);

                //Do simplex steps

                var shouldRunSimplexM = true;
                var simplexStepCount = 1;
                while (shouldRunSimplexM)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("<--- [Simplex-M Task: Step {0}] --->", simplexStepCount++);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write('\n');
                    Console.Write('\n');
                
                    shouldRunSimplexM = !SimplexStepM(ref myTaskSimplexTable, myTask.Limit);
                    OptimizeSimplexTable(ref myTaskSimplexTable);
                }

                var shouldRunSimplexZ = true;
                while (shouldRunSimplexZ)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("<--- [Simplex-M Task: Step {0}] --->", simplexStepCount++ - 1);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write('\n');
                    Console.Write('\n');
                
                    shouldRunSimplexZ = !SimplexStepZ(ref myTaskSimplexTable, myTask.Limit);
                    OptimizeSimplexTable(ref myTaskSimplexTable);
                }
                
                PrintSimplexTable(myTaskSimplexTable);
            }
        }
    }
}
