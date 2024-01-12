using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

public class Dolar
{
    public decimal GetExchangeRate()      // İnternetten dolar satış kurunu alma metodu
    {
        // TCMB'nin sunduğu XML verisini kullanarak döviz kuru alınıyor
        XmlDocument xmlVerisi = new XmlDocument();
        xmlVerisi.Load("http://www.tcmb.gov.tr/kurlar/today.xml");

        // XML içindeki dolar satış fiyatını al
        string exchangeRateStr = xmlVerisi.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteSelling").InnerText;
        if (decimal.TryParse(exchangeRateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal exchangeRate))
        {
            return exchangeRate;
        }
        else
        {
            Console.WriteLine("Hata: Döviz Kuru alınamadı");
            return 0;
        }
    }
}

public class PasswordMasking
{
    public static string ReadPassword()     // Şifre gizleme işlemi
    {
        string pass = "";
        do
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                    Console.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
        } while (true);
        return pass;
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal PriceToTl { get; set; }
}

public class UserAuthentication
{
    private const string CorrectUsername = "Doğukan";
    private const string CorrectPassword = "GedikUni";
    private const string CorrectUsernameAdmin = "Admin";
    private const string CorrectPasswordAdmin = "Admin";

    public static bool Authenticate(string username, string password)
    {
        if ((username == CorrectUsername && password == CorrectPassword) || (username == CorrectUsernameAdmin && password == CorrectPasswordAdmin))
        {
            Console.WriteLine("\nBot kontrol: Lütfen aşağıdaki soruyu yanıtlayın.");

            Random random = new Random();           // Bot kontrolünde rastgele sayılar üretmek için
            int num1 = random.Next(1, 49);
            int num2 = random.Next(1, 49);

            Console.Write($"{num1} + {num2} = ");
            int answer = int.Parse(Console.ReadLine());

            if (answer == num1 + num2)
            {
                Console.WriteLine("Doğrulama başarılı. Giriş yapılıyor...");
                return true;
            }
        }

        Console.WriteLine("Giriş başarısız. Program kapatılıyor.");
        return false;
    }
}

public class ProductManager
{
    private decimal ExchangeRate;  
    private static int NextProductId = 1;
    private List<Product> Products;

    public ProductManager(decimal exchangeRate)
    {
        ExchangeRate = exchangeRate;  
        Products = new List<Product>();
    }

    public void ShowMessage() // Her işlemden sonra yanıtın görülebilmesi ve kullanıcının beklemesi için bir metod
    {
        Console.WriteLine("\nDevam etmek için herhangi bir tuşa basın...");
        Console.ReadKey();
    }

    public void AddProduct(Product product) // Ürün ekleme özelliğini kullandığımız metod
    {
        if (Products.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"\nHata: \"{product.Name}\" adında bir ürün zaten var. Lütfen farklı bir isim kullanın.");
            ShowMessage();
            return;
        }

        product.Id = NextProductId++;
        product.PriceToTl = product.Price * ExchangeRate;
        Products.Add(product);
        ReorderProductIds();
    }

    public void UpdateStock(int productId, int quantity) // Ürün stoğunu güncellediğimiz metod
    {
        Product product = GetProductById(productId);

        if (product != null)
        {
            product.StockQuantity += quantity;
            product.PriceToTl = product.Price * ExchangeRate; // Stok değiştikten sonra TL fiyatlarını kura göre hesapla
            Console.WriteLine($"\n{product.Name} stoğu güncellendi. Yeni stok miktarı: {product.StockQuantity}");
            ShowMessage();
        }
        else
        {
            Console.WriteLine("\nÜrün bulunamadı.");
            ShowMessage();
        }
    }

    public void RemoveProduct(int productId) // Ürün sildiğimiz metod
    {
        Product product = GetProductById(productId);

        if (product != null)
        {
            Products.Remove(product);
            Console.WriteLine($"\n{product.Name} ürünü silindi.");
            ReorderProductIds();
            ShowMessage();
        }
        else
        {
            Console.WriteLine("\nÜrün bulunamadı.");
            ShowMessage();
        }
    }
    public void UpdatePrice(int productId, decimal price) // Ürün stoğunu güncellediğimiz metod
    {
        Product product = GetProductById(productId);

        if (product != null)
        {
            product.Price = price;
            product.PriceToTl = product.Price * ExchangeRate; // Stok değiştikten sonra TL fiyatlarını kura göre hesapla
            Console.WriteLine($"\n{product.Name} Fiyatı güncellendi. Yeni Fiyat : {product.Price}");
            ShowMessage();
        }
        else
        {
            Console.WriteLine("\nÜrün bulunamadı.");
            ShowMessage();
        }
    }

    private void ReorderProductIds()
    {
        Products = Products.OrderBy(p => p.Id).ToList();
    }

    private Product GetProductById(int productId)
    {
        return Products.Find(p => p.Id == productId);
    }

    public void ShowStockStatus() // Arayüz için kullandığımız metod
    {
        Console.WriteLine("### Stok Durumu ###");
        Console.WriteLine("{0,-5} {1,-15} {2,-10} {3,-15} {4,-10}", "ID", "Ürün Adı", "Miktar", "Fiyat($)", "Fiyat(TL)");
        foreach (var product in Products)
        {
            Console.WriteLine("{0,-5} {1,-15} {2,-10} {3,-15} {4,-10}", product.Id, product.Name, product.StockQuantity, product.Price.ToString("0.00"), product.PriceToTl.ToString("0.00"));
        }
    }
}

public class MenuManager
{
    private ProductManager productManager;
    private decimal ExchangeRate;

    public MenuManager(ProductManager productManager, decimal exchangeRate)
    {
        this.productManager = productManager;
        ExchangeRate = exchangeRate;
    }

    public void StartMenu() // Sıradan kullanıcılar için kullanım paneli
    {
        while (true)
        {
            productManager.ShowStockStatus();
            Console.WriteLine("\n### Stok Yönetimi ###");
            Console.WriteLine("1. Çıkış");

            Console.Write("Çıkmak İçin 1 Yazın: ");
            string choice = Console.ReadLine();

            ProcessUserChoiceForUser(choice);
        }
    }

    public void StartMenuAdmin() // Yönetici seviyesindekiler için kullanım paneli
    {
        while (true)
        {
            productManager.ShowStockStatus();
            Console.WriteLine("\n### Stok Yönetimi ###");
            Console.WriteLine("1. Stok Güncelle");
            Console.WriteLine("2. Yeni Ürün Ekle");
            Console.WriteLine("3. Ürün Sil");
            Console.WriteLine("4. Fiyat Güncelle");
            Console.WriteLine("5. Çıkış");

            Console.Write("Seçiminizi yapın (1-5): ");
            string choice = Console.ReadLine();

            ProcessUserChoice(choice);
        }
    }

    private void ProcessUserChoice(string choice) // Yönetici case leri
    {
        switch (choice)
        {
            case "1":
                UpdateStockMenu();
                Console.Clear();
                break;

            case "2":
                AddProductMenu();
                Console.Clear();
                break;

            case "3":
                RemoveProductMenu();
                Console.Clear();
                break;

            case "4":
                UpdatePriceMenu();
                Console.Clear();
                break;

            case "5":
                Console.Clear();
                Console.WriteLine("Program kapatılıyor.");
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("Geçersiz seçim. Lütfen tekrar deneyin.");
                Task.Delay(1000).Wait();
                Console.Clear();
                break;
        }
    }

    private void ProcessUserChoiceForUser(string choice) // Kullanıcı case leri 
    {
        switch (choice)
        {
            case "1":
                Console.Clear();
                Console.WriteLine("Program kapatılıyor.");
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("Geçersiz seçim. Lütfen tekrar deneyin.");
                Task.Delay(1000).Wait();
                Console.Clear();
                break;
        }
    }

    private void UpdateStockMenu()
    {
        int productId;

        do
        {
            Console.Write("\nÜrün ID'sini girin: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out productId))
            {
                break;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Ürün ID'si sadece sayı olmalıdır.");
            }
        } while (true);

        int quantity;

        do
        {
            Console.Write("\nEkleme veya Çıkarma miktarını girin (Çıkarma yapacaksanız başına '-' işareti koyunuz): ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out quantity) || (input.StartsWith("-") && int.TryParse(input.Substring(1), out quantity)))
            {
                break;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Miktar sadece sayı olmalıdır.");
            }
        } while (true);

        productManager.UpdateStock(productId, quantity);
    }

    private void AddProductMenu()
    {
        Product newProduct = new Product();

        Console.Write("\nYeni ürün adı: ");
        newProduct.Name = Console.ReadLine();

        do
        {
            Console.Write("\nStok miktarı: ");
            string stockQuantityInput = Console.ReadLine();

            if (int.TryParse(stockQuantityInput, out int stockQuantity))
            {
                newProduct.StockQuantity = stockQuantity;
                break;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Stok miktarı sadece sayı olmalıdır.");
            }
        } while (true);

        do
        {
            Console.Write("\nFiyatı girerken , kullanarak ondalık kısmı yazınız\n Fiyat: ");
            string priceInput = Console.ReadLine();

            if (decimal.TryParse(priceInput, out decimal price))
            {
                newProduct.Price = price;
                break;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Fiyat sadece sayı olmalıdır.");
            }
        } while (true);

        productManager.AddProduct(newProduct);
    }

    private void RemoveProductMenu()
    {
        int productId;
        string answerLower;

        do
        {
            Console.Write("\nÜrün ID'sini girin: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out productId))
            {
                Console.WriteLine($"\n{productId} Id'li ürünü silmek istediğinize emin misiniz? Evet ise 'evet' yazın.");
                answerLower = Console.ReadLine();
                string a = answerLower.ToLower();
                if (a == "evet")
                {
                    productManager.RemoveProduct(productId);
                    break;
                }
                else
                    return;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Ürün ID'si sadece sayı olmalıdır.");
                break;
            }
        } while (true);
    }
    private void UpdatePriceMenu()
    {
        int productId;

        do
        {
            Console.Write("\nÜrün ID'sini girin: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out productId))
            {
                break;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Ürün ID'si sadece sayı olmalıdır.");
            }
        } while (true);

        int price;

        do
        {
            Console.Write("\nYeni Fiyatı Girin: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out price) || (input.StartsWith("-") && int.TryParse(input.Substring(1), out price)))
            {
                break;
            }
            else
            {
                Console.WriteLine("\nHatalı giriş! Miktar sadece sayı olmalıdır.");
            }
        } while (true);

        productManager.UpdatePrice(productId, price);
    }
}

class Program
{
    static void Main()
    {
        Dolar dolar = new Dolar();
        decimal exchangeRate = dolar.GetExchangeRate();

        Console.Write("Kullanıcı Adı: ");
        string username = Console.ReadLine();

        Console.Write("Şifre: ");
        string password = PasswordMasking.ReadPassword();

        if (UserAuthentication.Authenticate(username, password))
        {
            Task.Delay(2000).Wait();
            Console.Clear();

            ProductManager productManager = new ProductManager(exchangeRate);

            // Örnek ürünler ekleyelim
            productManager.AddProduct(new Product { Name = "Bilgisayar", StockQuantity = 11, Price = 1500.00m });
            productManager.AddProduct(new Product { Name = "Telefon", StockQuantity = 18, Price = 800.00m });
            productManager.AddProduct(new Product { Name = "Tablet", StockQuantity = 33, Price = 500.00m });
            productManager.AddProduct(new Product { Name = "Televizyon", StockQuantity = 24, Price = 2500.00m });

            MenuManager menuManager = new MenuManager(productManager, exchangeRate);

            // Menüyü başlat
            if (username == "Admin")
                menuManager.StartMenuAdmin();
            else
                menuManager.StartMenu();
        }
    }
}