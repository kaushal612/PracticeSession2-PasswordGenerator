
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
namespace PasswordGenerator // Note: actual namespace depends on the project name.
{
    public class Program
    {
        const int DATASIZE = 3000000;
        const String query = "insert into PasswordTable(Password) values(@Password)";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string number = "0123456789";
        const string specialcharcters = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        const int noOfTask = 100;
        static long time = 0;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Password generating and storing under Processing.........");

            int eachTasksize = DATASIZE / noOfTask;
            int otherRemainingTasksize = DATASIZE % noOfTask;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                List<Task> task = new List<Task>();
                for (int i = 0; i < noOfTask; i++)
                {
                    int tasksize = eachTasksize;
                    if (i == 0)
                    {
                        tasksize += otherRemainingTasksize;
                    }
                    Task t1 = OperationOfPasswordGeneration(tasksize);
                    task.Add(t1);
                }

                Task t = Task.WhenAll(task);
                await t;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"{DATASIZE} Password generated and stored in database");
                Console.WriteLine($"execution time in ms {stopwatch.ElapsedMilliseconds}");
                Console.WriteLine($"execution time in second {(double)stopwatch.ElapsedMilliseconds / 1000}");
                Console.WriteLine($"execution time in minute {(double)stopwatch.ElapsedMilliseconds / 60000}");
                Console.WriteLine($"execution time of query {time}");
            }
        }
        private static async Task OperationOfPasswordGeneration(int tasksize)
        {
            SqlConnection Conn = new SqlConnection();
            Conn.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=MyDb;Integrated Security=True";
            try
            {
                Conn.Open();
                int cnt = 0;
                while (cnt < tasksize)
                {
                    string password = generatePassword();
                    SqlCommand command = new SqlCommand();
                    command.Connection = Conn;
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Password", password);
                    command.CommandType = CommandType.Text;
                    try
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        await command.ExecuteNonQueryAsync();
                        stopwatch.Stop();
                        time += stopwatch.ElapsedMilliseconds;
                        cnt++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Conn.Close();
            }
        }
        private static string generatePassword()
        {
            int ind;
            int typeLength;
            int remainingCharCount = 20;
            string password = "";
            Random random = new Random();
            typeLength = random.Next(1, 9);
            remainingCharCount -= typeLength;
            while (typeLength > 0)
            {
                ind = random.Next(0, 26);
                typeLength--;
                password += upper[ind];
            }


            typeLength = random.Next(1, 5);
            remainingCharCount -= typeLength;
            while (typeLength > 0)
            {
                ind = random.Next(0, specialcharcters.Length);
                password += specialcharcters[ind];
                typeLength--;
            }

            typeLength = random.Next(1, 7);
            remainingCharCount -= typeLength;
            while (typeLength > 0)
            {
                ind = random.Next(0, 10);
                typeLength--;
                password += number[ind];
            }

            typeLength = remainingCharCount;
            while (typeLength > 0)
            {
                ind = random.Next(0, 26);
                typeLength--;
                password += lower[ind];
            }

            return password;
        }
    }
}
