using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.Persistence.Entities.ValueTypes;
using Supermodel.ReflectionMapper;

namespace Supermodel.Presentation.Mvc.Models;

public abstract class BinaryFileModelBase : IRMapperCustom, IComparable
{
    #region Methods
    [JsonIgnore, NotRMapped] public virtual string Extension => Path.GetExtension(FileName);
    [JsonIgnore, NotRMapped] public virtual string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);
    [JsonIgnore, NotRMapped] public virtual bool IsEmpty => string.IsNullOrEmpty(FileName) && BinaryContent.Length == 0;

    public void Empty()
    {
        FileName = "";
        BinaryContent = Array.Empty<byte>();
    }
    #endregion

    #region IComparable implemtation
    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;
        if (!(obj is BinaryFileModelBase typedObj)) throw new SupermodelException("obj is not BinaryFileApiModel");
        //if (FileName == null) return 0; //if we are an empty object, we say it equals b/c then we do not override db value
        var result = string.CompareOrdinal(FileName, typedObj.FileName);
        return result != 0 ? result : BinaryContent.GetHashCode().CompareTo(typedObj.GetHashCode());
    }
    #endregion

    #region Overrides for Equal and Hash
    public override bool Equals(object? obj)
    {
        return Equals((BinaryFileModelBase?) obj);
    }

    public bool Equals(BinaryFileModelBase? other)
    {
        return other != null && FileName.Equals(other.FileName) && BinaryContent.SequenceEqual(other.BinaryContent);
    }
    public override int GetHashCode()
    {
        // http://stackoverflow.com/a/263416/39396
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            // ReSharper disable NonReadonlyMemberInGetHashCode
            hash = hash * 23 + FileName.GetHashCode();
            hash = hash * 23 + BinaryContent.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
            return hash;
        }
    }
    #endregion

    #region ICustomMapper implemtation
    public virtual Task MapFromCustomAsync<T>(T other)
    {
        if (!typeof(BinaryFile).IsAssignableFrom(typeof(T))) throw new PropertyCantBeAutomappedException($"{GetType().Name} can't be automapped to {typeof(T).Name}");

        var binaryFileOther = (BinaryFile?)(object?)other;
        FileName = binaryFileOther?.FileName ?? "";
        BinaryContent = binaryFileOther?.BinaryContent ?? Array.Empty<byte>();

        return Task.CompletedTask;
    }
    public virtual Task<T> MapToCustomAsync<T>(T other)
    {
        if (!typeof(BinaryFile).IsAssignableFrom(typeof(T))) throw new PropertyCantBeAutomappedException($"{GetType().Name} can't be automapped to {typeof(T).Name}");

        BinaryFile? binaryFileOther;
        if (other != null)
        {
            binaryFileOther = (BinaryFile?)(object?)other;
        }
        else
        {
            binaryFileOther = (BinaryFile?)ReflectionHelper.CreateType(typeof(T));
        }
        if (binaryFileOther == null) throw new SupermodelException("binaryFileOther == null: this should never happen");
        binaryFileOther.FileName = FileName;
        binaryFileOther.BinaryContent = BinaryContent;
        return Task.FromResult((T)(object)binaryFileOther);
    }
    #endregion

    #region Properties
    public string FileName { get; set; } = "";
    
    //we copy shallow here for performance reasons
    [RMCopyShallow] public byte[] BinaryContent { get; set; } = Array.Empty<byte>();
    #endregion
}