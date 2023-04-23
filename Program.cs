/* This is a C# console application that uses the Dapper and Npgsql packages to connect to and interact with a PostgreSQL database.

The program provides options to the user to view the database, add a user, delete a user, or update a user. */

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 23/04/2023 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

using Dapper;
using Npgsql;

namespace TestingPosgres;

// 4th commit

public class Program 
{
    // ~~~~~~~~~~~~~~~ Input a title ~~~~~~~~~~~~~~~
    static string readTitle()
    {
        string? title = Console.ReadLine(); 
        while (string.IsNullOrEmpty(title))
        {
            Console.Write("No Input. Please try again: ");
            title = Console.ReadLine();
        }
        return title;
    }
    // ~~~~~~~~~~~~~~~ Input true or false ~~~~~~~~~~~~~~~
    static bool readCompletion()
    {
        while (true)
        {
            Console.Write("Enter completion (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool completion))
            {
                return completion;
            }
            Console.WriteLine("Invalid input. Please try again.");
        }
    }
    // ~~~~~~~~~~~~~~~ Convert a string to an int ~~~~~~~~~~~~~~~
    static int StringToInt(string? inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
        {
            throw new ArgumentException("Input string is null or whitespace.", nameof(inputString));
        }

        if (!int.TryParse(inputString, out int intValue))
        {
            throw new ArgumentException("Input string is not a valid integer.", nameof(inputString));
        }

        return intValue;
    }
    // ~~~~~~~~~~~~~~~ Gets the number of rows ~~~~~~~~~~~~~~~
    static int getLastId(NpgsqlConnection connection)
    {
        connection.Open();
        int lastId = connection.QuerySingle<int>("SELECT COUNT(*) FROM users");
        connection.Close();

        return lastId;
    }
    // ~~~~~~~~~~~~~~~ Writes the database on the console ~~~~~~~~~~~~~~~
    static void writeDatabase(NpgsqlConnection connection)
    {
        connection.Open();
        var result = connection.Query<Users>("select * from users order by Id ;").ToList();
        connection.Close();

        if (!result.Any())
        {
            Console.WriteLine("No elements found in the database");

            return;
        }


        foreach (Users user in result)
        {
            string textUId = $"{user.UserId}";
            string textId = $"{user.Id}";           
            string textComp = $"{user.Completed}";
            string paddedtextUId = textUId.PadRight(3);
            string paddedTextId = textId.PadRight(4);
            string paddedtextComp = textComp.PadRight(6);
            Console.WriteLine(paddedTextId + paddedtextUId + paddedtextComp + user.Title);
        }
    }
    // ~~~~~~~~~~~~~~~ Main ~~~~~~~~~~~~~~~
    static void Main(string[] args)
    {
        var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mysecretpassword;Port=5432;";
        var connection = new NpgsqlConnection(connectionString);

        int lastId = getLastId(connection);

        while(true)
        {
            Console.Write("See the database(1), add a user(2), delete a user(3), update a user(4) or teminate the program(0): ");

            string? readChoice = Console.ReadLine();
            int inputChoice = StringToInt(readChoice);

            if (inputChoice == 0)
            {
                return;
            }
            else if (inputChoice == 1)
            {
                writeDatabase(connection);
            }
            else if (inputChoice == 2)
            {
                Console.Write("Input the title: ");
                string? inputTitle = readTitle();

                Console.Write("Input true or false: ");
                bool inputCompleted = readCompletion();

                lastId++;
                int lastUserId = 1 + (lastId - 1) / 20;
                connection.Open();
                connection.Execute("INSERT INTO users (UserId, Id, Title, Completed) VALUES (@userId, @id, @title, @completed)",
                   new { userId = lastUserId, id = lastId, title = inputTitle, completed = inputCompleted });

                connection.Close();
            }

            else if (inputChoice == 3)
            {
                Console.Write("Id of the user to be deleted: ");

                string? readChoice2 = Console.ReadLine(); 
                int deleteId = StringToInt(readChoice2);

                if (deleteId < 1 || deleteId > lastId)
                {
                    Console.WriteLine("Incorrect id. No user will be deleted.");
                    continue;
                }

                connection.Open();

                connection.Execute($"DELETE FROM users WHERE Id = {deleteId}");
                connection.Execute("CREATE TABLE new_users (id SERIAL PRIMARY KEY,userid INT,title TEXT,completed BOOLEAN);");
                connection.Execute("INSERT INTO new_users (userid, title, completed) SELECT userid, title, completed FROM users ORDER BY id;");
                connection.Execute("DROP TABLE users;");
                connection.Execute("ALTER TABLE new_users RENAME TO users;");
                connection.Execute("UPDATE users SET UserId = 1 + (Id - 1) / 20;");

                connection.Close();

                lastId--;
            }
            else if (inputChoice == 4)
            {
                Console.Write("Input the user's Id that you want to change the description: ");
                string? readChoice2 = Console.ReadLine(); 
                int updateId = StringToInt(readChoice2);

                if (updateId < 1 || updateId > lastId)
                {
                    Console.WriteLine("Incorrect id. Can't update a user.");
                    continue;
                }

                connection.Open();

                Console.Write("New title: ");
                string? updatedTitle = readTitle();
                
                Console.Write("true or false: ");
                bool? updatedCompletion = readCompletion();

                connection.Execute($"UPDATE users SET title = '{updatedTitle}', completed = {updatedCompletion} WHERE id = {updateId};");

                connection.Close();
            }
            else
            {
                Console.WriteLine("There is no such an option!");
            }

        }
        
    }

}

public class Users
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool Completed  { get; set; }
}