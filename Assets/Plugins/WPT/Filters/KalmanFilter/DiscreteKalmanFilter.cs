using System;
using WPT.Utilities;

namespace WPT.Filters
{
    public class DiscreteKalmanFilter<TState, TMeasurement> : BaseKalmanFilter<TState, TMeasurement>
    {
        public DiscreteKalmanFilter(
            TState initialState,
            double[,] initialStateError,
            int measurementVectorDimension,
            int controlVectorDimension,
            Func<TState, double[]> stateConvertFunc,
            Func<double[], TState> stateConvertBackFunc,
            Func<TMeasurement, double[]> measurementConvertFunc)
            : base(initialState, initialStateError, measurementVectorDimension, controlVectorDimension, stateConvertFunc, stateConvertBackFunc, measurementConvertFunc)
        {

        }

        protected override void PredictInternal(double[] controlVector)
        {
            state = TransitionMatrix.Multiply(state);

            if (controlVector != null)
            {
                state = MatrixUtils.Add(state, ControlMatrix.Multiply(controlVector));
            }

            EstimateCovariance = MatrixUtils.Add(TransitionMatrix.Multiply(EstimateCovariance).Multiply(TransitionMatrix.Transpose()), ProcessNoise);

            var b = MeasurementMatrix.Transpose();

            ResidualCovariance = MatrixUtils.Add(MeasurementMatrix.Multiply(EstimateCovariance).Multiply(b), MeasurementNoise);
            ResidualCovarianceInv = ResidualCovariance.Inverse();
            KalmanGain = EstimateCovariance.Multiply(b).Multiply(ResidualCovarianceInv);
        }

        protected override void CorrectInternal(double[] measurement)
        {
            if (measurement.Length != MeasurementVectorDimension)
            {
                throw new Exception("PredicitionError error vector (innovation vector) must have the same length as measurement.");
            }

            var innovation = MatrixUtils.Subtract(measurement, MeasurementMatrix.Multiply(state));

            state = MatrixUtils.Add(state, KalmanGain.Multiply(innovation));

            EstimateCovariance = MatrixUtils.Identity(StateVectorDimension).Subtract(KalmanGain.Multiply(MeasurementMatrix)).Multiply(EstimateCovariance);
        }
    }
}