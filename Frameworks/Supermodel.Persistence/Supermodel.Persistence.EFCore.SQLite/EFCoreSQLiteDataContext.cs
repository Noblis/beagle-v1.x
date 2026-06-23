using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Supermodel.Persistence.Repository;

namespace Supermodel.Persistence.EFCore.SQLite;

public abstract class EFCoreSQLiteDataContext : EFCoreDataContext
{
    #region Constructors
    static EFCoreSQLiteDataContext()
    {
        if (_dbFilePath == null)
        {
            //this is to make Cmd project point to the same db as web projects
            var workingPath = Directory.GetCurrentDirectory();

            string relativePath;
            if (workingPath.Contains("\\bin\\Debug\\")) relativePath = "../../../../";
            else if (workingPath.Contains("\\bin\\Release\\")) relativePath = "../../../../";
            else relativePath = "../";

            _dbFilePath = Path.Combine(workingPath, relativePath);
        }
    }
    protected EFCoreSQLiteDataContext(string connectionString, IRepoFactory? customRepoFactory = null)
        : base(connectionString, customRepoFactory)
    {
        // ReSharper disable once StringLiteralTypo
        //if in-memory db, we need to retain the connection
        if (connectionString.Trim().ToLower() != "datasource=:memory:")
        {
            Connection = new SqliteConnection(connectionString);
        }
        else
        {
            if (connectionString == MemoryConnectionStringRetainer && MemoryConnectionRetainer != null)
            {
                Connection = MemoryConnectionRetainer;
            }
            else
            {
                MemoryConnectionStringRetainer = connectionString;
                MemoryConnectionRetainer = Connection = new SqliteConnection(connectionString);
                Connection.Open();
            }
        }
    }
    #endregion

    #region Overrides
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite(Connection)
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging();
    }
    #endregion

    #region Methods
    public static string DbFilePath
    {
        get
        {
            //_dbFilePath should get calculated in the static constructor for this class
            //this is just in case this is called from a static constructor of some class
            //that happens to run before our static constructor 
            if (_dbFilePath == null)
            {
                //this is to make Cmd project point to the same db as web projects
                var workingPath = Directory.GetCurrentDirectory();

                string relativePath;
                if (workingPath.Contains("\\bin\\Debug\\")) relativePath = "../../../../";
                else if (workingPath.Contains("\\bin\\Release\\")) relativePath = "../../../../";
                else relativePath = "../";

                _dbFilePath = Path.Combine(workingPath, relativePath);
            }

            return _dbFilePath;
        }
    }
    private static string? _dbFilePath;
    #endregion

    #region Properties
    public SqliteConnection Connection { get; protected set; }

    //we need this for in memory db. In order for in memory db not to be disposed, connection must not be disposed
    public static string? MemoryConnectionStringRetainer { get; protected set; }
    public static SqliteConnection? MemoryConnectionRetainer { get; protected set; }
    #endregion
}