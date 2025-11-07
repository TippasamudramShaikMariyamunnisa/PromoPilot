using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Infrastructure.Formatters
{
    public class CampaignPdfGenerator
    {
        public byte[] Generate(IEnumerable<CampaignDto> campaigns)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Campaign Report").FontSize(20).Bold().AlignCenter();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // ID
                            columns.RelativeColumn(2); // Name
                            columns.RelativeColumn(2); // Start Date
                            columns.RelativeColumn(2); // End Date
                            columns.RelativeColumn(3); // Target Products
                            columns.RelativeColumn(3); // Store List
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("ID");
                            header.Cell().Element(CellStyle).Text("Name");
                            header.Cell().Element(CellStyle).Text("Start Date");
                            header.Cell().Element(CellStyle).Text("End Date");
                            header.Cell().Element(CellStyle).Text("Target Products");
                            header.Cell().Element(CellStyle).Text("Store List");
                        });

                        foreach (var campaign in campaigns)
                        {
                            table.Cell().Element(CellStyle).Text(campaign.CampaignId.ToString());
                            table.Cell().Element(CellStyle).Text(campaign.Name);
                            table.Cell().Element(CellStyle).Text(campaign.StartDate.ToShortDateString());
                            table.Cell().Element(CellStyle).Text(campaign.EndDate.ToShortDateString());
                            table.Cell().Element(CellStyle).Text(string.Join(", ", campaign.TargetProducts));
                            table.Cell().Element(CellStyle).Text(string.Join(", ", campaign.StoreList));
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}