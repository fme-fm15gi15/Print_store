using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QRCoder;

namespace Print_store.Services;

public class LabelRequest
{
    public string OrderNo { get; set; } = "";
    public string Sender { get; set; } = "";
    public string Receiver { get; set; } = "";
    public string ReceiverPhone { get; set; } = "";
    public string StoreCode { get; set; } = "";
    public string StoreName { get; set; } = "";
}

public class LabelService
{
    public byte[] GenerateLabel(LabelRequest req)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var qrBytes = GenerateQrCode(req.OrderNo);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(100, 150, Unit.Millimetre);
                page.Margin(5, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Lato", "Noto Sans TC"));

                page.Content().Column(col =>
                {
                    // 標題列
                    col.Item()
                        .Background("#1a1f2e")
                        .Padding(4, Unit.Millimetre)
                        .Text("超商店到店")
                        .FontSize(16).Bold().FontColor("#ffffff").AlignCenter();

                    col.Item().Height(3, Unit.Millimetre);

                    // QR Code
                    col.Item().AlignCenter().Width(45, Unit.Millimetre).Image(qrBytes);

                    // 訂單編號
                    col.Item()
                        .Text(req.OrderNo)
                        .FontSize(13).Bold().AlignCenter();

                    col.Item().Height(3, Unit.Millimetre);

                    // 取件門市
                    col.Item()
                        .Border(1).BorderColor("#cccccc")
                        .Padding(2, Unit.Millimetre)
                        .Column(c =>
                        {
                            c.Item().Text("取件門市").FontSize(8).FontColor("#888888");
                            c.Item().Text($"{req.StoreCode}  {req.StoreName}").Bold();
                        });

                    col.Item().Height(2, Unit.Millimetre);

                    // 寄件人 / 收件人
                    col.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Border(1).BorderColor("#cccccc")
                            .Padding(2, Unit.Millimetre)
                            .Column(c =>
                            {
                                c.Item().Text("寄件人").FontSize(8).FontColor("#888888");
                                c.Item().Text(req.Sender).Bold();
                            });

                        row.ConstantItem(2, Unit.Millimetre);

                        row.RelativeItem()
                            .Border(1).BorderColor("#cccccc")
                            .Padding(2, Unit.Millimetre)
                            .Column(c =>
                            {
                                c.Item().Text("收件人").FontSize(8).FontColor("#888888");
                                c.Item().Text(req.Receiver).Bold();
                                c.Item().Text(req.ReceiverPhone).FontSize(9).FontColor("#555555");
                            });
                    });

                    col.Item().Height(3, Unit.Millimetre);

                    // 底部注意事項
                    col.Item()
                        .Background("#f5f5f5")
                        .Padding(2, Unit.Millimetre)
                        .Text("請於到貨通知後 7 天內取件，逾期退回。")
                        .FontSize(8).FontColor("#888888").AlignCenter();
                });
            });
        }).GeneratePdf();
    }

    private static byte[] GenerateQrCode(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(10);
    }
}
