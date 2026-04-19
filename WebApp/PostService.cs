namespace WebApp
{
    using System.Collections.Generic;

    public class PostService
    {
        // פעולה בונה המיועדת להקמת טבלאות והזנת נתונים ראשוניים
        public PostService()
        {
            // יצירת טבלת הפוסטים עם מפתח זר למשתמשים
            DbHelper.RunSqlChange(@"
            CREATE TABLE IF NOT EXISTS Posts (
                Id INTEGER PRIMARY KEY,
                Text TEXT NOT NULL,
                ImageUrl TEXT,
                Likes INTEGER DEFAULT 0,
                UserId INTEGER NOT NULL,
                FOREIGN KEY(UserId) REFERENCES Users(Id)
            );");

            // יצירת טבלת הלייקים למניעת כפילויות
            DbHelper.RunSqlChange(@"
            CREATE TABLE IF NOT EXISTS Likes (
                PostId INTEGER,
                UserId INTEGER,
                PRIMARY KEY (PostId, UserId)
            );");

            // הזנת נתונים ראשוניים לבדיקה
            DbHelper.RunSqlChange(@"
            INSERT OR IGNORE INTO Posts (Id, Text, ImageUrl, Likes, UserId) VALUES 
            (1, 'Beautiful Sunset', 'https://loremflickr.com/200/200/sunset', 10, 1),
            (2, 'City Lights', 'https://loremflickr.com/200/200/city,night', 5, 2),
            (3, 'Mountain View', 'https://loremflickr.com/200/200/mountains', 20, 3),
            (4, 'Forest Path', 'https://loremflickr.com/200/200/forest', 0, 1),
            (5, 'Ocean Waves', 'https://loremflickr.com/200/200/ocean', 50, 2),
            (6, 'Desert Sand', 'https://loremflickr.com/200/200/desert', 15, 3),
            (7, 'Starry Night', 'https://loremflickr.com/200/200/stars', 8, 1),
            (8, 'Green Fields', 'https://loremflickr.com/200/200/field', 2, 2),
            (9, 'Autumn Leaves', 'https://loremflickr.com/200/200/leaves', 30, 3),
            (10, 'Snowy Peaks', 'https://loremflickr.com/200/200/snow', 40, 1),
            (11, 'Tropical Beach', 'https://loremflickr.com/200/200/beach', 12, 2),
            (12, 'Spring Flowers', 'https://loremflickr.com/200/200/flower', 18, 3),
            (13, 'Morning Coffee', 'https://loremflickr.com/200/200/coffee', 25, 1);");
        }

        // פעולה לשליפת כל הפוסטים עם פרטי הכותב ומזהה המשתמש
        public List<PostWithAuthor> GetAllPostsByLikes()
        {
            // השאילתה כוללת את מזהה המשתמש כדי לאפשר בדיקת בעלות בדפים
            string sql = $@"SELECT Posts.Id, Posts.Text, Posts.ImageUrl, Posts.Likes, Posts.UserId, Users.FullName 
                            FROM Posts 
                            JOIN Users ON Posts.UserId = Users.Id 
                            ORDER BY Posts.Likes DESC";

            List<PostWithAuthor> list = DbHelper.RunSelect<PostWithAuthor>(sql);
            return list;
        }

        // פעולה לשליפת פוסטים השייכים למשתמש ספציפי בלבד
        public List<PostWithAuthor> GetPostsByUserId(int userId)
        {
            string sql = $@"SELECT Posts.Id, Posts.Text, Posts.ImageUrl, Posts.Likes, Posts.UserId, Users.FullName 
                            FROM Posts 
                            JOIN Users ON Posts.UserId = Users.Id 
                            WHERE Posts.UserId = {userId}";

            List<PostWithAuthor> list = DbHelper.RunSelect<PostWithAuthor>(sql);
            return list;
        }

        // פעולה לשליפת עצם פוסט בודד לפי המזהה שלו
        public Post GetPostById(int id)
        {
            string sql = $"SELECT * FROM Posts WHERE Id = {id}";
            List<Post> results = DbHelper.RunSelect<Post>(sql);
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        // פעולה לעדכון תוכן של פוסט קיים
        public void UpdatePost(Post post)
        {
            string sql = $"UPDATE Posts SET Text = '{post.Text}', ImageUrl = '{post.ImageUrl}' WHERE Id = {post.Id}";
            DbHelper.RunSqlChange(sql);
        }

        // פעולה למחיקת פוסט וניקוי הלייקים הקשורים אליו
        public void DeletePost(int id)
        {
            // מחיקת הלייקים תחילה כדי לשמור על תקינות מסד הנתונים
            string sqlLikes = $"DELETE FROM Likes WHERE PostId = {id}";
            DbHelper.RunSqlChange(sqlLikes);

            string sqlPost = $"DELETE FROM Posts WHERE Id = {id}";
            DbHelper.RunSqlChange(sqlPost);
        }

        // פעולה להוספת פוסט חדש למערכת
        public void AddNewPost(Post post)
        {
            string sql = $"INSERT INTO Posts (Text, ImageUrl, Likes, UserId) VALUES ('{post.Text}', '{post.ImageUrl}', 0, {post.UserId})";
            DbHelper.RunSqlChange(sql);
        }

        // פעולה להוספת לייק תוך מניעת הצבעה כפולה של אותו משתמש
        public void AddLike(int postId, int userId)
        {
            // בדיקה האם המשתמש כבר נתן לייק לפוסט זה
            string checkSql = $"SELECT * FROM Likes WHERE PostId = {postId} AND UserId = {userId}";
            List<PostWithAuthor> results = DbHelper.RunSelect<PostWithAuthor>(checkSql);

            if (results.Count == 0)
            {
                // הוספת רישום בטבלת הלייקים
                string insertLike = $"INSERT INTO Likes (PostId, UserId) VALUES ({postId}, {userId})";
                DbHelper.RunSqlChange(insertLike);

                // עדכון מונה הלייקים בטבלת הפוסטים
                string updatePost = $"UPDATE Posts SET Likes = Likes + 1 WHERE Id = {postId}";
                DbHelper.RunSqlChange(updatePost);
            }
        }
    }
}