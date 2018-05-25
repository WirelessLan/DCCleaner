using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public enum LoginStatus {
        Success,
        PasswordError,
        IDError,
        ErrorBoth,
        NotLogin,
        NoSuchID,
        MaximumAttemptFailed,
        Unknown
    };
}
