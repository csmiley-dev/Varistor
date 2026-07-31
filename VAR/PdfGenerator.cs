using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VAR
{
    public class PdfGenerator
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly string _outputFolder;

        public PdfGenerator(DatabaseHelper dbHelper, string outputFolder)
        {
            _dbHelper = dbHelper;
            _outputFolder = outputFolder;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string GenerateSummaryPdf()
        {
            var projectInfo = _dbHelper.GetProjectInfo();
            var variations = _dbHelper.GetAllVariations();
            var summary = _dbHelper.GetVariationSummary();

            string fileName = $"VariationSummary_{projectInfo.ProjectNumber}_{DateTime.Now:ddMMyyyy_HHmmss}.pdf";
            string filePath = Path.Combine(_outputFolder, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);

                    page.Content().Element(container => ComposeSummaryContent(container, projectInfo, variations, summary));

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);

            return filePath;
        }

        public string GenerateVariationPdf(int variationId)
        {
            var projectInfo = _dbHelper.GetProjectInfo();
            var variation = _dbHelper.GetVariation(variationId);
            var lineItems = _dbHelper.GetLineItems(variationId);

            if (variation == null)
                throw new Exception("Variation not found");

            // Filter out empty rows
            lineItems = lineItems.Where(item =>
                !string.IsNullOrWhiteSpace(item.ItemDescription) ||
                item.MaterialQty != 0 ||
                item.MaterialCost != 0 ||
                item.HourlyQty != 0 ||
                item.HourlyRate != 0
            ).ToList();

            string fileName = $"{variation.VariationNumber.Replace("#", "")}_{variation.VariationName}_{DateTime.Now:ddMMyyyy_HHmmss}.pdf";
            string filePath = Path.Combine(_outputFolder, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);

                    page.Content().Element(container => ComposeVariationContent(container, projectInfo, variation, lineItems));

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);

            return filePath;
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                // LJ Services Logo
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LJ Logo base transparent.png");

                row.RelativeItem().Column(column =>
                {
                    if (File.Exists(logoPath))
                    {
                        column.Item().Height(50).Image(logoPath);
                    }
                    else
                    {
                        // Fallback to text if logo not found
                        column.Item().Text("LJ SERVICES").FontSize(24).Bold().FontColor(Colors.Orange.Darken2);
                        column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Orange.Darken2);
                    }
                });
            });
        }

        private void ComposeSummaryContent(IContainer container, ProjectInfo projectInfo, List<Variation> variations, VariationSummary summary)
        {
            container.Column(column =>
            {
                // Project Info
                column.Item().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text($"Project: {projectInfo.ProjectName} {projectInfo.ProjectNumber}").FontSize(14).Bold();
                    col.Item().Text($"Client: {projectInfo.ClientName}").FontSize(12);
                });

                // Variations Table
                column.Item().PaddingTop(10).Text("Variations Summary").FontSize(14).Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);  // Variation #
                        columns.RelativeColumn(3);    // Name
                        columns.ConstantColumn(60);  // Date
                        columns.ConstantColumn(45);  // Type
                        columns.ConstantColumn(65);  // Total Value
                        columns.ConstantColumn(75); // Approved By
                        columns.ConstantColumn(60);  // PO
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Variation #").Bold();
                        header.Cell().Element(CellStyle).Text("Name").Bold();
                        header.Cell().Element(CellStyle).Text("Date").Bold();
                        header.Cell().Element(CellStyle).Text("Type").Bold();
                        header.Cell().Element(CellStyle).Text("Total Value").Bold();
                        header.Cell().Element(CellStyle).Text("Approved By").Bold();
                        header.Cell().Element(CellStyle).Text("PO").Bold();
                    });

                    // Rows
                    foreach (var variation in variations)
                    {
                        var bgColor = variation.IsApproved ? Colors.Green.Lighten4 : Colors.White;

                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).Text(variation.VariationNumber);
                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).Text(variation.VariationName);
                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).Text(variation.VariationDate);
                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).Text(variation.VariationType);
                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).AlignRight().Text($"${variation.TotalValue:N2}");
                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).Text(variation.ApprovedBy ?? "");
                        table.Cell().Element(container => CellStyleWithBg(container, bgColor)).Text(variation.PurchaseOrder ?? "");
                    }
                });

                // Summary Totals
                column.Item().PaddingTop(20).Column(col =>
                {
                    col.Item().Text("All Variations Summary").FontSize(12).Bold();
                    col.Item().PaddingLeft(10).Text($"Total Additions: ${summary.TotalAdditions:N2}");
                    col.Item().PaddingLeft(10).Text($"Total Credits: ${summary.TotalCredits:N2}");
                    col.Item().PaddingLeft(10).Text($"Net Value: ${summary.NetValue:N2}").Bold();

                    col.Item().PaddingTop(10).Text("Approved Variations Summary").FontSize(12).Bold();
                    col.Item().PaddingLeft(10).Text($"Approved Additions: ${summary.ApprovedAdditions:N2}");
                    col.Item().PaddingLeft(10).Text($"Approved Credits: ${summary.ApprovedCredits:N2}");
                    col.Item().PaddingLeft(10).Text($"Approved Net Value: ${summary.ApprovedNetValue:N2}").Bold();
                });
            });
        }

        private void ComposeVariationContent(IContainer container, ProjectInfo projectInfo, Variation variation, List<LineItem> lineItems)
        {
            container.Column(column =>
            {
                // Project and Variation Info
                column.Item().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text($"Project: {projectInfo.ProjectName} {projectInfo.ProjectNumber}").FontSize(14).Bold();
                    col.Item().Text($"Client: {projectInfo.ClientName}").FontSize(12);
                    if (!string.IsNullOrEmpty(variation.ClientContact))
                        col.Item().Text($"Contact: {variation.ClientContact}").FontSize(11);
                    col.Item().PaddingTop(5).Text($"Variation: {variation.VariationNumber} - {variation.VariationName}").FontSize(13).Bold();
                    col.Item().Text($"Date: {variation.VariationDate}").FontSize(11);
                });

                // Line Items Table
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);   // Item #
                        columns.RelativeColumn(3);     // Description
                        columns.ConstantColumn(35);   // Type
                        columns.ConstantColumn(40);   // Mat Qty
                        columns.ConstantColumn(50);   // Mat Cost
                        columns.ConstantColumn(55);   // Mat Total
                        columns.ConstantColumn(40);   // Hour Qty
                        columns.ConstantColumn(50);   // Hour Rate
                        columns.ConstantColumn(55);   // Labour Total
                        columns.ConstantColumn(60);   // Line Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#").Bold();
                        header.Cell().Element(CellStyle).Text("Description").Bold();
                        header.Cell().Element(CellStyle).Text("Type").Bold();
                        header.Cell().Element(CellStyle).Text("Mat Qty").Bold();
                        header.Cell().Element(CellStyle).Text("Mat Cost").Bold();
                        header.Cell().Element(CellStyle).Text("Mat Total").Bold();
                        header.Cell().Element(CellStyle).Text("Hour Qty").Bold();
                        header.Cell().Element(CellStyle).Text("Hour Rate").Bold();
                        header.Cell().Element(CellStyle).Text("Labour Total").Bold();
                        header.Cell().Element(CellStyle).Text("Line Total").Bold();
                    });

                    // Rows
                    foreach (var item in lineItems)
                    {
                        table.Cell().Element(CellStyle).Text(item.ItemNumber.ToString());
                        table.Cell().Element(CellStyle).Text(item.ItemDescription);
                        table.Cell().Element(CellStyle).Text(item.ItemType);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.MaterialQty > 0 ? item.MaterialQty.ToString("N2") : "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.MaterialCost > 0 ? $"${item.MaterialCost:N2}" : "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.MaterialTotal != 0 ? $"${item.MaterialTotal:N2}" : "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.HourlyQty > 0 ? item.HourlyQty.ToString("N2") : "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.HourlyRate > 0 ? $"${item.HourlyRate:N2}" : "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.LabourTotal != 0 ? $"${item.LabourTotal:N2}" : "");
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.LineTotal:N2}").Bold();
                    }
                });

                // Totals
                decimal materialTotal = lineItems.Sum(i => i.MaterialTotal);
                decimal labourTotal = lineItems.Sum(i => i.LabourTotal);
                decimal grandTotal = lineItems.Sum(i => i.LineTotal);

                column.Item().PaddingTop(15).AlignRight().Column(col =>
                {
                    col.Item().Text($"Material Subtotal: ${materialTotal:N2}").FontSize(11);
                    col.Item().Text($"Labour Subtotal: ${labourTotal:N2}").FontSize(11);
                    col.Item().PaddingTop(5).Text($"Grand Total: ${grandTotal:N2}").FontSize(13).Bold();
                });

                // Approval Info
                if (variation.IsApproved)
                {
                    column.Item().PaddingTop(20).Column(col =>
                    {
                        col.Item().Text("Approval Information").FontSize(12).Bold();
                        col.Item().PaddingLeft(10).Text($"Approved By: {variation.ApprovedBy}");
                        col.Item().PaddingLeft(10).Text($"Approved Date: {variation.ApprovedDate}");
                        if (!string.IsNullOrEmpty(variation.PurchaseOrder))
                            col.Item().PaddingLeft(10).Text($"Purchase Order: {variation.PurchaseOrder}");
                    });
                }
            });
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(3);
        }

        private static IContainer CellStyleWithBg(IContainer container, string backgroundColor)
        {
            return container.Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(3);
        }
    }
}
