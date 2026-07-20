using System;
using Sentiment;

namespace Sentiment
{
    class Program
    {
        static void Main()
        {
            var processor = new TextProcessor();
            var sentimentAnalyzer = new SentimentAnalyzer();
            var categoryAnalyzer = new CategoryAnalyzer();

            Console.WriteLine("=== Advanced Text & Content Analysis Framework ===");
            Console.WriteLine("Please enter the text to analyze (between 256 and 1024 characters):");

            string userInput = Console.ReadLine();

            if (processor.IsValidLength(userInput))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[✓] Valid text length: ({userInput.Length} characters)");
                Console.ResetColor();

                // 1. Analyze human activity classification (STEEPLED) and display results
                var catResult = categoryAnalyzer.AnalyzeActivity(userInput);
                string category = catResult.Category;
                string catConfText = $" (confidence: {catResult.Confidence:P0})";

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"\n[📊 Content Classification: {category}{catConfText}]");
                Console.ResetColor();

                // 2. Analyze text energy and emotions, then display results with specific styling
                var sentResult = sentimentAnalyzer.AnalyzeSentiment(userInput);
                string emotion = sentResult.Emotion;
                float confidence = sentResult.Confidence;
                string confidenceText = $" (confidence: {confidence:P0})";

                if (emotion == "Joy")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[🌱 Text energy: Joy. Calming, stable, and supports inner peace.{confidenceText}]");
                }
                else if (emotion == "Sadness")
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"[⚠️ Text energy: Sadness. Heavy or low energy. May cause emotional fatigue.{confidenceText}]");
                }
                else if (emotion == "Fear")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[⚔️ Text energy: Fear/Danger. High alert status. Keep your inner peace.{confidenceText}]");
                }
                else if (emotion == "Anger")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[🔥 Text energy: Anger. High tension or sharp words. Take a deep breath.{confidenceText}]");
                }
                else if (emotion == "Trust")
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"[🌱 Text energy: Trust. Safe, reliable, and deeply comforting for the soul.{confidenceText}]");
                }
                else if (emotion == "Disgust")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine($"[🛑 Text energy: Disgust. A strong feeling of rejection or toxicity. Keep your distance.{confidenceText}]");
                }
                else if (emotion == "Surprise")
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[⚡ Text energy: Surprise. Dynamic and unexpected. Keep your focus sharp.{confidenceText}]");
                }
                else if (emotion == "Anticipation")
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"[⏳ Text energy: Anticipation. Forward-looking and expectant. Stay grounded while you wait.{confidenceText}]");
                }
                else if (emotion == "Neutral")
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"[⚪ Text energy: Neutral. Balanced or informational tone, no strong emotional charge detected.{confidenceText}]");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[📖 Text energy: Unknown template. Read with a clear mind.{confidenceText}]");
                }

                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"\n[✕] Invalid text: Length is {userInput?.Length ?? 0} chars. (Must be 256-1024)");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}