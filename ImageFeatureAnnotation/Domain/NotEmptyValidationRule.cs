﻿using System.Globalization;
using System.Windows.Controls;

namespace ImageFeatureAnnotation.Domain
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "不得为空.")
                : ValidationResult.ValidResult;
        }
    }
}