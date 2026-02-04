# Quick start

### Prereqs
- .NET SDK (net10.0)
- SQL Server instance you can connect to

### Environment
Configure environment variables via a `.env` file or your shell environment.

Setup .env
1) Copy `.env.example` to `.env`.
2) Replace the placeholder values with real secrets and settings.

## Development Guidelines

### Commit Convention

This project uses [Conventional Commits](https://www.conventionalcommits.org/) enforced by Husky.Net.

**Format:**
```
<type>[optional scope]: <description>
```

**Types:**
- `feat` - A new feature
- `fix` - A bug fix
- `docs` - Documentation changes
- `style` - Code style changes (formatting, etc.)
- `refactor` - Code refactoring
- `test` - Adding or updating tests
- `chore` - Maintenance tasks, tooling
- `perf` - Performance improvements
- `ci` - CI/CD changes

**Examples:**
```
feat: add user login
fix(auth): resolve token expiration
docs: update API documentation
```