"""Fine-tune DistilBERT on the Hugging Face emotion dataset (six labels).

Runs on CPU or CUDA with no extra data files:

    python fine_tune_emotion.py                      # full recipe, 2 epochs
    python fine_tune_emotion.py --max-train-samples 2000 --epochs 1   # quick smoke run
    python fine_tune_emotion.py --plots              # also show the two class plots

It writes the model to ./distilbert-finetuned-emotion/ (checkpoints inside, final
weights at the top level). appp.py loads checkpoint-1000, which the full
2-epoch recipe produces (16,000 training rows / batch 32 = 500 steps per epoch).
"""
import argparse
import inspect

import numpy as np
import torch
from datasets import load_dataset
from sklearn.metrics import accuracy_score, classification_report, f1_score
from transformers import (
    AutoModelForSequenceClassification,
    AutoTokenizer,
    Trainer,
    TrainingArguments,
)

parser = argparse.ArgumentParser()
parser.add_argument("--epochs", type=int, default=2)
parser.add_argument("--output-dir", default="distilbert-finetuned-emotion")
parser.add_argument("--max-train-samples", type=int, default=None,
                    help="train on the first N rows only (smoke test)")
parser.add_argument("--plots", action="store_true",
                    help="show the class-frequency and words-per-tweet plots (blocks until closed)")
args = parser.parse_args()

# The dataset moved from "emotion" to "dair-ai/emotion"; older names no longer resolve.
emotion = load_dataset("dair-ai/emotion")
classes = emotion["train"].features["label"].names
print("Classes:", classes)

if args.plots:
    import matplotlib.pyplot as plt

    df = emotion["train"].to_pandas()
    df["label_name"] = df["label"].apply(lambda x: classes[x])
    df["label_name"].value_counts(ascending=True).plot.barh()
    plt.title("Frequency of Classes")
    plt.show()
    df["Words Per Tweet"] = df["text"].str.split().apply(len)
    df.boxplot(column="Words Per Tweet", by="label_name")
    plt.title("Words Per Tweet by Class")
    plt.suptitle("")
    plt.show()

if args.max_train_samples:
    emotion["train"] = emotion["train"].select(range(args.max_train_samples))

use_cuda = torch.cuda.is_available()
device = torch.device("cuda:0" if use_cuda else "cpu")
print("Using device:", device)

model_ckpt = "distilbert-base-uncased"
tokenizer = AutoTokenizer.from_pretrained(model_ckpt)


def tokenize(batch):
    return tokenizer(batch["text"], padding=True, truncation=True)


emotions_encoded = emotion.map(tokenize, batched=True, batch_size=None)

model = AutoModelForSequenceClassification.from_pretrained(
    model_ckpt, num_labels=len(classes)
).to(device)

# transformers renamed evaluation_strategy to eval_strategy and later dropped the old
# name, and renamed Trainer(tokenizer=...) to processing_class; pick what this install takes.
ta_params = inspect.signature(TrainingArguments.__init__).parameters
eval_kw = "eval_strategy" if "eval_strategy" in ta_params else "evaluation_strategy"
training_args = TrainingArguments(
    output_dir=args.output_dir,
    num_train_epochs=args.epochs,
    learning_rate=2e-5,
    per_device_train_batch_size=32,
    per_device_eval_batch_size=32,
    weight_decay=0.01,
    disable_tqdm=False,
    fp16=use_cuda,  # mixed precision only exists on CUDA
    **{eval_kw: "epoch"},
)


def compute_metrics(pred):
    labels = pred.label_ids
    preds = pred.predictions.argmax(-1)
    return {
        "accuracy": accuracy_score(labels, preds),
        "f1": f1_score(labels, preds, average="weighted"),
    }


tok_kw = (
    {"processing_class": tokenizer}
    if "processing_class" in inspect.signature(Trainer.__init__).parameters
    else {"tokenizer": tokenizer}
)
trainer = Trainer(
    model=model,
    args=training_args,
    compute_metrics=compute_metrics,
    train_dataset=emotions_encoded["train"],
    eval_dataset=emotions_encoded["validation"],
    **tok_kw,
)

trainer.train()
trainer.save_model(args.output_dir)

# Both splits, so the figures can be re-checked against the README.
for split in ("validation", "test"):
    out = trainer.predict(emotions_encoded[split])
    print(f"{split} metrics:", out.metrics)

out = trainer.predict(emotions_encoded["test"])
y_preds = np.argmax(out.predictions, axis=1)
y_true = emotions_encoded["test"]["label"]
print("Classification report (test):")
print(classification_report(y_true, y_preds, target_names=classes))

# Sanity prediction on one sentence.
text = "I want to kill you"
enc = tokenizer(text, return_tensors="pt").to(device)
with torch.no_grad():
    logits = model(**enc).logits
pred = torch.argmax(logits, dim=1).item()
print("Prediction:", pred, "Class:", classes[pred])
