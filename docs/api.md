# API reference and setup details

## Backend (accounts API)

```bash
cd Backend
npm install
npm start
```

Create `Backend/.env` first (there is an example env file in the folder):

```
PORT=3000
DBUSER=<mongodb user>
DBPASS=<mongodb password>
```

## AI service (emotion and generation)

```bash
cd "Metaverse Files"
python -m venv venv && source venv/Scripts/activate   # Windows Git Bash
pip install -r requirements.txt
python appp.py          # serves on http://localhost:5002
```

Model weights are not committed. Before the first run:

- produce the emotion model with `python fine_tune_emotion.py` (writes `distilbert-finetuned-emotion/`), and
- download `Qwen/Qwen2.5-0.5B-Instruct` into `Metaverse Files/Qwen2.5-0.5B-Instruct/`.

## Unity client

Open `Frontend/Metaverse/` as a project in Unity. Start the AI service first. The committed client does not point at `http://localhost:5002`: `EmotionAnalyzer.cs`, `AdaptiveQuiz.cs` and `TopicChooser.cs` hard-code an ngrok URL, so set their endpoint fields to `http://localhost:5002/...` (see the table in the README), and set the register and login URLs in `RegisterUser.cs` and `LoginUser.cs` to your own backend.

## Endpoints

| Method | Endpoint | Body | Returns |
|---|---|---|---|
| POST | `/predict` | `{ "text": "..." }` | predicted emotion label, class index, latency |
| POST | `/generate_quiz` | `{ "topic", "chunk_text", "difficulty" }` | 3-question quiz as JSON |
| POST | `/generate_topic_chunk` | `{ "text": "..." }` | primary topic and key chunks as JSON |
| POST | `/user/register` | `{ "name", "email", "password" }` | created user |
| POST | `/user/login` | `{ "email", "password" }` | auth result |

`/predict`, `/generate_quiz` and `/generate_topic_chunk` are served by the Flask app on port 5002. The two `/user/*` routes and `GET /` (health) are served by the Express backend.

## Emotion model

`fine_tune_emotion.py` fine-tunes `distilbert-base-uncased` on the Hugging Face `emotion` dataset, optionally augmented with a custom `additional_data.xlsx`, over six classes (sadness, joy, love, anger, fear, surprise): 2 epochs, learning rate 2e-5, batch 32, fp16. `accuracy_test.py` sends 48 hand-written sentences (eight per class) to the live API and prints per-class precision and recall, a confusion matrix and latency. `benchmark.py` times `/predict` (50 calls) and `/generate_quiz` (20 calls).

The `additional_data.xlsx` file is not in this repo, so re-running the script trains on the Hugging Face dataset alone. Supply your own `text`/`label` sheet in `Metaverse Files/` to reproduce the augmented run.

## Repository structure

```
MAGSEL/
├── Backend/            Node/Express + MongoDB API (accounts)
├── Frontend/Metaverse/ Unity client (Assets/Scripts/Terrain, EmotionAnalyzer.cs)
└── Metaverse Files/    Python AI service and training
    ├── appp.py                 Flask API (:5002)
    ├── fine_tune_emotion.py    DistilBERT fine-tuning
    ├── accuracy_test.py        eval harness
    └── benchmark.py            latency benchmark
```
