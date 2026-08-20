# BIM Intelligence — Revit AI Assistant

BIM Intelligence is an AI-powered assistant for Autodesk Revit that allows users to query information from an open Revit model using natural language.

The system combines the Revit API with Groq-hosted LLMs and structured tool calling to translate natural-language questions into controlled Revit API operations.

---

## 1. Project Overview

The goal of this project is to provide a conversational interface for querying BIM data without requiring users to manually navigate the Revit model or understand the Revit API.

Example questions supported by the current implementation:

- How many rooms are on Level 1?
- Which level has the most doors?
- List all rooms with area less than 20 sqm.
- What is the average room size on Level 1?
- Which rooms have no windows?

The LLM does not directly access the Revit API.

Instead, it selects from a controlled set of application tools. Those tools execute the required operations against the currently open Revit document.

---

# 2. Architecture

```text
                         USER
                           |
                           v
                  +------------------+
                  |   WPF Chat UI    |
                  | BIM Intelligence |
                  +--------+---------+
                           |
                           v
                  +------------------+
                  |     Groq LLM     |
                  |  Llama 3.3 70B   |
                  +--------+---------+
                           |
                     Tool Calling
                           |
                           v
                  +------------------+
                  |   Tool Registry  |
                  +--------+---------+
                           |
                           v
                  +------------------+
                  |  ExternalEvent   |
                  +--------+---------+
                           |
                           v
                  +------------------+
                  |    Revit API     |
                  | RoomQueryService |
                  +--------+---------+
                           |
                           v
                  +------------------+
                  |    Tool Result   |
                  +--------+---------+
                           |
                           v
                  +------------------+
                  |     Groq LLM     |
                  | Final Response   |
                  +--------+---------+
                           |
                           v
                         USER

Request Flow
The user enters a natural-language question in the chatbot.
The question is sent to Groq.
Groq determines whether one of the available tools can answer the question.
If a tool is required, Groq returns a structured tool call.
ToolRegistry validates and executes the requested operation.
Revit-specific operations are executed through ExternalEvent.
The Revit API queries the currently open document.
The tool result is returned to Groq.
Groq converts the structured result into a natural-language response.
The response is displayed in the chatbot.


The LLM is intentionally not given direct access to Revit API objects.

Instead, the application exposes a controlled set of capabilities.

For example:
User:
How many rooms are on Level 1?

        ↓

Groq

        ↓

get_room_statistics
{
    "level": "Level 1"
}

        ↓

ToolRegistry

        ↓

RoomQueryService

        ↓

Revit API

        ↓

RoomStatistics

        ↓

Groq

        ↓

"There are 30 rooms on Level 1."

Current Tools

The current implementation intentionally keeps the tool surface small and focused on the assessment requirements.

get_rooms

Retrieves room information including:

Room name
Room number
Level
Area
Door count
Window count
get_room_statistics

Provides statistics for a specific level:

Number of rooms
Average room area
Minimum room area
Maximum room area
get_level_door_counts

Returns door counts grouped by Revit level.

get_rooms_without_windows

Returns rooms that have no associated windows.



Project Strucutre : 
BIMIntelligence/
│
├── AI/
│   ├── GroqService.cs
│   └── GroqLogger.cs
│
├── Commands/
│   ├── OpenChatCommand.cs
│   ├── RoomDataCommand.cs
│   └── GroqTestCommand.cs
│
├── Models/
│   ├── RoomData.cs
│   ├── RoomStatistics.cs
│   └── ToolCallRequest.cs
│
├── Revit/
│   ├── RoomQueryService.cs
│   ├── LevelResolver.cs
│   └── ChatExternalEventHandler.cs
│
├── Tools/
│   ├── ToolRegistry.cs
│   └── ToolResult.cs
│
├── UI/
│   ├── ChatWindow.xaml
│   └── ChatWindow.xaml.cs
│
├── RevitApplication.cs
│
└── BIMIntelligence.csproj



Running the Application
Step 1 — Build

Open the solution in Visual Studio and build the project.

The compiled assembly will be generated under the project's bin directory.

Step 2 — Configure the Revit Add-in

Ensure the .addin manifest points to the correct BIMIntelligence.dll.

Step 3 — Start Revit

Open Autodesk Revit 2027 and load a model containing rooms, doors, and windows.

Step 4 — Open BIM Intelligence

The Revit ribbon contains the:

BIM Intelligence
    |
    +-- BIM Chat

button.

Clicking it opens the chatbot.


Error Handling

The implementation separates application errors from user-facing responses.

Examples of errors handled include:

Invalid tool arguments
Revit API exceptions
Invalid level references
Groq response parsing errors
Unsupported queries
Tool execution failures

Technical information can be written to the application/Groq logs while the user receives a concise explanation.

Design Decisions and Trade-offs

This implementation was developed under a strict three-day assessment constraint.

The primary goal was therefore to demonstrate:

Ability to understand an unfamiliar technical environment
Ability to understand the Revit API execution model
Ability to integrate an external AI service
Ability to introduce an extensible tool-calling architecture
Ability to safely execute Revit operations from a modeless UI
Ability to handle unsupported requests gracefully
Ability to deliver a working end-to-end feature within a constrained timeline

The project intentionally avoids unnecessary complexity.



The architecture is intentionally lightweight for the assessment while providing a clear extension path for additional Revit capabilities.
