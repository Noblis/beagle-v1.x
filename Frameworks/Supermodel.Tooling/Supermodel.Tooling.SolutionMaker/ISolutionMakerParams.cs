namespace Supermodel.Tooling.SolutionMaker;

public interface ISolutionMakerParams
{
    #region Properties
    string SolutionName { get; } 
    string SolutionDirectory { get; }

    WebFrameworkEnum WebFramework { get; }
    DatabaseEnum Database { get; }

    string CalculateFullPath();
    #endregion
}