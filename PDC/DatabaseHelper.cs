using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace PDC
{
    public class DatabaseHelper
    {
        public static void CreateClientsDatabase(string dbPath)
        {
            if (File.Exists(dbPath))
                return;

            SQLiteConnection.CreateFile(dbPath);

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            string createClientsTable = @"
                CREATE TABLE IF NOT EXISTS Clients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientName TEXT NOT NULL UNIQUE
                );";

            string createContactsTable = @"
                CREATE TABLE IF NOT EXISTS ClientContacts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientName TEXT NOT NULL,
                    ContactName TEXT NOT NULL,
                    FOREIGN KEY(ClientName) REFERENCES Clients(ClientName)
                );";

            using (var command = new SQLiteCommand(createClientsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createContactsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            // Seed with some default clients
            SeedDefaultClients(connection);
        }

        private static void SeedDefaultClients(SQLiteConnection connection)
        {
            var defaultClients = new Dictionary<string, List<string>>
            {
                { "Client A", new List<string> { "John Smith", "Jane Doe" } },
                { "Client B", new List<string> { "Bob Johnson", "Alice Williams" } },
                { "Client C", new List<string> { "Charlie Brown", "Diana Prince" } }
            };

            foreach (var client in defaultClients)
            {
                string insertClient = "INSERT OR IGNORE INTO Clients (ClientName) VALUES (@clientName)";
                using (var command = new SQLiteCommand(insertClient, connection))
                {
                    command.Parameters.AddWithValue("@clientName", client.Key);
                    command.ExecuteNonQuery();
                }

                foreach (var contact in client.Value)
                {
                    string insertContact = "INSERT INTO ClientContacts (ClientName, ContactName) VALUES (@clientName, @contactName)";
                    using (var command = new SQLiteCommand(insertContact, connection))
                    {
                        command.Parameters.AddWithValue("@clientName", client.Key);
                        command.Parameters.AddWithValue("@contactName", contact);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static List<string> GetClients(string dbPath)
        {
            var clients = new List<string>();

            if (!File.Exists(dbPath))
                return clients;

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            string query = "SELECT ClientName FROM Clients ORDER BY ClientName";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                clients.Add(reader.GetString(0));
            }

            return clients;
        }

        public static List<string> GetClientContacts(string dbPath, string clientName)
        {
            var contacts = new List<string>();

            if (!File.Exists(dbPath))
                return contacts;

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            string query = "SELECT ContactName FROM ClientContacts WHERE ClientName = @clientName ORDER BY ContactName";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@clientName", clientName);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                contacts.Add(reader.GetString(0));
            }

            return contacts;
        }

        public static void CreateProjectDatabase(string dbPath, string projectName, string projectNumber, string clientName)
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            SQLiteConnection.CreateFile(dbPath);

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            string createProjectTable = @"
                CREATE TABLE IF NOT EXISTS Project (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectName TEXT NOT NULL,
                    ProjectNumber TEXT NOT NULL,
                    ClientName TEXT NOT NULL
                );";

            string createVariationsTable = @"
                CREATE TABLE IF NOT EXISTS Variations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VariationNumber TEXT NOT NULL UNIQUE,
                    VariationName TEXT NOT NULL,
                    VariationDate TEXT NOT NULL,
                    ClientContact TEXT,
                    IsApproved INTEGER DEFAULT 0,
                    ApprovedBy TEXT,
                    ApprovedDate TEXT,
                    PurchaseOrder TEXT,
                    JobNumber TEXT,
                    TotalValue REAL DEFAULT 0,
                    DisplayOrder INTEGER DEFAULT 0
                );";

            string createLineItemsTable = @"
                CREATE TABLE IF NOT EXISTS LineItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VariationId INTEGER NOT NULL,
                    ItemNumber INTEGER NOT NULL,
                    ItemDescription TEXT,
                    ItemType TEXT NOT NULL,
                    MaterialQty REAL,
                    MaterialCost REAL,
                    MaterialTotal REAL,
                    HourlyQty REAL,
                    HourlyRate REAL,
                    LabourTotal REAL,
                    LineTotal REAL,
                    FOREIGN KEY(VariationId) REFERENCES Variations(Id)
                );";

            string createHourlyRatesTable = @"
                CREATE TABLE IF NOT EXISTS HourlyRates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RateName TEXT NOT NULL UNIQUE,
                    RateValue REAL NOT NULL
                );";

            using (var command = new SQLiteCommand(createProjectTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createVariationsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createLineItemsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createHourlyRatesTable, connection))
            {
                command.ExecuteNonQuery();
            }

            // Insert project info
            string insertProject = "INSERT INTO Project (ProjectName, ProjectNumber, ClientName) VALUES (@name, @number, @client)";
            using (var command = new SQLiteCommand(insertProject, connection))
            {
                command.Parameters.AddWithValue("@name", projectName);
                command.Parameters.AddWithValue("@number", projectNumber);
                command.Parameters.AddWithValue("@client", clientName);
                command.ExecuteNonQuery();
            }

            // Seed default hourly rates
            SeedHourlyRates(connection);
        }

        private static void SeedHourlyRates(SQLiteConnection connection)
        {
            var defaultRates = new Dictionary<string, double>
            {
                { "Standard", 100.0 },
                { "Senior", 150.0 },
                { "Specialist", 200.0 }
            };

            foreach (var rate in defaultRates)
            {
                string insertRate = "INSERT INTO HourlyRates (RateName, RateValue) VALUES (@name, @value)";
                using var command = new SQLiteCommand(insertRate, connection);
                command.Parameters.AddWithValue("@name", rate.Key);
                command.Parameters.AddWithValue("@value", rate.Value);
                command.ExecuteNonQuery();
            }
        }
    }
}
