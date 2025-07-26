using UnityEngine;
using WPT.Utilities;

namespace WPT.Filters
{
    public class ConstantVelocity3DModel
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public ConstantVelocity3DModel()
        {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
        }

        public static double[,] GetTransitionMatrix(double timeInterval = 1.0) => new double[6, 6]
        {
            {
                1.0,
                timeInterval,
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
                timeInterval,
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
                timeInterval,
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

        public static double[,] GetPositionMeasurementMatrix() => new double[3, 6]
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

        public static double[,] GetProcessNoise(double accelerationNoise, double timeInterval = 1.0)
        {
            var numArray = new double[6, 3];
            numArray[0, 0] = timeInterval * timeInterval / 2.0;
            numArray[1, 0] = timeInterval;
            numArray[2, 1] = timeInterval * timeInterval / 2.0;
            numArray[3, 1] = timeInterval;
            numArray[4, 2] = timeInterval * timeInterval / 2.0;
            numArray[5, 2] = timeInterval;

            return numArray.Multiply(MatrixUtils.Diagonal(numArray.GetLength(1), accelerationNoise)).Multiply(numArray.Transpose());
        }

        public static ConstantVelocity3DModel FromArray(double[] arr) => new()
        {
            Position = new Vector3((float)arr[0], (float)arr[2], (float)arr[4]),
            Velocity = new Vector3((float)arr[1], (float)arr[3], (float)arr[5])
        };

        public static double[] ToArray(ConstantVelocity3DModel modelState) => new double[6]
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