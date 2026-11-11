using Print_store.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LabelService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();

// GET /label/{orderNo} - 快速測試用（帶預設值）
app.MapGet("/label/{orderNo}", (string orderNo, LabelService svc) =>
{
    var pdf = svc.GenerateLabel(new LabelRequest
    {
        OrderNo = orderNo,
        Sender = "測試寄件人",
        Receiver = "測試收件人",
        ReceiverPhone = "0912-345-678",
        StoreCode = "006358",
        StoreName = "7-ELEVEN 信義門市"
    });
    return Results.File(pdf, "application/pdf", $"label_{orderNo}.pdf");
});

// POST /label - 完整資料
app.MapPost("/label", (LabelRequest req, LabelService svc) =>
{
    if (string.IsNullOrWhiteSpace(req.OrderNo))
        return Results.BadRequest(new { message = "OrderNo 必填" });

    var pdf = svc.GenerateLabel(req);
    return Results.File(pdf, "application/pdf", $"label_{req.OrderNo}.pdf");
});

// 健康檢查
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Print_store Label Service" }));

app.Run();
