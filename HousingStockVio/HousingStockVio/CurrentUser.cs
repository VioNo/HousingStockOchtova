using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingStockVio
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string FullName { get; set; }
        public static string RoleName { get; set; }
        public static bool IsAuthenticated { get; set; }

        public static void Initialize(int userId, string fullName, string roleName)
        {
            UserId = userId;
            FullName = fullName;
            RoleName = roleName;
            IsAuthenticated = true;
        }

        public static void Clear()
        {
            UserId = 0;
            FullName = string.Empty;
            RoleName = string.Empty;
            IsAuthenticated = false;
        }
    }
}
