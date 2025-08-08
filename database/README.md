# Database Scripts and Data

This folder contains database-related files for the ESG Platform.

## Structure

### `/scripts/`
- **01_CreateDatabase.sql** - Initial database creation
- **02_CreateTables.sql** - Table creation scripts
- **03_CreateIndexes.sql** - Index creation for performance
- **04_CreateConstraints.sql** - Foreign key and check constraints
- **05_CreateViews.sql** - Database views for reporting
- **06_CreateStoredProcedures.sql** - Stored procedures

### `/seeddata/`
- **ESGCategories.sql** - Standard ESG categories and frameworks
- **SampleCompanies.sql** - Sample company data for testing
- **DefaultUsers.sql** - Default user accounts
- **ESGMetricsTemplates.sql** - Standard ESG metrics templates

### `/backups/`
- Database backup files
- Version-specific backups
- Development and testing snapshots

## Database Design Considerations
- **Companies** - Core company information and profiles
- **ESG_Metrics** - ESG measurements and scores
- **ESG_Reports** - Generated reports and assessments
- **Users** - User accounts and permissions
- **Categories** - ESG categories and subcategories
- **Audits** - Change tracking and audit trails

## Best Practices
- Use proper indexing for performance
- Implement foreign key constraints
- Create views for complex reporting queries
- Maintain referential integrity
- Use stored procedures for complex operations
