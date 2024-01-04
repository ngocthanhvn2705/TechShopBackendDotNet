using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TechShopBackendDotnet.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TechShopBackendDotnet.Controllers.ProductController;

namespace TechShopBackendDotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly TechShopContext _context;
        public ProductController(TechShopContext context)
        {
            _context = context;
        }

        [HttpGet("read")]
        public ActionResult Read()
        {
            var allProductQuery = from p in _context.Products
                                  select new
                                  {
                                      id = p.Id,
                                      name = p.Name,
                                      price = p.Price,
                                      description = p.Description,
                                      category = p.Category,
                                      brand = p.Brand,
                                      pre_discount = p.PreDiscount,
                                      discount_percent = p.DiscountPercent,
                                      image = p.Image
                                  };

            var allProduct = allProductQuery.ToList();

            var result = new
            {
                product = allProduct
            };

            return Ok(result);
        }


        [HttpGet("show_color")]
        public ActionResult ShowColor(int id)
        {
            var colorQuery = from p in _context.Products
                             join pq in _context.ProductQuantities on p.Id equals pq.ProductId
                             where p.Id == id
                             select pq.Color;

            var colors = colorQuery.ToList();

            var result = new
            {
                color = colors
            };

            return Ok(result);
        }

        [HttpGet("show_by_category")]
        public ActionResult ShowByCategory(string name_category)
        {
            var productQuery = from p in _context.Products
                               join c in _context.Categories on p.Category equals c.Id
                               where c.Name == name_category
                               select new
                               {
                                   p.Id,
                                   p.Name,
                                   p.Price,
                                   p.Description,
                                   p.Image,
                                   p.Category,
                                   p.Brand,
                                   p.PreDiscount,
                                   p.DiscountPercent
                               };

            var products = productQuery.ToList();

            var result = new
            {
                product = products 
            };

            return Ok(result);
        }
        [HttpGet("show_by_category_and_brand")]
        public ActionResult ShowByCategoryAndBrand(string categoryName = null, string brand = null)
        {
            var productQuery = from p in _context.Products
                               join c in _context.Categories on p.Category equals c.Id
                               where (string.IsNullOrEmpty(categoryName) || c.Name == categoryName) &&
                                     (string.IsNullOrEmpty(brand) || p.Brand == brand)
                               select new
                               {
                                   p.Id,
                                   p.Name,
                                   p.Price,
                                   p.Description,
                                   p.Image,
                                   p.Category,
                                   p.Brand,
                                   p.PreDiscount,
                                   p.DiscountPercent
                               };

            var products = productQuery.ToList();

            var result = new
            {
                product = products
            };

            return Ok(result);
        }

        [HttpGet("search")]
        public ActionResult Search(string key = null)
        {
            var normalizedKeyword = key?.Replace(" ", "").ToLower(); 

            var productQuery = from p in _context.Products
                               join c in _context.Categories on p.Category equals c.Id
                               let normalizedName = p.Name.Replace(" ", "").ToLower()
                               let normalizedBrand = p.Brand.Replace(" ", "").ToLower()
                               let normalizedCategory = c.Name.Replace(" ", "").ToLower()
                               where (string.IsNullOrEmpty(normalizedKeyword) ||
                                      normalizedName.Contains(normalizedKeyword) ||
                                      normalizedBrand.Contains(normalizedKeyword) ||
                                      normalizedCategory.Contains(normalizedKeyword))
                               orderby c.Id == 1 ? 0 :
                                       c.Id == 2 ? 1 :
                                       c.Id == 3 ? 2 :
                                       c.Id == 4 ? 3 :
                                       4, normalizedName 
                               select new
                               {
                                   id =  p.Id,
                                   name = p.Name,
                                   price = p.Price,
                                   description = p.Description,
								   image = p.Image,
								   category = p.Category,
								   brand = p.Brand,
								   pre_discount = p.PreDiscount,
								   discount_percent = p.DiscountPercent
                               };


            var product = productQuery.ToList();

            var result = new
            {
                product = product
            };

            return Ok(result);
        }

        [HttpGet("show_all_brand_by_category")]
        public ActionResult ShowAllBrandByCategory(string categoryName)
        {
            var productQuery = (from p in _context.Products
                                join c in _context.Categories on p.Category equals c.Id
                                where c.Name == categoryName
                                select p.Brand).Distinct().ToList();

            var result = productQuery.Select(brand => new { brand = brand }).ToList();

            return Ok(new { brand = result });
        }

        [HttpGet("get_param")]
        public ActionResult GetParam(int id)
        {
            var productQuery = (from p in _context.Products
                                join c in _context.Categories on p.Category equals c.Id
                                where p.Id == id
                                select c.Name).FirstOrDefault();

            switch (productQuery)
            {
                case "phone":
                    var phoneParametersQuery = from pp in _context.ParameterPhones
                                               where pp.ProductId == id
                                               select new
                                               {
                                                   product_id = pp.ProductId,
                                                   screen = pp.Screen,
                                                   operating_system = pp.OperatingSystem,
                                                   back_camera = pp.BackCamera,
                                                   front_camera = pp.FrontCamera,
                                                   chip = pp.Chip,
                                                   ram = pp.Ram,
                                                   rom = pp.Rom,
                                                   sim = pp.Sim,
                                                   battery_charger = pp.BatteryCharger
                                               };

                    if (phoneParametersQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = phoneParametersQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                case "cable":
                    var cablePrameterQuery = from c in _context.ParameterCables
                                             where c.ProductId == id
                                             select new
                                             {
                                                 product_id = c.ProductId,
                                                 madein = c.Madein,
                                                 brandof = c.Brandof,
                                                 brand = c.Brand,
                                                 tech = c.Tech,
                                                 function = c.Function,
                                                 input = c.Input,
                                                 output = c.Output,
                                                 length = c.Length,
                                                 maximum = c.Maximum
                                             };

                    if (cablePrameterQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = cablePrameterQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                case "adapter":
                    var adapterPrameterQuery = from c in _context.ParameterAdapters
                                             where c.ProductId == id
                                             select new
                                             {
                                                 product_id = c.ProductId,
                                                 madein = c.Madein,
                                                 brandof = c.Brandof,
                                                 brand = c.Brand,
                                                 model = c.Model,
                                                 function = c.Function,
                                                 input = c.Input,
                                                 output = c.Output,
                                                 maximum = c.Maximum,
                                                 size = c.Size,
                                                 tech = c.Tech
                                             };

                    if (adapterPrameterQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = adapterPrameterQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                case "backupcharger":
                    var backupchargerPrameterQuery = from c in _context.ParameterBackupchargers
                                                   where c.ProductId == id
                                                   select new
                                                   {
                                                       product_id = c.ProductId,
                                                       madein = c.Madein,
                                                       brandof = c.Brandof,
                                                       brand = c.Brand,
                                                       efficiency = c.Efficiency,
                                                       capacity = c.Capacity,
                                                       time_full_charge = c.Timefullcharge,
                                                       input = c.Input,
                                                       output = c.Output,
                                                       core = c.Core,
                                                       tech = c.Tech,
                                                       size = c.Size,
                                                       weight = c.Weight
                                                       
                                                   };

                    if (backupchargerPrameterQuery.Any())
                    {
                        var responseData = new
                        {
                            status = 200,
                            data = backupchargerPrameterQuery.ToList()
                        };

                        return Ok(responseData);
                    }
                    else
                    {
                        return NotFound();
                    }

                default:
                    return NotFound(); 
            }

        }

		[HttpGet("/api/product/sort/adapter/showProp")]
		public IActionResult ShowPropAdapter(string prop)
		{
			List<string> data = new List<string>();

			switch (prop)
			{
				case "brand":
					var brandQuery = (from p in _context.Products
										where p.Category == 2
										select p.Brand).Distinct().ToList();
					data.AddRange(brandQuery);
					break;
				case "output":
					data.Add("Type C");
					data.Add("USB");
					data.Add("Không dây");
					break;
				case "charger":
					data.Add("Dưới 15 W");
					data.Add("Từ 15-25 W");
					data.Add("Từ 26-60 W");
					data.Add("Trên 60 W");
					break;
				case "numberport":
					data.Add("1 cổng");
					data.Add("2 cổng");
					data.Add("3 cổng");
					data.Add("4 cổng");
					break;
				default:
					return Ok(new { status = 404, message = "NOT FOUND" });
			}

			return Ok(new { status = 200, data });
		}

        [HttpPost("/api/product/sort/adapter/sort")]
		public IActionResult SortAdapter(SortAdapterModel sortAdapterModel)
		{
			string[] brand = SplitStringToArray(sortAdapterModel.Brand);
			string[] price = SplitStringToArray(sortAdapterModel.Price);
			string[] numberport = SplitStringToArray(sortAdapterModel.Numberport);
			string[] output = SplitStringToArray(sortAdapterModel.Output);
			string[] charger = SplitStringToArray(sortAdapterModel.Charger);

			string query = @"SELECT p.* FROM product p INNER JOIN parameter_adapter pa ON p.ID = pa.PRODUCT_ID WHERE ";

			if (price != null && price.Length == 2 && decimal.TryParse(price[0], out decimal price1) && decimal.TryParse(price[1], out decimal price2))
			{
				query += $"p.PRICE BETWEEN {price1} AND {price2} ";
			}

			List<string> conditions = new List<string>();

			if (brand != null)
			{
				if (brand.Length > 0)
				{
					string inValues = "'" + string.Join("','", brand) + "'";
					conditions.Add($"p.BRAND IN ({inValues})");
				}
				else
				{
					conditions.Add($"p.BRAND = '{brand}'");
				}
			}

			if (output != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in output)
				{
					string newValue = value == "Type C" ? value.Insert(4, "%") : value;
					likeConditions.Add($"pa.OUTPUT LIKE '%{newValue}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}

			if (numberport != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in numberport)
				{
					if (value == "1 cổng")
					{
						likeConditions.Add("p.NAME NOT LIKE '%cổng%'");
					}
					likeConditions.Add($"p.NAME LIKE '%{value}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}

			if (conditions.Count > 0)
			{
				query += " AND " + string.Join(" AND ", conditions);
			}
			else
			{
				query += " AND 1";
			}

			string checkCharger = "CAST(SUBSTRING_INDEX(MAXIMUM, ' ', 1) AS DECIMAL(10))";
			if (charger != null)
			{
				if (charger != null && charger.Length > 1)
				{
					List<string> chargerConditions = new List<string>();
					foreach (var item in charger)
					{
						switch (item)
						{
							case "15":
								chargerConditions.Add($"{checkCharger} < 15");
								break;
							case "1525":
								chargerConditions.Add($"{checkCharger} BETWEEN 15 AND 25");
								break;
							case "2660":
								chargerConditions.Add($"{checkCharger} BETWEEN 26 AND 60");
								break;
							case "60":
								chargerConditions.Add($"{checkCharger} > 60");
								break;
						}
					}
					if (chargerConditions.Count > 0)
					{
						query += " AND (" + string.Join(" OR ", chargerConditions) + ")";
					}
				}
				else
				{
					switch (charger[0])
					{
						case "15":
							query += $" AND {checkCharger} < 15 ";
							break;
						case "1525":
							query += $" AND {checkCharger} BETWEEN 15 AND 25 ";
							break;
						case "2660":
							query += $" AND {checkCharger} BETWEEN 26 AND 60 ";
							break;
						case "60":
							query += $" AND {checkCharger} > 60 ";
							break;
					}
				}
			}
	
			var products = _context.Products
						.FromSqlRaw(query)
						.Select(p => new
						{
							id = p.Id,
							name = p.Name,
							price = p.Price,
							description = p.Description,
							category = p.Category,
							brand = p.Brand,
							pre_discount = p.PreDiscount,
							discount_percent = p.DiscountPercent,
							image = p.Image
						})
						.ToList();


			return Ok(new {product = products });

		}


		[HttpGet("/api/product/sort/backupcharger/showProp")]
		public ActionResult showPropBackupcharger(string prop)
		{
			List<string> data = new List<string>();

			switch (prop)
			{
				case "brand":
					var brandQuery = (from p in _context.Products
									  where p.Category == 4
									  select p.Brand).Distinct().ToList();
					data.AddRange(brandQuery);
					break;
				case "capacity":
					data.Add("Dưới 10000 mAh");
					data.Add("10000 mAh");
					data.Add("15000 mAh");
					data.Add("20000 mAh");
                    break;
				case "input":
					data.Add("Type C");
					data.Add("Micro USB");
					data.Add("Lightning");
                    break;
				case "output":
					data.Add("Type C");
					data.Add("USB");
					data.Add("Không dây");
					break;
				case "charger":
					data.Add("Dưới 15 W");
					data.Add("Từ 15-25 W");
					data.Add("Từ 26-60 W");
					data.Add("Trên 60 W");
					break;
				default:
					return Ok( new { status = 404, message = "NOT FOUND" });
			}

			return Ok(new { status = 200, data });
		}

		[HttpPost("/api/product/sort/backupcharger/sort")]
		public IActionResult SortBackupcharger(SortBackupchargerModel sortBackupchargerModel)
		{
			string[] brand = SplitStringToArray(sortBackupchargerModel.Brand);
			string[] price = SplitStringToArray(sortBackupchargerModel.Price);
			string[] capacity = SplitStringToArray(sortBackupchargerModel.Capacity);
			string[] input = SplitStringToArray(sortBackupchargerModel.Input);
			string[] output = SplitStringToArray(sortBackupchargerModel.Output);
			string[] charger = SplitStringToArray(sortBackupchargerModel.Charger);

			string query = @"SELECT p.* FROM product p INNER JOIN parameter_backupcharger pb ON p.ID = pb.PRODUCT_ID WHERE ";

			if (price != null && price.Length == 2 && decimal.TryParse(price[0], out decimal price1) && decimal.TryParse(price[1], out decimal price2))
			{
				query += $"p.PRICE BETWEEN {price1} AND {price2} ";
			}

			List<string> conditions = new List<string>();

			if (brand != null)
			{
				if (brand.Length > 0)
				{
					string inValues = "'" + string.Join("','", brand) + "'";
					conditions.Add($"p.BRAND IN ({inValues})");
				}
				else
				{
					conditions.Add($"p.BRAND = '{brand}'");
				}
			}

			if (input != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in input)
				{
					string newValue = value == "Type C" ? value.Insert(4, "%") : value;
					likeConditions.Add($"pb.INPUT LIKE '%{newValue}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}

			if (output != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in output)
				{
					string newValue = value == "Type C" ? value.Insert(4, "%") : value;
					likeConditions.Add($"pb.OUTPUT LIKE '%{newValue}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}


			if (conditions.Count > 0)
			{
				query += " AND " + string.Join(" AND ", conditions);
			}
			else
			{
				query += " AND 1";
			}

			string checkCapacity = "CAST(SUBSTRING_INDEX(CAPACITY, ' ', 1) AS DECIMAL(10))";
			if (capacity != null)
			{
				if (capacity != null && capacity.Length > 1)
				{
					List<string> capacityConditions = new List<string>();
					foreach (var item in capacity)
					{
						switch (item)
						{
							case "Dưới 10000 mAh":
								capacityConditions.Add($"{checkCapacity} < 10000 ");
								break;
							case "10000 mAh":
								capacityConditions.Add($"{checkCapacity} = 10000 ");
								break;
							case "15000 mAh":
								capacityConditions.Add($"{checkCapacity} = 15000 ");
								break;
							case "20000 mAh":
								capacityConditions.Add($"{checkCapacity} = 20000 ");
								break;
						}
					}
					if (capacityConditions.Count > 0)
					{
						query += " AND (" + string.Join(" OR ", capacityConditions) + ")";
					}
				}
				else
				{
					switch (capacity[0])
					{
						case "Dưới 10000 mAh":
							query += $" AND {checkCapacity} < 10000 ";
							break;
						case "10000 mAh":
							query += $" AND {checkCapacity} = 10000 ";
							break;
						case "15000 mAh":
							query += $" AND {checkCapacity} = 15000 ";
							break;
						case "20000 mAh":
							query += $" AND {checkCapacity} = 20000 ";
							break;
					}
				}
			}

			string checkCharger = "CAST(SUBSTRING_INDEX(SUBSTRING(SUBSTRING_INDEX(NAME, 'W', 1), -5), ' ', -1)AS DECIMAL(10,1))";
			if (charger != null)
			{
				if (charger != null && charger.Length > 1)
				{
					List<string> chargerConditions = new List<string>();
					foreach (var item in charger)
					{
						switch (item)
						{
							case "15":
								chargerConditions.Add($"{checkCharger} < 15");
								break;
							case "1525":
								chargerConditions.Add($"{checkCharger} BETWEEN 15 AND 25");
								break;
							case "2660":
								chargerConditions.Add($"{checkCharger} BETWEEN 26 AND 60");
								break;
							case "60":
								chargerConditions.Add($"{checkCharger} > 60");
								break;
						}
					}
					if (chargerConditions.Count > 0)
					{
						query += " AND (" + string.Join(" OR ", chargerConditions) + ")";
					}
				}
				else
				{
					switch (charger[0])
					{
						case "15":
							query += $" AND {checkCharger} < 15 ";
							break;
						case "1525":
							query += $" AND {checkCharger} BETWEEN 15 AND 25 ";
							break;
						case "2660":
							query += $" AND {checkCharger} BETWEEN 26 AND 60 ";
							break;
						case "60":
							query += $" AND {checkCharger} > 60 ";
							break;
					}
				}
			}

			var products = _context.Products
						.FromSqlRaw(query)
						.Select(p => new
						{
							id = p.Id,
							name = p.Name,
							price = p.Price,
							description = p.Description,
							category = p.Category,
							brand = p.Brand,
							pre_discount = p.PreDiscount,
							discount_percent = p.DiscountPercent,
							image = p.Image
						})
						.ToList();

			return Ok( new { product = products } );
		}



		[HttpGet("/api/product/sort/cable/showProp")]
		public ActionResult showPropCable(string prop)
		{
			List<string> data = new List<string>();

			switch (prop)
			{
				case "brand":
					var brandQuery = (from p in _context.Products
									  where p.Category == 3
									  select p.Brand).Distinct().ToList();
					data.AddRange(brandQuery);
					break;
				case "capacity":
					data.Add("Dưới 10000 mAh");
					data.Add("10000 mAh");
					data.Add("15000 mAh");
					data.Add("20000 mAh");
					break;
				case "input":
					data.Add("Type C");
					data.Add("Type A");
					break;
				case "output":
					data.Add("Lightning");
					data.Add("Type C");
					break;
				case "length":
					data.Add("Dưới 1 m");
					data.Add("Từ 1 - 2 m");
					break;
				case "charger":
					data.Add("Dưới 15 W");
					data.Add("Từ 15-25 W");
					data.Add("Từ 26-60 W");
					data.Add("Trên 60 W");
					break;
				default:
					return Ok(new { status = 404, message = "NOT FOUND" });
			}

			return Ok(new { status = 200, data });
		}

		[HttpPost("/api/product/sort/cable/sort")]
		public IActionResult SortCable(SortCableModel sortCableModel)
		{
			string[] brand = SplitStringToArray(sortCableModel.Brand);
			string[] price = SplitStringToArray(sortCableModel.Price);
			string[] input = SplitStringToArray(sortCableModel.Input);
			string[] output = SplitStringToArray(sortCableModel.Output);
			string[] length = SplitStringToArray(sortCableModel.Length);
			string[] charger = SplitStringToArray(sortCableModel.Charger);

			string query = @"SELECT p.* FROM product p INNER JOIN parameter_cable pc ON p.ID = pc.PRODUCT_ID WHERE ";

			if (price != null && price.Length == 2 && decimal.TryParse(price[0], out decimal price1) && decimal.TryParse(price[1], out decimal price2))
			{
				query += $"p.PRICE BETWEEN {price1} AND {price2} ";
			}

			List<string> conditions = new List<string>();

			if (brand != null)
			{
				if (brand.Length > 0)
				{
					string inValues = "'" + string.Join("','", brand) + "'";
					conditions.Add($"p.BRAND IN ({inValues})");
				}
				else
				{
					conditions.Add($"p.BRAND = '{brand}'");
				}
			}

			if (input != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in input)
				{
					string newValue;
					if (value == "Type C" || value == "Type A")
					{
						var value1 = value.Remove(4, 1);
						newValue = value1.Insert(4, "%");
					}
					else
					{
						newValue = value;
					}

					likeConditions.Add($"pc.INPUT LIKE '%{newValue}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}

			if (output != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in output)
				{
					string newValue;
					if (value == "Type C" || value == "Type A")
					{
						var value1 = value.Remove(4, 1);
						newValue = value1.Insert(4, "%");
					}
					else
					{
						newValue = value;
					}
					likeConditions.Add($"pc.OUTPUT LIKE '%{newValue}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}


			if (conditions.Count > 0)
			{
				query += " AND " + string.Join(" AND ", conditions);
			}
			else
			{
				query += " AND 1";
			}

			string checkLength = "CAST(SUBSTRING_INDEX(LENGTH, ' ', 1) AS DECIMAL(10,1))";
			if (length != null && length.Length > 1)
			{
				List<string> capacityConditions = new List<string>();
				foreach (var item in length)
				{
					switch (item)
					{
						case "1":
							capacityConditions.Add($"{checkLength} < 1 ");
							break;
						case "12":
							capacityConditions.Add($"{checkLength} BETWEEN 1 AND 2 ");
							break;
					}
				}
				if (capacityConditions.Count > 0)
				{
					query += " AND (" + string.Join(" OR ", capacityConditions) + ")";
				}
			}
			else
			{
				switch (length[0])
				{
					case "1":
						query += $" AND {checkLength} < 1 ";
						break;
					case "12":
						query += $" AND {checkLength} BETWEEN 1 AND 2 ";
						break;
				}
			}

			string checkCharger = "CAST(SUBSTRING_INDEX(MAXIMUM, ' W', 1) AS DECIMAL(10,1))";
			if (charger != null)
			{
				if (charger != null && charger.Length > 1)
				{
					List<string> chargerConditions = new List<string>();
					foreach (var item in charger)
					{
						switch (item)
						{
							case "15":
								chargerConditions.Add($"{checkCharger} < 15");
								break;
							case "1525":
								chargerConditions.Add($"{checkCharger} BETWEEN 15 AND 25");
								break;
							case "2660":
								chargerConditions.Add($"{checkCharger} BETWEEN 26 AND 60");
								break;
							case "60":
								chargerConditions.Add($"{checkCharger} > 60");
								break;
						}
					}
					if (chargerConditions.Count > 0)
					{
						query += " AND (" + string.Join(" OR ", chargerConditions) + ")";
					}
				}
				else
				{
					switch (charger[0])
					{
						case "15":
							query += $" AND {checkCharger} < 15 ";
							break;
						case "1525":
							query += $" AND {checkCharger} BETWEEN 15 AND 25 ";
							break;
						case "2660":
							query += $" AND {checkCharger} BETWEEN 26 AND 60 ";
							break;
						case "60":
							query += $" AND {checkCharger} > 60 ";
							break;
					}
				}
			}
			var products = _context.Products
						.FromSqlRaw(query)
						.Select(p => new
						{
							id = p.Id,
							name = p.Name,
							price = p.Price,
							description = p.Description,
							category = p.Category,
							brand = p.Brand,
							pre_discount = p.PreDiscount,
							discount_percent = p.DiscountPercent,
							image = p.Image
						})
						.ToList();

			return Ok(new { product = products });
		}




		[HttpGet("/api/product/sort/phone/showProp")]
		public ActionResult showPropPhone(string prop)
		{
			List<string> data = new List<string>();
			var parameterPhones = _context.ParameterPhones.ToList();
			switch (prop)
			{
				case "brand":
					var brandQuery = (from p in _context.Products
									  where p.Category == 1
									  select p.Brand).Distinct().ToList();
					data.AddRange(brandQuery);
					break;
				case "os":
					data.Add("Android");
					data.Add("iOS");
					break;
				case "ram":
					var ramQuery = (from pb in parameterPhones
									orderby pb.Ram.Contains("GB") ? Convert.ToUInt32(pb.Ram.Split(' ')[0]) : 0
									select pb.Ram).Distinct().ToList();
					data.AddRange(ramQuery);
					break;
				case "rom":
					var romQuery = (from pb in parameterPhones
									orderby pb.Rom.Contains("TB")
										? Convert.ToUInt32(pb.Rom.Split(' ')[0]) * 1024
										: (pb.Rom.Contains("GB")
											? Convert.ToUInt32(pb.Rom.Split(' ')[0])
											: 0)
									select pb.Rom).Distinct().ToList();
					data.AddRange(romQuery);
					break;
				case "charger":
					data.Add("Dưới 15 W");
					data.Add("Từ 15-25 W");
					data.Add("Từ 26-60 W");
					data.Add("Trên 60 W");
					break;
				default:
					return Ok(new { status = 404, message = "NOT FOUND" });
			}

			return Ok(new { status = 200, data });
		}

		[HttpPost("/api/product/sort/phone/sort")]
		public IActionResult SortPhone(SortPhoneModel sortPhoneModel)
		{
			string[] brand = SplitStringToArray(sortPhoneModel.Brand);
			string[] price = SplitStringToArray(sortPhoneModel.Price);
			string[] os = SplitStringToArray(sortPhoneModel.Os);
			string[] ram = SplitStringToArray(sortPhoneModel.Ram);
			string[] rom = SplitStringToArray(sortPhoneModel.Rom);
			string[] charger = SplitStringToArray(sortPhoneModel.Charger);

			string query = @"SELECT p.* FROM product p INNER JOIN parameter_phone pp ON p.ID = pp.PRODUCT_ID WHERE ";

			if (price != null && price.Length == 2 && decimal.TryParse(price[0], out decimal price1) && decimal.TryParse(price[1], out decimal price2))
			{
				query += $"p.PRICE BETWEEN {price1} AND {price2} ";
			}

			List<string> conditions = new List<string>();

			if (brand != null)
			{
				if (brand.Length > 0)
				{
					string inValues = "'" + string.Join("','", brand) + "'";
					conditions.Add($"p.BRAND IN ({inValues})");
				}
				else
				{
					conditions.Add($"p.BRAND = '{brand}'");
				}
			}

			if (os != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in os)
				{
					likeConditions.Add($"pp.OPERATING_SYSTEM LIKE '%{value}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}

			if (ram != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in ram)
				{
					likeConditions.Add($"pp.RAM LIKE '%{value}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}

			if (rom != null)
			{
				List<string> likeConditions = new List<string>();
				foreach (var value in rom)
				{
					likeConditions.Add($"pp.ROM LIKE '%{value}%'");
				}
				if (likeConditions.Count > 0)
				{
					conditions.Add($"({string.Join(" OR ", likeConditions)})");
				}
			}


			if (conditions.Count > 0)
			{
				query += " AND " + string.Join(" AND ", conditions);
			}
			else
			{
				query += " AND 1";
			}



			string checkCharger = "CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(BATTERY_CHARGER, 'mAh ', -1), ' ', 1) AS UNSIGNED) ";
			if (charger != null)
			{
				if (charger != null && charger.Length > 1)
				{
					List<string> chargerConditions = new List<string>();
					foreach (var item in charger)
					{
						switch (item)
						{
							case "15":
								chargerConditions.Add($"{checkCharger} < 15");
								break;
							case "1525":
								chargerConditions.Add($"{checkCharger} BETWEEN 15 AND 25");
								break;
							case "2660":
								chargerConditions.Add($"{checkCharger} BETWEEN 26 AND 60");
								break;
							case "60":
								chargerConditions.Add($"{checkCharger} > 60");
								break;
						}
					}
					if (chargerConditions.Count > 0)
					{
						query += " AND (" + string.Join(" OR ", chargerConditions) + ")";
					}
				}
				else
				{
					switch (charger[0])
					{
						case "15":
							query += $" AND {checkCharger} < 15 ";
							break;
						case "1525":
							query += $" AND {checkCharger} BETWEEN 15 AND 25 ";
							break;
						case "2660":
							query += $" AND {checkCharger} BETWEEN 26 AND 60 ";
							break;
						case "60":
							query += $" AND {checkCharger} > 60 ";
							break;
					}
				}
			}

			var products = _context.Products
						.FromSqlRaw(query)
						.Select(p => new
						{
							id = p.Id,
							name = p.Name,
							price = p.Price,
							description = p.Description,
							category = p.Category,
							brand = p.Brand,
							pre_discount = p.PreDiscount,
							discount_percent = p.DiscountPercent,
							image = p.Image
						})
						.ToList();

			return Ok(new { product = products });
		}

		public class SortAdapterModel
		{
            public string ?Brand { get; set; }
			public string ?Price { get; set; }
			public string ?Numberport { get; set; }
			public string ?Output { get; set; }
			public string ?Charger { get; set; }

		}
		public class SortBackupchargerModel
		{
			public string ?Brand { get; set; }
			public string ?Price { get; set; }
			public string ?Capacity { get; set; }
			public string ?Input { get; set; }
			public string ?Output { get; set; }
			public string ?Charger { get; set; }

		}
		public class SortCableModel
		{
			public string ?Brand { get; set; }
			public string ?Price { get; set; }
			public string ?Input { get; set; }
			public string ?Output { get; set; }
			public string ?Length { get; set; }
			public string ?Charger { get; set; }

		}
		public class SortPhoneModel
		{
			public string ?Brand { get; set; }
			public string ?Os { get; set; }
			public string ?Price { get; set; }
			public string ?Ram { get; set; }
			public string ?Rom { get; set; }
			public string ?Charger { get; set; }

		}
		private string[] SplitStringToArray(string input)
		{
			if (!string.IsNullOrEmpty(input) && input.Length > 0)
			{
				if (!input.Contains("-"))
				{
					return new string[] { input };
				}
				else
				{
					return input.Split('-');
				}
			}

			return null;
		}

	}
}
