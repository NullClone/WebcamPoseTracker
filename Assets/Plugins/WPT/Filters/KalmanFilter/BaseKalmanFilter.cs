using System;

namespace WPT.Filters
{
    public abstract class BaseKalmanFilter<TState, TMeasurement>
    {
        // Properties

        public TState State => stateConvertBackFunc(state);

        public double[,] ResidualCovariance { get; protected set; }

        public double[,] ResidualCovarianceInv { get; protected set; }

        public double[,] KalmanGain { get; protected set; }

        public double[,] EstimateCovariance { get; protected set; }

        public double[,] TransitionMatrix { get; set; }

        public double[,] ControlMatrix { get; set; }

        public double[,] MeasurementMatrix { get; set; }

        public double[,] ProcessNoise { get; set; }

        public double[,] MeasurementNoise { get; set; }

        public int StateVectorDimension { get; private set; }

        public int MeasurementVectorDimension { get; private set; }

        public int ControlVectorDimension { get; private set; }


        // Fields

        private readonly Func<TState, double[]> stateConvertFunc;
        private readonly Func<double[], TState> stateConvertBackFunc;
        private readonly Func<TMeasurement, double[]> measurementConvertFunc;
        protected double[] state;


        // Methods

        protected BaseKalmanFilter(
            TState initialState,
            double[,] initialStateError,
            int measurementVectorDimension,
            int controlVectorDimension,
            Func<TState, double[]> stateConvertFunc,
            Func<double[], TState> stateConvertBackFunc,
            Func<TMeasurement, double[]> measurementConvertFunc)
        {
            double[] numArray = stateConvertFunc(initialState);
            this.StateVectorDimension = numArray.Length;
            this.MeasurementVectorDimension = measurementVectorDimension;
            this.ControlVectorDimension = controlVectorDimension;
            this.state = numArray;
            this.EstimateCovariance = initialStateError;
            this.stateConvertFunc = stateConvertFunc;
            this.stateConvertBackFunc = stateConvertBackFunc;
            this.measurementConvertFunc = measurementConvertFunc;
        }

        public void Predict() => Predict(null);

        public void Predict(double[] controlVector) => PredictInternal(controlVector);

        public void Correct(TMeasurement measurement) => CorrectInternal(measurementConvertFunc(measurement));

        protected abstract void PredictInternal(double[] controlVector);

        protected abstract void CorrectInternal(double[] measurement);
    }
}