using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    public class LoginInfo
    {

        /// <summary>
        /// 로그인 여부를 표시합니다.
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// 로그인시 에러 메시지를 나타냅니다.
        /// </summary>
        public string ErrorMessage { get; set; } = null;

        /// <summary>
        /// 로그인 상태를 나타냅니다.
        /// </summary>
        public LoginStatus Status { get; set; } = LoginStatus.NotLogin;

        public LoginInfo()
        {
            Status = LoginStatus.NotLogin;
            IsLoggedIn = false;
        }
    }
}
