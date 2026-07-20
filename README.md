## Sentiment Analysis System (mindINJECTION)

**Sentiment-Analysis** is a core sub-project within the **mindINJECTION** app. It works as a built-in text tool that automatically checks any text using two main areas:

1. **Eight Basic Emotions:** Joy, sadness, fear, trust, disgust, surprise, anticipation, and anger (plus neutral).
2. **Seven Human Activities:** Politics, economics, society, technology, environment, law, and ethics (plus neutral).

Through this text analysis, the app gives users helpful insights and tips. This helps improve their experience when memorizing and reviewing ideas using spaced repetition.

## 🚀 How to Run
1. Ensure `sentiment-data.tsv` is in your build directory.
2. Clear any old `emotion_model.zip` to force the engine to retrain.
3. Build and execute the C# solution.