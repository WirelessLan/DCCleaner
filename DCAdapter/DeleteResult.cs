using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public class DeleteResult
    {
        public bool Success { get; } = false;

        public string ErrorMessage { get; } = "";

        public DeleteResult(bool success, string msg)
        {
            Success = success;
            ErrorMessage = msg;
        }
    }
}
