using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public class DeleteResult
    {
        bool success = false;
        string errMsg = "";

        public bool Success
        {
            get
            {
                return success;
            }
        }
        public string ErrorMessage
        {
            get
            {
                return errMsg;
            }
        }

        public DeleteResult(bool success, string msg)
        {
            this.success = success;
            this.errMsg = msg;
        }
    }
}
