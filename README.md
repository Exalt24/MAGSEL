# MAGSEL

A metaverse for gender-sensitive social-emotional education. Students learn inside a Unity 3D world that reads their emotional state in real time and adapts to it. Solo project, presented at the WILLS 2025 Conference, Kyoto University of Foreign Studies, Japan.

## What's in here

```
Backend/          Node/Express + MongoDB API (accounts, auth)
Frontend/         Unity 3D metaverse client
Metaverse Files/  Python ML pipeline (emotion model + Flask inference API)
```

## Highlights

- **Procedural terrain with Wave Function Collapse.** The world's terrain is generated with a WFC solver written from scratch in C#, with a networked terrain manager so the environment stays consistent across clients (`Frontend/Metaverse/Assets/Scripts/Terrain/` — `WaveFunctionCollapse.cs`, `Cell.cs`, `NetworkTerrainManager.cs`, `TerrainBenchmark.cs`).
- **Emotion-adaptive learning.** A DistilBERT classifier fine-tuned on six emotions (sadness, joy, love, anger, fear, surprise) reads student text and the metaverse responds to it. Reported ~92% accuracy / 0.93 F1 on the eval set (`Metaverse Files/fine_tune_emotion.py`, `accuracy_test.py`).
- **Real-time inference.** The model is served over a Flask API (`Metaverse Files/appp.py`, `POST /predict`) and consumed from Unity (`Frontend/Metaverse/Assets/Scripts/EmotionAnalyzer.cs`), returning a predicted label plus latency.

## Tech

Unity, C#, Python, PyTorch, Hugging Face Transformers, DistilBERT, Flask, Node.js, Express, MongoDB.

## Notes

Large model weights, base models, and binary art assets are kept out of the repo (see `.gitignore`); the fine-tuned emotion model is reproducible by running `Metaverse Files/fine_tune_emotion.py`. Backend secrets load from a local `.env` (not committed).
