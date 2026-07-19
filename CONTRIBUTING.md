# Contributing to ADRL-Rescue

Thank you for your interest in contributing to ADRL-Rescue! This document provides guidelines and information for contributors.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Documentation](#documentation)
- [Reporting Bugs](#reporting-bugs)
- [Requesting Features](#requesting-features)

---

## Code of Conduct

This project adheres to our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

---

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or later
- Python 3.8 or later
- Git
- GitHub account

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/ADRL-Rescue.git
   cd ADRL-Rescue
   ```
3. Add upstream remote:
   ```bash
   git remote add upstream https://github.com/NihaalGharat/ADRL-Rescue.git
   ```

---

## Development Setup

### Unity Setup

1. Open Unity Hub
2. Click "Open" → Navigate to `UnityProject/`
3. Select Unity 2022.3 LTS
4. Wait for project to import

### Python Setup

```bash
cd Python
pip install -r requirements.txt
```

### ML-Agents Setup

1. Open Window → Package Manager
2. Click "+" → "Add package from git URL"
3. Enter: `com.unity.ml-agents`

---

## How to Contribute

### Types of Contributions

| Type | Description | Example |
|------|-------------|---------|
| **Code** | New features, bug fixes | Implement sensor |
| **Documentation** | Improve docs | Fix typo, add example |
| **Testing** | Write tests | Unit tests for reward system |
| **Design** | Architecture proposals | New system design |
| **Research** | Algorithm research | RL paper implementation |

### Finding Issues

- Check [GitHub Issues](https://github.com/NihaalGharat/ADRL-Rescue/issues) for open tasks
- Look for `good first issue` label for beginners
- Look for `help wanted` label for needed contributions

---

## Pull Request Process

### 1. Create a Branch

```bash
git checkout develop
git checkout -b feature/your-feature-name
```

### 2. Make Changes

- Follow [coding standards](#coding-standards)
- Write clear commit messages
- Keep changes focused

### 3. Commit

```bash
git add .
git commit -m "feat(scope): description of change"
```

### 4. Push and Create PR

```bash
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub.

### 5. PR Description

Use the PR template:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Refactoring

## Testing
- [ ] Unit tests pass
- [ ] Manual testing completed
- [ ] No regression

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No new warnings
```

---

## Coding Standards

### C# Standards

- Use PascalCase for classes and public methods
- Use _camelCase for private fields
- Add XML documentation for public APIs
- Keep methods focused (single responsibility)

### Python Standards

- Use snake_case for functions and variables
- Use PascalCase for classes
- Add docstrings for functions
- Follow PEP 8 style

### Documentation

- Use clear, concise language
- Include code examples where helpful
- Update documentation when adding features

---

## Documentation

### Writing Documentation

- Use Markdown format
- Follow existing documentation style
- Include diagrams where helpful
- Link to related documents

### Documentation Types

| Type | Location | Purpose |
|------|----------|---------|
| API Docs | Code comments | Class/method documentation |
| Guides | `Documentation/` | How-to guides |
| Architecture | `Documentation/` | System design |
| README | Root | Project overview |

---

## Reporting Bugs

### Bug Report Template

```markdown
**Describe the bug**
A clear description of the bug

**To Reproduce**
Steps to reproduce the behavior

**Expected behavior**
What you expected to happen

**Screenshots**
If applicable

**Environment**
- Unity version:
- OS:
- Python version:
```

### Where to Report

- GitHub Issues: [Create Issue](https://github.com/NihaalGharat/ADRL-Rescue/issues/new)

---

## Requesting Features

### Feature Request Template

```markdown
**Is your feature request related to a problem?**
Description of the problem

**Describe the solution**
Your proposed solution

**Alternatives considered**
Other solutions considered

**Additional context**
Any other information
```

---

## Development Guidelines

### Git Workflow

- Use feature branches
- Write meaningful commit messages
- Keep commits atomic
- Don't commit generated files

### Testing

- Write tests for new features
- Ensure all tests pass before PR
- Aim for >80% code coverage

### Review Process

- All PRs require at least 1 review
- Address review feedback promptly
- Be respectful and constructive

---

## Getting Help

- Check existing documentation
- Ask in GitHub Discussions
- Open an issue for questions

---

## Recognition

Contributors will be recognized in:
- GitHub Contributors page
- Project README
- Release notes

---

Thank you for contributing to ADRL-Rescue! Together, we're building something meaningful for disaster response.
