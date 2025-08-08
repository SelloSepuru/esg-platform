# DTOs (Data Transfer Objects)

This folder contains all DTOs for data transfer between API layers.

## Purpose
- Define contracts for API requests and responses
- Provide data validation and transformation
- Decouple internal models from external APIs
- Ensure clean API interfaces

## Structure
- **Request/** - DTOs for incoming requests
  - CreateCompanyRequest.cs
  - UpdateESGMetricsRequest.cs
  - GenerateReportRequest.cs
- **Response/** - DTOs for API responses
  - CompanyResponse.cs
  - ESGMetricsResponse.cs
  - ReportResponse.cs
- **Common/** - Shared DTOs
  - PaginationRequest.cs
  - ApiResponse.cs

## Best Practices
- Use data annotations for validation
- Keep DTOs simple and focused
- Use AutoMapper for model mapping
- Implement proper naming conventions
