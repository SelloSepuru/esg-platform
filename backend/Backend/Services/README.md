# Services

This folder contains all the business logic services for the ESG Platform.

## Purpose
- Implement business logic and rules
- Orchestrate data operations
- Handle complex calculations and processing
- Manage external integrations

## Structure
- **ICompanyService.cs & CompanyService.cs** - Company management logic
- **IESGMetricsService.cs & ESGMetricsService.cs** - ESG calculations and scoring
- **IReportService.cs & ReportService.cs** - Report generation logic
- **IAuthService.cs & AuthService.cs** - Authentication logic
- **IEmailService.cs & EmailService.cs** - Email notifications
- **IDataImportService.cs & DataImportService.cs** - Data import/export

## Best Practices
- Use interface segregation principle
- Implement dependency injection
- Keep services focused on single responsibility
- Use async/await for database operations
