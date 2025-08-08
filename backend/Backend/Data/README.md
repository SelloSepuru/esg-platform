# Data

This folder contains database context and configuration for Entity Framework.

## Purpose
- Configure Entity Framework DbContext
- Define database connection strings
- Set up entity configurations and relationships
- Handle database initialization

## Structure
- **ESGDbContext.cs** - Main database context
- **Configurations/** - Entity configuration classes
- **Seeds/** - Database seeding data
- **ESGDbContextFactory.cs** - Context factory for migrations

## Best Practices
- Use fluent API for complex configurations
- Separate entity configurations into individual files
- Implement proper connection string management
- Use migrations for database schema changes
