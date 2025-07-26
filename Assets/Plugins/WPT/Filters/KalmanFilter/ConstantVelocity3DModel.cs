using UnityEngine;
using WPT.Utilities;

namespace WPT.Filters
{
    public class ConstantVelocity3DModel
    {
        public const int Dimension = 6;
        public Vector3 Position;
        public Vector3 Velocity;

        public ConstantVelocity3DModel()
        {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
        }

        public static double[,] GetTransitionMatrix(double timeInterval = 1.0)
        {
            double num = timeInterval;
            return new double[6, 6]
            {
                {
                    1.0,
                    num,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                },
                {
                    0.0,
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                },
                {
                    0.0,
                    0.0,
                    1.0,
                    num,
                    0.0,
                    0.0,
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                    0.0,
                    0.0,
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                    num,
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                }
            };
        }

        public static double[,] GetPositionMeasurementMatrix()
        {
            return new double[3, 6]
            {
                {
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                },
                {
                    0.0,
                    0.0,
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                    0.0,
                }
            };
        }

        public static double[,] GetProcessNoise(double accelerationNoise, double timeInterval = 1.0)
        {
            double[,] numArray1 = new double[6, 3];
            numArray1[0, 0] = timeInterval * timeInterval / 2.0;
            numArray1[1, 0] = timeInterval;
            numArray1[2, 1] = timeInterval * timeInterval / 2.0;
            numArray1[3, 1] = timeInterval;
            numArray1[4, 2] = timeInterval * timeInterval / 2.0;
            numArray1[5, 2] = timeInterval;
            double[,] numArray2 = numArray1;
            double[,] b = MatrixUtils.Diagonal<double>(numArray2.ColumnCount<double>(), accelerationNoise);
            return numArray2.Multiply(b).Multiply(numArray2.Transpose<double>());
        }

        public static ConstantVelocity3DModel FromArray(double[] arr)
        {
            return new ConstantVelocity3DModel()
            {
                Position = new Vector3((float)arr[0], (float)arr[2], (float)arr[4]),
                Velocity = new Vector3((float)arr[1], (float)arr[3], (float)arr[5])
            };
        }

        public static double[] ToArray(ConstantVelocity3DModel modelState)
        {
            return new double[6]
            {
                modelState.Position.x,
                modelState.Velocity.x,
                modelState.Position.y,
                modelState.Velocity.y,
                modelState.Position.z,
                modelState.Velocity.z
            };
        }
    }
}