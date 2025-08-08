# Models

This folder contains all the data models/entities for the ESG Platform.

## Purpose
- Define database entities and their relationships
- Represent business domain objects
- Configure Entity Framework mappings

## Structure
- **Company.cs** - Company entity
- **ESGMetric.cs** - ESG metrics and measurements
- **ESGReport.cs** - ESG reports and assessments
- **User.cs** - User accounts and roles
- **Category.cs** - ESG categories (Environmental, Social, Governance)
- **Audit.cs** - Audit trails and logging

## Best Practices
- Use data annotations for validation
- Define proper relationships between entities
- Implement IEntity interface for common properties
- Use meaningful property names
