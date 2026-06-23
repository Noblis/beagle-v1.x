using Newtonsoft.Json;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.Models;

namespace Supermodel.Client.Backend.DataContext.Sqlite;

public class DataRow<TModel> : DataRow where TModel : class, IModel, new()
{
    #region Constructors
    public DataRow(TModel model, long originalModelId, ISqlQueryProvider sqlQueryProvider) : base(model, originalModelId, sqlQueryProvider)
    {
        var modelTypeLogicalName1 = DataContextBase.GetModelTypeLogicalName<TModel>();
        var modelTypeLogicalName2 = DataContextBase.GetModelTypeLogicalName(model.GetType());
        if (modelTypeLogicalName1 != modelTypeLogicalName2) throw new Exception("DataContextBase.GetModelTypeLogicalName<TModel>() != DataContextBase.GetModelTypeLogicalName(model.GetType())");
    }
    public DataRow(){} //default constructor for SQLite.NET
    #endregion

    #region Methods
    public TModel GetModel()
    {
        return (TModel)GetModel(typeof(TModel));
    }
    #endregion
}
    
public class DataRow
{
    #region Constructors
    public DataRow(IModel model, long originalModelId, ISqlQueryProvider sqlQueryProvider)
    {
        ModelTypeLogicalName = DataContextBase.GetModelTypeLogicalName(model.GetType());
        ModelId = model.Id;
        OriginalModelId = originalModelId;
        //Json = JsonConvert.SerializeObject(model.PrepareForSerializingForLocalDb());
        Json = JsonConvert.SerializeObject(model);
        BroughtFromMasterDbOnUtc = model.BroughtFromMasterDbOnUtc;       
        Index0 = (string?)sqlQueryProvider.GetIndex(0, model);
        Index1 = (string?)sqlQueryProvider.GetIndex(1, model); 
        Index2 = (string?)sqlQueryProvider.GetIndex(2, model); 
        Index3 = (string?)sqlQueryProvider.GetIndex(3, model);
        Index4 = (string?)sqlQueryProvider.GetIndex(4, model); 
        Index5 = (string?)sqlQueryProvider.GetIndex(5, model);
        Index6 = (string?)sqlQueryProvider.GetIndex(6, model); 
        Index7 = (string?)sqlQueryProvider.GetIndex(7, model); 
        Index8 = (string?)sqlQueryProvider.GetIndex(8, model);
        Index9 = (string?)sqlQueryProvider.GetIndex(9, model); 
        Index10 = (long?)sqlQueryProvider.GetIndex(10, model);
        Index11 = (long?)sqlQueryProvider.GetIndex(11, model); 
        Index12 = (long?)sqlQueryProvider.GetIndex(12, model); 
        Index13 = (long?)sqlQueryProvider.GetIndex(13, model);
        Index14 = (long?)sqlQueryProvider.GetIndex(14, model); 
        Index15 = (long?)sqlQueryProvider.GetIndex(15, model);
        Index16 = (long?)sqlQueryProvider.GetIndex(16, model); 
        Index17 = (long?)sqlQueryProvider.GetIndex(17, model); 
        Index18 = (long?)sqlQueryProvider.GetIndex(18, model);
        Index19 = (long?)sqlQueryProvider.GetIndex(19, model); 
        Index20 = (double?)sqlQueryProvider.GetIndex(20, model);
        Index21 = (double?)sqlQueryProvider.GetIndex(21, model); 
        Index22 = (double?)sqlQueryProvider.GetIndex(22, model); 
        Index23 = (double?)sqlQueryProvider.GetIndex(23, model);
        Index24 = (double?)sqlQueryProvider.GetIndex(24, model); 
        Index25 = (double?)sqlQueryProvider.GetIndex(25, model);
        Index26 = (double?)sqlQueryProvider.GetIndex(26, model); 
        Index27 = (double?)sqlQueryProvider.GetIndex(27, model); 
        Index28 = (double?)sqlQueryProvider.GetIndex(28, model);
        Index29 = (double?)sqlQueryProvider.GetIndex(29, model); 
    }
    public DataRow(){} //default constructor for SQLite.NET
    #endregion

    #region Methods
    public IModel GetModel(Type modelType)
    {
        if (Json == null) throw new NullReferenceException(nameof(Json));

        var model = (IModel?)JsonConvert.DeserializeObject(Json, modelType);
        if (model!.Id != ModelId || ModelTypeLogicalName != DataContextBase.GetModelTypeLogicalName(modelType)) throw new Exception("Database corruption: (THIS SHOULD NEVER HAPPEN): model.Id != DataRow.ModelId || ModelTypeLogicalName != DataContextBase.GetModelTypeLogicalName<TModel>()");
        return model;
    }
    public string GenerateSqlInsertOrReplace(string dataTableName)
    {
        if (Json == null) throw new NullReferenceException(nameof(Json));

        // ReSharper disable once UseStringInterpolation
        var sql = string.Format(@"INSERT OR REPLACE INTO [{0}] 
			                          (ModelTypeLogicalName, ModelId, Json, BroughtFromMasterDbOnUtcTicks, Index0, Index1, Index2, Index3, Index4, Index5, Index6, Index7, Index8, Index9, Index10, Index11, Index12, Index13, Index14, Index15, Index16, Index17, Index18, Index19, Index20, Index21, Index22, Index23, Index24, Index25, Index26, Index27, Index28, Index29) 
                                      VALUES ('{1}', '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34})",
            dataTableName,
            ModelTypeLogicalName,
            ModelId,
            Json.Replace("'", "''"),
            BroughtFromMasterDbOnUtc?.Ticks.ToString() ?? "NULL",
            Index0 != null ? "'" + Index0.Replace("'", "''") + "'" : "NULL",
            Index1 != null ? "'" + Index1.Replace("'", "''") + "'" : "NULL",
            Index2 != null ? "'" + Index2.Replace("'", "''") + "'" : "NULL",
            Index3 != null ? "'" + Index3.Replace("'", "''") + "'" : "NULL",
            Index4 != null ? "'" + Index4.Replace("'", "''") + "'" : "NULL",
            Index5 != null ? "'" + Index5.Replace("'", "''") + "'" : "NULL",
            Index6 != null ? "'" + Index6.Replace("'", "''") + "'" : "NULL",
            Index7 != null ? "'" + Index7.Replace("'", "''") + "'" : "NULL",
            Index8 != null ? "'" + Index8.Replace("'", "''") + "'" : "NULL",
            Index9 != null ? "'" + Index9.Replace("'", "''") + "'" : "NULL",
            Index10 != null ? Index10.ToString() : "NULL",
            Index11 != null ? Index11.ToString() : "NULL",
            Index12 != null ? Index12.ToString() : "NULL",
            Index13 != null ? Index13.ToString() : "NULL",
            Index14 != null ? Index14.ToString() : "NULL",
            Index15 != null ? Index15.ToString() : "NULL",
            Index16 != null ? Index16.ToString() : "NULL",
            Index17 != null ? Index17.ToString() : "NULL",
            Index18 != null ? Index18.ToString() : "NULL",
            Index19 != null ? Index19.ToString() : "NULL",
            Index20 != null ? Index20.ToString() : "NULL",
            Index21 != null ? Index21.ToString() : "NULL",
            Index22 != null ? Index22.ToString() : "NULL",
            Index23 != null ? Index23.ToString() : "NULL",
            Index24 != null ? Index24.ToString() : "NULL",
            Index25 != null ? Index25.ToString() : "NULL",
            Index26 != null ? Index26.ToString() : "NULL",
            Index27 != null ? Index27.ToString() : "NULL",
            Index28 != null ? Index28.ToString() : "NULL",
            Index29 != null ? Index29.ToString() : "NULL");
        return sql;
    }
    public string GenerateSqlInsert(string dataTableName)
    {
        if (Json == null) throw new NullReferenceException(nameof(Json));

        // ReSharper disable once UseStringInterpolation
        var sql = string.Format(@"INSERT INTO [{0}] 
			                          (ModelTypeLogicalName, ModelId, Json, BroughtFromMasterDbOnUtcTicks, Index0, Index1, Index2, Index3, Index4, Index5, Index6, Index7, Index8, Index9, Index10, Index11, Index12, Index13, Index14, Index15, Index16, Index17, Index18, Index19, Index20, Index21, Index22, Index23, Index24, Index25, Index26, Index27, Index28, Index29) 
                                      VALUES ('{1}', '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34})",
            dataTableName,
            ModelTypeLogicalName, 
            ModelId, 
            Json.Replace("'", "''"),
            BroughtFromMasterDbOnUtc?.Ticks.ToString() ?? "NULL",
            Index0 != null ? "'" + Index0.Replace("'", "''") + "'" : "NULL",
            Index1 != null ? "'" + Index1.Replace("'", "''") + "'" : "NULL",
            Index2 != null ? "'" + Index2.Replace("'", "''") + "'" : "NULL",
            Index3 != null ? "'" + Index3.Replace("'", "''") + "'" : "NULL",
            Index4 != null ? "'" + Index4.Replace("'", "''") + "'" : "NULL",
            Index5 != null ? "'" + Index5.Replace("'", "''") + "'" : "NULL",
            Index6 != null ? "'" + Index6.Replace("'", "''") + "'" : "NULL",
            Index7 != null ? "'" + Index7.Replace("'", "''") + "'" : "NULL",
            Index8 != null ? "'" + Index8.Replace("'", "''") + "'" : "NULL",
            Index9 != null ? "'" + Index9.Replace("'", "''") + "'" : "NULL",
            Index10 != null ? Index10.ToString() : "NULL",
            Index11 != null ? Index11.ToString() : "NULL",
            Index12 != null ? Index12.ToString() : "NULL",
            Index13 != null ? Index13.ToString() : "NULL",
            Index14 != null ? Index14.ToString() : "NULL",
            Index15 != null ? Index15.ToString() : "NULL",
            Index16 != null ? Index16.ToString() : "NULL",
            Index17 != null ? Index17.ToString() : "NULL",
            Index18 != null ? Index18.ToString() : "NULL",
            Index19 != null ? Index19.ToString() : "NULL",
            Index20 != null ? Index20.ToString() : "NULL",
            Index21 != null ? Index21.ToString() : "NULL",
            Index22 != null ? Index22.ToString() : "NULL",
            Index23 != null ? Index23.ToString() : "NULL",
            Index24 != null ? Index24.ToString() : "NULL",
            Index25 != null ? Index25.ToString() : "NULL",
            Index26 != null ? Index26.ToString() : "NULL",
            Index27 != null ? Index27.ToString() : "NULL",
            Index28 != null ? Index28.ToString() : "NULL",
            Index29 != null ? Index29.ToString() : "NULL");
        return sql;
    }
    public string GenerateSqlUpdate(string dataTableName)
    {
        if (Json == null) throw new NullReferenceException(nameof(Json));

        var sql = string.Format(@"UPDATE [{0}] 
			                            SET 
			                            ModelId = {35},
			                            Json = '{3}', 
			                            BroughtFromMasterDbOnUtcTicks = {4},
			                            Index0 = {5}, 
			                            Index1 = {6}, 
			                            Index2 = {7}, 
			                            Index3 = {8}, 
			                            Index4 = {9},
			                            Index5 = {10}, 
			                            Index6 = {11}, 
			                            Index7 = {12}, 
			                            Index8 = {13}, 
			                            Index9 = {14},
			                            Index10 = {15}, 
			                            Index11 = {16}, 
			                            Index12 = {17}, 
			                            Index13 = {18}, 
			                            Index14 = {19},
			                            Index15 = {20}, 
			                            Index16 = {21}, 
			                            Index17 = {22}, 
			                            Index18 = {23}, 
			                            Index19 = {24},
			                            Index20 = {25}, 
			                            Index21 = {26}, 
			                            Index22 = {27}, 
			                            Index23 = {28}, 
			                            Index24 = {29},
			                            Index25 = {30}, 
			                            Index26 = {31}, 
			                            Index27 = {32}, 
			                            Index28 = {33}, 
			                            Index29 = {34}
			                            WHERE ModelTypeLogicalName = '{1}' AND ModelId = {2}",
            dataTableName,
            ModelTypeLogicalName, 
            OriginalModelId, 
            Json.Replace("'", "''"),
            BroughtFromMasterDbOnUtc?.Ticks.ToString() ?? "NULL",
            Index0 != null ? "'" + Index0.Replace("'", "''") + "'" : "NULL",
            Index1 != null ? "'" + Index1.Replace("'", "''") + "'" : "NULL",
            Index2 != null ? "'" + Index2.Replace("'", "''") + "'" : "NULL",
            Index3 != null ? "'" + Index3.Replace("'", "''") + "'" : "NULL",
            Index4 != null ? "'" + Index4.Replace("'", "''") + "'" : "NULL",
            Index5 != null ? "'" + Index5.Replace("'", "''") + "'" : "NULL",
            Index6 != null ? "'" + Index6.Replace("'", "''") + "'" : "NULL",
            Index7 != null ? "'" + Index7.Replace("'", "''") + "'" : "NULL",
            Index8 != null ? "'" + Index8.Replace("'", "''") + "'" : "NULL",
            Index9 != null ? "'" + Index9.Replace("'", "''") + "'" : "NULL",
            Index10 != null ? Index10.ToString() : "NULL",
            Index11 != null ? Index11.ToString() : "NULL",
            Index12 != null ? Index12.ToString() : "NULL",
            Index13 != null ? Index13.ToString() : "NULL",
            Index14 != null ? Index14.ToString() : "NULL",
            Index15 != null ? Index15.ToString() : "NULL",
            Index16 != null ? Index16.ToString() : "NULL",
            Index17 != null ? Index17.ToString() : "NULL",
            Index18 != null ? Index18.ToString() : "NULL",
            Index19 != null ? Index19.ToString() : "NULL",
            Index20 != null ? Index20.ToString() : "NULL",
            Index21 != null ? Index21.ToString() : "NULL",
            Index22 != null ? Index22.ToString() : "NULL",
            Index23 != null ? Index23.ToString() : "NULL",
            Index24 != null ? Index24.ToString() : "NULL",
            Index25 != null ? Index25.ToString() : "NULL",
            Index26 != null ? Index26.ToString() : "NULL",
            Index27 != null ? Index27.ToString() : "NULL",
            Index28 != null ? Index28.ToString() : "NULL",
            Index29 != null ? Index29.ToString() : "NULL",
            ModelId);
        return sql;
    }
    #endregion

    #region Properties
    public string? ModelTypeLogicalName { get; set; }
    public long ModelId { get; set; }
    public long OriginalModelId { get; set; }
    public string? Json { get; set; }
    public DateTime? BroughtFromMasterDbOnUtc { get; set; }
    public string? Index0 { get; set; }
    public string? Index1 { get; set; }
    public string? Index2 { get; set; }
    public string? Index3 { get; set; }
    public string? Index4 { get; set; }
    public string? Index5 { get; set; }
    public string? Index6 { get; set; }
    public string? Index7 { get; set; }
    public string? Index8 { get; set; }
    public string? Index9 { get; set; }
    public long? Index10 { get; set; }
    public long? Index11 { get; set; }
    public long? Index12 { get; set; }
    public long? Index13 { get; set; }
    public long? Index14 { get; set; }
    public long? Index15 { get; set; }
    public long? Index16 { get; set; }
    public long? Index17 { get; set; }
    public long? Index18 { get; set; }
    public long? Index19 { get; set; }
    public double? Index20 { get; set; }
    public double? Index21 { get; set; }
    public double? Index22 { get; set; }
    public double? Index23 { get; set; }
    public double? Index24 { get; set; }
    public double? Index25 { get; set; }
    public double? Index26 { get; set; }
    public double? Index27 { get; set; }
    public double? Index28 { get; set; }
    public double? Index29 { get; set; }
    #endregion
}