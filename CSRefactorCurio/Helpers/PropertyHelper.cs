using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio.Helpers
{
    internal static class PropertyHelper
    {

        public static object GetProperty(EnvDTE.ProjectItem item, [CallerMemberName]string propertyName = null)
        {

            foreach (EnvDTE.Property property in item.Properties)
            {
                if (property.Name == propertyName)
                {
                    return property.Value;
                }
            }

            return null;
        }

        public static object GetProperty(EnvDTE.Solution sln, [CallerMemberName] string propertyName = null)
        {

            foreach (EnvDTE.Property property in sln.Properties)
            {

                if (property.Name == propertyName)
                {
                    return property.Value;
                }
            }

            return null;
        }

        public static object GetProperty(EnvDTE.Project project, [CallerMemberName] string propertyName = null)
        {
            foreach(EnvDTE.Property property in project.Properties)
            {
                if (property.Name == propertyName)
                {
                    return property.Value;
                }
            }

            return null;
        }
    }
}
