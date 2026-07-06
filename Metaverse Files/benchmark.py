import time
import requests

BASE = "http://localhost:5002"

def bench_predict(n=50):
    latencies = []
    for _ in range(n):
        t0 = time.time()
        r = requests.post(f"{BASE}/predict", json={"text": "I am happy today!"})
        latencies.append(time.time() - t0)
    print(f"/predict avg latency (ms): {sum(latencies)/n*1000:.1f}")

def bench_quiz(n=20):
    payload = {
        "topic": "Photosynthesis",
        "difficulty": "Medium",
        "chunk_text": [
            "Plants convert light into chemical energy.",
            "Chlorophyll absorbs light.",
            "Oxygen is released as a byproduct."
        ]
    }
    latencies = []
    for _ in range(n):
        t0 = time.time()
        r = requests.post(f"{BASE}/generate_quiz", json=payload)
        latencies.append(time.time() - t0)
    print(f"/generate_quiz avg latency (ms): {sum(latencies)/n*1000:.1f}")

def bench_topic_chunk(n=20):
    payload = {"text": "I'm really curious about renewable energy and electric vehicles."}
    latencies = []
    for _ in range(n):
        t0 = time.time()
        r = requests.post(f"{BASE}/generate_topic_chunk", json=payload)
        latencies.append(time.time() - t0)
    print(f"/generate_topic_chunk avg latency (ms): {sum(latencies)/n*1000:.1f}")

if __name__ == "__main__":
    bench_predict()
    bench_quiz()
    bench_topic_chunk()
