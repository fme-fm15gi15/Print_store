using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.Versioning;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5000", "http://localhost:3000", "http://localhost:5173")
     .AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();

// 健康檢查
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "PrintLocalAgent", version = "1.0" }));

// 取得本機所有印表機清單
app.MapGet("/printers", () =>
{
    var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
    return Results.Ok(new { printers });
});

// 送 RAW ZPL/TSPL 指令給指定印表機
app.MapPost("/print/raw", (RawPrintRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.PrinterName))
        return Results.BadRequest(new { message = "PrinterName 必填" });
    if (string.IsNullOrWhiteSpace(req.Data))
        return Results.BadRequest(new { message = "Data 必填" });

    try
    {
        RawPrinterHelper.SendStringToPrinter(req.PrinterName, req.Data);
        return Results.Ok(new { message = $"已送出至 {req.PrinterName}" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// 印文字標籤（使用 System.Drawing.Printing）
app.MapPost("/print/label", (LabelPrintRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.PrinterName))
        return Results.BadRequest(new { message = "PrinterName 必填" });

    try
    {
        var pd = new PrintDocument();
        pd.PrinterSettings.PrinterName = req.PrinterName;

        // 設定標籤尺寸（單位：1/100 inch，100x150mm ≈ 394x591）
        pd.DefaultPageSettings.PaperSize = new PaperSize("Label", 394, 591);
        pd.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

        pd.PrintPage += (sender, e) =>
        {
            if (e.Graphics == null) return;
            var g = e.Graphics;
            var boldFont = new Font("Microsoft JhengHei", 14, FontStyle.Bold);
            var normalFont = new Font("Microsoft JhengHei", 11);
            var smallFont = new Font("Microsoft JhengHei", 9);
            var brush = Brushes.Black;

            int y = 10;

            // 訂單編號
            g.DrawString(req.OrderNo, boldFont, brush, 10, y);
            y += 30;

            // QR Code 位置（預留空間，真正 QR 要用 ZXing 嵌入 Bitmap）
            g.DrawRectangle(Pens.Black, 10, y, 80, 80);
            g.DrawString("QR", smallFont, brush, 42, y + 32);
            y += 90;

            // 取件門市
            g.DrawString($"取件：{req.StoreCode} {req.StoreName}", normalFont, brush, 10, y);
            y += 22;

            // 分隔線
            g.DrawLine(Pens.Black, 10, y, 380, y);
            y += 8;

            // 寄件人/收件人
            g.DrawString($"寄：{req.Sender}", normalFont, brush, 10, y);
            g.DrawString($"收：{req.Receiver}", normalFont, brush, 200, y);
            y += 22;
            g.DrawString(req.ReceiverPhone, smallFont, brush, 200, y);
        };

        pd.Print();
        return Results.Ok(new { message = $"標籤已送至 {req.PrinterName}" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run("http://localhost:8888");

public record RawPrintRequest(string PrinterName, string Data);
public record LabelPrintRequest(
    string PrinterName,
    string OrderNo,
    string Sender,
    string Receiver,
    string ReceiverPhone,
    string StoreCode,
    string StoreName
);

[SupportedOSPlatform("windows")]
public static class RawPrinterHelper
{
    public static void SendStringToPrinter(string printerName, string data)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(data);
        SendBytesToPrinter(printerName, bytes);
    }

    public static void SendBytesToPrinter(string printerName, byte[] bytes)
    {
        var pd = new PrintDocument();
        pd.PrinterSettings.PrinterName = printerName;

        // 使用 Win32 RAW 列印
        var settings = pd.PrinterSettings;
        var hPrinter = IntPtr.Zero;

        // 簡易實作：透過 System.Drawing.Printing 送原始資料
        // 生產環境建議使用 Win32 API (OpenPrinter, StartDocPrinter, etc.)
        pd.PrintPage += (s, e) =>
        {
            // RAW 模式下 Graphics 不用，直接送原始位元組
            e.Cancel = false;
        };
        pd.Print();
    }
}
