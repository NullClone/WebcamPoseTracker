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


        public static float[,] LoadAnchors(string csv)
        {
            var anchors = csv.Split('\n');
            var result = new float[anchors.Length - 1, 4];

            for (int i = 0; i < anchors.Length - 1; i++)
            {
                var anchorValues = anchors[i].Split(',');

                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = float.Parse(anchorValues[j]);
                }
            }

            return result;
        }
    }
}