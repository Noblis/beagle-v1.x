namespace Supermodel.Client.Backend.DataContext.Sqlite;

public interface ISqlQueryProvider
{
    object? GetIndex<TModel>(int idxNum0To29, TModel model);
    string? GetWhereClause<TModel>(object? searchBy, string? sortBy);
    string GetSkipAndTakeForWhereClause<TModel>(int? skip, int? take);
}