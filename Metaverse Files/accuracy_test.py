# accuracy_test.py
import time
import requests
import json
from collections import defaultdict, Counter
import numpy as np
from sklearn.metrics import accuracy_score, precision_recall_fscore_support, confusion_matrix
import matplotlib.pyplot as plt
import seaborn as sns

BASE = "http://localhost:5002"

# Test dataset with ground truth labels
TEST_DATA = [
    # Sadness examples
    {"text": "I feel so sad and depressed today", "true_label": "sadness"},
    {"text": "This is the worst day of my life", "true_label": "sadness"},
    {"text": "I'm feeling down and lonely", "true_label": "sadness"},
    {"text": "Everything seems hopeless right now", "true_label": "sadness"},
    {"text": "I can't stop crying", "true_label": "sadness"},
    {"text": "I'm heartbroken and devastated", "true_label": "sadness"},
    {"text": "This test is making me feel overwhelmed and sad", "true_label": "sadness"},
    {"text": "I'm disappointed with my performance", "true_label": "sadness"},
    
    # Joy examples
    {"text": "I am so happy and excited!", "true_label": "joy"},
    {"text": "This is the best day ever!", "true_label": "joy"},
    {"text": "I feel amazing and full of energy", "true_label": "joy"},
    {"text": "I'm thrilled about this opportunity", "true_label": "joy"},
    {"text": "Life is wonderful and beautiful", "true_label": "joy"},
    {"text": "I passed my exam and I'm so happy!", "true_label": "joy"},
    {"text": "I feel confident and ready for this test", "true_label": "joy"},
    {"text": "This review session is going great!", "true_label": "joy"},
    
    # Love examples
    {"text": "I love spending time with my family", "true_label": "love"},
    {"text": "My heart is full of love and warmth", "true_label": "love"},
    {"text": "I adore this subject and enjoy learning", "true_label": "love"},
    {"text": "I have deep affection for this topic", "true_label": "love"},
    {"text": "I cherish these moments of studying", "true_label": "love"},
    {"text": "I'm passionate about this material", "true_label": "love"},
    {"text": "I love how this platform helps me learn", "true_label": "love"},
    {"text": "This environment makes me feel cared for", "true_label": "love"},
    
    # Anger examples
    {"text": "I'm so angry and frustrated right now", "true_label": "anger"},
    {"text": "This is completely unfair and infuriating", "true_label": "anger"},
    {"text": "I'm furious about this situation", "true_label": "anger"},
    {"text": "This test is making me so mad", "true_label": "anger"},
    {"text": "I'm irritated and annoyed", "true_label": "anger"},
    {"text": "This is ridiculous and makes me angry", "true_label": "anger"},
    {"text": "I'm fed up with these difficult questions", "true_label": "anger"},
    {"text": "This system is frustrating me", "true_label": "anger"},
    
    # Fear examples
    {"text": "I'm scared and anxious about this test", "true_label": "fear"},
    {"text": "I feel nervous and worried", "true_label": "fear"},
    {"text": "This makes me feel afraid and uncertain", "true_label": "fear"},
    {"text": "I'm terrified of failing", "true_label": "fear"},
    {"text": "I have test anxiety and feel panicked", "true_label": "fear"},
    {"text": "I'm worried I won't remember anything", "true_label": "fear"},
    {"text": "This environment makes me feel uneasy", "true_label": "fear"},
    {"text": "I'm afraid I'm not prepared enough", "true_label": "fear"},
    
    # Surprise examples
    {"text": "Wow, I didn't expect that result!", "true_label": "surprise"},
    {"text": "That's amazing and unexpected", "true_label": "surprise"},
    {"text": "I'm shocked by this outcome", "true_label": "surprise"},
    {"text": "What a pleasant surprise this is", "true_label": "surprise"},
    {"text": "I'm astonished by how well this works", "true_label": "surprise"},
    {"text": "This is surprisingly effective", "true_label": "surprise"},
    {"text": "I'm amazed by this platform's capabilities", "true_label": "surprise"},
    {"text": "This caught me completely off guard", "true_label": "surprise"},
]

class EmotionAccuracyTester:
    def __init__(self, base_url=BASE):
        self.base_url = base_url
        self.class_labels = ["sadness", "joy", "love", "anger", "fear", "surprise"]
        self.results = []
        
    def predict_emotion(self, text):
        """Send prediction request to the API"""
        try:
            response = requests.post(
                f"{self.base_url}/predict", 
                json={"text": text},
                timeout=10
            )
            if response.status_code == 200:
                return response.json()
            else:
                print(f"Error: {response.status_code} - {response.text}")
                return None
        except Exception as e:
            print(f"Request failed: {e}")
            return None
    
    def run_accuracy_test(self, test_data=TEST_DATA):
        """Run accuracy test on the provided dataset"""
        print("Running Emotion Analysis Accuracy Test...")
        print("=" * 50)
        
        true_labels = []
        predicted_labels = []
        latencies = []
        
        for i, item in enumerate(test_data):
            text = item["text"]
            true_label = item["true_label"]
            
            print(f"Testing {i+1}/{len(test_data)}: {text[:50]}...")
            
            start_time = time.time()
            result = self.predict_emotion(text)
            end_time = time.time()
            
            if result:
                predicted_label = result["predicted_label"]
                latency = result.get("latency_ms", (end_time - start_time) * 1000)
                
                true_labels.append(true_label)
                predicted_labels.append(predicted_label)
                latencies.append(latency)
                
                # Store detailed results
                self.results.append({
                    "text": text,
                    "true_label": true_label,
                    "predicted_label": predicted_label,
                    "correct": true_label == predicted_label,
                    "latency_ms": latency
                })
                
                status = "✓" if true_label == predicted_label else "✗"
                print(f"  {status} True: {true_label}, Predicted: {predicted_label}")
            else:
                print(f"  ✗ Failed to get prediction")
        
        return true_labels, predicted_labels, latencies
    
    def calculate_metrics(self, true_labels, predicted_labels):
        """Calculate accuracy metrics"""
        if not true_labels or not predicted_labels:
            print("No valid predictions to calculate metrics")
            return
        
        # Overall accuracy
        accuracy = accuracy_score(true_labels, predicted_labels)
        
        # Precision, Recall, F1-score for each class
        precision, recall, f1, support = precision_recall_fscore_support(
            true_labels, predicted_labels, 
            labels=self.class_labels, 
            average=None, 
            zero_division=0
        )
        
        # Macro averages
        macro_precision, macro_recall, macro_f1, _ = precision_recall_fscore_support(
            true_labels, predicted_labels, 
            average='macro', 
            zero_division=0
        )
        
        return {
            "accuracy": accuracy,
            "precision": precision,
            "recall": recall,
            "f1": f1,
            "support": support,
            "macro_precision": macro_precision,
            "macro_recall": macro_recall,
            "macro_f1": macro_f1
        }
    
    def print_results(self, true_labels, predicted_labels, latencies, metrics):
        """Print comprehensive results"""
        print("\n" + "=" * 60)
        print("EMOTION ANALYSIS ACCURACY RESULTS")
        print("=" * 60)
        
        # Overall metrics
        print(f"\nOVERALL PERFORMANCE:")
        print(f"  Total Samples: {len(true_labels)}")
        print(f"  Accuracy: {metrics['accuracy']:.3f} ({metrics['accuracy']*100:.1f}%)")
        print(f"  Macro Precision: {metrics['macro_precision']:.3f}")
        print(f"  Macro Recall: {metrics['macro_recall']:.3f}")
        print(f"  Macro F1-Score: {metrics['macro_f1']:.3f}")
        
        # Per-class metrics
        print(f"\nPER-CLASS PERFORMANCE:")
        print(f"{'Emotion':<12} {'Precision':<10} {'Recall':<10} {'F1-Score':<10} {'Support':<8}")
        print("-" * 55)
        for i, emotion in enumerate(self.class_labels):
            print(f"{emotion:<12} {metrics['precision'][i]:<10.3f} {metrics['recall'][i]:<10.3f} "
                  f"{metrics['f1'][i]:<10.3f} {metrics['support'][i]:<8.0f}")
        
        # Performance metrics
        print(f"\nPERFORMANCE METRICS:")
        print(f"  Average Latency: {np.mean(latencies):.1f} ms")
        print(f"  Median Latency: {np.median(latencies):.1f} ms")
        print(f"  Min Latency: {np.min(latencies):.1f} ms")
        print(f"  Max Latency: {np.max(latencies):.1f} ms")
        
        # Confusion matrix
        cm = confusion_matrix(true_labels, predicted_labels, labels=self.class_labels)
        print(f"\nCONFUSION MATRIX:")
        print("Predicted →")
        print(f"{'True ↓':<12}", end="")
        for label in self.class_labels:
            print(f"{label:<8}", end="")
        print()
        
        for i, true_label in enumerate(self.class_labels):
            print(f"{true_label:<12}", end="")
            for j in range(len(self.class_labels)):
                print(f"{cm[i][j]:<8}", end="")
            print()
    
    def save_results_to_file(self, filename="emotion_accuracy_results.json"):
        """Save detailed results to JSON file"""
        results_summary = {
            "test_summary": {
                "total_samples": len(self.results),
                "correct_predictions": sum(1 for r in self.results if r["correct"]),
                "accuracy": sum(1 for r in self.results if r["correct"]) / len(self.results) if self.results else 0,
                "average_latency_ms": np.mean([r["latency_ms"] for r in self.results]) if self.results else 0
            },
            "detailed_results": self.results
        }
        
        with open(filename, 'w') as f:
            json.dump(results_summary, f, indent=2)
        
        print(f"\nDetailed results saved to: {filename}")

def main():
    """Main function to run the accuracy test"""
    tester = EmotionAccuracyTester()
    
    # Run the test
    true_labels, predicted_labels, latencies = tester.run_accuracy_test()
    
    if true_labels and predicted_labels:
        # Calculate metrics
        metrics = tester.calculate_metrics(true_labels, predicted_labels)
        
        # Print results
        tester.print_results(true_labels, predicted_labels, latencies, metrics)
        
        # Save results
        tester.save_results_to_file()
        
        print(f"\nAccuracy test completed successfully!")
        print(f"Final Accuracy: {metrics['accuracy']*100:.1f}%")
    else:
        print("No valid predictions obtained. Please check your server connection.")

if __name__ == "__main__":
    main()