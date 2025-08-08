# Configuration Files

This folder contains configuration files for the ESG Platform backend.

## Files Overview

### `appsettings.json` (Local Development)
- **WARNING**: Contains local development credentials
- **NEVER** commit this to version control in production
- Use for local development only

### `appsettings.template.json` (Template)
- Template file showing the structure
- Use this to create your local `appsettings.json`
- Replace placeholder values with your actual credentials

### `appsettings.Development.json`
- Development environment overrides
- Safe to commit (no secrets)

### `appsettings.Production.json`
- Production environment template
- Uses environment variables for secrets
- Safe to commit (no hardcoded secrets)

## Environment Variables for Production

Set these environment variables in your CI/CD pipeline:

```bash
DB_HOST=your-database-host
DB_NAME=esg_platform
DB_USER=your-database-user
DB_PASSWORD=your-database-password
```

## CI/CD Pipeline Setup

### Azure DevOps / GitHub Actions
```yaml
env:
  DB_HOST: ${{ secrets.DB_HOST }}
  DB_NAME: esg_platform
  DB_USER: ${{ secrets.DB_USER }}
  DB_PASSWORD: ${{ secrets.DB_PASSWORD }}
```

### Docker
```dockerfile
ENV DB_HOST=localhost
ENV DB_NAME=esg_platform
ENV DB_USER=myuser
ENV DB_PASSWORD=mypassword
```

## Security Best Practices

1. **Never commit secrets** to version control
2. **Use environment variables** in production
3. **Use Azure Key Vault** or similar for production secrets
4. **Rotate credentials** regularly
5. **Use different credentials** for each environment

## Local Development Setup

1. Copy `appsettings.template.json` to `appsettings.json`
2. Replace placeholder values with your local database credentials
3. The `appsettings.json` file is gitignored and won't be committed
