from flask import Flask, request, jsonify
from transformers import AutoTokenizer, AutoModelForSequenceClassification, AutoModelForCausalLM
import torch
import logging
import json
import time

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

app = Flask(__name__)

model_name = "./distilbert-finetuned-emotion/checkpoint-1000"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForSequenceClassification.from_pretrained(model_name)

class_labels = ["sadness", "joy", "love", "anger", "fear", "surprise"]

qwen_directory = "./Qwen2.5-0.5B-Instruct"
qwen_model = AutoModelForCausalLM.from_pretrained(
    qwen_directory,
    torch_dtype=torch.float32,
    device_map="cuda:0" if torch.cuda.is_available() else "cpu"
)
qwen_tokenizer = AutoTokenizer.from_pretrained(qwen_directory)

@app.route("/predict", methods=["POST"])
def predict():
    t0 = time.time()
    data = request.json
    if not data or "text" not in data:
        return jsonify({"error": "No text provided"}), 400

    text = data["text"]

    inputs = tokenizer(text, return_tensors="pt", padding=True, truncation=True)
    outputs = model(**inputs)
    

    logits = outputs.logits
    predicted_class = torch.argmax(logits, dim=1).item()
    predicted_label = class_labels[predicted_class]

    duration = (time.time() - t0) * 1000
    logger.info(f"Sentiment-analysis whole pipeline took: {duration:.1f} ms")

    return jsonify({
        "text": text,
        "predicted_class": predicted_class,
        "predicted_label": predicted_label,
        "latency_ms": duration
    })


def format_quiz_prompt(topic, chunk_text, difficulty):
    chunks_formatted = "\n".join([f"[Chunk {i+1}]: {txt}" for i, txt in enumerate(chunk_text)])
    prompt = f"""
Generate a questionnaire with 3 quiz questions based on the following topic, difficulty, and context.
Output must be a valid JSON object with the following structure, and each question must include exactly 3 incorrect answers and only 1 correct answer strictly:

Example:
{{
  "difficulty": "Easy",
  "questions": [
    {{
      "question": "What does the theory of relativity describe?",
      "answers": {{
          "correct": "It explains the laws of gravity and the relationship between space and time.",
          "incorrect": [
              "It describes how planets orbit the sun.",
              "It explains the water cycle.",
              "It predicts weather patterns."
          ]
      }}
    }},
    ... (generate 2 more questions)
  ]
}}

Now, generate a questionnaire in the same format. STRICTLY EACH QUESTION MUST HAVE 3 INCORRECT ANSWERS AND ONLY 1 CORRECT ANSWER.

Topic: {topic}
Difficulty: {difficulty}

Context:
{chunks_formatted}

### Answer:
"""
    return prompt.strip()

def generate_quiz(topic, chunk_text, difficulty):
    start_time = time.time()
    prompt = format_quiz_prompt(topic, chunk_text, difficulty)
    logger.info("Quiz prompt formatting took: %.2f seconds", time.time() - start_time)
    logger.info("Quiz prompt: %s", prompt)
    
    messages = [
        {
            "role": "system", 
            "content": (
                "You are an educational assistant specialized in generating questionnaires for learning purposes. "
                "Your task is to output a questionnaire in a strict JSON format containing 3 quiz questions, "
                "each with one correct answer and three incorrect answers strictly. Follow the provided example exactly and output only valid JSON."
            )
        },
        {"role": "user", "content": prompt}
    ]
    
    start_time = time.time()
    text = qwen_tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
    logger.info("Quiz chat template application took: %.2f seconds", time.time() - start_time)
    
    start_time = time.time()
    model_inputs = qwen_tokenizer([text], return_tensors="pt")
    model_inputs = {k: v.to(qwen_model.device) for k, v in model_inputs.items()}
    logger.info("Quiz tokenization took: %.2f seconds", time.time() - start_time)
    
    start_time = time.time()
    if torch.cuda.is_available():
        torch.cuda.synchronize()
    generated_ids = qwen_model.generate(**model_inputs, max_new_tokens=512)
    if torch.cuda.is_available():
        torch.cuda.synchronize()
    logger.info("Quiz generation took: %.2f seconds", time.time() - start_time)
    
    start_time = time.time()
    input_length = model_inputs["input_ids"].shape[1]
    trimmed_ids = [output_ids[input_length:] for output_ids in generated_ids]
    generated_text = qwen_tokenizer.batch_decode(trimmed_ids, skip_special_tokens=True)[0]
    logger.info("Quiz post-processing took: %.2f seconds", time.time() - start_time)
    
    generated_text = remove_markdown_fences(generated_text)
    logger.info("Generated quiz: %s", generated_text)
    return generated_text

def format_topic_chunk_prompt(userText):
    prompt = f"""
Analyze the following text, determine the primary topic using only key nouns or a short phrase, and generate at least three key text chunks that provide additional relevant insights not explicitly stated in the text.

Output a valid JSON object with the following structure:
{{
  "topic": "<primary topic (short, noun-based)>",
  "chunks": [
      "<key text chunk 1>",
      "<key text chunk 2>",
      "<key text chunk 3>",
      "... additional chunks if relevant"
  ]
}}

Example Input:
{{
  "text": "I'm really curious about renewable energy and electric vehicles."
}}

Example Output:
{{
  "topic": "Renewable Energy, Electric Vehicles",
  "chunks": [
      "Renewable energy sources such as solar, wind, and hydro are transforming global energy systems.",
      "Electric vehicles are growing in popularity due to advances in battery technology.",
      "Government incentives and environmental concerns drive the shift towards sustainable transportation."
  ]
}}

Now, analyze the text below and produce the JSON accordingly.

Text: "{userText}"

### Answer:
"""
    return prompt.strip()

def generate_topic_chunk(userText):
    stringPrompt = format_topic_chunk_prompt(userText)
    logger.info("Topic/Chunk prompt: %s", stringPrompt)
    
    messages = [
        {"role": "system", "content": "You are an intelligent assistant specializing in extracting the primary topic and key information from text."},
        {"role": "user", "content": stringPrompt}
    ]
    
    text = qwen_tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
    logger.info("Topic/Chunk chat template applied: %s", text)
    
    model_inputs = qwen_tokenizer([text], return_tensors="pt")
    model_inputs = {k: v.to(qwen_model.device) for k, v in model_inputs.items()}
    
    if torch.cuda.is_available():
        torch.cuda.synchronize()
    generated_ids = qwen_model.generate(**model_inputs, max_new_tokens=256)
    if torch.cuda.is_available():
        torch.cuda.synchronize()
    
    input_length = model_inputs["input_ids"].shape[1]
    trimmed_ids = [output_ids[input_length:] for output_ids in generated_ids]
    generated_text = qwen_tokenizer.batch_decode(trimmed_ids, skip_special_tokens=True)[0]
    generated_text = remove_markdown_fences(generated_text)
    logger.info("Generated topic/chunk: %s", generated_text)
    return generated_text

def remove_markdown_fences(text):
    text = text.strip()
    if text.startswith("```"):
        lines = text.splitlines()
        if lines[0].startswith("```"):
            if lines[-1].startswith("```"):
                lines = lines[1:-1]
            else:
                lines = lines[1:]
        text = "\n".join(lines).strip()
    return text

@app.route("/generate_quiz", methods=["POST"])
def generate_quiz_endpoint():
    t_start = time.time()
    data = request.get_json()
    if data is None or "topic" not in data or "chunk_text" not in data or "difficulty" not in data:
        return jsonify({"error": "Missing 'topic', 'chunk_text', or 'difficulty' parameter"}), 400
    
    topic = data["topic"]
    difficulty = data["difficulty"]
    chunkText = data["chunk_text"]
    
    generatedQuiz = generate_quiz(topic, chunkText, difficulty)
    total_ms = (time.time() - t_start) * 1000
    logger.info(f"End-to-end quiz generation took: {total_ms:.1f} ms")
    try:
        quizJson = json.loads(generatedQuiz)
        quizJson["latency_ms"] = total_ms
        return jsonify(quizJson)
    except Exception as e:
        logger.error("Failed to parse generated output as JSON: " + str(e))
        return jsonify({
            "generated_quiz": generatedQuiz,
            "latency_ms": total_ms
        })

@app.route("/generate_topic_chunk", methods=["POST"])
def generate_topic_chunk_endpoint():
    t_start = time.time()
    data = request.get_json()
    if data is None or "text" not in data:
        return jsonify({"error": "Missing 'text' parameter"}), 400
    userText = data["text"]
    generatedTopicChunk = generate_topic_chunk(userText)
    total_ms = (time.time() - t_start) * 1000
    logger.info(f"End-to-end topic/chunk generation took: {total_ms:.1f} ms")
    try:
        topicChunkJson = json.loads(generatedTopicChunk)
        topicChunkJson["latency_ms"] = total_ms
        return jsonify(topicChunkJson)
    except Exception as e:
        logger.error("Failed to parse generated topic/chunk output as JSON: " + str(e))
        return jsonify({
            "generated_topic_chunk": generatedTopicChunk,
            "latency_ms": total_ms
        })

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5002)
