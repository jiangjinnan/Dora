using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public enum PostHandlingAction
    {
        None,
        ThrowOriginal,
        ThrowNew
    }
}
