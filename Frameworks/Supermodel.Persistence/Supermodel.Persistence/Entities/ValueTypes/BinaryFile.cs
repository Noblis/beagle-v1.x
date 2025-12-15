using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json.Serialization;

namespace Supermodel.Persistence.Entities.ValueTypes;

public class BinaryFile : ValueObject, IComparable
{
    #region Methods
    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;
        var typedObj = (BinaryFile)obj;
        if (IsEmpty) return 0; //if we are an empty object, we say it equals b/c then we do not override db value
        var result = string.CompareOrdinal(FileName, typedObj.FileName);
        if (result != 0) return result;
        return BinaryContent.GetHashCode().CompareTo(typedObj.GetHashCode());
    }
    public override bool Equals(object? obj)
    {
        return Equals((BinaryFile?) obj);
    }
    public bool Equals(BinaryFile? other)
    {
        return other != null && FileName.Equals(other.FileName) && BinaryContent.SequenceEqual(other.BinaryContent);
    }
    public void Empty()
    {
        FileName = "";
        BinaryContent = Array.Empty<byte>();
    }
    #endregion

    #region Overrides
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
    public override string ToString()
    {
        return FileName;
    }
    #endregion

    #region Properties
    [JsonIgnore, NotMapped] public bool IsEmpty => string.IsNullOrEmpty(FileName) && BinaryContent.Length == 0;
    [JsonIgnore, NotMapped] public string Extension => Path.GetExtension(FileName);
    [JsonIgnore, NotMapped] public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);

    [MaxLength(100)] public string FileName { get; set; } = "";
    public byte[] BinaryContent { get; set; } = Array.Empty<byte>();
    #endregion
}