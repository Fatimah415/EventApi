# AI Tool Decision Report

## Scenario
Developing a .NET 8 Web API for a Library Management System.

---

## Task 1: Design SQL Server Database (6–8 Tables)

**Recommended Tool:** Claude Code

**Reason:**
Claude Code is best for database design and architecture planning. It can analyze the project structure and suggest a clean relational database schema.

---

## Task 2: Implement Full CRUD API with EF Core

**Recommended Tool:** GitHub Copilot

**Reason:**
GitHub Copilot works directly inside Visual Studio and helps generate controllers, repositories, services, and EF Core code quickly.

---

## Task 3: Integrate OpenLibrary API

**Recommended Tool:** Claude Code

**Reason:**
Claude Code is better for understanding third-party API documentation and planning integrations.

---

## Task 4: Refactor LoansService

**Recommended Tool:** Claude Code

**Reason:**
Claude Code is excellent for multi-file refactoring and maintaining consistent coding standards.

---

## Using CLAUDE.md

CLAUDE.md stores project conventions such as naming rules, architecture, coding style, async usage, and Entity Framework practices. Claude reads these instructions in every session, so they don't need to be repeated.

---

## Using MCP

An MCP server can connect AI with a live SQL Server database. This allows the AI to understand tables, columns, relationships, and generate more accurate Entity Framework Core code.