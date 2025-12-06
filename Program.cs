using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace simpleClasses
{
    public class ComplexNumber
    {
        public double Real { get; }
        public double Imaginary { get; }
        public ComplexNumber(double Real, double Imaginary)
        {
            this.Real = Real;
            this.Imaginary = Imaginary;
        }

        public ComplexNumber Add(ComplexNumber other)
        {
            return new ComplexNumber(Real + other.Real, Imaginary + other.Imaginary);
        }

        public ComplexNumber Subtract(ComplexNumber other)
        {
            return new ComplexNumber(Real - other.Real, Imaginary - other.Imaginary);
        }
        public ComplexNumber Multiply(ComplexNumber other)
        {
            return new ComplexNumber(Real * other.Real - Imaginary * other.Imaginary,
                Real * other.Imaginary + Imaginary * other.Real);
        }
        public double SquireValue()
        {
            return Real * Real + Imaginary * Imaginary;
        }
        public void PrintComplexNumber()
        {
            Console.Write($"{Real}+i{Imaginary} \t");
        }
    }

    public class Matrix {
        public int Rows { get; }
        public int Cols { get; }
        private ComplexNumber[,] data;
        public Matrix(int Rows, int Cols)
        {
            this.Rows = Rows;
            this.Cols = Cols;
            data = new ComplexNumber[Rows, Cols];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    data[i, j] = new ComplexNumber(0, 0);
                }
            }
                
        }

        public Matrix(ComplexNumber[,] x)
        {
            Rows = x.GetLength(0);
            Cols = x.GetLength(1);
            data = new ComplexNumber[Rows, Cols];
            for (int i = 0; i < x.GetLength(0); i++)
            {
                for (int j = 0; j < x.GetLength(1); j++)
                {
                    data[i, j] = new ComplexNumber(x[i, j].Real, x[i, j].Imaginary);
                }
            }
        }

        public ComplexNumber Get(int r, int c)
        {
            return data[r, c];
        }
        public void Set(int r, int c, ComplexNumber value) {
            data[r, c] = value;
        }

        public Matrix Multiply(Matrix other)
        {
            if (Cols != other.Rows)
            {
                Console.WriteLine("Error! col != row. Multi not possible");
                return new Matrix(Cols, other.Rows);
            }
            Matrix result = new Matrix(Rows, other.Cols);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < other.Cols; j++)
                {
                    ComplexNumber sum = new ComplexNumber(0, 0);
                    for (int k = 0; k < Cols; k++)
                    {
                        sum = sum.Add(data[i, k].Multiply(other.data[k, j]));
                    }
                    result.Set(i, j, sum);
                }
            return result;
        }
        public Matrix KroneckerProduct(Matrix other)
        {
            Matrix result = new Matrix(Rows * other.Rows, Cols * other.Cols);
            for(int i = 0; i < Rows; i++)
            {
                for(int j = 0; j < Cols; j++)
                {
                    for (int k = 0; k < other.Rows; k++)
                    {
                        for (int l = 0; l < other.Cols; l++)
                        {
                            result.Set(other.Rows * i + k, other.Cols * j + l, data[i, j].Multiply(other.Get(k, l)));

                        }
                    }
                }
            }
            return result;
        }
        public void PrintMatrix()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    data[i, j].PrintComplexNumber();
                }
            }
        }
    }

    class StateVector
    {
        public int NumberOfQubits { get; }
        public ComplexNumber[] Amplitudes { get; }
        Random rnd = new Random();

        public StateVector(int numberOQB)
        {
            NumberOfQubits = numberOQB;
            Amplitudes = new ComplexNumber[(int) Math.Pow(2, numberOQB)];
            Amplitudes[0] = new ComplexNumber(1, 0);
            for (int i = 1; i < Amplitudes.Length; i++)
            {
                Amplitudes[i] = new ComplexNumber(0, 0);
            }
        }
        public StateVector(ComplexNumber[] amplitudes)
        {
            NumberOfQubits = (int)Math.Log2(Amplitudes.Length);
            Amplitudes = new ComplexNumber[amplitudes.Length];
            for (int i = 1; i < Amplitudes.Length; i++)
            {
                Amplitudes[i] = amplitudes[i];
            }
        }

        public void Normalize()
        {
            double sum = 0;
            for(int i = 0; i < Amplitudes.Length; i++)
            {
                sum = sum + Amplitudes[i].SquireValue();
            }
            double normalConstant = 1 / Math.Sqrt(sum == 0? 1: sum);
            for (int i = 0; i < Amplitudes.Length; i++)
            {
                Amplitudes[i] = Amplitudes[i].Multiply(new ComplexNumber(normalConstant, 0));
            }
        }

        public double[] GetProbability(int target)
        {
            double probability0 = 0;
            double probability1 = 0;

            for (int i = 0; i <Amplitudes.Length; i++)
            {
                if((int)(i/Math.Pow(2, target))%2 == 0){
                    probability0 = probability0 + Amplitudes[i].SquireValue();
                }
                else
                {
                    probability1 = probability1 + Amplitudes[i].SquireValue();
                }
            }
            double[] probabilities = new double[2];
            probabilities[0] = probability0;
            probabilities[1] = probability1;

            return probabilities;
        }

        public int MeasureSingleQubit(int target)
        {
            double randomNumber = rnd.NextDouble();

            double[] p = GetProbability(target);
            double p0 = p[0];
            double p1 = p[1];
            int measuredValue = randomNumber < p0 ? 0 : 1;
            for(int i = 0; i < Amplitudes.Length; i++)
            {
                if ((int)(i / Math.Pow(2, target)) % 2 != measuredValue){
                    Amplitudes[i] = new ComplexNumber(0, 0);
                }
            }
            Normalize();
            return measuredValue;
        }
        public int[] MeasureAllQubit()
        {
            double randomNumber = rnd.NextDouble();
            double probabilityRange = 0;
            int[] qubitArray = new int[NumberOfQubits];
            int k = 0;
            for(int i = 0; i < Amplitudes.Length; i++)
            {
                probabilityRange += Amplitudes[i].SquireValue();
                if(randomNumber < probabilityRange)
                {
                    k = i;
                    break;
                }
            }
            for (int i = 0; i < Amplitudes.Length; i++)
            {
                if (i!=k)
                {
                    Amplitudes[i] = new ComplexNumber(0, 0);
                }
            }
            Normalize();
            for (int i = NumberOfQubits - 1; i >=0; i--)
            {
                qubitArray[i] = k % 2;
                k = k / 2;
            }
            return qubitArray;
        }
        
        public void ApplySingleQubitGate(Matrix gate, int target)
        {
            if (target >= NumberOfQubits || gate.Rows != 2 || gate.Cols != 2) return;

            int steps = (int)Math.Pow(2, target);

            for (int i = 0; i < Amplitudes.Length; i += steps * 2)
            {
                for (int j = i; j < i + steps; j++)
                {
                    int i0 = j;
                    int i1 = j + steps;
                    ComplexNumber a0 = gate.Get(0, 0).Multiply(Amplitudes[i0]).Add(gate.Get(0, 1).Multiply(Amplitudes[i1]));
                    ComplexNumber a1 = gate.Get(1, 0).Multiply(Amplitudes[i0]).Add(gate.Get(1, 1).Multiply(Amplitudes[i1]));
                    Amplitudes[i0] = a0;
                    Amplitudes[i1] = a1;
                }
            }
        }

        public void ApplySingleQubitGate(Matrix gate, int control, int target)
        {
            
            {
                if (target >= NumberOfQubits || gate.Rows != 2 || gate.Cols != 2) return;

                int steps = (int)Math.Pow(2, target);

                for (int i = 0; i < Amplitudes.Length; i += steps * 2)
                {
                    for (int j = i; j < i + steps; j++)
                    {
                        if((int) (j/Math.Pow(2, control)) %2== 1)
                        {
                            int i0 = j;
                            int i1 = j + steps;
                            ComplexNumber a0 = gate.Get(0, 0).Multiply(Amplitudes[i0]).Add(gate.Get(0, 1).Multiply(Amplitudes[i1]));
                            ComplexNumber a1 = gate.Get(1, 0).Multiply(Amplitudes[i0]).Add(gate.Get(1, 1).Multiply(Amplitudes[i1]));
                            Amplitudes[i0] = a0;
                            Amplitudes[i1] = a1;
                        }
                    }
                }
            
        }

    }

    public static class GateLibrary
    {
        public static Matrix X()
        {
            return new Matrix(new ComplexNumber[,]
            {
            { new ComplexNumber(0, 0), new ComplexNumber(1, 0) },
            { new ComplexNumber(1, 0), new ComplexNumber(0, 0) }
            });
        }

        public static Matrix Z()
        {
            return new Matrix(new ComplexNumber[,]
            {
            { new ComplexNumber(1, 0),  new ComplexNumber(0, 0) },
            { new ComplexNumber(0, 0), new ComplexNumber(-1, 0) }
            });
        }

        public static Matrix H()
        {
            double s = 1.0 / Math.Sqrt(2);
            return new Matrix(new ComplexNumber[,]
            {
            { new ComplexNumber(s, 0),  new ComplexNumber(s, 0) },
            { new ComplexNumber(s, 0),  new ComplexNumber(-s, 0) }
            });
        }

        public static Matrix Y()
        {
            return new Matrix(new ComplexNumber[,]
            {
            { new ComplexNumber(0, 0),  new ComplexNumber(0, -1) },
            { new ComplexNumber(0, 1),  new ComplexNumber(0, 0) }
            });
        }
    }

    public class Program
    {
        static void PrintState(string label, StateVector sv)
        {
            Console.WriteLine(label);
            for (int i = 0; i < sv.Amplitudes.Length; i++)
            {
                Console.Write($"|{Convert.ToString(i, 2).PadLeft(sv.NumberOfQubits, '0')}>: ");
                sv.Amplitudes[i].PrintComplexNumber();
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            // ========= Test 1: single qubit, X gate on |0> -> |1> =========
            Console.WriteLine("=== Test 1: single qubit X on |0> ===");
            StateVector sv1 = new StateVector(1); // |0>
            PrintState("Initial state |0>:", sv1);

            sv1.ApplySingleQubitGate(GateLibrary.X(), 0);
            PrintState("After X gate (should be |1>):", sv1);

            int m1 = sv1.MeasureSingleQubit(0);
            Console.WriteLine($"Measured value (expected 1): {m1}");
            PrintState("State after measurement:", sv1);

            // ========= Test 2: single qubit, H gate -> superposition =========
            Console.WriteLine("=== Test 2: single qubit H on |0> ===");
            StateVector sv2 = new StateVector(1); // |0>
            PrintState("Initial state |0>:", sv2);

            sv2.ApplySingleQubitGate(GateLibrary.H(), 0);
            PrintState("After H gate (should be superposition):", sv2);

            double[] probs2 = sv2.GetProbability(0);
            Console.WriteLine($"P(0) ≈ {probs2[0]}, P(1) ≈ {probs2[1]}");
            int m2 = sv2.MeasureSingleQubit(0);
            Console.WriteLine($"Measured value (0 or 1 with ~50% prob): {m2}");
            PrintState("State after measurement:", sv2);

            // ========= Test 3: two qubits, H on qubit 0 =========
            Console.WriteLine("=== Test 3: 2 qubits, H on qubit 0 ===");
            StateVector sv3 = new StateVector(2); // |00>
            PrintState("Initial state |00>:", sv3);

            // Apply H to qubit 0 (least significant bit in your ApplySingleQubitGate logic)
            sv3.ApplySingleQubitGate(GateLibrary.H(), 0);
            PrintState("After H on qubit 0:", sv3);

            double[] probs3_q0 = sv3.GetProbability(0);
            double[] probs3_q1 = sv3.GetProbability(1);
            Console.WriteLine($"Qubit 0: P(0) = {probs3_q0[0]}, P(1) = {probs3_q0[1]}");
            Console.WriteLine($"Qubit 1: P(0) = {probs3_q1[0]}, P(1) = {probs3_q1[1]}");

            int[] all3 = sv3.MeasureAllQubit();
            Console.WriteLine("Measured all qubits (bitstring): " + string.Join("", all3));
            PrintState("State after measuring all qubits:", sv3);

            // ========= Test 4: matrix multiplication & Kronecker product =========
            Console.WriteLine("=== Test 4: Matrix and Kronecker tests ===");
            Matrix xGate = GateLibrary.X();
            Matrix hGate = GateLibrary.H();

            // X * X should be identity
            Matrix xx = xGate.Multiply(xGate);
            Console.WriteLine("X * X (should act like identity on |0>):");
            StateVector sv4 = new StateVector(1);
            sv4.ApplySingleQubitGate(xx, 0);
            PrintState("After XX on |0>:", sv4);

            // H ⊗ H (Kronecker product)
            Matrix hTensorH = hGate.KroneckerProduct(hGate);
            Console.WriteLine("H ⊗ H matrix elements:");
            hTensorH.PrintMatrix();
            Console.WriteLine();

            Console.ReadKey();
        }
    }
}




