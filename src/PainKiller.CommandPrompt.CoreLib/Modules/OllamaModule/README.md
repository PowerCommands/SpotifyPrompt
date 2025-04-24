# Ollama Module Version 1.0
The AiChatModule is a module for CommandPrompt that enables interaction with an AI model via Ollama. The module uses the Ollama server as a backend and provides a simple chat functionality directly in the terminal.

Features:
Automatically starts the Ollama server if it is not already running.

Initiates a conversation with the AI model (default: "gemma3:latest").

Users can type messages in the terminal and receive responses from the AI model.

End the conversation by typing /bye.

## Requirements:
The Ollama server must be installed and accessible via the command ollama serve.

The Ollama server runs locally on port 11434 by default but you can change that in config.

## ChatCommand
Initiates a conversation with the AI model (default: "gemma3:latest").



