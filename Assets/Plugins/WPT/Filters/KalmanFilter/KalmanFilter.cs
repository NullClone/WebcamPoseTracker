using System;
using UnityEngine;
using WPT.Utilities;

namespace WPT.Filters
{
    public class KalmanFilter
    {
        private readonly DiscreteKalmanFilter<ConstantVelocity3DModel, Vector3> kalmanFilter;

        public KalmanFilter(double timeInterval, double noise)
        {
            kalmanFilter = new(
                new ConstantVelocity3DModel(),
                ConstantVelocity3DModel.GetProcessNoise(noise, timeInterval),
                3,
                0,
                new Func<ConstantVelocity3DModel, double[]>(ConstantVelocity3DModel.ToArray),
                new Func<double[], ConstantVelocity3DModel>(ConstantVelocity3DModel.FromArray),
                measurementConvertFunc);

            kalmanFilter.ProcessNoise = ConstantVelocity3DModel.GetProcessNoise(noise, timeInterval);
            kalmanFilter.MeasurementNoise = MatrixUtils.Diagonal(kalmanFilter.MeasurementVectorDimension, 1.0);
            kalmanFilter.MeasurementMatrix = ConstantVelocity3DModel.GetPositionMeasurementMatrix();
            kalmanFilter.TransitionMatrix = ConstantVelocity3DModel.GetTransitionMatrix(timeInterval);
            kalmanFilter.Predict();

            static double[] measurementConvertFunc(Vector3 value) => new double[3] { value.x, value.y, value.z };
        }

        public void UpdateFilterParameter(double timeInterval, double noise)
        {
            kalmanFilter.ProcessNoise = ConstantVelocity3DModel.GetProcessNoise(noise, timeInterval);
            kalmanFilter.TransitionMatrix = ConstantVelocity3DModel.GetTransitionMatrix(timeInterval);
        }

        public Vector3 CorrectAndPredict(Vector3 value)
        {
            kalmanFilter.Correct(value);
            kalmanFilter.Predict();

            return kalmanFilter.State.Position;
        }
    }
}