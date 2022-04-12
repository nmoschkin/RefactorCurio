using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRefactorCurio.ViewModels
{
    internal class RequestCloseEventArgs : EventArgs
    {

        public bool? IsSuccess { get; private set; } = null;

        public bool Cancel { get; set; }

        public RequestCloseEventArgs()
        {
        }

        public RequestCloseEventArgs(bool success)
        {
            IsSuccess = success;
        }

    }
}
