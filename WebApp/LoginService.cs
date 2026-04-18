using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace WebApp
{
    public class LoginService
    {
        private User loggedUser;
        private IJSRuntime js;

        // פעולה בונה המקבלת את הגישה לדפדפן
        public LoginService(IJSRuntime jsRuntime)
        {
            js = jsRuntime;
        }

        // פעולה מאוחדת לשליפת המשתמש המחובר - בודקת בזיכרון השרת או בשחזור מהדפדפן
        public async Task<User> GetLoggedUser()
        {
            // אם העצם כבר קיים בזיכרון, נחזיר אותו מיד
            if (loggedUser != null)
            {
                return loggedUser;
            }

            // ניסיון לשלוף את המזהה מהאחסון המקומי של הדפדפן (localStorage)
            string userIdStr = await js.InvokeAsync<string>("localStorage.getItem", "userId");

            if (userIdStr != null)
            {
                int userId = int.Parse(userIdStr);

                UserService service = new UserService();
                loggedUser = service.GetUserById(userId);
            }

            return loggedUser;
        }

        // פעולה לביצוע כניסה - מקבלת שם משתמש וסיסמה ומחזירה אמת אם הפרטים נכונים
        public async Task<bool> Login(string userName, string password)
        {
            string sql = "SELECT * FROM Users WHERE UserName = '" + userName + "' AND Password = '" + password + "'";
            List<User> results = DbHelper.RunSelect<User>(sql);

            if (results.Count > 0)
            {
                // שמירת המשתמש שנמצא בזיכרון של השירות
                loggedUser = results[0];

                // שמירת המזהה בדפדפן כדי שהחיבור יישאר גם לאחר רענון הדף
                await js.InvokeVoidAsync("localStorage.setItem", "userId", loggedUser.Id.ToString());

                return true;
            }

            return false;
        }

        // פעולה לביצוע יציאה
        public async Task Logout()
        {
            loggedUser = null;
            // מחיקת המידע מהדפדפן
            await js.InvokeVoidAsync("localStorage.removeItem", "userId");
        }
    }
}