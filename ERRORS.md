## [2026-04-18 00:25] - Build Error CS1929 in MedicalRecordService.cs

- **Type**: [Agent/Syntax]
- **Severity**: [Medium]
- **File**: `QuanLyDoanKham.API\Services\MedicalRecords\MedicalRecordService.cs:347`
- **Agent**: [komi]
- **Root Cause**: Attempted to use Service Location by calling `_context.GetService<IGeminiService>()` where `_context` is `ApplicationDbContext` (not an `IServiceProvider`). This likely occurred during a previous AI implementation phase.
- **Error Message**: 
  ```
  error CS1929: 'ApplicationDbContext' does not contain a definition for 'GetService' and the best extension method overload 'ServiceProviderServiceExtensions.GetService<IGeminiService>(IServiceProvider)' requires a receiver of type 'System.IServiceProvider'
  ```
- **Fix Applied**: Properly injected `IGeminiService` via the constructor of `MedicalRecordService` and used the private field to call AI methods.
- **Prevention**: Always prefer Constructor Injection over Service Location. If Service Location is absolutely necessary, ensure the caller is an `IServiceProvider`.
- **Status**: [Fixed]

---
