using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public class LoginInfo
    {
        LoginStatus status = LoginStatus.NotLogin;
        string errMsg = null;
        bool isLoggedIn;

        /// <summary>
        /// 로그인 여부를 표시합니다.
        /// </summary>
        public bool IsLoggedIn
        {
            get
            {
                return isLoggedIn;
            }
            set
            {
                isLoggedIn = value;
            }
        }

        /// <summary>
        /// 로그인시 에러 메시지를 나타냅니다.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return errMsg;
            }
            set
            {
                errMsg = value;
            }
        }

        /// <summary>
        /// 로그인 상태를 나타냅니다.
        /// </summary>
        public LoginStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }

        public LoginInfo()
        {
            status = LoginStatus.NotLogin;
            isLoggedIn = false;
        }
    }
}
