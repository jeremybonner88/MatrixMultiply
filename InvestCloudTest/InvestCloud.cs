using InvestCloudTest.Controllers;
using InvestCloudTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestCloudTest
{
    class InvestCloud
    {
        private ApiController ac = new ApiController();
        private readonly int aColumnSize = 1000;
        private readonly int bColumnSize = 1000;
        private readonly int aRowSize = 1000;
        private readonly int bRowSize = 1000;
        private readonly int matriciesInitializationSize = 1000;

        static void Main(string[] args)
        {
            InvestCloud IC = new InvestCloud();

            IC.Run();
        }

        public async void Run()
        {
            Console.WriteLine("Initializing matrices");
            InitializeMatricies();
            Matrix A = new Matrix(aRowSize, aColumnSize);
            Matrix B = new Matrix(bRowSize, bColumnSize);
            //populate the matrices
            Console.WriteLine("Building matrices A and B");
            Parallel.For(0, aRowSize, async i =>
            {
                Task<ResponseData> rowDataTaskA = (ac.GetRowOrColumnDataSet("A", "row", i));
                rowDataTaskA.Wait();
                var rowDataA = (await rowDataTaskA).Value;
                Task<ResponseData> rowDataTaskB = (ac.GetRowOrColumnDataSet("B", "row", i));
                rowDataTaskB.Wait();
                var rowDataB = (await rowDataTaskB).Value;
                Parallel.For(0, aColumnSize, j =>
                {
                    A.matrix[i, j] = rowDataA[j];
                    B.matrix[i, j] = rowDataB[j];
                });
            });
            Console.WriteLine("Multiplying matrices A and B");
            var matrixResult = A * B;
            //now convert to string
            Console.WriteLine("Encoding Result Matrix");
            var stringToEncode = TranslateMatrixToString(matrixResult);
            var encodedString = CreateMD5(stringToEncode);
            Console.WriteLine("Checking Passcode");
            Task<string> resultTask = ac.ValidateMatrix(encodedString);
            resultTask.Wait();
            var result = await resultTask;
            Console.Write(result);
            Console.Read();
        }
        public string TranslateMatrixToString(Matrix m)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m.rows; i++)
            {
                for (int j = 0; j < m.columns; j++)
                {
                    sb.Append(m.matrix[i, j].ToString());
                }
            }
            return sb.ToString();
        }

        public async void InitializeMatricies()
        {
            var Task = ac.InitializedMatrices(matriciesInitializationSize);
            Task.Wait();
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        //public int[][] MatrixCreate(int rows, int cols)
        //{
        //    // do error checking here
        //    int[][] result = new int[rows][];
        //    for (int i = 0; i < rows; ++i)
        //        result[i] = new int[cols];
        //    return result;
        //}

        //public int[][] MatrixProduct(int aRowSize, int bRowSize, int aColSize, int bColSize)
        //{
        //    if (aColSize != bColSize)
        //        throw new Exception("These matrices cannot be multiplied");

        //    int[][] result = MatrixCreate(aRowSize, bColSize);

        //    Parallel.For(0, aRowSize, async i =>
        //    {
        //        //get aRow index i
        //        Task<ResponseData> rowDataTask = (ac.GetRowOrColumnDataSet("A", "row", i));
        //        rowDataTask.Wait();
        //        var rowData = (await rowDataTask).Value;
        //        Parallel.For(0, bColSize, async j => {
        //            Task<ResponseData> colDataTask = (ac.GetRowOrColumnDataSet("B", "col", j));
        //            colDataTask.Wait();
        //            var colData = (await colDataTask).Value;
        //            for (int k = 0; k < aColSize; ++k) // could use k < bRowSize
        //                result[i][j] += rowData[k] * colData[k];
        //        });
        //    });

        //    return result;
        //}
    }
}
