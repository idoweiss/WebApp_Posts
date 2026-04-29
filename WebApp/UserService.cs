using System;
using System.Collections.Generic;

namespace WebApp
{
    public class UserService
    {
        // פעולה בונה סטאטית המאתחלת את הטבלה ומזינה נתוני דוגמה ראשוניים
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
            ('Neta Cohen', 'netac', 'p456', 'neta@example.com'),
            ('Yossi Levi', 'yossi_l', 'p789', 'yossi@example.com');");
        }

        // פעולה המחזירה רשימה של כל המשתמשים הקיימים במסד הנתונים
        public List<User> GetAllUsers()
        {
            List<User> usersList = DbHelper.RunSelect<User>("SELECT * FROM Users");
            return usersList;
        }

        // פעולה המוחקת משתמש ממסד הנתונים לפי המזהה שלו
        public void DeleteUser(int id)
        {
            DbHelper.RunSqlChange($"DELETE FROM Users WHERE Id = {id}");
        }

        // פעולה המוסיפה רשומה חדשה של משתמש למסד הנתונים
        public void AddNewUser(User newUser)
        {
            DbHelper.RunSqlChange($"INSERT INTO Users (FullName, UserName, Password, Email) VALUES ('{newUser.FullName}', '{newUser.UserName}', '{newUser.Password}', '{newUser.Email}')");
        }

        // פעולה המעדכנת את הפרטים של משתמש קיים במסד הנתונים
        public void UpdateUser(User user)
        {
            string sql = $"UPDATE Users SET FullName = '{user.FullName}', UserName = '{user.UserName}', Email = '{user.Email}' WHERE Id = {user.Id}";
            DbHelper.RunSqlChange(sql);
        }

        // פעולה הבודקת את פרטי ההתחברות ומחזירה את העצם של המשתמש אם הם תקינים
        public User GetUserByLogin(string userName, string password)
        {
            string sql = $"SELECT * FROM Users WHERE UserName = '{userName}' AND Password = '{password}'";
            List<User> results = DbHelper.RunSelect<User>(sql);

            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        // פעולה השולפת מהמסד ומחזירה עצם של משתמש לפי המזהה שלו
        public User GetUserById(int id)
        {
            string sql = $"SELECT * FROM Users WHERE Id = {id}";
            List<User> results = DbHelper.RunSelect<User>(sql);

            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }
    }
}