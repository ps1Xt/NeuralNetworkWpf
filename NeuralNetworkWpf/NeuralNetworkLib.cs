using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkWpf
{
    public class NeuralNetwork
    {
        int iNodes;
        int oNodes;
        int hNodes;
        int hNodes2;
        double lr;
        double[,] wih;
        double[,] wih2;
        double[,] who;
        Matrix m;
        double[] hidden_inputs;
        double[] hidden_outputs;
        double[] hidden_inputs2;
        double[] hidden_outputs2;
        double[] final_inputs;
        public double[] final_outputs;
        double[] inputs;
        double[] target;
        public double[] output_errors;
        double[] final;
        public double[] hidden_errors;
        double[] hidden_errors2;
        public NeuralNetwork(int inodes, int hnodes, int hnodes2, int outnodes, double learningRate)
        {

            iNodes = inodes;
            hNodes = hnodes;
            oNodes = outnodes;
            hNodes2 = hnodes2;
            lr = learningRate;
            m = new Matrix();
            wih = new double[iNodes, hNodes];
            wih2 = new double[hNodes, hNodes2];
            who = new double[hNodes2, oNodes];
            wih = m.rand(iNodes, hNodes);
            wih2 = m.rand(hNodes, hNodes2);
            who = m.rand(hNodes2, oNodes);
            hidden_inputs = new double[hNodes];
            hidden_outputs = new double[hNodes];
            hidden_inputs2 = new double[hNodes2];
            hidden_outputs2 = new double[hNodes2];
            final_inputs = new double[oNodes];
            final_outputs = new double[oNodes];
            inputs = new double[iNodes];
        }
        public void Train(double[] inputs_list, double[] target_list)
        {
            inputs = inputs_list;
            target = target_list;
            hidden_inputs = m.GetX(wih, inputs);
            hidden_outputs = Activation(hidden_inputs);
            hidden_inputs2 = m.GetX(wih2, hidden_outputs);
            hidden_outputs2 = Activation(hidden_inputs2);
            final_inputs = m.GetX(who, hidden_outputs2);
            final_outputs = Activation(final_inputs);
            output_errors = m.Minus(target, final_outputs);
            hidden_errors2 = m.BackMul(who, output_errors);
            hidden_errors = m.BackMul(wih2, hidden_errors2);
            who = m.Plus(who, m.DeltaWeights(output_errors, who, hidden_outputs2, lr));
            wih2 = m.Plus(wih2, m.DeltaWeights(hidden_errors2, wih2, hidden_outputs, lr));
            wih = m.Plus(wih, m.DeltaWeights(hidden_errors, wih, inputs, lr));
        }
        public double[] Query(double[] inputs_list)
        {
            inputs = inputs_list;
            hidden_inputs = m.GetX(wih, inputs);
            hidden_outputs = Activation(hidden_inputs);
            hidden_inputs2 = m.GetX(wih2, hidden_outputs);
            hidden_outputs2 = Activation(hidden_inputs2);
            final_inputs = m.GetX(who, hidden_outputs2);
            final_outputs = Activation(final_inputs);
            return final_outputs;

        }
        double[] Activation(double[] x)
        {
            return m.Sigmoid(x);
        }
        public void Save(string path)
        {

            string WHO = path + "\\WHO.txt";
            string WIH2 = path + "\\WIH2.txt";
            string WIH = path + "\\WIH.txt";
            using (StreamWriter save = new StreamWriter(WHO, false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < who.GetLength(0); i++)
                {
                    for (int j = 0; j < who.GetLength(1); j++)
                    {
                        save.Write(who[i, j] + " ");
                    }
                    save.WriteLine();
                }
            }
            using (StreamWriter save = new StreamWriter(WIH2, false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < wih2.GetLength(0); i++)
                {
                    for (int j = 0; j < wih2.GetLength(1); j++)
                    {
                        save.Write(wih2[i, j] + " ");
                    }
                    save.WriteLine();
                }
            }
            using (StreamWriter save = new StreamWriter(WIH, false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < wih.GetLength(0); i++)
                {
                    for (int j = 0; j < wih.GetLength(1); j++)
                    {
                        save.Write(wih[i, j] + " ");
                    }
                    save.WriteLine();
                }
            }
        }
        public void Load(string path)
        {
            string WHO = path + "\\WHO.txt";
            string WIH2 = path + "\\WIH2.txt";
            string WIH = path + "\\WIH.txt";
            char split = ' ';
            using (StreamReader load = new StreamReader(WHO, System.Text.Encoding.Default))
            {
                string[] local = new string[who.GetLength(1)];
                for (int i = 0; i < who.GetLength(0); i++)
                {

                    local = load.ReadLine().Split(split);
                    for (int k = 0; k < who.GetLength(1); k++)
                    {
                        who[i, k] = Convert.ToDouble(local[k]);
                    }

                }
            }
            using (StreamReader load = new StreamReader(WIH2, System.Text.Encoding.Default))
            {
                string[] local = new string[wih2.GetLength(1)];
                for (int i = 0; i < wih2.GetLength(0); i++)
                {

                    local = load.ReadLine().Split(split);
                    for (int k = 0; k < wih2.GetLength(1); k++)
                    {
                        wih2[i, k] = Convert.ToDouble(local[k]);
                    }

                }
            }
            using (StreamReader load = new StreamReader(WIH, System.Text.Encoding.Default))
            {
                string[] local = new string[wih.GetLength(1)];
                for (int i = 0; i < wih.GetLength(0); i++)
                {

                    local = load.ReadLine().Split(split);
                    for (int k = 0; k < wih.GetLength(1); k++)
                    {
                        wih[i, k] = Convert.ToDouble(local[k]);
                    }

                }
            }
        }
    }
    class Matrix
    {
        public double[] GetX(double[,] weight, double[] input, double bias = 0)
        {
            int len1 = weight.GetLength(0);
            int len2 = weight.GetLength(1);

            double[] x = new double[len2];
            for (int i = 0; i < len1; i++)
            {
                for (int j = 0; j < len2; j++)
                {
                    x[j] += weight[i, j] * input[i];
                    x[j] += bias;
                }

            }
            return x;
        }
        public double[] Sigmoid(double[] input)
        {
            int len = input.Length;
            double[] sig = new double[len];
            for (int i = 0; i < len; i++)
            {
                sig[i] = 1 / (1 + Math.Exp(-input[i]));
            }
            return sig;
        }
        public double[,] Sigmoid(double[,] input)
        {

            double[,] sig = new double[input.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    sig[i, j] = 1 / (1 + Math.Exp(-input[i, j]));
                }

            }
            return sig;
        }
        public double[,] Normalize(double num, double[,] weight)
        {

            double sum = 0;
            //double sred = 0;
            for (int i = 0; i < weight.GetLength(0); i++)
            {
                for (int j = 0; j < weight.GetLength(1); j++)
                {
                    sum += weight[i, j];
                }
            }
            num += sum / weight.Length;
            for (int i = 0; i < weight.GetLength(0); i++)
            {
                for (int j = 0; j < weight.GetLength(1); j++)
                {
                    weight[i, j] -= num;
                }
            }
            return weight;
        }
        public double[,] rand(int rows, int columns)
        {
            Random r = new Random();
            double[,] ran = new double[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    ran[i, j] = r.NextDouble() - 0.5;
                }
            }
            return ran;
        }
        public double[,] T(double[,] input)
        {
            double[,] local = new double[input.GetLength(1), input.GetLength(0)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    local[j, i] = input[i, j];
                }
            }
            return local;
        }
        public double[] BasicMul(double[] first, double[] second)
        {
            double[] final = new double[second.Length];
            for (int i = 0; i < second.Length; i++)
            {
                final[i] = first[i] * second[i];
            }
            return final;
        }
        public double[,] T(double[] input)
        {
            double[,] local = new double[input.Length, 1];
            for (int i = 0; i < input.Length; i++)
            {
                local[i, 0] = input[i];
            }
            return local;
        }
        public double[] BackT(double[,] input)
        {
            double[] local = new double[input.GetLength(0)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                local[i] = input[i, 0];
            }
            return local;
        }
        public double[] Minus(double[] first, double[] second)
        {
            double[] final = new double[second.Length];
            for (int i = 0; i < second.Length; i++)
            {
                final[i] = first[i] - second[i];
            }
            return final;
        }
        public double[,] Plus(double[,] weights, double[,] delta)
        {
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    weights[i, j] += delta[i, j];
                }
            }
            return weights;
        }
        public double[,] MulVectors(double[] first, double[] second)
        {
            double[,] local = new double[first.Length, second.Length];
            for (int i = 0; i < first.Length; i++)
            {
                for (int j = 0; j < second.Length; j++)
                {
                    local[i, j] = first[i] * second[j];
                }
            }
            return local;
        }
        public double[] Plus(double[] first, double[] second)
        {
            double[] final = new double[second.Length];
            for (int i = 0; i < second.Length; i++)
            {
                final[i] = first[i] + second[i];
            }
            return final;
        }

        public double[] BackMul(double[,] first, double[] second)
        {
            double[] local = new double[first.GetLength(0)];
            for (int i = 0; i < first.GetLength(0); i++)
            {
                for (int j = 0; j < first.GetLength(1); j++)
                {

                    local[i] += first[i, j] * second[j];


                }
            }
            return local;
        }
        public double[,] DeltaWeights(double[] errors, double[,] weights, double[] outputs, double lr)
        {
            double[,] summa = new double[weights.GetLength(1), weights.GetLength(0)];
            for (int k = 0; k < weights.GetLength(0); k++)
            {
                for (int i = 0; i < weights.GetLength(0); i++)
                {
                    for (int j = 0; j < weights.GetLength(1); j++)
                    {
                        summa[j, i] += weights[k, j] * outputs[k];
                    }
                }
            }
            double[,] sig = new double[summa.GetLength(0), summa.GetLength(1)]; //Такой размер чтобы вставить Т матрицу
            sig = Sigmoid(T(summa));

            for (int i = 0; i < sig.GetLength(0); i++)
            {
                for (int j = 0; j < sig.GetLength(1); j++)
                {
                    sig[i, j] -= sig[i, j] * sig[i, j];
                }
            }

            double[,] final = new double[sig.GetLength(0), sig.GetLength(1)];
            for (int i = 0; i < final.GetLength(1); i++)
            {
                for (int j = 0; j < final.GetLength(0); j++)
                {
                    final[j, i] = lr * errors[i] * sig[j, i] * outputs[j];
                }
            }
            return final;
        }
    }
}