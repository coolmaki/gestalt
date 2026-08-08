# RELIABILITY.md

Reliability principles and practices for Gestalt.

## Core Principles

1. **No silent failures.** Every fallible operation returns `Result<T>`. Failures are explicit and traceable.
2. **Immutability by default.** Shared mutable state is the root of most concurrency bugs. Records with `{ get; init; }` eliminate this class of errors.
3. **No partial updates.** Aggregate boundaries enforce consistency. An aggregate is saved entirely or not at all.

## Current State

The project is in early development. No runtime, persistence, or external integrations exist yet. This document will be updated as reliability-relevant features are added.

## Checklist

- [ ] Observability strategy (logs, metrics, traces) defined
- [ ] Health check endpoints planned
- [ ] Retry/backoff policies for external calls defined
- [ ] Graceful degradation patterns documented
- [ ] Data backup and recovery strategy defined
