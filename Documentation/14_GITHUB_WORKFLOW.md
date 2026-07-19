# 14 - GitHub Workflow

---

## Overview

This document defines the Git workflow and contribution process for ADRL-Rescue.

---

## Branch Strategy

```mermaid
gitgraph
    commit id: "init"
    branch develop
    checkout develop
    commit id: "drone-physics"
    commit id: "sensors"
    branch feature/ray-sensor
    checkout feature/ray-sensor
    commit id: "ray-sensor-impl"
    checkout develop
    merge feature/ray-sensor
    commit id: "ml-agents"
    checkout main
    merge develop tag: "v0.2.0"
```

### Branch Types

| Branch | Purpose | Naming |
|--------|---------|--------|
| `main` | Stable releases | `main` |
| `develop` | Integration branch | `develop` |
| `feature/*` | New features | `feature/drone-physics` |
| `fix/*` | Bug fixes | `fix/collision-detection` |
| `docs/*` | Documentation | `docs/readme-update` |
| `release/*` | Release prep | `release/v1.0.0` |

---

## Commit Convention

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

| Type | Description | Example |
|------|-------------|---------|
| `feat` | New feature | `feat(drone): add hover stabilization` |
| `fix` | Bug fix | `fix(sensors): correct ray detection` |
| `docs` | Documentation | `docs: update README` |
| `style` | Code style | `style: format code` |
| `refactor` | Refactoring | `refactor(ai): extract observation logic` |
| `test` | Tests | `test: add reward system tests` |
| `chore` | Maintenance | `chore: update dependencies` |

### Examples

```bash
# Feature
git commit -m "feat(drone): implement flight controller"

# Bug fix
git commit -m "fix(sensors): correct thermal sensor range"

# Documentation
git commit -m "docs: add architecture diagram"

# Breaking change
git commit -m "feat(api)!: change observation format

BREAKING CHANGE: observation vector size changed from 32 to 44"
```

---

## Pull Request Process

### 1. Create Feature Branch

```bash
git checkout develop
git checkout -b feature/drone-physics
```

### 2. Make Changes

```bash
# Make changes
git add .
git commit -m "feat(drone): add basic physics"
```

### 3. Push and Create PR

```bash
git push origin feature/drone-physics
# Create PR on GitHub
```

### 4. PR Template

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

### 5. Review and Merge

- At least 1 review required
- All CI checks must pass
- Squash and merge to `develop`

---

## Release Process

### Version Numbering

```
MAJOR.MINOR.PATCH

MAJOR: Breaking changes
MINOR: New features (backward compatible)
PATCH: Bug fixes
```

### Release Steps

1. Create release branch from `develop`
2. Update version numbers
3. Update CHANGELOG.md
4. Create PR to `main`
5. Review and merge
6. Tag release
7. Merge back to `develop`

```bash
# Create release
git checkout develop
git checkout -b release/v1.0.0

# Update files
# ...

# Merge to main
git checkout main
git merge release/v1.0.0
git tag -a v1.0.0 -m "Release v1.0.0"

# Merge back to develop
git checkout develop
git merge release/v1.0.0
```

---

## Commit Best Practices

### Do

- Commit often (small, focused commits)
- Write clear commit messages
- Reference issues when applicable
- Keep commits atomic

### Don't

- Commit generated files
- Commit secrets or credentials
- Mix unrelated changes
- Force push to shared branches

---

## Issue Templates

### Bug Report

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
```

### Feature Request

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

## CI/CD Pipeline

### GitHub Actions

```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.10'
      - run: pip install -r requirements.txt
      - run: python -m pytest tests/
```

---

## Navigation

| Document | Description |
|----------|-------------|
| [13_CODING_STANDARDS](13_CODING_STANDARDS.md) | Coding conventions |
| [15_TESTING_GUIDE](15_TESTING_GUIDE.md) | Testing procedures |
| [CONTRIBUTING](../CONTRIBUTING.md) | Contribution guidelines |

---

*Last updated: July 2026*
