# Import necessary libraries
import os
import pandas as pd
import torch
from datasets import load_dataset, Dataset, concatenate_datasets, ClassLabel
from transformers import (
    AutoTokenizer,
    AutoModelForSequenceClassification,
    TrainingArguments,
    Trainer,
)
from sklearn.metrics import accuracy_score, f1_score
import matplotlib.pyplot as plt
import numpy as np

# Limit GPU usage
os.environ['CUDA_VISIBLE_DEVICES'] = '0'  # Use GPU 0
os.environ['PYTORCH_CUDA_ALLOC_CONF'] = 'max_split_size_mb:1024'  # Increase to 1GB

# Check device
device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")
print("Using device:", device)

from datasets import load_dataset, Dataset
import pandas as pd

# Load the emotion dataset
emotion = load_dataset("emotion")
emotion_train = emotion["train"]  # Focus only on the 'train' split for now

# Load the Excel file
additional_data = pd.read_excel("additional_data.xlsx")

# Convert the DataFrame to a Hugging Face Dataset
additional_dataset = Dataset.from_pandas(additional_data)

# Get the ClassLabel feature from the original dataset
label_feature = emotion_train.features["label"]

# Convert the 'label' column in additional_dataset to use the same ClassLabel
additional_dataset = additional_dataset.map(
    lambda x: {"label": label_feature.str2int(x["label"])}  # Convert string labels to integers
    if isinstance(x["label"], str) else {"label": x["label"]}  # Retain integer labels
)

# Apply ClassLabel to the label column
additional_dataset = additional_dataset.cast_column("label", label_feature)

# Combine the datasets
emotion_train = concatenate_datasets([emotion_train, additional_dataset])


# Combine the datasets
emotion_train = concatenate_datasets([emotion_train, additional_dataset])

# Convert the updated train split to Pandas DataFrame for preview
df = emotion_train.to_pandas()  # Use 'to_pandas' directly
print(df.head())

# Extract class names from the original dataset
classes = emotion["train"].features["label"].names
print("Classes:", classes)

# Add a human-readable label name column
df["label_name"] = df["label"].apply(lambda x: classes[x])
print(df.head())

# Visualize class distribution
label_counts = df['label_name'].value_counts(ascending=True)
label_counts.plot.barh()
plt.title('Frequency of Classes')
plt.show()

# Add words per tweet column
df['Words Per Tweet'] = df['text'].str.split().apply(len)
df.boxplot(column='Words Per Tweet', by='label_name')
plt.title('Words Per Tweet by Class')
plt.suptitle('')
plt.show()

# Tokenizer setup
model_ckpt = "distilbert-base-uncased"
tokenizer = AutoTokenizer.from_pretrained(model_ckpt)

# Tokenization example
text = "I love Machine Learning! Tokenization is awesome"
encoded_text = tokenizer(text)
print("Encoded text:", encoded_text)
tokens = tokenizer.convert_ids_to_tokens(encoded_text.input_ids)
print("Tokens:", tokens)
print("Vocab size:", tokenizer.vocab_size, "Max length:", tokenizer.model_max_length)

# Reset dataset format
emotion.reset_format()

# Tokenization function
def tokenize(batch):
    return tokenizer(batch['text'], padding=True, truncation=True)

print("Tokenization sample:", tokenize(emotion['train'][:5]))

# Tokenize dataset
emotions_encoded = emotion.map(tokenize, batched=True, batch_size=None)
print(emotions_encoded)

# Define model
num_labels = len(classes)
model = AutoModelForSequenceClassification.from_pretrained(model_ckpt, num_labels=num_labels).to(device)

# Training arguments
batch_size = 32  # Increase this value
model_name = "distilbert-finetuned-emotion"
training_args = TrainingArguments(
    output_dir=model_name,
    num_train_epochs=2,
    learning_rate=2e-5,
    per_device_train_batch_size=32,  # Larger batch size
    per_device_eval_batch_size=32,
    weight_decay=0.01,
    evaluation_strategy='epoch',
    disable_tqdm=False,
    fp16=True,  # Use mixed precision
)
torch.cuda.set_per_process_memory_fraction(0.8, 0)  # Use 80% of GPU memory

# Metrics function
def compute_metrics(pred):
    labels = pred.label_ids
    preds = pred.predictions.argmax(-1)
    f1 = f1_score(labels, preds, average='weighted')
    acc = accuracy_score(labels, preds)
    return {"accuracy": acc, "f1": f1}

# Trainer setup
trainer = Trainer(
    model=model,
    args=training_args,
    compute_metrics=compute_metrics,
    train_dataset=emotions_encoded['train'],
    eval_dataset=emotions_encoded['validation'],
    tokenizer=tokenizer,
)

# Train the model
trainer.train()

# Evaluate the model
preds_outputs = trainer.predict(emotions_encoded['test'])
print("Evaluation metrics:", preds_outputs.metrics)

# Classification report
y_preds = np.argmax(preds_outputs.predictions, axis=1)
y_true = emotions_encoded['test'][:]['label']
from sklearn.metrics import classification_report
print("Classification report:")
print(classification_report(y_true, y_preds, target_names=classes))

# Test the model with new input
text = "I want to kill you"
input_encoded = tokenizer(text, return_tensors='pt').to(device)
with torch.no_grad():
    outputs = model(**input_encoded)

logits = outputs.logits
pred = torch.argmax(logits, dim=1).item()
print("Prediction:", pred, "Class:", classes[pred])
