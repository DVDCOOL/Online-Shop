using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.IO;

namespace c_sharp
{
    class User
    {
        public int id;
        public string surname;
        public string last_name;

        public string user_name;
        public string password;

        public int age;
        public string country;
        public int balance;
        public List<int> user_products = new List<int>();

        public User(int _id, string _user, string _pass, string _sur, string _last, int _age, string _country)
        {
            id = _id;
            surname = _sur;
            last_name = _last;
            user_name = _user;
            password = _pass;
            age = _age;
            country = _country;

            balance = 0;
        }

        public void Add_Money(int amount)
        {
            if(amount >= 0)
            {
                balance += amount;
            }
            Console.WriteLine("Your balance now is "+ balance);
        }

        public void Buy_item(Product item)
        {
            if(item.amount > 0)
            {
                int cost = item.price;
                string place = item.country;

                if(place != country)
                {
                    cost += 5;
                }

                if(cost > balance)
                {
                    Console.WriteLine("You don't have enougth money to buy that item");
                }else
                {
                    Console.WriteLine("You successfully bougth the item " + item.id + " called: '" + item.name + "'");
                    item.amount -=1 ;
                    Program.users[item.user_id].balance =+ item.price;
                }
            }else
            {
                Console.WriteLine("Item " + item.id + " called: " + item.name + " is out of stock");
            }
        }

        public Product Add_product(int index)
        {
            Console.WriteLine("What is the name of the product?");
            string prod_name = Console.ReadLine();
            while (prod_name == "")
            {
                prod_name = Console.ReadLine();
            }
            
            Program.Print("What is the price of the product?");
            int prod_price = Program.StringToInt(Program.Read());
            
            Program.Print("How many items do you have");
            int prod_amount = Program.StringToInt(Program.Read());

            
            user_products.Add(index);

            return new Product(index, prod_name, country, prod_price, prod_amount, id);
        }
        public void UserData()
        {
            Program.Print("User ID: " + id);
            Program.Print("Username: " + user_name);
            Program.Print("Surname: " + surname);
            Program.Print("Lastname: " + last_name);
            Program.Print("Age: " + age);
            Program.Print("Country: " +country);
            Program.Print("Products: " );
            if(user_products.Count > 0){
                for (int i = 0; i < user_products.Count; i++)
                {
                    Product product =Program.products[i] ;
                    Program.Print(product.amount + " "+ product.name);
                }
            }else Program.Print("None");
        }

    }

    class Product
    {
        public int id;
        public string name;
        public string country;
        public int price;
        public int amount;
        public int user_id;

        public Product(int _id, string _name, string _country, int _price, int _amount, int _user)
        {
            id = _id;
            name = _name;
            country = _country;
            price = _price;
            amount = _amount;
            user_id = _user;
        }
    }


    class Program
    {
        static List<User> users = new List<User>();
        public static List<Product> products = new List<Product>();
        
        static void Main(string[] args)
        {
            DB.InitializeDatabase();

            users = DB.GetAllUsers();
            products = DB.GetAllProducts();
            
            bool loged = false;
            while(!loged){
                User main_user = null;
                if (users.Count == 0){
                    Print("You are the first User!");
                    main_user = SignIn();
                    loged = true;
                }else{
                    Print("Press 1 for Log In or 2 for Sign In");
                    
                    while(! loged)
                    {
                        string command = Read();
                        if (command == "1")
                        {
                            main_user = LogIn();
                            loged = true;
                        }else if(command == "2")
                        {
                            main_user = SignIn();
                            loged = true;
                        }else
                        {
                            Print("You didn't give me a command. Try again");
                        }
                    }

                }
                Print("Welcome "+ main_user.user_name);
                Print("Press 1 for adding Products \nPress 2 for User Data \nPress 3 for viewing all Items \nPress L for Log Out");

                while(loged){

                    string command = Read();
                    if(command == "1"){
                        var newProd = main_user.Add_product(products.Count());
                        DB.AddProduct(newProd);
                        products.Add(newProd);
                    }else if (command == "2")
                    {
                        main_user.UserData();
                    }else if (command == "3")
                    {
                        ViewItems();
                    }else if(command == "L")
                    {
                        Print("Logging Out!");
                        loged = false;
                    }else
                    {
                        Print("You gave me no command");
                    }

                }
            }
            Console.ReadKey();
        }

        public static void ViewItems()
        {
            if (products.Count > 0)
            {
                
            
                for (int i = 0; i < products.Count; i++)
                {
                    Product pro = products[i];
                    Print(pro.amount + "x Product: " +pro.id + " Name: " +pro.name+ " Cost: " + pro.price + "$ in " + pro.country);
                }
            }else Print("There are no Items");
        }

        public static User LogIn()
        {
            bool user_found = false;
            int user_index = 0;

            while(! user_found)
            {
                Print("Username: ");
                string username = Read();
                for (int i = 0; i < users.Count; i++)
                {
                    if (username == users[i].user_name)
                    {
                        Print("Password: ");
                        string password = Read();
                        
                        
                        if (password == users[i].password)
                        {
                            user_index = i;
                            Print("Welcome " + users[i].user_name);
                            user_found = true;
                        }else
                        {
                            Print("Password is wrong");
                        }
                        
                    }
                }
                if(! user_found){
                    Print("No user called "+ username);
                }
            }
            
            
            return users[user_index];
        }

        public static User SignIn()
        {
            Print("Username: ");
            string username = UsernameSearch();
            Print("Password: ");
            string password = FindStrings(Read());
            Print("Surname: ");
            string surname = FindStrings(Read());
            Print("Lastname: ");
            string lastname = FindStrings(Read());
            Print("Age: ");
            int age = StringToInt(Read());
            Print("Country: ");
            string country = FindStrings(Read());

            var newUser = new User(users.Count, username, password, surname, lastname, age, country);
            DB.AddUser(newUser);
            users.Add(newUser);

            return newUser;
        }

        public static string UsernameSearch()
        {
            bool us_found = false;

            string username = "";

            while(! us_found)
            {
                username = Read();
                if(username != "")
                {
                    bool us_ex = false;
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (username == users[i].user_name)
                        {
                            Print("Username already exists");
                            us_ex = true;
                            break;
                        }
                    }
                    if(!us_ex)
                    {
                        us_found = true;
                    }

                }
            }

            return username;
        }

        public static string FindStrings(string name)
        {

            while (name == "")
            {
                Print("You didn't type anything. Try again");
                name = Read();
            }

            return name;
        }


        public static void Print(string text)
        {
            Console.WriteLine(text);
        }

        public static string Read()
        {
            return Console.ReadLine();
        }

        public static int StringToInt(string num)
        {
            int val = 0;

            while(! Int32.TryParse(num, out val))
            {
                Print("You didn't type a number");
                num = Read();
            }
            return val;
        }

    }   

    class DB 
    {
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection("Data Source=database.db");
        }

        public static void AddUser(User user)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Users 
                                (username, password, surname, last_name, age, country, balance)
                                VALUES (@username, @password, @surname, @last_name, @age, @country, @balance)";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.user_name);
                    command.Parameters.AddWithValue("@password", user.password);
                    command.Parameters.AddWithValue("@surname", user.surname);
                    command.Parameters.AddWithValue("@last_name", user.last_name);
                    command.Parameters.AddWithValue("@age", user.age);
                    command.Parameters.AddWithValue("@country", user.country);
                    command.Parameters.AddWithValue("@balance", user.balance);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Users";

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User(
                                reader.GetInt32(0),  // id
                                reader.GetString(1), // username
                                reader.GetString(2), // password
                                reader.GetString(3), // surname
                                reader.GetString(4), // last_name
                                reader.GetInt32(5),  // age
                                reader.GetString(6)  // country
                            )
                            {
                                balance = reader.GetInt32(7) // balance
                            });
                        }
                    }
                }
            }
            return users;
        }

        public static void AddProduct(Product product)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Products 
                                (name, country, price, amount, user_id) 
                                VALUES (@name, @country, @price, @amount, @user_id)";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", product.name);
                    command.Parameters.AddWithValue("@country", product.country);
                    command.Parameters.AddWithValue("@price", product.price);
                    command.Parameters.AddWithValue("@amount", product.amount);
                    command.Parameters.AddWithValue("@user_id", product.user_id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Products";

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product(
                                reader.GetInt32(0),  // id
                                reader.GetString(1), // name
                                reader.GetString(2), // country
                                reader.GetInt32(3),  // price
                                reader.GetInt32(4),  // amount
                                reader.GetInt32(5)   // user_id
                            ));
                        }
                    }
                }
            }
            return products;
        }


        public static void InitializeDatabase()
        {
            // Check if the database file exists
            string dbFile = "database.db";
            if (!File.Exists(dbFile))
            {
                // Create the database file
                File.Create(dbFile).Close();
            }

            using (var connection = GetConnection())
            {
                connection.Open();

                // SQL to create Users table
                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT UNIQUE NOT NULL,
                        password TEXT NOT NULL,
                        surname TEXT NOT NULL,
                        last_name TEXT NOT NULL,
                        age INTEGER NOT NULL,
                        country TEXT NOT NULL,
                        balance INTEGER DEFAULT 0
                    );";

                // SQL to create Products table
                string createProductsTable = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        country TEXT NOT NULL,
                        price INTEGER NOT NULL,
                        amount INTEGER NOT NULL,
                        user_id INTEGER NOT NULL,
                        FOREIGN KEY (user_id) REFERENCES Users (id)
                    );";

                // Execute the SQL commands
                using (var command = new SqliteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqliteCommand(createProductsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }



    } 
}
