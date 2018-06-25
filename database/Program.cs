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
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=DB_CourceProject;Integrated Security=True;"))
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

        static public string SelectRowsByCategory(int User_id)
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=DB_CourceProject;Integrated Security=True;"
              ))
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText = $@"select name_category, sum(amount)
                                             from costs
                                             where user_id = {User_id} 
                                             group by name_category";


                    QC.SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        sb.Append("").Append(String.Format("\n{0}\t{1} ", reader.GetValue(0), reader.GetDouble(1)));
                    }
                    

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

        static public void InsertUser(int User_id, string Name)
        {
            using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=DB_CourceProject;Integrated Security=True;"))
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
                connection.Close();
            }
        }

        static public void InsertAmount(int User_id, double Amount, string Categore)
        {
            using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=DB_CourceProject;Integrated Security=True;"))
            {
                connection.Open();
                DateTime dt = DateTime.Now;
                QC.SqlParameter parameter;
                using (var command = new QC.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = DT.CommandType.Text;
                    command.CommandText = $@"INSERT INTO [dbo].[costs] (user_id, amount, name_category, dt) VALUES(@User_id, @Amount, @Categore, @dt);";

                    parameter = new QC.SqlParameter("@User_id", DT.SqlDbType.Int);
                    parameter.Value = User_id;
                    command.Parameters.Add(parameter);

                    parameter = new QC.SqlParameter("@Amount", DT.SqlDbType.Float);
                    parameter.Value = Amount;
                    command.Parameters.Add(parameter);

                    parameter = new QC.SqlParameter("@Categore", DT.SqlDbType.NChar, 50);
                    parameter.Value = Categore;
                    command.Parameters.Add(parameter);

                    parameter = new QC.SqlParameter("@dt", DT.SqlDbType.DateTime);
                    parameter.Value = dt;
                    command.Parameters.Add(parameter);
                    QC.SqlDataReader reader = command.ExecuteReader();
                    connection.Close();
                }
            }
        }

        static public void DeleteRows(int User_id)
        {
            using (var command = new QC.SqlCommand())
            {
                using (var connection = new QC.SqlConnection("Data Source=LENOVO-PC;Initial Catalog=DB_CourceProject;Integrated Security=True;"))
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
                    connection.Close();
                }
            }
        }
    }
}
