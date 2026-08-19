using System;
using Sentiment;
using System.Text;

namespace Sentiment
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processor = new TextProcessor();
            var sentimentAnalyzer = new SentimentAnalyzer();
            var category_analyzer = new CategoryAnalyzer();
            var language_level = new LanguageLevelAnalyzer();

            Console.WriteLine("=== Advanced Text & Content Analysis Framework ===");
            Console.WriteLine("Please enter the text to analyze (between 256 and 1024 characters):");

            // Create a builder to hold multiple lines of text
            StringBuilder sb = new StringBuilder();

            // Create a variable to store each line
            string line;

            // Read lines until the user presses Enter on an empty line
            while (!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                // Add the current line to our text builder
                sb.AppendLine(line);
            }

            // Convert the builder content to string and remove extra spaces
            string user_input = sb.ToString().Trim();


            if (processor.IsValidLength(user_input))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[✓] Valid text length: ({user_input.Length} characters)");
                Console.ResetColor();


                // 1. Run all three analysis pipelines concurrently in parallel tasks
                var categoryTask = Task.Run(() => category_analyzer.AnalyzeActivity(user_input));
                var levelTask = Task.Run(() => language_level.AnalyzeLevel(user_input));
                var sentimentTask = Task.Run(() => sentimentAnalyzer.AnalyzeSentiment(user_input));

                // Wait for all three tasks to finish execution
                await Task.WhenAll(categoryTask, levelTask, sentimentTask);

                // Extract the results
                var cat_result = categoryTask.Result;
                var level_result = levelTask.Result;
                var sentResult = sentimentTask.Result;

                // 2. Display Content Classification (STEEPLED)
                string category = cat_result.Category;
                string category_confidence_text = $" (confidence: {cat_result.Confidence:P0})";
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"\n[📊 Content Classification: {category}{category_confidence_text}]");

                // 3. Display Language Level Classification (CEFR)
                string level = level_result.Level;
                string level_confidence_text = $" (confidence: {level_result.Confidence:P0})";
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[🎓 Language Level: {level}{level_confidence_text}]");

                // 4. Display Text Energy & Emotion Results
                string emotion = sentResult.Emotion;
                float confidence = sentResult.Confidence;
                string confidenceText = $" (confidence: {confidence:P0})";
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[⚡ Sentiment/Emotion: {emotion}{confidenceText}]");
                Console.ResetColor();

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
                Console.WriteLine($"\n[✕] Invalid text: Length is {user_input?.Length ?? 0} chars. (Must be 256-1024)");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}