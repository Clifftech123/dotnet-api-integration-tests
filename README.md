

#  TestWebApi – Sample .NET API Testing Setup

This is a sample repository for setting up API testing in .NET.

It demonstrates how to build a robust and maintainable testing environment for ASP.NET Core Web APIs using modern tools and practices.

---

##  What's Included

- **Mocking Services**  
  Easily isolate dependencies using mocking frameworks like Moq to test service behavior without relying on real implementations.

- **Logging to Test Output**  
  Capture and display application logs directly in test results using xUnit's `ITestOutputHelper`, making debugging smoother and more transparent.

- **Integration Testing with Testcontainers**  
  Run tests against a real SQL Server instance using [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet), ensuring realistic database interactions without polluting your local environment.

---

##  Project Structure

```
TestWebApi.sln
├── TestWebApi                 # Main Web API project
├── TestWebApi.DataLayer       # EF Core DbContext, migrations
├── TestWebApi.Shared          # Shared models, DTOs, utilities
├── TestWebApi.Tests           # Unit + integration tests
│   ├── Mocks                  # Mocked services and helpers
│   ├── Integration            # Testcontainers setup
│   └── Logging                # ITestOutputHelper usage
├── TestWebApi.Tests.Shared    # Shared test utilities
├── Directory.Build.props      # Centralized build settings
├── Directory.Packages.props   # Centralized NuGet versions
```

---

## 🛠️ Getting Started

1. **Build the solution**  
   ```bash
   dotnet build
   ```

2. **Run the tests**  
   ```bash
   dotnet test
   ```

>  Make sure Docker is running for Testcontainers to work properly.

---

##  Technologies Used

- [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/dotnet-eight)
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/moq/moq4)
- [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)

---

## 🤝 Contributing

Feel free to fork, clone, and contribute! Whether it's improving test coverage, adding new examples, or refining the setup — all contributions are welcome.


