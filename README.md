# Multi-Agent Travel Planner

A travel planning application built with a **multi-agent architecture** using ASP.NET Core and OpenAI's GPT-4o.

## Overview

This application uses a **Supervisor Agent** that coordinates with specialized sub-agents to gather and present travel information in a user-friendly format. Each agent focuses on a specific aspect of travel planning:

- **Flight Agent** - Finds flight options
- **Hotel Agent** - Recommends accommodations
- **Attraction Agent** - Suggests tourist destinations
- **Restaurant Agent** - Finds dining options

The Supervisor Agent orchestrates these sub-agents, collects their responses, and presents a comprehensive travel itinerary.

## Technology Stack

- **Framework**: ASP.NET Core 9.0
- **AI Model**: OpenAI GPT-4o
- **Architecture**: Multi-agent system with supervisor coordination
- **Deployment**: Azure Container Apps (Docker)

## Live Application

🌐 **[https://multi-agent-travel-planner.wittyfield-418fa895.westeurope.azurecontainerapps.io](https://multi-agent-travel-planner.wittyfield-418fa895.westeurope.azurecontainerapps.io)**

