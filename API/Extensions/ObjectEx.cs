using System.ComponentModel.DataAnnotations;

namespace API.Extensions;

public static class ObjectEx
{
    public static (List<ValidationResult> Results, bool IsValid) DataAnnotationsValidate(
        this object model
    )
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, context, results, true);

        return (results, isValid);
    }
}
