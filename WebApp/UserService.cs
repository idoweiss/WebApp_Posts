using System;
using System.Collections.Generic;

namespace WebApp
{
    public class UserService
    {
        // פעולה בונה סטאטית המאתחלת את הטבלה ואת נתוני הדוגמה
        static UserService()
        {
            DbHelper.RunSqlChange(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY,
                FullName TEXT NOT NULL,
                UserName TEXT UNIQUE NOT NULL,
                Password TEXT NOT NULL,
                Email TEXT UNIQUE NOT NULL
            );");

            DbHelper.RunSqlChange(@"
            INSERT OR IGNORE INTO Users (FullName, UserName, Password, Email) VALUES
            ('מנהל מערכת', 'admin', 'admin123', 'admin@webapp.com'),
            ('Israel Israeli', 'israel1', 'p123', 'israel@example.com'),
            ('Noa Cohen', 'noac', 'p456', 'noa@example.com'),
            ('Yossi Levi', 'yossi_l', 'p789', 'yossi@example.com');");
        }

        // פעולה להחזרת כל המשתמשים מהמסד
        public List<User> GetAllUsers()
        {
            List<User> usersList = DbHelper.RunSelect<User>("SELECT * FROM Users");
            return usersList;
        }

        public void DeleteUser(int id)
        {
            DbHelper.RunSqlChange("DELETE FROM Users WHERE Id = " + id);
        }

        public void AddNewUser(User newUser)
        {
            DbHelper.RunSqlChange("INSERT INTO Users (FullName, UserName, Password, Email) VALUES ('" + newUser.FullName + "', '" + newUser.UserName + "', '" + newUser.Password + "', '" + newUser.Email + "')");
        }

        public void UpdateUser(User user)
        {
            string sql = "UPDATE Users SET FullName = '" + user.FullName + "', UserName = '" + user.UserName + "', Email = '" + user.Email + "' WHERE Id = " + user.Id;
            DbHelper.RunSqlChange(sql);
        }

        // פעולה לאימות פרטי כניסה
        public User GetUserByLogin(string userName, string password)
        {
            string sql = "SELECT * FROM Users WHERE UserName = '" + userName + "' AND Password = '" + password + "'";
            List<User> results = DbHelper.RunSelect<User>(sql);

            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        // פעולה לשליפת עצם של משתמש לפי המזהה שלו
        public User GetUserById(int id)
        {
            string sql = "SELECT * FROM Users WHERE Id = " + id;
            List<User> results = DbHelper.RunSelect<User>(sql);

            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }
    }
}