using ElectronicsStoreAss3.Models;
using Microsoft.AspNetCore.Identity;

namespace ElectronicsStoreAss3.Data
{
    public static class Init
    {
        public static void SeedOwnerAccount(AppDbContext context)
        {
            if (context.Accounts.Any(a => a.Role == Role.Owner))
                return;

            var hasher = new PasswordHasher<object>();
            var account = new Account
            {
                Email = "owner@test.com",
                PasswordHash = hasher.HashPassword(null, "password"),
                Role = Role.Owner
            };

            context.Accounts.Add(account);
            context.SaveChanges();

            var owner = new Owner
            {
                AccountId = account.Id,
                Name = "Max",
                LastName = "Isghey",
                Email = "MaxIsGhey@gmail.com"
            };

            context.Owners.Add(owner);
            context.SaveChanges();
        }

        public static void SeedCustomerAccount(AppDbContext context)
        {
            if (context.Accounts.Any(a => a.Role == Role.Customer))
                return;
            var hasher = new PasswordHasher<object>();
            var account = new Account
            {
                Email = "Arbnor@test.com",
                PasswordHash = hasher.HashPassword(null, "password"),
                Role = Role.Customer
            };
            context.Accounts.Add(account);
            context.SaveChanges();

            var customer = new Customer
            {
                AccountId = account.Id,
                FirstName = "Arbnor",
                LastName = "Caravaku",
                Mobile = "0412345678",
                Email = "Arbnor@test.com"
            };
            context.Customers.Add(customer);
            context.SaveChanges();
        }

        public static void SeedTestProductsAndInventory(AppDbContext context)
        {
            if (context.Product.Any())
                return;

            var products = new List<Product>
            {
                // Smartphones (15 items)
                new()
                {
                    ProductId = 1, Name = "iPhone 15 Pro Max", SKU = "APPLE-IP15PM-256", Category = "Smartphones",
                    Brand = "Apple", Price = 1999.99m,
                    Description = "The ultimate iPhone with titanium design, A17 Pro chip, and advanced camera system.",
                    Specifications =
                        "6.7-inch Super Retina XDR display\nA17 Pro chip\n256GB storage\nPro camera system with 5x Telephoto\nTitanium design\nUSB-C connector",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/746483-Product-0-I-638615190006144138.jpg?v=1735868822",
                    IsActive = true
                },
                new()
                {
                    ProductId = 2, Name = "iPhone 15 Pro", SKU = "APPLE-IP15P-128", Category = "Smartphones",
                    Brand = "Apple", Price = 1649.99m,
                    Description = "Pro performance in a more compact size with titanium design and A17 Pro chip.",
                    Specifications =
                        "6.1-inch Super Retina XDR display\nA17 Pro chip\n128GB storage\nPro camera system with 3x Telephoto\nTitanium design\nAction Button",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/746468-Product-0-I-638615207420385582.jpg?v=1735868837",
                    IsActive = true
                },
                new()
                {
                    ProductId = 3, Name = "iPhone 15", SKU = "APPLE-IP15-128", Category = "Smartphones",
                    Brand = "Apple", Price = 1499.99m,
                    Description = "The new iPhone 15 with Dynamic Island and 48MP camera system.",
                    Specifications =
                        "6.1-inch Super Retina XDR display\nA16 Bionic chip\n128GB storage\n48MP Main camera\nDynamic Island\nUSB-C connector",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/products/639168-Product-0-I-638301483010334192.jpg?v=1698811953",
                    IsActive = true
                },
                new()
                {
                    ProductId = 4, Name = "Samsung Galaxy S24 Ultra", SKU = "SAMSUNG-GS24U-256",
                    Category = "Smartphones",
                    Brand = "Samsung", Price = 1949.99m,
                    Description = "The most powerful Galaxy with S Pen, AI features, and 200MP camera.",
                    Specifications =
                        "6.8-inch Dynamic AMOLED 2X display\nSnapdragon 8 Gen 3\n256GB storage\n200MP quad camera system\nBuilt-in S Pen\nTitanium frame",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/672256-Product-0-I-638490165603385142.jpg?v=1713419788",
                    IsActive = true
                },
                new()
                {
                    ProductId = 5, Name = "Samsung Galaxy S24+", SKU = "SAMSUNG-GS24P-256", Category = "Smartphones",
                    Brand = "Samsung", Price = 1549.99m,
                    Description = "Enhanced Galaxy experience with larger display and advanced AI features.",
                    Specifications =
                        "6.7-inch Dynamic AMOLED 2X display\nSnapdragon 8 Gen 3\n256GB storage\n50MP triple camera system\nGalaxy AI features\nArmor Aluminum frame",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/672255-Product-0-I-638490165604779472_4b6497e5-061b-4f4c-ab84-638183a5d02c.jpg?v=1716881250",
                    IsActive = true
                },
                new()
                {
                    ProductId = 6, Name = "Samsung Galaxy S24", SKU = "SAMSUNG-GS24-128", Category = "Smartphones",
                    Brand = "Samsung", Price = 1349.99m,
                    Description = "Compact Galaxy flagship with AI-powered photography and performance.",
                    Specifications =
                        "6.2-inch Dynamic AMOLED 2X display\nSnapdragon 8 Gen 3\n128GB storage\n50MP triple camera system\nGalaxy AI features\nArmor Aluminum frame",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/672240-Product-0-I-638490161404439522_83f02869-9d14-4bad-87eb-f477c44e2608.jpg?v=1716881004",
                    IsActive = true
                },
                new()
                {
                    ProductId = 7, Name = "Google Pixel 8 Pro", SKU = "GOOGLE-P8P-128", Category = "Smartphones",
                    Brand = "Google", Price = 1699.99m,
                    Description = "Google's most advanced Pixel with Tensor G3 and AI photography features.",
                    Specifications =
                        "6.7-inch LTPO OLED display\nGoogle Tensor G3\n128GB storage\n50MP triple camera system\nMagic Eraser\n7 years of updates",
                    ImagePath = "https://www.jbhifi.com.au/cdn/shop/files/662718-Product-0-I-638302518603197997.jpg?v=1717049313",
                    IsActive = true
                },
                new()
                {
                    ProductId = 8, Name = "Google Pixel 8", SKU = "GOOGLE-P8-128", Category = "Smartphones",
                    Brand = "Google", Price = 1199.99m,
                    Description = "Pure Google experience with advanced computational photography.",
                    Specifications =
                        "6.2-inch OLED display\nGoogle Tensor G3\n128GB storage\n50MP dual camera system\nMagic Eraser\n7 years of updates",
                    ImagePath = "https://www.jbhifi.com.au/cdn/shop/files/662713-Product-0-I-638319943214794111.jpg?v=1717049072",
                    IsActive = true
                },
                new()
                {
                    ProductId = 9, Name = "OnePlus 12", SKU = "ONEPLUS-OP12-256", Category = "Smartphones",
                    Brand = "OnePlus", Price = 1299.99m,
                    Description = "Flagship killer with Snapdragon 8 Gen 3 and 100W fast charging.",
                    Specifications =
                        "6.82-inch LTPO AMOLED display\nSnapdragon 8 Gen 3\n256GB storage\n50MP triple camera system\n100W SuperVOOC charging\nOxygenOS 14",
                    ImagePath =
                        "https://m.media-amazon.com/images/I/715PZO9NA0L._AC_UF894,1000_QL80_.jpg",
                    IsActive = true
                },
                new()
                {
                    ProductId = 10, Name = "Xiaomi 14 Ultra", SKU = "XIAOMI-14U-512", Category = "Smartphones",
                    Brand = "Xiaomi", Price = 1799.99m,
                    Description = "Photography powerhouse with Leica cameras and Snapdragon 8 Gen 3.",
                    Specifications =
                        "6.73-inch LTPO AMOLED display\nSnapdragon 8 Gen 3\n512GB storage\n50MP quad Leica camera system\n90W fast charging\nMIUI 15",
                    ImagePath = "https://m.media-amazon.com/images/I/41Yb3ZpArjL.jpg",
                    IsActive = true
                },
                new()
                {
                    ProductId = 11, Name = "Nothing Phone (2a)", SKU = "NOTHING-P2A-256", Category = "Smartphones",
                    Brand = "Nothing", Price = 699.99m,
                    Description = "Unique transparent design with Glyph Interface and clean Android experience.",
                    Specifications =
                        "6.7-inch AMOLED display\nMediaTek Dimensity 7200 Pro\n256GB storage\n50MP dual camera system\nGlyph Interface\nNothing OS 2.5",
                    ImagePath = "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcRMtBHjJqJDHixqV6h-TSV6rSJuRyUCAWhQxiP331EOshpuz9XpHKuTcfxjDZfWDawjkbPjnHpv3kkv1F-yPmQYuN-INISt",
                    IsActive = true
                },
                new()
                {
                    ProductId = 12, Name = "Sony Xperia 1 VI", SKU = "SONY-X1VI-256", Category = "Smartphones",
                    Brand = "Sony", Price = 1899.99m,
                    Description = "Professional camera phone with 4K 120Hz display and Alpha camera technology.",
                    Specifications =
                        "6.5-inch 4K OLED display\nSnapdragon 8 Gen 3\n256GB storage\n48MP triple camera system\nAlpha camera tech\n3.5mm headphone jack",
                    ImagePath = "https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcTWa7M78wX5nMfd8p1MHT3DLZ0V7oPBrYISy_mLHDDSXL0jj5JGLYCgOhTeemkQ1qiZh3b0VVsOXIP7H3qTZ47DeZDTtZrcDaTtrmOrCQOMO-oubOJFbGDMpjQ",
                    IsActive = true
                },
                new()
                {
                    ProductId = 13, Name = "OPPO Find X7 Ultra", SKU = "OPPO-FX7U-512", Category = "Smartphones",
                    Brand = "OPPO", Price = 1699.99m,
                    Description = "Hasselblad camera system with periscope telephoto and fast charging.",
                    Specifications =
                        "6.82-inch LTPO AMOLED display\nSnapdragon 8 Gen 3\n512GB storage\n50MP quad Hasselblad camera system\n100W SuperVOOC charging\nColorOS 14",
                    ImagePath =
                        "https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcTaUw0wtyZ8Xie0JpBOfUvtMt79pl336AXo0M4Rng_0neeJvY2jo3TmE6KsB5y2muD-nktpWazh1YzVOgqvV1MLAZSPzV4_",
                    IsActive = true
                },
                new()
                {
                    ProductId = 14, Name = "Huawei P60 Pro", SKU = "HUAWEI-P60P-256", Category = "Smartphones",
                    Brand = "Huawei", Price = 1549.99m,
                    Description = "Professional photography with Ultra Lighting camera and elegant design.",
                    Specifications =
                        "6.67-inch LTPO OLED display\nSnapdragon 8+ Gen 1\n256GB storage\n48MP triple camera system\nUltra Lighting camera\nHarmonyOS 3.1",
                    ImagePath =
                        "https://i.ebayimg.com/images/g/vlIAAOSwnlxkHRlo/s-l1200.jpg",
                    IsActive = true
                },
                new()
                {
                    ProductId = 15, Name = "Motorola Edge 50 Ultra", SKU = "MOTOROLA-E50U-512",
                    Category = "Smartphones",
                    Brand = "Motorola", Price = 1399.99m,
                    Description = "Premium smartphone with 200MP camera and 125W fast charging.",
                    Specifications =
                        "6.7-inch pOLED display\nSnapdragon 8s Gen 3\n512GB storage\n50MP triple camera system\n125W TurboPower charging\nMyUX on Android 14",
                    ImagePath = "https://www.jbhifi.com.au/cdn/shop/files/781690-Product-0-I-638593570204663109.jpg?v=1723770674",
                    IsActive = true
                },

                // Laptops (15 items)
                new()
                {
                    ProductId = 16, Name = "MacBook Pro 16\" M3 Max", SKU = "APPLE-MBP16-M3MAX", Category = "Laptops",
                    Brand = "Apple", Price = 4999.99m,
                    Description = "Ultimate creative powerhouse with M3 Max chip and Liquid Retina XDR display.",
                    Specifications =
                        "16.2-inch Liquid Retina XDR display\nApple M3 Max chip\n36GB unified memory\n1TB SSD storage\n40-core GPU\n22-hour battery life\nThunderbolt 4 ports",
                    ImagePath =
                        "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcSi5AuDf6KFUWfoXRyoiMWntrok8FrnGiQOti8ntQHUJ00IKJYiyUqW4E6ENsa3DIir2d2RQA4_k0B_JilT3P3LATh4F02zSHILnd7giHMv05zH5WX7q-rdNQ",
                    IsActive = true
                },
                new()
                {
                    ProductId = 17, Name = "MacBook Pro 14\" M3 Pro", SKU = "APPLE-MBP14-M3PRO", Category = "Laptops",
                    Brand = "Apple", Price = 3499.99m,
                    Description = "Professional performance in a compact form with M3 Pro chip.",
                    Specifications =
                        "14.2-inch Liquid Retina XDR display\nApple M3 Pro chip\n18GB unified memory\n512GB SSD storage\n18-core GPU\n18-hour battery life\nThunderbolt 4 ports",
                    ImagePath =
                        "https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcQldArsc5nTexVCv_cJWfYKG56we6mcQK1ki1PQoPY542XILNtD",
                    IsActive = true
                },
                new()
                {
                    ProductId = 18, Name = "MacBook Air 15\" M3", SKU = "APPLE-MBA15-M3", Category = "Laptops",
                    Brand = "Apple", Price = 2199.99m,
                    Description = "Larger Air with incredible performance and all-day battery life.",
                    Specifications =
                        "15.3-inch Liquid Retina display\nApple M3 chip\n8GB unified memory\n256GB SSD storage\n10-core GPU\n18-hour battery life\nMagSafe 3 charging",
                    ImagePath =
                        "https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcTOENkVwsPMuIlJgctA1WTi-FTMGeV2sm0KDuF2nE48ts--cDA",
                    IsActive = true
                },
                new()
                {
                    ProductId = 19, Name = "MacBook Air 13\" M3", SKU = "APPLE-MBA13-M3", Category = "Laptops",
                    Brand = "Apple", Price = 1799.99m,
                    Description = "The world's most popular laptop, now with M3 chip for incredible performance.",
                    Specifications =
                        "13.6-inch Liquid Retina display\nApple M3 chip\n8GB unified memory\n256GB SSD storage\n8-core GPU\n18-hour battery life\nMagSafe 3 charging",
                    ImagePath =
                        "https://encrypted-tbn3.gstatic.com/shopping?q=tbn:ANd9GcTXeAfyrN_N-wIyxh75ZXe5Xd2LQz7GF4ZTy6P4suIaMy5y6cA",
                    IsActive = true
                },
                new()
                {
                    ProductId = 20, Name = "Dell XPS 15 OLED", SKU = "DELL-XPS15-OLED", Category = "Laptops",
                    Brand = "Dell", Price = 3299.99m,
                    Description = "Premium Windows laptop with stunning OLED display and powerful performance.",
                    Specifications =
                        "15.6-inch 3.5K OLED InfinityEdge display\nIntel Core i7-13700H\n32GB DDR5 RAM\n1TB PCIe SSD\nNVIDIA GeForce RTX 4060\nThunderbolt 4\nWindows 11 Pro",
                    ImagePath =
                        "https://i.dell.com/is/image/DellContent/content/dam/ss2/product-images/dell-client-products/notebooks/xps-notebooks/xps-15-9530/media-gallery/touch-black/notebook-xps-15-9530-t-black-gallery-5.psd",
                    IsActive = true
                },
                new()
                {
                    ProductId = 21, Name = "Dell XPS 13 Plus", SKU = "DELL-XPS13P-I7", Category = "Laptops",
                    Brand = "Dell", Price = 2199.99m,
                    Description = "Ultra-modern ultrabook with edge-to-edge keyboard and invisible touchpad.",
                    Specifications =
                        "13.4-inch 4K+ InfinityEdge display\nIntel Core i7-1360P\n16GB LPDDR5 RAM\n512GB PCIe SSD\nIntel Iris Xe Graphics\nThunderbolt 4\nWindows 11 Home",
                    ImagePath =
                        "https://m.media-amazon.com/images/I/710EGJBdIML.jpg",
                    IsActive = true
                },
                new()
                {
                    ProductId = 22, Name = "Microsoft Surface Laptop Studio 2", SKU = "MICROSOFT-SLS2-I7",
                    Category = "Laptops",
                    Brand = "Microsoft", Price = 3699.99m,
                    Description = "Most powerful Surface laptop with unique rotating touchscreen design.",
                    Specifications =
                        "14.4-inch PixelSense Flow touchscreen\nIntel Core i7-13700H\n32GB LPDDR5X RAM\n1TB PCIe SSD\nNVIDIA GeForce RTX 4060\nSurface Pen support\nWindows 11 Pro",
                    ImagePath = "https://cdn-dynmedia-1.microsoft.com/is/image/microsoftcorp/b00-Surface-Laptop-Studio-2-03",
                    IsActive = true
                },
                new()
                {
                    ProductId = 23, Name = "Microsoft Surface Laptop 5", SKU = "MICROSOFT-SL5-AMD",
                    Category = "Laptops",
                    Brand = "Microsoft", Price = 1899.99m,
                    Description = "Elegant laptop with premium materials and excellent performance.",
                    Specifications =
                        "13.5-inch PixelSense touchscreen\nAMD Ryzen 7 5980HS\n16GB LPDDR4X RAM\n512GB PCIe SSD\nAMD Radeon Graphics\nSurface Pen support\nWindows 11 Home",
                    ImagePath = "https://www.jbhifi.com.au/cdn/shop/files/805977-Product-0-I-638821678815712286.jpg?v=1746571157",
                    IsActive = true
                },
                new()
                {
                    ProductId = 24, Name = "Lenovo ThinkPad X1 Carbon Gen 11", SKU = "LENOVO-X1C11-I7",
                    Category = "Laptops",
                    Brand = "Lenovo", Price = 2799.99m,
                    Description = "Business ultrabook with military-grade durability and enterprise security.",
                    Specifications =
                        "14-inch 2.8K OLED display\nIntel Core i7-1365U\n32GB LPDDR5 RAM\n1TB PCIe SSD\nIntel Iris Xe Graphics\nThunderbolt 4\nWindows 11 Pro",
                    ImagePath =
                        "https://p3-ofp.static.pub//fes/cms/2024/07/05/umcrxcnsm2br1itju6gvundeb9s6tf364734.png",
                    IsActive = true
                },
                new()
                {
                    ProductId = 25, Name = "HP Spectre x360 16", SKU = "HP-SPECTRE-X360-16", Category = "Laptops",
                    Brand = "HP", Price = 2599.99m,
                    Description = "Convertible laptop with 3K OLED display and premium design.",
                    Specifications =
                        "16-inch 3K OLED touchscreen\nIntel Core i7-13700H\n16GB DDR5 RAM\n1TB PCIe SSD\nIntel Arc A370M Graphics\nHP Pen included\nWindows 11 Home",
                    ImagePath = "https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcQa0YD7pgegg-LU_ntGT4tayIk3ImrtMjg63nisTgfZ9C-WT81XlwESWq0v8f6YaHIhIfRhie40JYvf03PEubKySgCYdEnShds9nXqEwqaGQl0FwDa2ljKKOA",
                    IsActive = true
                },
                new()
                {
                    ProductId = 26, Name = "ASUS ROG Zephyrus G16", SKU = "ASUS-ROGG16-RTX4080", Category = "Laptops",
                    Brand = "ASUS", Price = 4199.99m,
                    Description = "Gaming laptop with RTX 4080 graphics and premium OLED display.",
                    Specifications =
                        "16-inch 2.5K OLED 240Hz display\nIntel Core i9-13900H\n32GB DDR5 RAM\n1TB PCIe SSD\nNVIDIA GeForce RTX 4080\nROG Nebula HDR display\nWindows 11 Home",
                    ImagePath = "https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcRGk1eZDgA0yZNrLc62ri8L5ZLY3CsD01E2l4odyOy_7AGkc7r1dfO1Su4A8JJDYENWqH-zRHaaBsHDxLRNLnxaUrCsckctHYvb7vFgJ8hWg3_lL4dcOhS2RQ",
                    IsActive = true
                },
                new()
                {
                    ProductId = 27, Name = "Framework Laptop 13", SKU = "FRAMEWORK-L13-I7", Category = "Laptops",
                    Brand = "Framework", Price = 1899.99m,
                    Description = "Modular laptop designed for repairability and upgradability.",
                    Specifications =
                        "13.5-inch 2256×1504 display\nIntel Core i7-1360P\n32GB DDR4 RAM\n1TB NVMe SSD\nIntel Iris Xe Graphics\nModular ports\nWindows 11 Home",
                    ImagePath = "https://static.frame.work/uq78hkvl9829a8ps1emnuicfcmhm",
                    IsActive = true
                },
                new()
                {
                    ProductId = 28, Name = "Razer Blade 15 Advanced", SKU = "RAZER-B15-RTX4070", Category = "Laptops",
                    Brand = "Razer", Price = 3799.99m,
                    Description = "Gaming laptop with RTX 4070 graphics and high refresh rate display.",
                    Specifications =
                        "15.6-inch QHD 240Hz display\nIntel Core i7-13800H\n32GB DDR5 RAM\n1TB PCIe SSD\nNVIDIA GeForce RTX 4070\nPer-key RGB keyboard\nWindows 11 Home",
                    ImagePath =
                        "https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcSsCSvR1VKCTtx4EzazJqpIxr64gKF9q32CV4SjBVxCQvXgQTICO0CIz5amW75ptDIQYVZzzILQT7lZ2a1wEtdBrnxKPDFCBgv4_q-d9zZ_s0zLJ-orRuHa",
                    IsActive = true
                },
                new()
                {
                    ProductId = 29, Name = "LG Gram 17", SKU = "LG-GRAM17-I7", Category = "Laptops",
                    Brand = "LG", Price = 2299.99m,
                    Description = "Ultra-lightweight 17-inch laptop with all-day battery life.",
                    Specifications =
                        "17-inch WQXGA IPS display\nIntel Core i7-1360P\n16GB LPDDR5 RAM\n512GB NVMe SSD\nIntel Iris Xe Graphics\n1.35kg weight\nWindows 11 Home",
                    ImagePath = "https://www.lg.com/content/dam/channel/wcms/au/images/laptops/17z90q-g_aa78a_ehap_au_c/gallery/17Z90Q-G.AA76A3-Laptops-DZ-1.jpg",
                    IsActive = true
                },
                new()
                {
                    ProductId = 30, Name = "Alienware m18 R1", SKU = "ALIENWARE-M18-RTX4090", Category = "Laptops",
                    Brand = "Dell", Price = 5999.99m,
                    Description = "Ultimate gaming laptop with RTX 4090 graphics and desktop-class performance.",
                    Specifications =
                        "18-inch QHD+ 165Hz display\nIntel Core i9-13980HX\n64GB DDR5 RAM\n2TB PCIe SSD\nNVIDIA GeForce RTX 4090\nAdvanced cooling\nWindows 11 Home",
                    ImagePath =
                        "https://i.dell.com/is/image/DellContent/content/dam/ss2/product-images/dell-client-products/notebooks/alienware-notebooks/alienware-m18-intel/media-gallery/ir/notebook-alienware-awm18-hd-black-gallery-12.psd?fmt=png-alpha&pscan=auto&scl=1&hei=402&wid=617&qlt=100,1&resMode=sharp2&size=617,402&chrss=full",
                    IsActive = true
                },

                // Tablets (10 items)
                new()
                {
                    ProductId = 31, Name = "iPad Pro 12.9\" M2", SKU = "APPLE-IPADPRO129-M2", Category = "Tablets",
                    Brand = "Apple", Price = 1749.99m,
                    Description = "The ultimate iPad experience with M2 chip and Liquid Retina XDR display.",
                    Specifications =
                        "12.9-inch Liquid Retina XDR display\nApple M2 chip\n128GB storage\n12MP cameras\nApple Pencil hover\nThunderbolt / USB 4",
                    ImagePath =
                        "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/ipad-air-model-unselect-gallery-1-202405",
                    IsActive = true
                },
                new()
                {
                    ProductId = 32, Name = "iPad Pro 11\" M2", SKU = "APPLE-IPADPRO11-M2", Category = "Tablets",
                    Brand = "Apple", Price = 1399.99m,
                    Description = "Pro performance in a portable design with M2 chip.",
                    Specifications =
                        "11-inch Liquid Retina display\nApple M2 chip\n128GB storage\n12MP cameras\nApple Pencil hover\nThunderbolt / USB 4",
                    ImagePath =
                        "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/ipad-air-model-unselect-gallery-1-202405",
                    IsActive = true
                },
                new()
                {
                    ProductId = 33, Name = "iPad Air 10.9\" M1", SKU = "APPLE-IPADAIR-M1", Category = "Tablets",
                    Brand = "Apple", Price = 899.99m,
                    Description = "Colorful and powerful with M1 chip and advanced cameras.",
                    Specifications =
                        "10.9-inch Liquid Retina display\nApple M1 chip\n64GB storage\n12MP cameras\nApple Pencil (2nd gen)\nUSB-C connector",
                    ImagePath =
                        "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/ipad-air-model-unselect-gallery-1-202405",
                    IsActive = true
                },
                new()
                {
                    ProductId = 34, Name = "iPad 10.9\" 10th Gen", SKU = "APPLE-IPAD109-10TH", Category = "Tablets",
                    Brand = "Apple", Price = 649.99m,
                    Description = "Colorful all-screen design with powerful A14 Bionic chip.",
                    Specifications =
                        "10.9-inch Liquid Retina display\nA14 Bionic chip\n64GB storage\n12MP cameras\nApple Pencil (1st gen)\nUSB-C connector",
                    ImagePath =
                        "https://cdsassets.apple.com/live/SZLF0YNV/images/sp/111886_sp850-ipad-mini-6gen-480.png",
                    IsActive = true
                },
                new()
                {
                    ProductId = 35, Name = "iPad mini 8.3\" 6th Gen", SKU = "APPLE-IPADMINI-6TH", Category = "Tablets",
                    Brand = "Apple", Price = 749.99m,
                    Description = "Mega performance in a compact design with A15 Bionic chip.",
                    Specifications =
                        "8.3-inch Liquid Retina display\nA15 Bionic chip\n64GB storage\n12MP cameras\nApple Pencil (2nd gen)\nUSB-C connector",
                    ImagePath =
                        "https://cdsassets.apple.com/live/SZLF0YNV/images/sp/111886_sp850-ipad-mini-6gen-480.png",
                    IsActive = true
                },
                new()
                {
                    ProductId = 36, Name = "Samsung Galaxy Tab S9 Ultra", SKU = "SAMSUNG-TABS9U-512",
                    Category = "Tablets",
                    Brand = "Samsung", Price = 1899.99m,
                    Description = "Largest Galaxy Tab with S Pen and desktop-class performance.",
                    Specifications =
                        "14.6-inch Dynamic AMOLED 2X display\nSnapdragon 8 Gen 2\n512GB storage\nS Pen included\nDual cameras\nDeX mode support",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/655528-Product-0-I-638524501805725350.jpg?v=1717558874",
                    IsActive = true
                },
                new()
                {
                    ProductId = 37, Name = "Samsung Galaxy Tab S9+", SKU = "SAMSUNG-TABS9P-256", Category = "Tablets",
                    Brand = "Samsung", Price = 1399.99m,
                    Description = "Premium Android tablet with AMOLED display and S Pen.",
                    Specifications =
                        "12.4-inch Dynamic AMOLED 2X display\nSnapdragon 8 Gen 2\n256GB storage\nS Pen included\nDual cameras\nDeX mode support",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/655503-Product-0-I-638524496405880100.jpg?v=1716878180",
                    IsActive = true
                },
                new()
                {
                    ProductId = 38, Name = "Microsoft Surface Pro 9", SKU = "MICROSOFT-SP9-I7", Category = "Tablets",
                    Brand = "Microsoft", Price = 1899.99m,
                    Description = "2-in-1 tablet with laptop performance and versatility.",
                    Specifications =
                        "13-inch PixelSense Flow touchscreen\nIntel Core i7-1255U\n16GB LPDDR5 RAM\n512GB SSD\nThunderbolt 4\nSurface Pen support\nWindows 11 Pro",
                    ImagePath = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcREgfH14iefiCLhEMiwEAbswa0F0uVv8qT63g",
                    IsActive = true
                },
                new()
                {
                    ProductId = 39, Name = "Lenovo Tab P12 Pro", SKU = "LENOVO-TABP12PRO", Category = "Tablets",
                    Brand = "Lenovo", Price = 899.99m,
                    Description = "Premium Android tablet with OLED display and productivity features.",
                    Specifications =
                        "12.6-inch 2K OLED display\nMediaTek Kompanio 1300T\n8GB RAM\n256GB storage\nQuad JBL speakers\nLenovo Precision Pen 3",
                    ImagePath =
                        "https://p2-ofp.static.pub/fes/cms/2021/10/28/juqs65pgl1gh3dysi7yv1tnvtsiqva364946.png",
                    IsActive = true
                },
                new()
                {
                    ProductId = 40, Name = "Amazon Fire Max 11", SKU = "AMAZON-FIREMAX11", Category = "Tablets",
                    Brand = "Amazon", Price = 349.99m,
                    Description = "Largest Fire tablet with vivid display and Alexa built-in.",
                    Specifications =
                        "11-inch 2K display\nOcta-core processor\n4GB RAM\n64GB storage\n14-hour battery\nAlexa hands-free\nFire OS",
                    ImagePath = "https://m.media-amazon.com/images/I/71wk6xXIzPL._AC_SL1500_.jpg",
                    IsActive = true
                },

                // Accessories (10 items)
                new()
                {
                    ProductId = 41, Name = "AirPods Pro 2nd Gen", SKU = "APPLE-AIRPODSPRO2", Category = "Accessories",
                    Brand = "Apple", Price = 399.99m,
                    Description = "Next-generation AirPods Pro with H2 chip and enhanced audio.",
                    Specifications =
                        "H2 chip\nAdaptive Transparency\nPersonalized Spatial Audio\nUp to 6 hours listening time\nMagSafe charging case\nTouch control",
                    ImagePath = "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/airpods-pro-2-hero-select-202409",
                    IsActive = true
                },
                new()
                {
                    ProductId = 42, Name = "AirPods 3rd Generation", SKU = "APPLE-AIRPODS3", Category = "Accessories",
                    Brand = "Apple", Price = 279.99m,
                    Description = "Reimagined AirPods with Spatial Audio and longer battery life.",
                    Specifications =
                        "H1 chip\nSpatial Audio\nAdaptive EQ\nUp to 6 hours listening time\nMagSafe charging case\nSweat and water resistant",
                    ImagePath = "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/airpods-4-hero-select-202409",
                    IsActive = true
                },
                new()
                {
                    ProductId = 43, Name = "AirPods Max", SKU = "APPLE-AIRPODSMAX", Category = "Accessories",
                    Brand = "Apple", Price = 899.99m,
                    Description = "Over-ear headphones with Active Noise Cancellation and Spatial Audio.",
                    Specifications =
                        "H1 chip\nActive Noise Cancellation\nTransparency mode\nSpatial Audio\n20-hour battery life\nDigital Crown\nStainless steel frame",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/754746-Product-0-I-638616074404449760.jpg?v=1726011017",
                    IsActive = true
                },
                new()
                {
                    ProductId = 44, Name = "Apple Watch Series 9 45mm", SKU = "APPLE-WATCH9-45",
                    Category = "Accessories",
                    Brand = "Apple", Price = 649.99m,
                    Description = "Most advanced Apple Watch with Double Tap gesture and S9 chip.",
                    Specifications =
                        "45mm Always-On Retina display\nS9 SiP\nDouble Tap gesture\nBlood Oxygen app\nECG app\n18-hour battery life\nCrash Detection",
                    ImagePath =
                        "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/refurb-45-nc-alum-midnight-sport-band-midnight-s9",
                    IsActive = true
                },
                new()
                {
                    ProductId = 45, Name = "Apple Watch SE 2nd Gen 44mm", SKU = "APPLE-WATCHSE2-44",
                    Category = "Accessories",
                    Brand = "Apple", Price = 399.99m,
                    Description = "Essential Apple Watch features at an accessible price.",
                    Specifications =
                        "44mm Retina display\nS8 SiP\nActivity tracking\nSleep tracking\n18-hour battery life\nCrash Detection\nWater resistant to 50 meters",
                    ImagePath =
                        "https://www.jbhifi.com.au/cdn/shop/files/785300-Product-0-I-638615484007911090.jpg?v=1725965174",
                    IsActive = true
                },
                new()
                {
                    ProductId = 46, Name = "Magic Keyboard for iPad Pro", SKU = "APPLE-MAGICKB-IPADPRO",
                    Category = "Accessories",
                    Brand = "Apple", Price = 499.99m,
                    Description = "Full-size keyboard with trackpad for iPad Pro productivity.",
                    Specifications =
                        "Backlit keys\nPrecision trackpad\nUSB-C pass-through charging\nFloating cantilever design\nCompatible with iPad Pro 11-inch and 12.9-inch",
                    ImagePath = "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/magic-keyboard-ipad-air-202503_GEO_AU",
                    IsActive = true
                },
                new()
                {
                    ProductId = 47, Name = "Apple Pencil 2nd Generation", SKU = "APPLE-PENCIL2",
                    Category = "Accessories",
                    Brand = "Apple", Price = 199.99m,
                    Description = "Precision drawing and writing tool for iPad with magnetic attachment.",
                    Specifications =
                        "Pixel-perfect precision\nTilt and pressure sensitivity\nMagnetic attachment and charging\nDouble-tap to change tools\nCompatible with iPad Pro, iPad Air, iPad mini",
                    ImagePath =
                        "https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/MU8F2",
                    IsActive = true
                },
                new()
                {
                    ProductId = 48, Name = "Samsung Galaxy Buds2 Pro", SKU = "SAMSUNG-BUDS2PRO",
                    Category = "Accessories",
                    Brand = "Samsung", Price = 349.99m,
                    Description = "Premium wireless earbuds with intelligent ANC and 360 Audio.",
                    Specifications =
                        "Intelligent Active Noise Cancellation\n360 Audio\nVoice Detect\nUp to 8 hours battery\nWireless charging case\nIPX7 water resistance",
                    ImagePath =
                        "https://images.samsung.com/is/image/samsung/p6pim/ca/2208/gallery/ca-galaxy-buds2-pro-r510-sm-r510nzaaxac-533189030?$684_547_PNG$",
                    IsActive = true
                },
                new()
                {
                    ProductId = 49, Name = "Sony WH-1000XM5", SKU = "SONY-WH1000XM5", Category = "Accessories",
                    Brand = "Sony", Price = 549.99m,
                    Description = "Industry-leading noise canceling with exceptional sound quality.",
                    Specifications =
                        "Industry-leading noise canceling\n30-hour battery life\nMultipoint connection\nSpeak-to-Chat technology\nTouch Sensor controls\nQuick Charge",
                    ImagePath = "https://www.sony.com.au/image/6145c1d32e6ac8e63a46c912dc33c5bb",
                    IsActive = true
                },
                new()
                {
                    ProductId = 50, Name = "Logitech MX Master 3S", SKU = "LOGITECH-MXMASTER3S",
                    Category = "Accessories",
                    Brand = "Logitech", Price = 149.99m,
                    Description = "Advanced wireless mouse with ultra-precise scrolling and customization.",
                    Specifications =
                        "MagSpeed electromagnetic scrolling\n4000 DPI sensor\n70-day battery life\nFlow cross-computer control\nUSB-C quick charging\nQuiet clicks",
                    ImagePath =
                        "https://resource.logitech.com/w_692,c_lpad,ar_4:3,q_auto,f_auto,dpr_1.0/d_transparent.gif/content/dam/logitech/en/products/mice/mx-master-3s/gallery/mx-master-3s-mouse-top-view-graphite.png",
                    IsActive = true
                }
            };

            context.Product.AddRange(products);
            context.SaveChanges();

            var inventory = new List<Inventory>
            {
                // Smartphones inventory (15 items)
                new() { InventoryId = 1, ProductId = 1, StockLevel = 15, LowStockThreshold = 5 },
                new() { InventoryId = 2, ProductId = 2, StockLevel = 20, LowStockThreshold = 5 },
                new() { InventoryId = 3, ProductId = 3, StockLevel = 25, LowStockThreshold = 8 },
                new() { InventoryId = 4, ProductId = 4, StockLevel = 12, LowStockThreshold = 3 },
                new() { InventoryId = 5, ProductId = 5, StockLevel = 18, LowStockThreshold = 5 },
                new() { InventoryId = 6, ProductId = 6, StockLevel = 22, LowStockThreshold = 6 },
                new() { InventoryId = 7, ProductId = 7, StockLevel = 8, LowStockThreshold = 3 },
                new() { InventoryId = 8, ProductId = 8, StockLevel = 14, LowStockThreshold = 4 },
                new() { InventoryId = 9, ProductId = 9, StockLevel = 10, LowStockThreshold = 3 },
                new() { InventoryId = 10, ProductId = 10, StockLevel = 6, LowStockThreshold = 2 },
                new() { InventoryId = 11, ProductId = 11, StockLevel = 16, LowStockThreshold = 5 },
                new() { InventoryId = 12, ProductId = 12, StockLevel = 7, LowStockThreshold = 2 },
                new() { InventoryId = 13, ProductId = 13, StockLevel = 5, LowStockThreshold = 2 },
                new() { InventoryId = 14, ProductId = 14, StockLevel = 9, LowStockThreshold = 3 },
                new() { InventoryId = 15, ProductId = 15, StockLevel = 11, LowStockThreshold = 3 },

                // Laptops inventory (15 items)
                new() { InventoryId = 16, ProductId = 16, StockLevel = 5, LowStockThreshold = 2 },
                new() { InventoryId = 17, ProductId = 17, StockLevel = 8, LowStockThreshold = 3 },
                new() { InventoryId = 18, ProductId = 18, StockLevel = 12, LowStockThreshold = 4 },
                new() { InventoryId = 19, ProductId = 19, StockLevel = 15, LowStockThreshold = 5 },
                new() { InventoryId = 20, ProductId = 20, StockLevel = 6, LowStockThreshold = 2 },
                new() { InventoryId = 21, ProductId = 21, StockLevel = 10, LowStockThreshold = 3 },
                new() { InventoryId = 22, ProductId = 22, StockLevel = 4, LowStockThreshold = 2 },
                new() { InventoryId = 23, ProductId = 23, StockLevel = 9, LowStockThreshold = 3 },
                new() { InventoryId = 24, ProductId = 24, StockLevel = 7, LowStockThreshold = 2 },
                new() { InventoryId = 25, ProductId = 25, StockLevel = 8, LowStockThreshold = 3 },
                new() { InventoryId = 26, ProductId = 26, StockLevel = 3, LowStockThreshold = 1 },
                new() { InventoryId = 27, ProductId = 27, StockLevel = 12, LowStockThreshold = 4 },
                new() { InventoryId = 28, ProductId = 28, StockLevel = 5, LowStockThreshold = 2 },
                new() { InventoryId = 29, ProductId = 29, StockLevel = 11, LowStockThreshold = 4 },
                new() { InventoryId = 30, ProductId = 30, StockLevel = 2, LowStockThreshold = 1 },

                // Tablets inventory (10 items)
                new() { InventoryId = 31, ProductId = 31, StockLevel = 8, LowStockThreshold = 3 },
                new() { InventoryId = 32, ProductId = 32, StockLevel = 12, LowStockThreshold = 4 },
                new() { InventoryId = 33, ProductId = 33, StockLevel = 15, LowStockThreshold = 5 },
                new() { InventoryId = 34, ProductId = 34, StockLevel = 20, LowStockThreshold = 6 },
                new() { InventoryId = 35, ProductId = 35, StockLevel = 18, LowStockThreshold = 5 },
                new() { InventoryId = 36, ProductId = 36, StockLevel = 6, LowStockThreshold = 2 },
                new() { InventoryId = 37, ProductId = 37, StockLevel = 9, LowStockThreshold = 3 },
                new() { InventoryId = 38, ProductId = 38, StockLevel = 7, LowStockThreshold = 2 },
                new() { InventoryId = 39, ProductId = 39, StockLevel = 10, LowStockThreshold = 3 },
                new() { InventoryId = 40, ProductId = 40, StockLevel = 25, LowStockThreshold = 8 },

                // Accessories inventory (10 items)
                new() { InventoryId = 41, ProductId = 41, StockLevel = 30, LowStockThreshold = 10 },
                new() { InventoryId = 42, ProductId = 42, StockLevel = 35, LowStockThreshold = 12 },
                new() { InventoryId = 43, ProductId = 43, StockLevel = 8, LowStockThreshold = 3 },
                new() { InventoryId = 44, ProductId = 44, StockLevel = 16, LowStockThreshold = 5 },
                new() { InventoryId = 45, ProductId = 45, StockLevel = 22, LowStockThreshold = 7 },
                new() { InventoryId = 46, ProductId = 46, StockLevel = 12, LowStockThreshold = 4 },
                new() { InventoryId = 47, ProductId = 47, StockLevel = 25, LowStockThreshold = 8 },
                new() { InventoryId = 48, ProductId = 48, StockLevel = 18, LowStockThreshold = 6 },
                new() { InventoryId = 49, ProductId = 49, StockLevel = 14, LowStockThreshold = 4 },
                new() { InventoryId = 50, ProductId = 50, StockLevel = 20, LowStockThreshold = 6 }
            };

            context.Inventory.AddRange(inventory);
            context.SaveChanges();
        }

        public static void SeedTestOrdersAndShipments(AppDbContext context)
        {
            if (context.Orders.Any())
                return;

            try
            {
                // Get a valid customer account
                var customerAccount = context.Accounts
                    .FirstOrDefault(a => a.Role == Role.Customer);

                if (customerAccount == null)
                {
                    Console.WriteLine("No customer account found. Creating one first.");
                    SeedCustomerAccount(context);
                    customerAccount = context.Accounts.FirstOrDefault(a => a.Role == Role.Customer);

                    if (customerAccount == null)
                    {
                        throw new Exception("Failed to create customer account.");
                    }
                }

                // Verify that this account has a corresponding Customer record
                var customer = context.Customers.FirstOrDefault(c => c.AccountId == customerAccount.Id);
                if (customer == null)
                {
                    Console.WriteLine("Customer record not found for account ID: " + customerAccount.Id);
                    Console.WriteLine("Creating a new Customer record for this account.");

                    customer = new Customer
                    {
                        AccountId = customerAccount.Id,
                        FirstName = "Default",
                        LastName = "Customer",
                        Mobile = "0400000000",
                        Email = customerAccount.Email,
                        Address = "123 Main St, Melbourne VIC 3000"
                    };
                    context.Customers.Add(customer);
                    context.SaveChanges();
                }

                Console.WriteLine(
                    $"Using customer account with ID: {customerAccount.Id} and Customer ID: {customer.Id}");

                // Create orders with valid AccountId
                var orders = new List<Order>
                {
                    new Order
                    {
                        AccountId = customerAccount.Id,
                        OrderDate = DateTime.Now.AddDays(-10),
                        LastModified = DateTime.Now.AddDays(-10),
                        Status = "Delivered",
                        OrderNotes = "Regular delivery",
                        TotalAmount = 2399.98m
                    },
                    new Order
                    {
                        AccountId = customerAccount.Id,
                        OrderDate = DateTime.Now.AddDays(-7),
                        LastModified = DateTime.Now.AddDays(-6),
                        Status = "In Transit",
                        OrderNotes = "Express shipping requested",
                        TotalAmount = 899.99m
                    },
                    new Order
                    {
                        AccountId = customerAccount.Id,
                        OrderDate = DateTime.Now.AddDays(-2),
                        LastModified = DateTime.Now.AddDays(-2),
                        Status = "Processing",
                        OrderNotes = "Gift wrapping requested",
                        TotalAmount = 1049.98m
                    },
                    new Order
                    {
                        AccountId = customerAccount.Id,
                        OrderDate = DateTime.Now.AddDays(-15),
                        LastModified = DateTime.Now.AddDays(-14),
                        Status = "Delivered",
                        OrderNotes = "Special instructions for delivery",
                        TotalAmount = 699.99m
                    }
                };

                // Add and save orders individually to ensure proper ID assignment
                foreach (var order in orders)
                {
                    // Explicitly set the Customer navigation property
                    order.Customer = customer;
                    context.Orders.Add(order);
                    try
                    {
                        context.SaveChanges();
                        Console.WriteLine($"Successfully added order with ID: {order.OrderId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding order: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }

                        throw;
                    }
                }

                // Now retrieve the saved orders to get their IDs
                var savedOrders = context.Orders.ToList();

                // Get customer info for address
                var customerName = $"{customer?.FirstName ?? "Customer"} {customer?.LastName ?? "User"}";

                // Add order items
                var orderItems = new List<OrderItem>
                {
                    new OrderItem
                        { OrderId = savedOrders[0].OrderId, ProductId = 3, Quantity = 1, UnitPrice = 1499.99m },
                    new OrderItem
                        { OrderId = savedOrders[0].OrderId, ProductId = 41, Quantity = 2, UnitPrice = 399.99m },
                    new OrderItem
                        { OrderId = savedOrders[1].OrderId, ProductId = 33, Quantity = 1, UnitPrice = 899.99m },
                    new OrderItem
                        { OrderId = savedOrders[2].OrderId, ProductId = 42, Quantity = 1, UnitPrice = 279.99m },
                    new OrderItem
                        { OrderId = savedOrders[2].OrderId, ProductId = 47, Quantity = 1, UnitPrice = 199.99m },
                    new OrderItem
                        { OrderId = savedOrders[2].OrderId, ProductId = 50, Quantity = 1, UnitPrice = 149.99m },
                    new OrderItem
                        { OrderId = savedOrders[3].OrderId, ProductId = 11, Quantity = 1, UnitPrice = 699.99m }
                };

                context.OrderItems.AddRange(orderItems);
                context.SaveChanges();

                // Create shipments 
                var shipments = new List<Shipment>
                {
                    new Shipment
                    {
                        OrderId = savedOrders[0].OrderId,
                        Status = "Delivered",
                        TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}001",
                        CarrierName = "AWE Express",
                        EstimatedDeliveryDate = DateTime.Now.AddDays(-5),
                        ShippedDate = DateTime.Now.AddDays(-8),
                        DeliveredDate = DateTime.Now.AddDays(-5),
                        DeliveryNotes = "Left at front door",
                        ShippingAddress = $"{customerName}, {customer.Address}",
                        CreatedDate = DateTime.Now.AddDays(-10),
                        LastUpdated = DateTime.Now.AddDays(-5)
                    },
                    new Shipment
                    {
                        OrderId = savedOrders[1].OrderId,
                        Status = "In Transit",
                        TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}002",
                        CarrierName = "AWE Express",
                        EstimatedDeliveryDate = DateTime.Now.AddDays(1),
                        ShippedDate = DateTime.Now.AddDays(-2),
                        DeliveryNotes = "Package in transit to local facility",
                        ShippingAddress = $"{customerName}, {customer.Address}",
                        CreatedDate = DateTime.Now.AddDays(-7),
                        LastUpdated = DateTime.Now.AddDays(-2)
                    },
                    new Shipment
                    {
                        OrderId = savedOrders[2].OrderId,
                        Status = "Processing",
                        TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}003",
                        CarrierName = "AWE Express",
                        EstimatedDeliveryDate = DateTime.Now.AddDays(5),
                        DeliveryNotes = "Preparing for shipment",
                        ShippingAddress = $"{customerName}, {customer.Address}",
                        CreatedDate = DateTime.Now.AddDays(-2),
                        LastUpdated = DateTime.Now.AddDays(-1)
                    },
                    new Shipment
                    {
                        OrderId = savedOrders[3].OrderId,
                        Status = "Delivered",
                        TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}004",
                        CarrierName = "AWE Express",
                        EstimatedDeliveryDate = DateTime.Now.AddDays(-10),
                        ShippedDate = DateTime.Now.AddDays(-14),
                        DeliveredDate = DateTime.Now.AddDays(-10),
                        DeliveryNotes = "Signed by recipient",
                        ShippingAddress = $"{customerName}, {customer.Address}",
                        CreatedDate = DateTime.Now.AddDays(-15),
                        LastUpdated = DateTime.Now.AddDays(-10)
                    }
                };

                // Add shipments individually
                foreach (var shipment in shipments)
                {
                    context.Shipments.Add(shipment);
                    context.SaveChanges();
                }

                Console.WriteLine($"Successfully seeded {savedOrders.Count} orders and {shipments.Count} shipments");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding orders and shipments: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                throw;
            }
        }
        
        public static void SeedAll(AppDbContext context)
        {
            SeedTestProductsAndInventory(context);
            SeedOwnerAccount(context);
            SeedCustomerAccount(context);
            SeedTestOrdersAndShipments(context);
        }
    }
}