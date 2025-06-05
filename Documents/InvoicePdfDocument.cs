using ElectronicsStoreAss3.Models.Invoice;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElectronicsStoreAss3.Documents
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly Invoice _invoice;

        public InvoicePdfDocument(Invoice invoice)
        {
            _invoice = invoice;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Row(innerRow =>
                {
                    innerRow.ConstantItem(66).Image("wwwroot/img/logo.png");

                    innerRow.RelativeItem().AlignLeft().Column(column =>
                    {
                        column.Item().AlignLeft().PaddingBottom(35).PaddingLeft(20).Text($"AWE Electronics")
                            .FontSize(50).Bold();

                        column.Item().PaddingBottom(10).Text($"Invoice #{_invoice.InvoiceNumber}")
                            .FontSize(15).SemiBold().FontColor(Colors.Grey.Darken1);

                        column.Item().Text(text =>
                        {
                            text.Span("Order #").SemiBold();
                            text.Span($"{_invoice.OrderId}");
                        });

                        column.Item().Text(text =>
                        {
                            text.Span("Invoice Date: ").SemiBold();
                            text.Span($"{_invoice.InvoiceDate:MMMM dd, yyyy}");
                        });

                        if (_invoice.PaidDate.HasValue)
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("Paid Date: ").SemiBold();
                                text.Span($"{_invoice.PaidDate.Value:MMMM dd, yyyy}");
                            });
                        }
                    });
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(7);

                column.Item().Row(row =>
                {
                    row.RelativeItem().PaddingBottom(30).Component(new InfoBlock(
                        "Billed To", 
                        _invoice.CustomerName, 
                        _invoice.CustomerEmail,
                        _invoice.BillingAddress
                    ));
                    row.ConstantItem(50);
                    row.RelativeItem().PaddingBottom(30).Component(new InfoBlock("From", "AWE Electronics",
                        "John St, Hawthorn VIC 3122, Australia", "support@aweelectronics.com"));
                });

                column.Item().Element(ComposeTable);

                decimal gstAmount = _invoice.TotalAmount * 0.10m;
                decimal finalTotal = _invoice.TotalAmount + gstAmount;

                column.Item().PaddingTop(5).AlignRight().Text($"GST (10%): ${gstAmount:F2}").FontSize(10);
                column.Item().AlignRight().Text($"SUBTOTAL: ${_invoice.TotalAmount:F2}")
                    .FontSize(14);
                column.Item().AlignRight().Text($"TOTAL: ${finalTotal:F2}")
                    .FontSize(20).SemiBold().Underline();

                column.Item().PaddingTop(20).Text("Thank you for shopping with us!")
                    .FontColor(Colors.Grey.Darken1).Italic();
            });
        }

        void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Text("#");
                    header.Cell().Text("Product").Style(headerStyle);
                    header.Cell().AlignRight().Text("Qty").Style(headerStyle);
                    header.Cell().AlignRight().Text("Price").Style(headerStyle);
                    header.Cell().AlignRight().Text("Total").Style(headerStyle);

                    header.Cell().ColumnSpan(5).BorderBottom(1).BorderColor(Colors.Black).PaddingBottom(5);
                });

                int index = 1;
                foreach (var item in _invoice.Order.OrderItems)
                {
                    table.Cell().Element(CellStyle).Text($"{index++}");
                    table.Cell().Element(CellStyle).Text(item.Product.Name);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.UnitPrice:F2}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.Quantity * item.UnitPrice:F2}");
                }

                static IContainer CellStyle(IContainer container) =>
                    container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            });
        }

        private class InfoBlock : IComponent
        {
            private readonly string _title;
            private readonly string _name;
            private readonly string _address;
            private readonly string _email;

            public InfoBlock(string title, string name, string address, string email = "")
            {
                _title = title;
                _name = name;
                _address = address;
                _email = email;
            }

            public void Compose(IContainer container)
            {
                container.Column(col =>
                {
                    col.Spacing(3);
                    col.Item().Text(_title).SemiBold();
                    col.Item().LineHorizontal(1);
                    col.Item().Text(_name);
                    col.Item().Text(_address);
                    if (!string.IsNullOrWhiteSpace(_email))
                        col.Item().Text(_email);
                });
            }
        }
    }
}