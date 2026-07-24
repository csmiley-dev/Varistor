using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace VAR
{
    public class DatabaseHelper
    {
        private readonly string _dbPath;

        public DatabaseHelper(string dbPath)
        {
            _dbPath = dbPath;
        }

        public ProjectInfo GetProjectInfo()
        {
            var projectInfo = new ProjectInfo();

            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = "SELECT ProjectName, ProjectNumber, ClientName FROM Project LIMIT 1";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                projectInfo.ProjectName = reader.GetString(0);
                projectInfo.ProjectNumber = reader.GetString(1);
                projectInfo.ClientName = reader.GetString(2);
            }

            return projectInfo;
        }

        public List<Variation> GetAllVariations()
        {
            var variations = new List<Variation>();

            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = @"SELECT Id, VariationNumber, VariationName, VariationDate, ClientContact,
                            IsApproved, ApprovedBy, ApprovedDate, TotalValue
                            FROM Variations ORDER BY Id";

            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                variations.Add(new Variation
                {
                    Id = reader.GetInt32(0),
                    VariationNumber = reader.GetString(1),
                    VariationName = reader.GetString(2),
                    VariationDate = reader.GetString(3),
                    ClientContact = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsApproved = reader.GetInt32(5) == 1,
                    ApprovedBy = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ApprovedDate = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TotalValue = reader.GetDecimal(8)
                });
            }

            return variations;
        }

        public VariationSummary GetVariationSummary()
        {
            var summary = new VariationSummary();
            var variations = GetAllVariations();

            foreach (var variation in variations)
            {
                if (variation.TotalValue > 0)
                {
                    summary.TotalAdditions += variation.TotalValue;
                    if (variation.IsApproved)
                        summary.ApprovedAdditions += variation.TotalValue;
                }
                else if (variation.TotalValue < 0)
                {
                    summary.TotalCredits += variation.TotalValue;
                    if (variation.IsApproved)
                        summary.ApprovedCredits += variation.TotalValue;
                }
            }

            return summary;
        }

        public Variation? GetVariation(int variationId)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = @"SELECT Id, VariationNumber, VariationName, VariationDate, ClientContact,
                            IsApproved, ApprovedBy, ApprovedDate, TotalValue
                            FROM Variations WHERE Id = @id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@id", variationId);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Variation
                {
                    Id = reader.GetInt32(0),
                    VariationNumber = reader.GetString(1),
                    VariationName = reader.GetString(2),
                    VariationDate = reader.GetString(3),
                    ClientContact = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsApproved = reader.GetInt32(5) == 1,
                    ApprovedBy = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ApprovedDate = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TotalValue = reader.GetDecimal(8)
                };
            }

            return null;
        }

        public List<LineItem> GetLineItems(int variationId)
        {
            var lineItems = new List<LineItem>();

            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = @"SELECT Id, VariationId, ItemNumber, ItemDescription, ItemType,
                            MaterialQty, MaterialCost, MaterialTotal, HourlyQty, HourlyRate,
                            LabourTotal, LineTotal
                            FROM LineItems WHERE VariationId = @variationId ORDER BY ItemNumber";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@variationId", variationId);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lineItems.Add(new LineItem
                {
                    Id = reader.GetInt32(0),
                    VariationId = reader.GetInt32(1),
                    ItemNumber = reader.GetInt32(2),
                    ItemDescription = reader.GetString(3),
                    ItemType = reader.GetString(4),
                    MaterialQty = reader.GetDecimal(5),
                    MaterialCost = reader.GetDecimal(6),
                    HourlyQty = reader.GetDecimal(8),
                    HourlyRate = reader.GetDecimal(9)
                });
            }

            return lineItems;
        }

        public int SaveVariation(Variation variation, List<LineItem> lineItems)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int variationId;

                if (variation.Id == 0)
                {
                    // Insert new variation
                    string insertVariation = @"INSERT INTO Variations
                        (VariationNumber, VariationName, VariationDate, ClientContact, IsApproved, ApprovedBy, ApprovedDate, TotalValue)
                        VALUES (@number, @name, @date, @contact, @approved, @approvedBy, @approvedDate, @totalValue)";

                    using var command = new SQLiteCommand(insertVariation, connection);
                    command.Parameters.AddWithValue("@number", variation.VariationNumber);
                    command.Parameters.AddWithValue("@name", variation.VariationName);
                    command.Parameters.AddWithValue("@date", variation.VariationDate);
                    command.Parameters.AddWithValue("@contact", (object?)variation.ClientContact ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approved", variation.IsApproved ? 1 : 0);
                    command.Parameters.AddWithValue("@approvedBy", (object?)variation.ApprovedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approvedDate", (object?)variation.ApprovedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@totalValue", variation.TotalValue);
                    command.ExecuteNonQuery();

                    variationId = (int)connection.LastInsertRowId;
                }
                else
                {
                    // Update existing variation
                    string updateVariation = @"UPDATE Variations SET
                        VariationNumber = @number, VariationName = @name, VariationDate = @date,
                        ClientContact = @contact, IsApproved = @approved, ApprovedBy = @approvedBy,
                        ApprovedDate = @approvedDate, TotalValue = @totalValue
                        WHERE Id = @id";

                    using var command = new SQLiteCommand(updateVariation, connection);
                    command.Parameters.AddWithValue("@id", variation.Id);
                    command.Parameters.AddWithValue("@number", variation.VariationNumber);
                    command.Parameters.AddWithValue("@name", variation.VariationName);
                    command.Parameters.AddWithValue("@date", variation.VariationDate);
                    command.Parameters.AddWithValue("@contact", (object?)variation.ClientContact ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approved", variation.IsApproved ? 1 : 0);
                    command.Parameters.AddWithValue("@approvedBy", (object?)variation.ApprovedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approvedDate", (object?)variation.ApprovedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@totalValue", variation.TotalValue);
                    command.ExecuteNonQuery();

                    variationId = variation.Id;

                    // Delete existing line items
                    string deleteLineItems = "DELETE FROM LineItems WHERE VariationId = @variationId";
                    using var deleteCommand = new SQLiteCommand(deleteLineItems, connection);
                    deleteCommand.Parameters.AddWithValue("@variationId", variationId);
                    deleteCommand.ExecuteNonQuery();
                }

                // Insert line items
                foreach (var lineItem in lineItems)
                {
                    string insertLineItem = @"INSERT INTO LineItems
                        (VariationId, ItemNumber, ItemDescription, ItemType, MaterialQty, MaterialCost,
                        MaterialTotal, HourlyQty, HourlyRate, LabourTotal, LineTotal)
                        VALUES (@variationId, @itemNumber, @description, @type, @matQty, @matCost,
                        @matTotal, @hourQty, @hourRate, @labourTotal, @lineTotal)";

                    using var command = new SQLiteCommand(insertLineItem, connection);
                    command.Parameters.AddWithValue("@variationId", variationId);
                    command.Parameters.AddWithValue("@itemNumber", lineItem.ItemNumber);
                    command.Parameters.AddWithValue("@description", lineItem.ItemDescription);
                    command.Parameters.AddWithValue("@type", lineItem.ItemType);
                    command.Parameters.AddWithValue("@matQty", lineItem.MaterialQty);
                    command.Parameters.AddWithValue("@matCost", lineItem.MaterialCost);
                    command.Parameters.AddWithValue("@matTotal", lineItem.MaterialTotal);
                    command.Parameters.AddWithValue("@hourQty", lineItem.HourlyQty);
                    command.Parameters.AddWithValue("@hourRate", lineItem.HourlyRate);
                    command.Parameters.AddWithValue("@labourTotal", lineItem.LabourTotal);
                    command.Parameters.AddWithValue("@lineTotal", lineItem.LineTotal);
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return variationId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void ApproveVariation(int variationId, string approvedBy)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = @"UPDATE Variations SET
                IsApproved = 1, ApprovedBy = @approvedBy, ApprovedDate = @approvedDate
                WHERE Id = @id";

            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@id", variationId);
            command.Parameters.AddWithValue("@approvedBy", approvedBy);
            command.Parameters.AddWithValue("@approvedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.ExecuteNonQuery();
        }

        public void UnapproveVariation(int variationId)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = @"UPDATE Variations SET
                IsApproved = 0, ApprovedBy = NULL, ApprovedDate = NULL
                WHERE Id = @id";

            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@id", variationId);
            command.ExecuteNonQuery();
        }

        public bool VariationNumberExists(string variationNumber, int? excludeId = null)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = excludeId.HasValue
                ? "SELECT COUNT(*) FROM Variations WHERE VariationNumber = @number AND Id != @excludeId"
                : "SELECT COUNT(*) FROM Variations WHERE VariationNumber = @number";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@number", variationNumber);
            if (excludeId.HasValue)
                command.Parameters.AddWithValue("@excludeId", excludeId.Value);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public bool VariationNameExists(string variationName, int? excludeId = null)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = excludeId.HasValue
                ? "SELECT COUNT(*) FROM Variations WHERE VariationName = @name AND Id != @excludeId"
                : "SELECT COUNT(*) FROM Variations WHERE VariationName = @name";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@name", variationName);
            if (excludeId.HasValue)
                command.Parameters.AddWithValue("@excludeId", excludeId.Value);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public string GetNextVariationNumber()
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = "SELECT COUNT(*) FROM Variations";
            using var command = new SQLiteCommand(query, connection);
            int count = Convert.ToInt32(command.ExecuteScalar());

            return $"VAR#{count + 1}";
        }

        public List<string> GetClientContacts()
        {
            var contacts = new List<string>();

            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = "SELECT ContactName FROM ClientContacts ORDER BY ContactName";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                contacts.Add(reader.GetString(0));
            }

            return contacts;
        }

        public List<HourlyRate> GetHourlyRates()
        {
            var rates = new List<HourlyRate>();

            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = "SELECT Id, RateName, RateValue FROM HourlyRates ORDER BY RateName";
            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                rates.Add(new HourlyRate
                {
                    Id = reader.GetInt32(0),
                    RateName = reader.GetString(1),
                    RateValue = reader.GetDecimal(2)
                });
            }

            return rates;
        }

        public void DeleteVariation(int variationId)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete line items first
                string deleteLineItems = "DELETE FROM LineItems WHERE VariationId = @variationId";
                using (var command = new SQLiteCommand(deleteLineItems, connection))
                {
                    command.Parameters.AddWithValue("@variationId", variationId);
                    command.ExecuteNonQuery();
                }

                // Delete variation
                string deleteVariation = "DELETE FROM Variations WHERE Id = @id";
                using (var command = new SQLiteCommand(deleteVariation, connection))
                {
                    command.Parameters.AddWithValue("@id", variationId);
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
