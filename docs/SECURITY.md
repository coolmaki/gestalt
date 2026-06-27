# SECURITY.md

Security principles and practices for Supercluster.

## Core Principles

1. **Parse at boundaries.** All external input is validated to a known shape before reaching business logic.
2. **Errors don't leak internals.** `Error.Description` is human-readable but must not expose stack traces, connection strings, or internal paths.
3. **Least privilege.** Each component accesses only the resources it needs.
4. **Secrets never in code.** Environment variables or secret managers only.

## Current State

The project is in early development. No authentication, authorization, or network-facing components exist yet. This document will be updated as security-relevant features are added.

## Checklist

- [ ] Secrets management strategy defined
- [ ] AuthN/AuthZ architecture defined
- [ ] Input validation framework in place
- [ ] Dependency scanning configured in CI
- [ ] Error response sanitization verified
