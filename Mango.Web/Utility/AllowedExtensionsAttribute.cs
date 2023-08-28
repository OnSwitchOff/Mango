using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class AllowedExtensionsAttribute: ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
                _allowedExtensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);

                if(!_allowedExtensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult("This file extension is not allowed!");
                }
            }

            return ValidationResult.Success;
        }
    }
}
