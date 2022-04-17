using DataTools.CSTools;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CSRefactorCurio.Validators
{
    internal class DirExistsValidator : ValidationRule
    {
        public CurioProject Project { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            
            if (value is string s)
            {
                if (s.Trim() == "") return ValidationResult.ValidResult;

                if (Project == null)
                {
                    return Directory.Exists(s) ? ValidationResult.ValidResult : new ValidationResult(false, $"Directory '{s}' does not exist.");
                }
                else
                {
                    if (s.ToLower().StartsWith(Project.ProjectRootPath.ToLower()))
                    {
                        return Directory.Exists(s) ? ValidationResult.ValidResult : new ValidationResult(false, $"Directory '{s}' does not exist.");
                    }
                    else
                    {
                        return new ValidationResult(false, "Directory is not within the current project.");
                    }
                }
            }
            else if (value is null)
            {
                return ValidationResult.ValidResult;
            }
            throw new NotImplementedException();
        }
    }
}
