using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project.Models;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Identity.Client;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        ShopContext context = new ShopContext();
        public IActionResult TrangChu(int? cid)
        {
            var query = context.Products.Include(o => o.Category).AsQueryable();

            if (cid != null)
            {
                query = query.Where(p => p.CategoryId == cid && p.Status == true && p.Quantity > 0);
            }

            var pro = query.OrderByDescending(x => x.Id).ToList();
            ViewBag.Ci = cid;

            string id = HttpContext.Session.GetString("id");
            string name = HttpContext.Session.GetString("name");
            string role = HttpContext.Session.GetString("role");
            string sum = HttpContext.Session.GetString("slsp");
            if (sum == null)
            {
                sum = "0";
            }

            ViewBag.id = id;
            ViewBag.name = name;
            ViewBag.sum = sum;
            ViewBag.role = role;
            ViewBag.cid = cid;

            if (pro.Count == 0)
            {
                ViewBag.Message = "Không có sản phẩm trong danh mục này.";
            }
            else
            {
                ViewBag.listsp = pro;
            }

            return View();
        }


        public IActionResult FormLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login()
        {
            string name = HttpContext.Request.Form["username"];
            string pass = HttpContext.Request.Form["pass"];

            Account acc = context.Accounts.Where(x => x.Username == name && x.Password == pass).SingleOrDefault();
            if (acc != null)
            {
                HttpContext.Session.SetString("id", acc.Id.ToString());
                HttpContext.Session.SetString("name", acc.Name.ToString());
                HttpContext.Session.SetString("role", acc.Role.ToString());
                string role = HttpContext.Session.GetString("role");
                if (role == "2")
                {
                    return RedirectToAction(nameof(TrangChu));
                }
                else
                {
                    return RedirectToAction(nameof(Admin));
                }
            }
            else
            {
                return RedirectToAction(nameof(FormLogin));
            }


        }

        public IActionResult Thoat()
        {
            HttpContext.Session.Clear();
            return Redirect("/Home/Trangchu");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PRegister()
        {
            string user = HttpContext.Request.Form["username"];
            string pass = HttpContext.Request.Form["pass"];
            string pass1 = HttpContext.Request.Form["pass1"];
            string name = HttpContext.Request.Form["name"];
            if (pass == pass1)
            {
                {
                    Account acc = context.Accounts.Where(x => x.Username == user).SingleOrDefault();
                    if (acc != null)
                    {
                        //Tao session
                        HttpContext.Session.SetString("err", "Tai khoan da ton tai");
                        return RedirectToAction(nameof(Register));
                    }
                    else
                    {
                        Account a = new Account();
                        a.Username = user;
                        a.Password = pass;
                        a.Name = name;
                        a.Role = 2;
                        context.Accounts.Add(a);
                        context.SaveChanges();
                        HttpContext.Session.SetString("mess", "Dang ki thanh cong");
                        return RedirectToAction(nameof(FormLogin));
                    }
                }
            }
            else
            {
                HttpContext.Session.SetString("err", "Xac nhan mat khau khong thanh cong");
                return RedirectToAction(nameof(Register));
            }

        }
        [HttpPost]
        public IActionResult Search()
        {
            string key = HttpContext.Request.Form["search"];
            List<Product> products = new List<Product>();
            if (products != null)
            {
                products = context.Products.Where(x => x.Status == true && x.ProductName.ToLower().Contains(key.ToLower())).ToList();

            }
            string id = HttpContext.Session.GetString("id");
            string role = HttpContext.Session.GetString("role");
            string name = HttpContext.Session.GetString("name");
            string sum = HttpContext.Session.GetString("slsp");
            if (sum == null)
            {
                sum = "0";
            }
            ViewBag.listsp = products;
            ViewBag.id = id;
            ViewBag.role = role;
            ViewBag.name = name;
            ViewBag.sum = sum;
            return View(products);
        }

        public IActionResult Cart()
        {
            string json = HttpContext.Session.GetString("cart");
            if (json != null)
            {
                IDictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(json);
                int? sum = 0;
                foreach (KeyValuePair<int, int> item in cart)
                {
                    Product p = context.Products.Where(x => x.Id == item.Key).SingleOrDefault();
                    sum += Convert.ToInt32(p.Price * item.Value);
                }
                ViewBag.sum = sum;
                ViewBag.cart = cart;
            }
            string id = HttpContext.Session.GetString("id");
            string name = HttpContext.Session.GetString("name");
            ViewBag.id = id;
            ViewBag.name = name;
            return View();
        }

        public IActionResult Addcart(int id)
        {
            string cart1 = HttpContext.Session.GetString("cart");
            IDictionary<int, int> cart = new Dictionary<int, int>();

            if (cart1 != null)
            {
                cart = System.Text.Json.JsonSerializer.Deserialize<IDictionary<int, int>>(cart1);
                if (cart.ContainsKey(id))
                {
                    cart[id] += 1;
                }
                else
                {
                    cart.Add(id, 1);
                }
            }
            else
            {
                cart.Add(id, 1);
            }
            int sum = 0;
            foreach (KeyValuePair<int, int> item in cart)
            {
                sum += Convert.ToInt32(item.Value);
            }


            HttpContext.Session.SetString("slsp", sum.ToString());
            string json = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("cart", json);
            return Redirect("/Home");
        }
        public IActionResult Increase(int id)
        {
            string cart1 = HttpContext.Session.GetString("cart");

            IDictionary<int, int> cart = new Dictionary<int, int>();
            if (cart1 != null)
            { // vi sp 1 co sl1 la cart1
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cart1);
                if (cart.ContainsKey(id))
                {
                    cart[id] += 1;
                }

            }
            // la truong hop add lan dau tien


            int sum = 0;
            foreach (KeyValuePair<int, int> item in cart)
            {
                sum += item.Value;
            }
            HttpContext.Session.SetString("slsp", sum.ToString());
            // chuyen cart ve string de luu trong session
            string jsonData = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("cart", jsonData);

            return RedirectToAction("Cart");
        }

        public IActionResult Decrease(int id)
        {
            string cart1 = HttpContext.Session.GetString("cart");

            IDictionary<int, int> cart = new Dictionary<int, int>();
            if (cart1 != null)
            { // vi sp 1 co sl1 la cart1
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cart1);
                if (cart.ContainsKey(id))
                {
                    if (cart[id] == 1)
                    {
                        cart.Remove(id);

                    }
                    else
                    {
                        cart[id] -= 1;
                    }
                }

            }
            // la truong hop add lan dau tien


            int sum = 0;
            foreach (KeyValuePair<int, int> item in cart)
            {
                sum += item.Value;
            }
            HttpContext.Session.SetString("slsp", sum.ToString());
            // chuyen cart ve string de luu trong session
            string jsonData = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("cart", jsonData);

            return RedirectToAction("Cart");
        }
    

        [HttpPost]
        public IActionResult AddOrder(int sum)
        {
            string address = HttpContext.Request.Form["address"];
            string id = HttpContext.Session.GetString("id");
            string pid = HttpContext.Session.GetString("cart");
            List<Order> orders = context.Orders.ToList();

            IDictionary<int, int> cid = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(pid);

            foreach (KeyValuePair<int, int> i in cid)
            {
                int aid = Convert.ToInt32(context.Products.FirstOrDefault(x => x.Id == i.Key)?.Id ?? 0);
                Order order = new Order();
                order.ProductId = aid;
                order.AccountId = int.Parse(id);
                order.Address = address;
                order.Quantity = i.Value;
                order.TotalPrice = sum;
                order.Date = System.DateTime.Now;
                Product product = context.Products.FirstOrDefault(x => x.Id == i.Key);
                product.Quantity -= i.Value;
                context.Products.Update(product);
                context.Orders.Add(order);
                context.SaveChanges();
            }

            
            IDictionary<int, int> cart = new Dictionary<int, int>();
            cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(pid);

            // sau khi thanh toan xong can xoa het cac sp trong cart
            cart = new Dictionary<int, int>();
            string json = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("cart", json);

            // set so luong cua gio hang
            int sum1 = 0;
            foreach (KeyValuePair<int, int> item in cart)
            {
                sum1 += item.Value;
            }
            HttpContext.Session.SetString("slsp", sum1.ToString());
            return RedirectToAction("TrangChu");
        }

        public IActionResult Admin()
        {
            string id = HttpContext.Session.GetString("id");
            string name = HttpContext.Session.GetString("name");
            string role = HttpContext.Session.GetString("role");
            ViewBag.id = id;
            ViewBag.name = name;
            ViewBag.role = role;
            List<Product> products = new List<Product>();
            products = context.Products.ToList();
            ViewBag.list = products;
            ViewBag.lsp = context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct()
        {
            string id = HttpContext.Session.GetString("id");
            string name = HttpContext.Request.Form["product-name"];
            string image = HttpContext.Request.Form["image"];
            string price = HttpContext.Request.Form["price"];
            string quantity = HttpContext.Request.Form["quantity"];
            string cate = HttpContext.Request.Form["category"];
            Product product = new Product();
            product.AccountId = int.Parse(id);
            product.ProductName = name;
            product.Image = image;
            product.Price = double.Parse(price);
            product.Quantity = int.Parse(quantity);
            product.CategoryId = int.Parse(cate);
            product.Status = true;
            context.Products.Add(product);
            context.SaveChanges();
            return RedirectToAction(nameof(Admin));
        }

        public IActionResult OnOff(int id, int status)
        {
            Product product = context.Products.Where(x => x.Id == id).SingleOrDefault();
            if (status == 0)
            {
                product.Status = false;
            }
            else
            {
                product.Status = true;
            }
            context.Products.Update(product);
            context.SaveChanges();
            return RedirectToAction(nameof(Admin));
        }
        [HttpPost]
        public IActionResult Editsp(int id)
        {
            string cid = HttpContext.Session.GetString("id");
            string name = HttpContext.Request.Form["ProductName"];
            string image = HttpContext.Request.Form["Image"];
            string price = HttpContext.Request.Form["Price"];
            string quantity = HttpContext.Request.Form["Quantity"];
            string cate = HttpContext.Request.Form["CategoryId"];
            Product product = context.Products.FirstOrDefault(x => x.Id == id);
            if (product != null)
            {
                product.AccountId = int.Parse(cid);
                product.ProductName = name;
                product.Image = image;
                product.Price = int.Parse(price);
                product.Quantity = int.Parse(quantity);
                product.CategoryId = int.Parse(cate);
                context.Products.Update(product);
                context.SaveChanges();
            }
            return RedirectToAction(nameof(Admin));
        }

        public IActionResult Edit(int id)
        {
            string cid = HttpContext.Session.GetString("id");
            string role = HttpContext.Session.GetString("role");
            Product product = context.Products.Where(p => p.Id == id).SingleOrDefault();
            if (product != null)
            {
                ViewBag.product = product;
            }
            ViewBag.lsp = context.Categories.ToList();
            ViewBag.id = cid;
            ViewBag.role = role;
            ViewBag.name = HttpContext.Session.GetString("name");
            return View();
        }

        public IActionResult Order()
        {
            string id = HttpContext.Session.GetString("id");
            List<Order> orders = context.Orders.ToList();
            foreach (Order item in orders)
            {
                item.Product = context.Products.Where(x => x.Id == item.ProductId).Single();
                item.Account = context.Accounts.Where(a => a.Id == item.AccountId).Single();
            }
            ViewBag.list = orders.OrderByDescending(x => x.Id);
            ViewBag.id = id;
            ViewBag.name = HttpContext.Session.GetString("name");
            return View(orders);
        }

        public IActionResult Account()
        {
            List<Account> accounts = context.Accounts.ToList();
            ViewBag.list = accounts;
            ViewBag.id = HttpContext.Session.GetString("id");
            ViewBag.name = HttpContext.Session.GetString("name");
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id != null)
            {
                Account account = context.Accounts.FirstOrDefault(x => x.Id == id);
                List<Order> orders = context.Orders.Where(x => x.AccountId == id).ToList();
                foreach (Order item in orders)
                {
                    context.Remove(item);
                    context.SaveChanges();
                }

                if (account.Role == 2)
                {
                    context.Accounts.Remove(account);
                    context.SaveChanges();
                }

            }
            ViewBag.message = "Xóa thành công";
            return RedirectToAction(nameof(Account));
        }

        public IActionResult Transaction(int? id)
        {
            List<Order> orders = context.Orders.Where(x => x.AccountId == id).ToList();
            foreach (Order order in orders)
            {
                order.Product = context.Products.FirstOrDefault(x => x.Id == order.ProductId);
                order.Account = context.Accounts.FirstOrDefault(x => x.Id == order.AccountId);
            }
            ViewBag.list = orders;
            ViewBag.id = HttpContext.Session.GetString("id");
            ViewBag.name = HttpContext.Session.GetString("name");
            return View();
        }
    }
}