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
            MigrateDatabase();
        }

        private void MigrateDatabase()
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            // Check if IsVoided column exists, if not add it
            string checkColumn = "PRAGMA table_info(Variations)";
            using var checkCommand = new SQLiteCommand(checkColumn, connection);
            using var reader = checkCommand.ExecuteReader();

            bool hasIsVoided = false;
            while (reader.Read())
            {
                string columnName = reader.GetString(1);
                if (columnName == "IsVoided")
                {
                    hasIsVoided = true;
                    break;
                }
            }
            reader.Close();

            // Add IsVoided column if it doesn't exist
            if (!hasIsVoided)
            {
                string addColumn = "ALTER TABLE Variations ADD COLUMN IsVoided INTEGER DEFAULT 0";
                using var alterCommand = new SQLiteCommand(addColumn, connection);
                alterCommand.ExecuteNonQuery();
            }
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
                            IsApproved, IsVoided, ApprovedBy, ApprovedDate, PurchaseOrder, JobNumber, TotalValue, DisplayOrder
                            FROM Variations ORDER BY DisplayOrder, Id";

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
                    IsVoided = reader.GetInt32(6) == 1,
                    ApprovedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ApprovedDate = reader.IsDBNull(8) ? null : reader.GetString(8),
                    PurchaseOrder = reader.IsDBNull(9) ? null : reader.GetString(9),
                    JobNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                    TotalValue = reader.GetDecimal(11),
                    DisplayOrder = reader.IsDBNull(12) ? 0 : reader.GetInt32(12)
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
                    if (variation.IsVoided)
                        summary.VoidedAdditions += variation.TotalValue;
                }
                else if (variation.TotalValue < 0)
                {
                    summary.TotalCredits += variation.TotalValue;
                    if (variation.IsApproved)
                        summary.ApprovedCredits += variation.TotalValue;
                    if (variation.IsVoided)
                        summary.VoidedCredits += variation.TotalValue;
                }
            }

            return summary;
        }

        public Variation? GetVariation(int variationId)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = @"SELECT Id, VariationNumber, VariationName, VariationDate, ClientContact,
                            IsApproved, IsVoided, ApprovedBy, ApprovedDate, PurchaseOrder, JobNumber, TotalValue, DisplayOrder
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
                    IsVoided = reader.GetInt32(6) == 1,
                    ApprovedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ApprovedDate = reader.IsDBNull(8) ? null : reader.GetString(8),
                    PurchaseOrder = reader.IsDBNull(9) ? null : reader.GetString(9),
                    JobNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                    TotalValue = reader.GetDecimal(11),
                    DisplayOrder = reader.IsDBNull(12) ? 0 : reader.GetInt32(12)
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
                    // Get the maximum DisplayOrder and add 1 for new variation (ensures it goes to the bottom)
                    string getMaxOrder = "SELECT COALESCE(MAX(DisplayOrder), -1) FROM Variations";
                    using (var maxCommand = new SQLiteCommand(getMaxOrder, connection))
                    {
                        int maxDisplayOrder = Convert.ToInt32(maxCommand.ExecuteScalar());
                        variation.DisplayOrder = maxDisplayOrder + 1;
                    }

                    // Insert new variation
                    string insertVariation = @"INSERT INTO Variations
                        (VariationNumber, VariationName, VariationDate, ClientContact, IsApproved, IsVoided, ApprovedBy, ApprovedDate, PurchaseOrder, JobNumber, TotalValue, DisplayOrder)
                        VALUES (@number, @name, @date, @contact, @approved, @voided, @approvedBy, @approvedDate, @purchaseOrder, @jobNumber, @totalValue, @displayOrder)";

                    using var command = new SQLiteCommand(insertVariation, connection);
                    command.Parameters.AddWithValue("@number", variation.VariationNumber);
                    command.Parameters.AddWithValue("@name", variation.VariationName);
                    command.Parameters.AddWithValue("@date", variation.VariationDate);
                    command.Parameters.AddWithValue("@contact", (object?)variation.ClientContact ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approved", variation.IsApproved ? 1 : 0);
                    command.Parameters.AddWithValue("@voided", variation.IsVoided ? 1 : 0);
                    command.Parameters.AddWithValue("@approvedBy", (object?)variation.ApprovedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approvedDate", (object?)variation.ApprovedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@purchaseOrder", (object?)variation.PurchaseOrder ?? DBNull.Value);
                    command.Parameters.AddWithValue("@jobNumber", (object?)variation.JobNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@totalValue", variation.TotalValue);
                    command.Parameters.AddWithValue("@displayOrder", variation.DisplayOrder);
                    command.ExecuteNonQuery();

                    variationId = (int)connection.LastInsertRowId;
                }
                else
                {
                    // Update existing variation
                    string updateVariation = @"UPDATE Variations SET
                        VariationNumber = @number, VariationName = @name, VariationDate = @date,
                        ClientContact = @contact, IsApproved = @approved, IsVoided = @voided, ApprovedBy = @approvedBy,
                        ApprovedDate = @approvedDate, PurchaseOrder = @purchaseOrder, JobNumber = @jobNumber, TotalValue = @totalValue, DisplayOrder = @displayOrder
                        WHERE Id = @id";

                    using var command = new SQLiteCommand(updateVariation, connection);
                    command.Parameters.AddWithValue("@id", variation.Id);
                    command.Parameters.AddWithValue("@number", variation.VariationNumber);
                    command.Parameters.AddWithValue("@name", variation.VariationName);
                    command.Parameters.AddWithValue("@date", variation.VariationDate);
                    command.Parameters.AddWithValue("@contact", (object?)variation.ClientContact ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approved", variation.IsApproved ? 1 : 0);
                    command.Parameters.AddWithValue("@voided", variation.IsVoided ? 1 : 0);
                    command.Parameters.AddWithValue("@approvedBy", (object?)variation.ApprovedBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@approvedDate", (object?)variation.ApprovedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@purchaseOrder", (object?)variation.PurchaseOrder ?? DBNull.Value);
                    command.Parameters.AddWithValue("@jobNumber", (object?)variation.JobNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@totalValue", variation.TotalValue);
                    command.Parameters.AddWithValue("@displayOrder", variation.DisplayOrder);
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
                IsApproved = 1, IsVoided = 0, ApprovedBy = @approvedBy, ApprovedDate = @approvedDate
                WHERE Id = @id";

            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@id", variationId);
            command.Parameters.AddWithValue("@approvedBy", approvedBy);
            command.Parameters.AddWithValue("@approvedDate", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            command.ExecuteNonQuery();
        }

        public void UnapproveVariation(int variationId)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = @"UPDATE Variations SET
                IsApproved = 0, IsVoided = 0, ApprovedBy = NULL, ApprovedDate = NULL, PurchaseOrder = NULL, JobNumber = NULL
                WHERE Id = @id";

            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@id", variationId);
            command.ExecuteNonQuery();
        }

        public void VoidVariation(int variationId, string voidReason)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = @"UPDATE Variations SET
                IsVoided = 1, IsApproved = 0, ApprovedBy = @voidReason, ApprovedDate = @voidDate, PurchaseOrder = NULL, JobNumber = NULL
                WHERE Id = @id";

            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@id", variationId);
            command.Parameters.AddWithValue("@voidReason", voidReason);
            command.Parameters.AddWithValue("@voidDate", DateTime.Now.ToString("dd-MM-yyyy"));
            command.ExecuteNonQuery();
        }

        public bool VariationNumberExists(string variationNumber, int? excludeId = null)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string query = excludeId.HasValue
                ? "SELECT COUNT(*) FROM Variations WHERE TRIM(VariationNumber) = TRIM(@number) COLLATE NOCASE AND Id != @excludeId"
                : "SELECT COUNT(*) FROM Variations WHERE TRIM(VariationNumber) = TRIM(@number) COLLATE NOCASE";

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

        public int DuplicateVariation(int variationId)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Get original variation
                var original = GetVariation(variationId);
                if (original == null)
                    throw new Exception("Variation not found");

                // Generate new variation number
                string newVariationNumber = original.VariationNumber + "_1";
                int suffix = 1;
                while (VariationNumberExists(newVariationNumber))
                {
                    suffix++;
                    newVariationNumber = original.VariationNumber + "_" + suffix;
                }

                // Insert duplicate variation
                string insertVariation = @"INSERT INTO Variations
                    (VariationNumber, VariationName, VariationDate, ClientContact, IsApproved, IsVoided, ApprovedBy, ApprovedDate, PurchaseOrder, JobNumber, TotalValue, DisplayOrder)
                    VALUES (@number, @name, @date, @contact, 0, 0, NULL, NULL, NULL, NULL, @totalValue, @displayOrder)";

                using var command = new SQLiteCommand(insertVariation, connection);
                command.Parameters.AddWithValue("@number", newVariationNumber);
                command.Parameters.AddWithValue("@name", original.VariationName);
                command.Parameters.AddWithValue("@date", DateTime.Now.ToString("dd-MM-yyyy"));
                command.Parameters.AddWithValue("@contact", (object?)original.ClientContact ?? DBNull.Value);
                command.Parameters.AddWithValue("@totalValue", original.TotalValue);
                command.Parameters.AddWithValue("@displayOrder", original.DisplayOrder + 1);
                command.ExecuteNonQuery();

                int newVariationId = (int)connection.LastInsertRowId;

                // Copy line items
                var lineItems = GetLineItems(variationId);
                foreach (var lineItem in lineItems)
                {
                    string insertLineItem = @"INSERT INTO LineItems
                        (VariationId, ItemNumber, ItemDescription, ItemType, MaterialQty, MaterialCost,
                        MaterialTotal, HourlyQty, HourlyRate, LabourTotal, LineTotal)
                        VALUES (@variationId, @itemNumber, @description, @type, @matQty, @matCost,
                        @matTotal, @hourQty, @hourRate, @labourTotal, @lineTotal)";

                    using var lineCommand = new SQLiteCommand(insertLineItem, connection);
                    lineCommand.Parameters.AddWithValue("@variationId", newVariationId);
                    lineCommand.Parameters.AddWithValue("@itemNumber", lineItem.ItemNumber);
                    lineCommand.Parameters.AddWithValue("@description", lineItem.ItemDescription);
                    lineCommand.Parameters.AddWithValue("@type", lineItem.ItemType);
                    lineCommand.Parameters.AddWithValue("@matQty", lineItem.MaterialQty);
                    lineCommand.Parameters.AddWithValue("@matCost", lineItem.MaterialCost);
                    lineCommand.Parameters.AddWithValue("@matTotal", lineItem.MaterialTotal);
                    lineCommand.Parameters.AddWithValue("@hourQty", lineItem.HourlyQty);
                    lineCommand.Parameters.AddWithValue("@hourRate", lineItem.HourlyRate);
                    lineCommand.Parameters.AddWithValue("@labourTotal", lineItem.LabourTotal);
                    lineCommand.Parameters.AddWithValue("@lineTotal", lineItem.LineTotal);
                    lineCommand.ExecuteNonQuery();
                }

                transaction.Commit();
                return newVariationId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateDisplayOrder(int variationId, int newDisplayOrder)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = "UPDATE Variations SET DisplayOrder = @displayOrder WHERE Id = @id";
            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@displayOrder", newDisplayOrder);
            command.Parameters.AddWithValue("@id", variationId);
            command.ExecuteNonQuery();
        }

        public void UpdatePurchaseOrder(int variationId, string? purchaseOrder)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = "UPDATE Variations SET PurchaseOrder = @purchaseOrder WHERE Id = @id";
            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@purchaseOrder", (object?)purchaseOrder ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", variationId);
            command.ExecuteNonQuery();
        }

        public void UpdateJobNumber(int variationId, string? jobNumber)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = "UPDATE Variations SET JobNumber = @jobNumber WHERE Id = @id";
            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@jobNumber", (object?)jobNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", variationId);
            command.ExecuteNonQuery();
        }

        public void UpdateApprovedBy(int variationId, string approvedBy)
        {
            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string update = "UPDATE Variations SET ApprovedBy = @approvedBy WHERE Id = @id";
            using var command = new SQLiteCommand(update, connection);
            command.Parameters.AddWithValue("@approvedBy", approvedBy);
            command.Parameters.AddWithValue("@id", variationId);
            command.ExecuteNonQuery();
        }
    }
}
