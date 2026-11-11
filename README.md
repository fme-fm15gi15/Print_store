# Print_store

.NET 8 Minimal API — 超商店到店標籤列印服務（QR Code + PDF）

## 功能

- 產生超商店到店標籤 PDF（10×15cm）
- 標籤包含：QR Code、訂單編號、取件門市、寄件人、收件人

## 快速啟動

```bash
cd Print_store
dotnet run
# 開啟 http://localhost:5000
```

## API

### GET /label/{orderNo}
快速預覽（使用預設資料）

```
GET http://localhost:5000/label/EC2024111100001
```

### POST /label
完整資料產生標籤

```json
POST http://localhost:5000/label
Content-Type: application/json

{
  "orderNo": "EC2024111100001",
  "sender": "王小明",
  "receiver": "李大華",
  "receiverPhone": "0912-345-678",
  "storeCode": "006358",
  "storeName": "7-ELEVEN 信義門市"
}
```

回傳 `application/pdf`，可直接用瀏覽器開啟列印。

## 前端觸發列印

```javascript
async function printLabel(orderData) {
    const resp = await fetch('http://localhost:5000/label', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(orderData)
    });
    const blob = await resp.blob();
    const url = URL.createObjectURL(blob);
    const win = window.open(url);
    win.addEventListener('load', () => win.print());
}
```

## 套件

- [QuestPDF](https://www.questpdf.com/) — PDF 排版
- [ZXing.Net](https://github.com/micjahn/ZXing.Net) — QR Code 產生
