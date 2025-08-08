# Controllers

This folder contains all the API controllers for the ESG Platform backend.

## Purpose
- Handle HTTP requests and responses
- Route incoming requests to appropriate services
- Return formatted responses to clients

## Structure
- **CompanyController.cs** - Manage company ESG data
- **ESGMetricsController.cs** - Handle ESG metrics and scoring
- **ReportsController.cs** - Generate and manage ESG reports
- **AuthController.cs** - Authentication and authorization
- **DashboardController.cs** - Dashboard data aggregation

## Best Practices
- Keep controllers thin - delegate business logic to services
- Use proper HTTP status codes
- Implement proper error handling
- Use DTOs for data transfer
