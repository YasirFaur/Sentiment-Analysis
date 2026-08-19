using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace Sentiment
{
    // Define the input schema for language level classification
    public class LanguageLevelData
    {
        [LoadColumn(0)]
        public string TextContent { get; set; }

        [LoadColumn(1)]
        public string LevelLabel { get; set; }
    }

    // Define the output prediction schema
    public class LanguageLevelPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }

        public float[] Score { get; set; }
    }

    public class LanguageLevelAnalyzer
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<LanguageLevelData, LanguageLevelPrediction> _predictionEngine;

        // Stores the order of language levels (A1, A2, B1, B2, C1, C2) from the model
        private readonly string[] _labelOrder;

        // Path to save the trained language model locally
        private const string ModelPath = "language_level_model.zip";

        // Path to external training data file (tab-separated: TextContent<TAB>LevelLabel)
        private const string TrainingDataPath = "language-level-training-data.tsv";

        // Minimum confidence required to accept a level (Fair distribution across 6 levels is ~16.6%)
        private const float ConfidenceThreshold = 0.25f;

        public LanguageLevelAnalyzer()
        {
            // Initialize MLContext with a fixed seed for reproducible results
            _mlContext = new MLContext(seed: 1);

            ITransformer model;
            DataViewSchema schema;

            // Check if a trained model already exists on the disk
            if (File.Exists(ModelPath))
            {
                // Load the existing model to skip the training phase
                model = _mlContext.Model.Load(ModelPath, out schema);
            }
            else
            {
                // Load training samples from external TSV file
                var trainingData = _mlContext.Data.LoadFromTextFile<LanguageLevelData>(
                    path: TrainingDataPath,
                    separatorChar: '\t',
                    hasHeader: true,
                    allowQuoting: true);

                // Build pipeline: extract word and character N-Grams to capture complex vocabulary patterns
                var pipeline = _mlContext.Transforms.Conversion
                    .MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(LanguageLevelData.LevelLabel))
                    .Append(_mlContext.Transforms.Text.FeaturizeText(
                        outputColumnName: "Features",
                        options: new TextFeaturizingEstimator.Options
                        {
                            WordFeatureExtractor = new WordBagEstimator.Options
                            {
                                NgramLength = 2,
                                UseAllLengths = true
                            }
                        },
                        inputColumnNames: nameof(LanguageLevelData.TextContent)
                        )
                    )
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: "Label", featureColumnName: "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                        outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));

                // Train the model
                model = pipeline.Fit(trainingData);
                schema = trainingData.Schema;

                // Save the model for future runs
                _mlContext.Model.Save(model, schema, ModelPath);
            }

            // Create prediction engine
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<LanguageLevelData, LanguageLevelPrediction>(model);

            // Extract slot names for label matching
            VBuffer<ReadOnlyMemory<char>> keys = default;
            _predictionEngine.OutputSchema["Score"].Annotations.GetValue("SlotNames", ref keys);

            // Convert keys to string array
            _labelOrder = keys.DenseValues().Select(k => k.ToString()).ToArray();
        }

        // Main method to evaluate language level
        public LanguageLevelResult AnalyzeLevel(string text)
        {
            // Return default empty result if input is empty
            if (string.IsNullOrWhiteSpace(text))
                return new LanguageLevelResult { Level = "Unknown", Confidence = 0f, Distribution = null };

            // Run prediction
            var r = _predictionEngine.Predict(new LanguageLevelData { TextContent = text });

            // Find highest confidence score
            var maxScore = r.Score.Max();

            // Map each level label to its score
            var distribution = _labelOrder
                .Zip(r.Score, (label, score) => (label, score))
                .ToDictionary(x => x.label, x => x.score);

            // Fallback to General/Intermediate if confidence is too low
            var finalLevel = maxScore < ConfidenceThreshold ? "B1" : r.Prediction;

            return new LanguageLevelResult
            {
                Level = finalLevel,
                Confidence = maxScore,
                Distribution = distribution
            };
        }
    }

    // Structure for language level analysis result
    public class LanguageLevelResult
    {
        public string Level { get; set; }
        public float Confidence { get; set; }
        public System.Collections.Generic.Dictionary<string, float> Distribution { get; set; }
    }
}