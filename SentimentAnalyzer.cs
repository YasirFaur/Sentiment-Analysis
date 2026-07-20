using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace Sentiment
{
    // Define the clean multiclass input schema
    public class SentimentData
    {
        [LoadColumn(0)]
        public string SentimentText { get; set; }

        [LoadColumn(1)]
        public string EmotionLabel { get; set; }
    }

    // Define the clean multiclass output schema for predictions
    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }

        public float[] Score { get; set; }
    }

    public class SentimentAnalyzer
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<SentimentData, SentimentPrediction> _predictionEngine;

        // Stores the order of emotion labels from the model
        private readonly string[] _labelOrder;

        // Path to save the trained model locally
        private const string ModelPath = "emotion_model.zip";

        // Path to the external training data file (tab-separated: SentimentText<TAB>EmotionLabel)
        private const string TrainingDataPath = "sentiment-data.tsv";

        // Minimum confidence required to accept an emotion classification
        private const float ConfidenceThreshold = 0.30f;

        public SentimentAnalyzer()
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
                // Load training samples from an external TSV file
                var trainingData = _mlContext.Data.LoadFromTextFile<SentimentData>(
                    path: TrainingDataPath,
                    separatorChar: '\t',
                    hasHeader: true,
                    allowQuoting: true);

                // Build the training pipeline converting text to features and adding the trainer
                var pipeline = _mlContext.Transforms.Conversion
                    .MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(SentimentData.EmotionLabel))
                    .Append(_mlContext.Transforms.Text.FeaturizeText(
                        outputColumnName: "Features",
                        options: new TextFeaturizingEstimator.Options
                        {
                            // Remove common English stop words to focus on discriminative, emotional words
                            StopWordsRemoverOptions = new StopWordsRemovingEstimator.Options(),
                            WordFeatureExtractor = new WordBagEstimator.Options
                            {
                                NgramLength = 2,
                                UseAllLengths = true
                            }
                        },
                        inputColumnNames: nameof(SentimentData.SentimentText)))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: "Label", featureColumnName: "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                        outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));

                // Train the model using the built pipeline and dataset
                model = pipeline.Fit(trainingData);
                schema = trainingData.Schema;

                // Save the trained model to a file for future runs
                _mlContext.Model.Save(model, schema, ModelPath);
            }

            // Create the prediction engine to evaluate new text inputs
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

            // Extract the slot names to match scores with correct labels
            VBuffer<ReadOnlyMemory<char>> keys = default;
            _predictionEngine.OutputSchema["Score"].Annotations.GetValue("SlotNames", ref keys);

            // Convert the extracted slot keys into a string array
            _labelOrder = keys.DenseValues().Select(k => k.ToString()).ToArray();
        }

        // Main method to analyze input text and return full sentiment details
        public SentimentResult AnalyzeSentiment(string text)
        {
            // Return default empty result if input text is empty or null
            if (string.IsNullOrWhiteSpace(text))
                return new SentimentResult { Emotion = "Unknown", Confidence = 0f, Distribution = null };

            // Run the prediction engine on the provided input text
            var r = _predictionEngine.Predict(new SentimentData { SentimentText = text });

            // Find the highest confidence score among all labels
            var maxScore = r.Score.Max();

            // Map each label to its corresponding score in a dictionary
            var distribution = _labelOrder
                .Zip(r.Score, (label, score) => (label, score))
                .ToDictionary(x => x.label, x => x.score);

            // Fallback to Neutral if the maximum score is below the threshold
            var finalEmotion = maxScore < ConfidenceThreshold ? "Neutral" : r.Prediction;

            // Return the structured object with all computed data
            return new SentimentResult
            {
                Emotion = finalEmotion,
                Confidence = maxScore,
                Distribution = distribution
            };
        }
    }

    // Class to structure the detailed analysis output
    public class SentimentResult
    {
        public string Emotion { get; set; }
        public float Confidence { get; set; }
        public System.Collections.Generic.Dictionary<string, float> Distribution { get; set; }
    }
}