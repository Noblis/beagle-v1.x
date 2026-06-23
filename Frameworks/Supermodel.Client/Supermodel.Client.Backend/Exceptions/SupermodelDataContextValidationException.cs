using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Validations;

namespace Supermodel.Client.Backend.Exceptions;

public class SupermodelDataContextValidationException : SupermodelException
{
    #region Embedded Classes
    public class ValidationError : List<ValidationError.Error>
    {
        #region Constructors
        public ValidationError() { }
        public ValidationError(IEnumerable<ValidationResult> validationResults, PendingAction failedAction, string message)
        {
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    var existingError = this.SingleOrDefault(x => x.Name == memberName);
                    if (existingError != null) existingError.ErrorMessages.Add(validationResult.ErrorMessage ?? "");
                    else Add(new Error(memberName, validationResult.ErrorMessage ?? ""));
                }
                if (!validationResult.MemberNames.Any()) Add(new Error("", validationResult.ErrorMessage ?? ""));
            }
            FailedAction = failedAction;
            Message = message;
        }
        #endregion

        #region Embedded Classes
        public class Error
        {
            #region Constructors
            public Error(string name, List<string> errorMessages)
            {
                Name = name;
                ErrorMessages = errorMessages;
            }
            public Error(string name, string errorMessage)
            {
                Name = name;
                ErrorMessages = [errorMessage];
            }
            public Error()
            {
                ErrorMessages = [];
            }
            #endregion

            #region Properties
            public string? Name { get; set; }
            public List<string> ErrorMessages { get; set; }
            #endregion
        }
        #endregion

        #region Properties
        [JsonIgnore] public PendingAction? FailedAction { get; set; }
        public string? Message { get; set; }
        #endregion
    }
    #endregion

    #region Constructors
    public SupermodelDataContextValidationException(ValidationError validationError)
    {
        _validationErrors = [validationError];
    }
    public SupermodelDataContextValidationException(List<ValidationError> validationErrors)
    {
        _validationErrors = validationErrors;
    }
    #endregion

    #region Methods
    protected virtual List<ValidationResultList> GetListOfValidationResultLists()
    {
        var lvrl = new List<ValidationResultList>();    
        foreach (var validationError in _validationErrors)
        {
            var vrl = new ValidationResultList();
            foreach (var error in validationError)
            {
                if (error.Name == "id") continue;
                if (error.Name!.StartsWith("apiModelItem.")) error.Name = error.Name.Split('.').Last();
                foreach (var errorMessage in error.ErrorMessages)
                {
                    var vr = new ValidationResult(errorMessage, [error.Name]);
                    if (!VrlContainsVr(vrl, vr)) vrl.Add(vr);
                }
            }
            lvrl.Add(vrl);
        }
        return lvrl;
    }
    private bool VrlContainsVr(ValidationResultList vrl, ValidationResult newVr)
    {
        foreach (var vr in vrl)
        {
            if (AreVrsEqual(vr, newVr)) return true;
        }
        return false;
    }
    private bool AreVrsEqual(ValidationResult vr1, ValidationResult vr2)
    {
        if (vr1.ErrorMessage != vr2.ErrorMessage) return false;
        var memberNames1 = vr1.MemberNames.ToArray();
        var memberNames2 = vr2.MemberNames.ToArray();
        if (memberNames1.Length != memberNames2.Length) return false;
        for (var i = 0; i < memberNames1.Length; i++)
        {
            if (memberNames1[i] != memberNames2[i]) return false;
        }
        return true;
    }
    #endregion

    #region Properties
    public List<ValidationResultList> ValidationErrors => GetListOfValidationResultLists();
    private readonly List<ValidationError> _validationErrors;
    #endregion
}