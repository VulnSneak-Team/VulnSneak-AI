<div align="center">

# 🛡️ VulnSneak — AI Agent for Sneaking into Vulnerabilities

### **An AI-powered agent that automatically detects and repairs security vulnerabilities in frontend and backend source code.**

Final Year Graduation Project — Computer Science, Zagazig University (2025–2026)

</div>

---

## 📖 Overview

**VulnSneak** is an AI-powered security platform designed to automatically detect and repair vulnerabilities in frontend and backend source code.

Manual code audits and traditional static analysis tools (SAST/DAST) don't scale well and can produce high false-positive rates or require deep security expertise to interpret.

VulnSneak addresses this gap with a **multi-stage AI pipeline** that goes beyond detection: it locates vulnerable regions in source code and generates **context-aware, minimal-diff repairs**, while aiming to preserve the original functionality of the code.

The system combines:

* Two supervised **Transformer classifiers** for binary and 8-class vulnerability classification.
* A **region-based repair agent** powered by locally hosted LLMs with cloud fallback.
* A **Raspberry Pi security proxy** that isolates backend and AI services from direct exposure.
* A cinematic, production-grade **web interface** for real-time interaction.

---

## ✨ Key Features

* 🔍 **Two-stage vulnerability detection** — Binary classification (vulnerable/safe) followed by 8-class vulnerability-family classification, using sliding-window segmentation with **24-line chunks and stride 12** to handle large files while preserving context.
* 🎯 **Line-level precision** — Identifies vulnerable line(s) instead of flagging entire files.
* 🩹 **Automated AI repair** — Generates minimal, syntax-validated patches for vulnerable regions while leaving surrounding code untouched.
* 🔐 **Raspberry Pi security gateway** — A hardware proxy layer that filters incoming/outgoing traffic and shields backend and AI services from direct exposure.
* 💬 **Conversational scanning agent** — Supports voice interaction through WebRTC using Retell AI.
* 📄 **High-fidelity PDF export** — Generates reports with Arabic/RTL text reshaping support.
* 🎨 **Cinematic 3D interface** — WebGL shader backgrounds, GSAP ScrollTrigger, React Three Fiber particle systems, and a custom animated cursor.
* 🌗 **Dark / Light theme system** — Full theme switching throughout the application.
* 📴 **Progressive Web App (PWA)** — Offline resilience with queued scan requests, background synchronization, and push notifications on scan completion.

---

## 🧠 AI Pipeline

| Stage                 | Component                                      | Description                                                                                                                                                 |
| :-------------------- | :--------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **1. Detection**      | Binary Classifier — CodeBERT-based Transformer | Determines whether a code snippet is vulnerable or safe.                                                                                                    |
| **2. Classification** | 8-Class Vulnerability Classifier               | Identifies the vulnerability family: SQLi, XSS, command injection, insecure deserialization, path traversal, CSRF, XML injection, or insecure cryptography. |
| **3. Repair**         | Region-Based Repair Agent                      | Sends only vulnerable regions to `qwen2.5-coder:7b`, `qwen3:4b`, or `gemini-2.5-flash` as fallback models.                                                  |
| **4. Validation**     | Syntax & Security Checks                       | Validates generated patches before presenting them to the developer.                                                                                        |

### 🏋️ Training Pipeline

The model-training pipeline is built using:

* **Hugging Face Transformers**
* **PyTorch**
* Tokenization
* Label encoding
* Data augmentation
* Curated vulnerability datasets

Training data is aggregated from **Hugging Face, Kaggle, and custom-labeled examples**.

---

## 🏗️ System Architecture

```text
                         Developer
                             │
                             ▼
                  ┌─────────────────────┐
                  │ Frontend React SPA  │
                  └──────────┬──────────┘
                             │
                             ▼
              ┌─────────────────────────────┐
              │ Raspberry Pi Security Proxy │
              │ Traffic Filtering / Gateway │
              └──────────────┬──────────────┘
                             │
                             ▼
             ┌────────────────────────────────┐
             │ .NET Backend                   │
             │ ASP.NET Core + EF Core         │
             │ SQL Server                     │
             │ Auth / Storage / Orchestration │
             └───────────────┬────────────────┘
                             │
                             ▼
             ┌────────────────────────────────┐
             │ FastAPI AI Service             │
             │ Python Detection + Repair      │
             └───────────────┬────────────────┘
                             │
                ┌────────────┴────────────┐
                ▼                         ▼
      ┌───────────────────┐    ┌────────────────────────┐
      │ Detection Models  │    │ Repair Agent           │
      │                   │    │                        │
      │ Binary Classifier │    │ Qwen2.5-Coder          │
      │ 8-Class Classifier│    │ Qwen3                  │
      │                   │    │ Gemini Cloud Fallback  │
      └─────────┬─────────┘    └────────────┬───────────┘
                │                           │
                └─────────────┬─────────────┘
                              ▼
                    ┌─────────────────────┐
                    │ Validation Pipeline │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Vulnerability Report│
                    │ Code Diff + PDF     │
                    └──────────┬──────────┘
                               │
                               ▼
                         React Frontend
```

---

## 📁 Project Structure

```text
VulnSneak-AI/
│
├── frontend/                    # React 18 + Vite 7 SPA
│   ├── src/
│   │   ├── components/          # UI components
│   │   ├── pages/               # Application pages
│   │   │   ├── SplineAgentPage.jsx
│   │   │   └── FlowchartSection.jsx
│   │   └── context/
│   │       ├── ThemeContext.jsx
│   │       └── AuthContext.jsx
│   └── package.json
│
├── backend-dotnet/              # ASP.NET Core + EF Core + SQL Server
│   ├── Controllers/
│   ├── Models/
│   └── VulnSneak.API.csproj
│
├── ai-service/                  # Python FastAPI AI pipeline
│   ├── app/
│   ├── models/
│   │   ├── binary-classifier/
│   │   └── vulnerability-classifier/
│   └── requirements.txt
│
├── raspberry-pi-proxy/          # Secure gateway / traffic filtering
│
├── dataset/                     # Curated vulnerable/fixed code dataset
│   └── README.md
│
├── docs/                        # Reports, diagrams, use cases & flowcharts
│
└── README.md
```

> **Large Files Note:** Trained model weights and the complete dataset are hosted externally through Hugging Face / Kaggle instead of being committed directly to Git. See [`dataset/README.md`](./dataset/README.md) for details.

---

## 🛠️ Tech Stack

### 🎨 Frontend

* React 18.2
* Vite 7.2
* Tailwind CSS v4
* Framer Motion
* GSAP 3 + ScrollTrigger
* Spline 3D
* React Three Fiber
* OGL / WebGL shaders
* React Markdown
* RTL / Arabic text support
* PWA / Service Worker

### ⚙️ Backend

* ASP.NET Core
* Entity Framework Core
* SQL Server
* JWT Authentication
* REST APIs

### 🧠 AI / Machine Learning

* Python
* FastAPI
* Hugging Face Transformers
* PyTorch
* TensorFlow
* CodeBERT
* Qwen2.5-Coder
* Qwen3
* Gemini 2.5 Flash

### 🔐 Security Infrastructure

* Raspberry Pi
* Hardware security proxy
* Traffic filtering
* Backend / AI service isolation

### 🚀 Deployment

| Component    | Platform                               |
| :----------- | :------------------------------------- |
| Frontend     | Vercel                                 |
| .NET Backend | Render / Railway                       |
| AI Service   | Render / Railway / Hugging Face Spaces |

---

## 🎯 Vulnerability Coverage

VulnSneak currently focuses on **8 vulnerability classes**:

|  #  | Vulnerability                     |
| :-: | :-------------------------------- |
|  1  | SQL Injection (SQLi)              |
|  2  | Cross-Site Scripting (XSS)        |
|  3  | OS Command Injection              |
|  4  | Insecure Deserialization          |
|  5  | Path Traversal                    |
|  6  | Cross-Site Request Forgery (CSRF) |
|  7  | XML Injection                     |
|  8  | Insecure Cryptography             |

Vulnerability labeling and repair guidance are aligned with security standards and resources including **OWASP Top 10**, **CWE**, and the **OWASP Cheat Sheet Series**.

---

## 🔄 Detection & Repair Workflow

```text
                    Source Code
                         │
                         ▼
                 File / Code Upload
                         │
                         ▼
                Sliding Window Split
                24 Lines / Stride 12
                         │
                         ▼
              ┌─────────────────────┐
              │ Binary Classifier   │
              │ Vulnerable / Safe   │
              └──────────┬──────────┘
                         │
                    Vulnerable?
                    ┌────┴────┐
                   NO        YES
                   │          │
                   ▼          ▼
                 Safe    8-Class Classifier
                              │
                              ▼
                     Vulnerability Family
                              │
                              ▼
                      Vulnerable Region
                              │
                              ▼
                       Repair Agent
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
        Qwen2.5-Coder       Qwen3       Gemini Flash
           Primary         Fallback      Cloud Fallback
              │               │               │
              └───────────────┼───────────────┘
                              ▼
                     Generated Patch
                              │
                              ▼
                    Syntax Validation
                              │
                              ▼
                     Security Validation
                              │
                              ▼
                     Before / After Diff
                              │
                              ▼
                      Final Scan Report
```

---

## 🩹 AI-Powered Repair

Unlike detection-only security tools, VulnSneak includes an automated repair pipeline.

The repair agent:

1. Receives the vulnerable code region.
2. Identifies the vulnerability type.
3. Applies a vulnerability-specific repair strategy.
4. Generates a minimal patch.
5. Validates the resulting syntax.
6. Compares the original and repaired code.
7. Presents the developer with a clear **before / after diff**.

The surrounding source code is preserved whenever possible to minimize unintended changes.

---

## 🔐 Security Architecture

A dedicated **Raspberry Pi security proxy** sits between the public-facing application and the internal backend/AI services.

```text
Internet
   │
   ▼
┌───────────────────────┐
│ Raspberry Pi Gateway  │
│                       │
│ • Traffic Filtering   │
│ • Request Validation  │
│ • Network Isolation   │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ .NET Backend           │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ FastAPI AI Service     │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ Local AI / Repair LLM  │
└───────────────────────┘
```

This architecture is intended to reduce direct exposure of the backend and AI inference services.

---

## 🚀 Getting Started

Each service is independently deployable.

### 1. Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend development server will start using Vite.

### 2. Backend

See the backend documentation:

[`backend-dotnet/README.md`](./backend-dotnet/README.md)

> Documentation coming soon.

### 3. AI Service

See the AI service documentation:

[`ai-service/README.md`](./ai-service/README.md)

> Documentation coming soon.

### 4. Dataset

Dataset sources and download instructions:

[`dataset/README.md`](./dataset/README.md)

---

## 👥 Team

### Zagazig University — Computer Science

**Final Year Graduation Project — 2025–2026**

**Supervised by:** Prof. Dr. Osama Sheta

| Name                              |
| :-------------------------------- |
| Hatem Waleed Ragab Abdelfattah    |
| Ibrahim Mahmoud Ibrahim AlDosooqy |
| Mohamed Khalid Mohamed Abdelwahab |
| Mohamed Hussein Ahmed Hussein     |
| Ziad Ahmed Awad Mohamed           |
| Youssef Amr Mohamed Ahmed         |
| Mahmoud Saber Abumesallem Gad     |
| Mohamed Mansour Mohamed Mansour   |

---

## 📚 References & Data Sources

### Datasets

* Hugging Face — `darkknight25/Vulnerable_Programming_Dataset`
* Hugging Face — `CyberNative/Code_Vulnerability_Security_DPO`
* Kaggle vulnerability/fix datasets
* Kaggle code-vulnerability datasets
* Kaggle SQL Injection datasets
* Custom `vulnsneak-final-model-v1` dataset

### Security Standards

* [OWASP Top 10](https://owasp.org/)
* [CWE](https://cwe.mitre.org/)
* [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)

### Related Security Tools

* [Semgrep](https://semgrep.dev/)
* [Astra Security](https://www.getastra.com/)

The complete reference list is available in the graduation report under [`docs/`](./docs).

---

## 🎓 Academic Context

VulnSneak was developed as a **Final Year Graduation Project** for the Bachelor's degree in Computer Science at **Zagazig University**, during the **2025–2026 academic year**.

The project covers the complete lifecycle of an AI-powered cybersecurity platform, including:

* Dataset curation
* Data preprocessing
* Transformer model training
* Vulnerability classification
* AI-powered code repair
* Backend development
* Security infrastructure
* Frontend development
* Model inference
* Automated validation
* Production-oriented deployment

---

## 🗺️ Future Roadmap

* [ ] Expand vulnerability coverage beyond the initial 8 classes.
* [ ] Add additional programming languages.
* [ ] Improve vulnerability localization.
* [ ] Add stronger patch validation.
* [ ] Integrate additional local coding LLMs.
* [ ] Add explainable AI insights.
* [ ] Improve model performance through larger datasets.
* [ ] Add automated regression testing for generated patches.
* [ ] Expand security telemetry and monitoring.
* [ ] Add CI/CD integration.

---

## 📄 License

Developed for **academic purposes** as part of a graduation requirement at Zagazig University.

---

<div align="center">

### 🛡️ VulnSneak

**Detect. Understand. Repair.**

*AI-powered vulnerability detection and automated code security.*

</div>
