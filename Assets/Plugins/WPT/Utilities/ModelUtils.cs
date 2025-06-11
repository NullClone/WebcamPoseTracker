using Unity.InferenceEngine;

namespace WPT.Utilities
{
    public static class ModelUtils
    {
        public static (FunctionalTensor, FunctionalTensor, FunctionalTensor) ArgMaxFiltering(FunctionalTensor rawBoxes, FunctionalTensor rawScores)
        {
            var detectionScores = ScoreFiltering(rawScores, 100f);
            var bestScoreIndex = Functional.ArgMax(rawScores, 1).Squeeze();

            var selectedBoxes = Functional.IndexSelect(rawBoxes, 1, bestScoreIndex).Unsqueeze(0);
            var selectedScores = Functional.IndexSelect(detectionScores, 1, bestScoreIndex).Unsqueeze(0);

            return (bestScoreIndex, selectedScores, selectedBoxes);
        }

        public static FunctionalTensor ScoreFiltering(FunctionalTensor rawScores, float scoreThreshold)
        {
            return Functional.Sigmoid(Functional.Clamp(rawScores, -scoreThreshold, scoreThreshold));
        }
    }
}