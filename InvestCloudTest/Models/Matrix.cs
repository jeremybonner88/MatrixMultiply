using System;
using System.Text.RegularExpressions;


public class Matrix
{
    public int rows;
    public int columns;
    public double[,] matrix;

    //constructor
    public Matrix(int numRows, int numCols)
    {
        rows = numRows;
        columns = numCols;
        matrix = new double[rows, columns];
    }

    //matrix represented as 2d array
    public double this[int iRow, int iCol]
    {
        get { return matrix[iRow, iCol]; }
        set { matrix[iRow, iCol] = value; }
    }

    //Create Zero Matrix
    public static Matrix ZeroMatrix(int iRows, int iCols) 
    {
        Matrix matrix = new Matrix(iRows, iCols);
        for (int i = 0; i < iRows; i++)
            for (int j = 0; j < iCols; j++)
                matrix[i, j] = 0;
        return matrix;
    }


    private static void ProtectedAplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++)     // cols
            {
                C[i, j] = 0;
                if (xa + j < A.columns && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                if (xb + j < B.columns && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
            }
    }

    private static void ProtectedAminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++)     // cols
            {
                C[i, j] = 0;
                if (xa + j < A.columns && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                if (xb + j < B.columns && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
            }
    }

    private static void ProtectedACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++)     // cols
            {
                C[i, j] = 0;
                if (xa + j < A.columns && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
            }
    }

    private static void AplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
    }

    private static void AminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
    }

    private static void ACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
    {
        for (int i = 0; i < size; i++)          // rows
            for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j];
    }

    private static Matrix StrassenMultiplication(Matrix A, Matrix B)                // Strassen Matrix Multiplication
    {
        if (A.columns != B.rows) throw new MatrixException("Matrices are incorrect dimensions");

        Matrix R;

        int matrixSize = Math.Max(Math.Max(A.rows, A.columns), Math.Max(B.rows, B.columns));

        if (matrixSize < 32)
        {
            R = ZeroMatrix(A.rows, B.columns);
            for (int i = 0; i < R.rows; i++)
                for (int j = 0; j < R.columns; j++)
                    for (int k = 0; k < A.columns; k++)
                        R[i, j] += A[i, k] * B[k, j];
            return R;
        }

        int size = 1; int n = 0;
        while (matrixSize > size) { size *= 2; n++; };
        int h = size / 2;


        Matrix[,] mField = new Matrix[n, 9];
        int z;
        for (int i = 0; i < n - 4; i++)          // rows
        {
            z = (int)Math.Pow(2, n - i - 1);
            for (int j = 0; j < 9; j++) mField[i, j] = new Matrix(z, z);
        }

        ProtectedAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
        ProtectedAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

        ProtectedAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
        ProtectedACopytoC(B, 0, 0, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

        ProtectedACopytoC(A, 0, 0, mField[0, 0], h);
        ProtectedAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

        ProtectedACopytoC(A, h, h, mField[0, 0], h);
        ProtectedAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

        ProtectedAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
        ProtectedACopytoC(B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

        ProtectedAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
        ProtectedAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

        ProtectedAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
        ProtectedAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
        StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

        R = new Matrix(A.rows, B.columns);                  // result

        /// C11
        for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
            for (int j = 0; j < Math.Min(h, R.columns); j++)     // cols
                R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];

        /// C12
        for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
            for (int j = h; j < Math.Min(2 * h, R.columns); j++)     // cols
                R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

        /// C21
        for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
            for (int j = 0; j < Math.Min(h, R.columns); j++)     // cols
                R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

        /// C22
        for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
            for (int j = h; j < Math.Min(2 * h, R.columns); j++)     // cols
                R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

        return R;
    }

    // function for square matrix 2^N x 2^N
    // A * B into C
    private static void StrassenMultiplyRun(Matrix A, Matrix B, Matrix C, int l, Matrix[,] tempMatrix)
    {
        int aSize = A.rows;
        int split = aSize / 2;

        if (aSize < 32)
        {
            for (int i = 0; i < C.rows; i++)
                for (int j = 0; j < C.columns; j++)
                {
                    C[i, j] = 0;
                    for (int k = 0; k < A.columns; k++) C[i, j] += A[i, k] * B[k, j];
                }
            return;
        }

        AplusBintoC(A, 0, 0, A, split, split, tempMatrix[l, 0], split);
        AplusBintoC(B, 0, 0, B, split, split, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 1], l + 1, tempMatrix); // (A11 + A22) * (B11 + B22);

        AplusBintoC(A, 0, split, A, split, split, tempMatrix[l, 0], split);
        ACopytoC(B, 0, 0, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 2], l + 1, tempMatrix); // (A21 + A22) * B11;

        ACopytoC(A, 0, 0, tempMatrix[l, 0], split);
        AminusBintoC(B, split, 0, B, split, split, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 3], l + 1, tempMatrix); //A11 * (B12 - B22);

        ACopytoC(A, split, split, tempMatrix[l, 0], split);
        AminusBintoC(B, 0, split, B, 0, 0, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 4], l + 1, tempMatrix); //A22 * (B21 - B11);

        AplusBintoC(A, 0, 0, A, split, 0, tempMatrix[l, 0], split);
        ACopytoC(B, split, split, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 5], l + 1, tempMatrix); //(A11 + A12) * B22;

        AminusBintoC(A, 0, split, A, 0, 0, tempMatrix[l, 0], split);
        AplusBintoC(B, 0, 0, B, split, 0, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 6], l + 1, tempMatrix); //(A21 - A11) * (B11 + B12);

        AminusBintoC(A, split, 0, A, split, split, tempMatrix[l, 0], split);
        AplusBintoC(B, 0, split, B, split, split, tempMatrix[l, 1], split);
        StrassenMultiplyRun(tempMatrix[l, 0], tempMatrix[l, 1], tempMatrix[l, 1 + 7], l + 1, tempMatrix); // (A12 - A22) * (B21 + B22);

        /// C11
        for (int i = 0; i < split; i++)          // rows
            for (int j = 0; j < split; j++)     // cols
                C[i, j] = tempMatrix[l, 1 + 1][i, j] + tempMatrix[l, 1 + 4][i, j] - tempMatrix[l, 1 + 5][i, j] + tempMatrix[l, 1 + 7][i, j];

        /// C12
        for (int i = 0; i < split; i++)          // rows
            for (int j = split; j < aSize; j++)     // cols
                C[i, j] = tempMatrix[l, 1 + 3][i, j - split] + tempMatrix[l, 1 + 5][i, j - split];

        /// C21
        for (int i = split; i < aSize; i++)          // rows
            for (int j = 0; j < split; j++)     // cols
                C[i, j] = tempMatrix[l, 1 + 2][i - split, j] + tempMatrix[l, 1 + 4][i - split, j];

        /// C22
        for (int i = split; i < aSize; i++)          // rows
            for (int j = split; j < aSize; j++)     // cols
                C[i, j] = tempMatrix[l, 1 + 1][i - split, j - split] - tempMatrix[l, 1 + 2][i - split, j - split] + tempMatrix[l, 1 + 3][i - split, j - split] + tempMatrix[l, 1 + 6][i - split, j - split];
    }

    // Helper Operator to run A * B and perform multiplication
    public static Matrix operator *(Matrix m1, Matrix m2)
    { return Matrix.StrassenMultiplication(m1, m2); }

}

//  The class for exceptions

public class MatrixException : Exception
{
    public MatrixException(string Message)
        : base(Message)
    { }
}