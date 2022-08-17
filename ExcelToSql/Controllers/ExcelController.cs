using ExcelToSql.Data;
using ExcelToSql.Data.Entites;
using ExcelToSql.DTOs;
using ExcelToSql.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelToSql.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly AppDbContext _con;
        private readonly IEmailService _eservice;
        private readonly IWebHostEnvironment _env;

        public ExcelController(AppDbContext con, IWebHostEnvironment env, IEmailService eservice)
        {
            _con = con;
            _env = env;
            _eservice = eservice;
        }

        /// <summary>
        /// Upload File 
        /// </summary>
        /// <param name="file"></param>
        /// sample request 
        /// Post /api/Excel
        /// <parametrs>File</parametrs>>
        /// 
        [HttpPost]
        public async Task<IActionResult> UploadData(IFormFile file)
        {

            if (file == null) return StatusCode(1, "File can't be null");
            var e = Path.GetExtension(file.FileName);
            if (e != ".xls" && e != ".xlsx") return StatusCode(2, "only .xls or .xlsx format");
            if (file.Length / (1024 * 1024) > 5) return StatusCode(3, "Oversize");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowcount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowcount; row++)
                    {
                        Example example = new Example();

                        example.Segment = worksheet.Cells[row, 1].Value.ToString().Trim();
                        example.Country = worksheet.Cells[row, 2].Value.ToString().Trim();
                        example.Product = worksheet.Cells[row, 3].Value.ToString().Trim();
                        example.DiscountBrand = worksheet.Cells[row, 4].Value.ToString().Trim();
                        example.UnitsSold = double.Parse(worksheet.Cells[row, 5].Value.ToString().Trim());
                        example.Manifactur = double.Parse(worksheet.Cells[row, 6].Value.ToString().Trim());
                        example.SalePrice = double.Parse(worksheet.Cells[row, 7].Value.ToString().Trim());
                        example.GrossSales = double.Parse(worksheet.Cells[row, 8].Value.ToString().Trim());
                        example.Discounts = double.Parse(worksheet.Cells[row, 9].Value.ToString().Trim());
                        example.Sales = double.Parse(worksheet.Cells[row, 10].Value.ToString().Trim());
                        example.COGS = double.Parse(worksheet.Cells[row, 11].Value.ToString().Trim());
                        example.Profit = double.Parse(worksheet.Cells[row, 12].Value.ToString().Trim());
                        example.Date = DateTime.Parse(worksheet.Cells[row, 13].Value.ToString().Trim());

                        await _con.AddAsync(example);
                    }

                }
                await _con.SaveChangesAsync(true);

            }
            return StatusCode(201, "datas saved to sql");
        }
        /// <summary>
        /// Get Methods
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task< IActionResult> SendRepo([FromQuery] SendFilterDto filter, [FromQuery] SendType type)
        {
            List<ReturnDataDto> dataList = new List<ReturnDataDto>();
            var datas = await _con.Examples.Where(e => e.Date <= filter.EndData && e.Date >= filter.StartData).AsQueryable().AsNoTracking().ToListAsync();
            switch (type)
            {
                case SendType.Segment:

                   dataList= datas.GroupBy(d => d.Segment)
                        .Select(data => new ReturnDataDto
                        {
                            Name = data.Key,
                            Count = data.Key.Count(),
                            totalProfits = data.Sum(x => x.Profit),
                            totalDiscounts = data.Sum(x => x.Discounts),
                            totalSales = data.Sum(x => x.Sales),
                        }).ToList();
                    break;
                case SendType.Country:
                  dataList = datas.GroupBy(d => d.Country)
                        .Select(data => new ReturnDataDto
                        {
                            Name= data.Key,
                            Count = data.Key.Count(),
                            totalProfits = data.Sum(x => x.Profit),
                            totalDiscounts = data.Sum(x => x.Discounts),
                            totalSales = data.Sum(x => x.Sales),
                        }).ToList();
                    break;
                case SendType.Product:
                  dataList=  datas.GroupBy(d => d.Product)
                        .Select(data => new ReturnDataDto
                        {
                            Name=data.Key,
                            Count = data.Key.Count(),
                            totalProfits = data.Sum(x => x.Profit),
                            totalDiscounts = data.Sum(x => x.Discounts),
                            totalSales = data.Sum(x => x.Sales),
                        }).ToList();
                    break;
                case SendType.Discount:
                   
                    break;
                default:
                    break;
            }

            string fileName = Guid.NewGuid().ToString() + ".xlsx";

            var pathFolder = Path.Combine(_env.WebRootPath, fileName);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1").Cells[1, 1].LoadFromCollection(dataList, true);
                package.SaveAs(pathFolder);

                MemoryStream ms = new MemoryStream();

                using (var file = new FileStream(pathFolder, FileMode.Open, FileAccess.Read))
                {
                    var bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                    file.Close();
                    _eservice.SendEmail(filter.AcceptorEmail, "Salam", "Hesabatiniz", fileName, bytes);
                }

                return Ok("Sended");
            }

        }
    }
}
