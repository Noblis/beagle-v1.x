using System;
using System.Collections.Generic;

namespace Supermodel.DataAnnotations.Validations;

public class ValidationResultException : Exception
{
    #region Constructors
    public ValidationResultException(string errorMessage, IEnumerable<string> memberNames)
    {
        ValidationResultList = [new(errorMessage, memberNames)];
    }
    public ValidationResultException(ValidationResultList validationResultList)
    {
        ValidationResultList = validationResultList;
    }
    public ValidationResultException(string errorMessage) : this(errorMessage, new List<string> { " " }) { }
    #endregion

    #region Properties
    public ValidationResultList ValidationResultList { get; protected set; }
    #endregion
}