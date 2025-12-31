using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MediaWiz.Forums.Extensions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _targetValue;

        public RequiredIfAttribute(string dependentProperty, object targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
            ErrorMessage = "{0} is required.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the dependent property
            PropertyInfo? property = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_dependentProperty}");
            }

            // Get the dependent property's value
            object? dependentValue = property.GetValue(validationContext.ObjectInstance, null);

            // Compare values
            if ((dependentValue == null && _targetValue == null) ||
                (dependentValue != null && dependentValue.Equals(_targetValue)))
            {
                // If condition matches and value is null/empty, fail validation
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    return new ValidationResult(
                        FormatErrorMessage(validationContext.DisplayName)
                    );
                }
            }

            return ValidationResult.Success!;
        }
    }
}
