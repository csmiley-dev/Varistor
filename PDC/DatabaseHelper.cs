using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace PDC
{
    public class ClientConfig
    {
        public string Name { get; set; } = "";
        public List<string> Contacts { get; set; } = new();
    }

    public class HourlyRateConfig
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
    }

    public class DatabaseHelper
    {
        private static readonly JsonSerializerOptions ConfigJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static void CreateClientsDatabase(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

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

            // Add any clients/contacts from clients.json that aren't in the database yet.
            // Never removes or overwrites existing rows, so it's safe to run on every launch.
            string configPath = Path.Combine(Path.GetDirectoryName(dbPath) ?? "", "clients.json");
            SyncClientsFromConfig(connection, configPath);
        }

        private static void SyncClientsFromConfig(SQLiteConnection connection, string configPath)
        {
            if (!File.Exists(configPath))
                return;

            List<ClientConfig>? clients;
            try
            {
                string json = File.ReadAllText(configPath);
                clients = JsonSerializer.Deserialize<List<ClientConfig>>(json, ConfigJsonOptions);
            }
            catch
            {
                // Malformed JSON shouldn't block the app from starting with whatever is already in the database.
                return;
            }

            if (clients == null)
                return;

            foreach (var client in clients)
            {
                if (string.IsNullOrWhiteSpace(client.Name))
                    continue;

                string insertClient = "INSERT OR IGNORE INTO Clients (ClientName) VALUES (@clientName)";
                using (var command = new SQLiteCommand(insertClient, connection))
                {
                    command.Parameters.AddWithValue("@clientName", client.Name);
                    command.ExecuteNonQuery();
                }

                foreach (var contact in client.Contacts)
                {
                    if (string.IsNullOrWhiteSpace(contact))
                        continue;

                    string checkExists = "SELECT COUNT(*) FROM ClientContacts WHERE ClientName = @clientName AND ContactName = @contactName";
                    using var checkCommand = new SQLiteCommand(checkExists, connection);
                    checkCommand.Parameters.AddWithValue("@clientName", client.Name);
                    checkCommand.Parameters.AddWithValue("@contactName", contact);
                    bool exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                    if (!exists)
                    {
                        string insertContact = "INSERT INTO ClientContacts (ClientName, ContactName) VALUES (@clientName, @contactName)";
                        using var insertCommand = new SQLiteCommand(insertContact, connection);
                        insertCommand.Parameters.AddWithValue("@clientName", client.Name);
                        insertCommand.Parameters.AddWithValue("@contactName", contact);
                        insertCommand.ExecuteNonQuery();
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

        public static void CreateProjectDatabase(string dbPath, string projectName, string projectNumber, string clientName, string configFolder)
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
                    IsVoided INTEGER DEFAULT 0,
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

            // Seed default hourly rates for this project from hourly-rates.json
            SeedHourlyRates(connection, configFolder);
        }

        private static void SeedHourlyRates(SQLiteConnection connection, string configFolder)
        {
            List<HourlyRateConfig>? rates = null;
            string configPath = Path.Combine(configFolder ?? "", "hourly-rates.json");

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    rates = JsonSerializer.Deserialize<List<HourlyRateConfig>>(json, ConfigJsonOptions);
                }
                catch
                {
                    rates = null;
                }
            }

            // Fall back to built-in defaults if hourly-rates.json is missing or invalid,
            // so a new project always gets usable rate options.
            rates ??= new List<HourlyRateConfig>
            {
                new() { Name = "Standard", Value = 100.0 },
                new() { Name = "Senior", Value = 150.0 },
                new() { Name = "Specialist", Value = 200.0 }
            };

            foreach (var rate in rates)
            {
                if (string.IsNullOrWhiteSpace(rate.Name))
                    continue;

                string insertRate = "INSERT INTO HourlyRates (RateName, RateValue) VALUES (@name, @value)";
                using var command = new SQLiteCommand(insertRate, connection);
                command.Parameters.AddWithValue("@name", rate.Name);
                command.Parameters.AddWithValue("@value", rate.Value);
                command.ExecuteNonQuery();
            }
        }
    }
}
