using E_Commerce_Orders.Models;
using System.Text.Json;

namespace E_Commerce_Orders.Services
{
    public class ApiService
    {
        private readonly HttpClient _http = new();

        public async Task<List<Order>> GetOrdersAsync()
        {
            var results = new List<Order>();

            // Small list of friendly, professional sample names for nicer UX
            var sampleNames = new[]
            {
                "Alex Morgan",
                "Taylor Reed",
                "Jordan Blake",
                "Riley Carter",
                "Casey Morgan",
                "Avery Quinn",
                "Morgan Ellis",
                "Parker Lee",
                "Rowan Brooks",
                "Dakota Hayes"
            };

            // Try FakeStoreAPI /orders
            try
            {
                var fsResp = await _http.GetAsync("https://fakestoreapi.com/orders");
                if (fsResp.IsSuccessStatusCode)
                {
                    using var stream = await fsResp.Content.ReadAsStreamAsync();
                    var doc = await JsonDocument.ParseAsync(stream);
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        var ord = new Order();
                        if (el.TryGetProperty("id", out var id)) ord.OrderId = id.GetInt32();
                        if (el.TryGetProperty("userId", out var uid))
                        {
                            var idVal = uid.GetInt32();
                            // map userId to a friendly name when possible
                            ord.CustomerName = sampleNames[(idVal - 1) % sampleNames.Length];
                        }
                        if (el.TryGetProperty("products", out var products))
                        {
                            foreach (var p in products.EnumerateArray())
                            {
                                var item = new ProductItem();
                                if (p.TryGetProperty("productId", out var pid)) item.Id = pid.GetInt32();
                                if (p.TryGetProperty("quantity", out var q)) item.Quantity = q.GetInt32();
                                ord.Products.Add(item);
                            }
                        }
                        results.Add(ord);
                    }
                }
            }
            catch { }

            // Try DummyJSON /carts
            try
            {
                var djResp = await _http.GetAsync("https://dummyjson.com/carts");
                if (djResp.IsSuccessStatusCode)
                {
                    using var stream = await djResp.Content.ReadAsStreamAsync();
                    var doc = await JsonDocument.ParseAsync(stream);
                    if (doc.RootElement.TryGetProperty("carts", out var carts))
                    {
                        foreach (var c in carts.EnumerateArray())
                        {
                            var ord = new Order();
                            if (c.TryGetProperty("id", out var id)) ord.OrderId = id.GetInt32();
                            if (c.TryGetProperty("total", out var total)) ord.TotalPrice = total.GetDecimal();
                            if (c.TryGetProperty("products", out var products))
                            {
                                foreach (var p in products.EnumerateArray())
                                {
                                    var item = new ProductItem();
                                    if (p.TryGetProperty("id", out var pid)) item.Id = pid.GetInt32();
                                    if (p.TryGetProperty("title", out var title)) item.Name = title.GetString() ?? string.Empty;
                                    if (p.TryGetProperty("quantity", out var q)) item.Quantity = q.GetInt32();
                                    if (p.TryGetProperty("price", out var pr)) item.Price = pr.GetDecimal();
                                    ord.Products.Add(item);
                                }
                            }
                            // Prefer a friendly generated customer name rather than product titles
                            ord.CustomerName = sampleNames[(ord.OrderId - 1) % sampleNames.Length];
                            ord.OrderStatus = (ord.OrderId % 3) switch { 0 => "Delivered", 1 => "Shipped", _ => "Pending" };
                            results.Add(ord);
                        }
                    }
                }
            }
            catch { }

            // Ensure totals
            foreach (var r in results)
            {
                if (r.TotalPrice == 0)
                    r.TotalPrice = r.Products.Sum(p => p.Price * p.Quantity);
                // Ensure CustomerName is never empty
                if (string.IsNullOrWhiteSpace(r.CustomerName))
                    r.CustomerName = $"Customer {r.OrderId}";
            }

            return results.OrderByDescending(o => o.OrderId).ToList();
        }
    }
}
