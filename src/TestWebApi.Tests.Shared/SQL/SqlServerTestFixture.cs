

using Testcontainers.MsSql;
using Xunit;

namespace TestWebApi.Tests.Shared.SQL
{
    public class SqlServerTestFixture : IAsyncLifetime
    {
        private MsSqlContainer _mssqlContainer;




        #region Initialization and Disposal

        public async Task InitializeAsync ()
        {
            try
            {
                _mssqlContainer = new MsSqlBuilder()
                 .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                 .Build();

                await _mssqlContainer.StartAsync();
                ConnectionString = _mssqlContainer.GetConnectionString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SQL Server container: {ex.Message}");
                throw;
            }
        }

       
        public async Task  DisposeAsync()
        {
          await  _mssqlContainer.DisposeAsync();
        }

        #endregion






        public string ConnectionString { get; private set; }


    }
}
