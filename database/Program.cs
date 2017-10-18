using System;
using DT = System.Data;
using QC = System.Data.SqlClient;

namespace DataBase
{
    public class Program
    {
        static public void Main()
        {

        }

        static public string SelectRowsByDates(int User_id)
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"
              ))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText =
                    $@"SELECT dt, amount FROM costs WHERE user_id = {User_id} 
                    ORDER BY dt DESC;";


                    QC.SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        sb.Append("").Append(String.Format("\n{0}\t{1} Р", reader.GetDateTime(0), reader.GetDouble(1)));
                    }

                    connection.Close();
                    command.CommandText = null;
                    string str = sb.ToString();
                    sb.Clear();
                    if (String.IsNullOrEmpty(str))
                    {
                        return String.Format("Пока что список твоих трат пуст\nотправь мне фотографию чека.");
                    }
                    else
                        return str;

                }
            }
        }

       /* static public string SelectRowsSumm(int User_id)
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=WalletKeeperDB;Integrated Security=True;"
              ))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText =
                    $@"SELECT dt, summ FROM [dbo].[profit] WHERE User_id = {User_id} 
                    ORDER BY dt DESC;";


                    QC.SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        sb.Append("").Append(String.Format("\n{0}\t{1} Р", reader.GetDateTime(0), reader.GetDouble(1)));
                    }

                    connection.Close();
                    command.CommandText = null;
                    string str = sb.ToString();
                    sb.Clear();
                    if (String.IsNullOrEmpty(str))
                    {
                        return String.Format("Пока что ты не вносил данные о доходах\n Для того, чтобы сделать это, набери /profit сумма");
                    }
                    else
                        return str;

                }
            }
        }*/

        static public string SelectRowsByCategory(int User_id)
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"
              ))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText = $@"select cat.name_category, sum(cos.amount)
                                             from categories cat, costs cos
                                             where cat.cost_id = cos.id
                                             group by cat.name_category";


                    QC.SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        sb.Append("").Append(String.Format("\n{0}\t{1} ", reader.GetValue(0), reader.GetDouble(1)));
                    }

                    connection.Close();

                    string str = sb.ToString();
                    if (String.IsNullOrEmpty(str))
                    {
                        return String.Format("Пока что список твоих трат по этой категории пуст\nотправь мне фотографию чека.");
                    }
                    else
                        return str;

                }
            }

        }

        static public void InsertCategory(int User_id, string Name)
        {
            using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"))
            {
                connection.Open();
                QC.SqlParameter parameter;
                using (var command = new QC.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText = $@"INSERT INTO [dbo].[categories] (name_category, cost_id) VALUES(@Name, (select top 1 id from costs order by id desc));";

                    parameter = new QC.SqlParameter("@Name", DT.SqlDbType.NChar, 50);
                    parameter.Value = Name;
                    command.Parameters.Add(parameter);

                    QC.SqlDataReader reader = command.ExecuteReader();
                    Console.WriteLine("Insert category - OK");

                }
            }

        }

        static public void InsertUser(int User_id, string Name)
        {
            using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"))
            {
                connection.Open();
                QC.SqlParameter parameter;
                using (var command = new QC.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText = $@"IF NOT EXISTS (SELECT name_user FROM users WHERE user_id = '{User_id}') 
                                            INSERT INTO users(user_id, name_user) VALUES( @User_id, @Name);";

                    parameter = new QC.SqlParameter("@Name", DT.SqlDbType.NChar, 50);
                    parameter.Value = Name;
                    command.Parameters.Add(parameter);

                    parameter = new QC.SqlParameter("@User_id", DT.SqlDbType.Int);
                    parameter.Value = (int)User_id;
                    command.Parameters.Add(parameter);
                    QC.SqlDataReader reader = command.ExecuteReader();
                    Console.WriteLine("Insert - OK");

                }
            }
        }

        static public string select()
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"
              ))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText =
                    $@"SELECT * from costs;";


                    QC.SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        sb.Append("").Append(String.Format("\n{0}\t{1} Р", reader.GetDateTime(0), reader.GetDouble(1)));
                    }

                    connection.Close();
                    command.CommandText = null;
                    string str = sb.ToString();
                    sb.Clear();
                    if (String.IsNullOrEmpty(str))
                    {
                        return String.Format("Пока что список твоих трат пуст\nотправь мне фотографию чека.");
                    }
                    else
                        return str;

                }
            }
        }
        static public void InsertAmount(int User_id, double Amount)
        {
            using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"))
            {
                connection.Open();
                DateTime dt = DateTime.Now;
                QC.SqlParameter parameter;
                using (var command = new QC.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText = $@"INSERT INTO [dbo].[costs] (user_id, amount, dt) VALUES(@User_id, @Amount, @dt);";

                    parameter = new QC.SqlParameter("@User_id", DT.SqlDbType.Int);
                    parameter.Value = User_id;
                    command.Parameters.Add(parameter);

                    parameter = new QC.SqlParameter("@Amount", DT.SqlDbType.Float);
                    parameter.Value = Amount;
                    command.Parameters.Add(parameter);

                    parameter = new QC.SqlParameter("@dt", DT.SqlDbType.DateTime);
                    parameter.Value = dt;
                    command.Parameters.Add(parameter);
                    QC.SqlDataReader reader = command.ExecuteReader();
/*
                    command.CommandText =
                    $@"SELECT dt, amount FROM costs WHERE user_id = {User_id} 
                    ORDER BY dt DESC;";

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    QC.SqlDataReader reader2 = command.ExecuteReader();

                    while (reader2.Read())
                    {
                        sb.Append("").Append(String.Format("\n{0}\t{1} Р", reader.GetDateTime(0), reader.GetDouble(1)));
                    }

                    connection.Close();
                    command.CommandText = null;
                    string str = sb.ToString();
                    Console.WriteLine(str);*/

                }
            }
        }

        /*        static public void InsertSumm(int User_id, double Amount)
                {
                    using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=WalletKeeperDB;Integrated Security=True;"))
                    {
                        connection.Open();
                        DateTime dt = DateTime.Now;
                        QC.SqlParameter parameter;
                        using (var command = new QC.SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandType = DT.CommandType.Text;
                            command.CommandText = $@"INSERT INTO [dbo].[profit] (User_id, summ, dt) VALUES(@User_id, @summ, @dt);";

                            parameter = new QC.SqlParameter("@User_id", DT.SqlDbType.Int);
                            parameter.Value = User_id;
                            command.Parameters.Add(parameter);

                            parameter = new QC.SqlParameter("@summ", DT.SqlDbType.Float);
                            parameter.Value = Amount;
                            command.Parameters.Add(parameter);

                            parameter = new QC.SqlParameter("@dt", DT.SqlDbType.DateTime);
                            parameter.Value = dt;
                            command.Parameters.Add(parameter);
                            QC.SqlDataReader reader = command.ExecuteReader();
                        }
                    }
                }
                */

        static public void DeleteLast()
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"))
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText =
                    $@"DELETE FROM categories WHERE cost_id=(SELECT MAX(cost_id) FROM categories)";
                    Console.WriteLine("DELETED");
                    QC.SqlDataReader reader = command.ExecuteReader();
                }
            }
        }
        static public void DeleteRows(int User_id)
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=HackathonABBYY;Integrated Security=True;"))
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText =
                    $@"DELETE FROM [dbo].[categories]
                       DELETE FROM [dbo].[costs] WHERE User_id = {User_id} 
                       DELETE FROM [dbo].[users] WHERE User_id = {User_id};";
                    QC.SqlDataReader reader = command.ExecuteReader();
                    Console.WriteLine("Delete - OK");
                }
            }
        }
    }
}
