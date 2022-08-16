using ExcelToSql.Data;
using ExcelToSql.Data.Entites;
using ExcelToSql.DTOs;
using ExcelToSql.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExcelToSql.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly AppDbContext _con;
        private readonly IEmailService _eservice;

        public ExcelController(AppDbContext con)
        {
            _con = con;
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
                       
                        example.Segment = worksheet.Cells[row,1].Value.ToString().Trim();
                        example.Country = worksheet.Cells[row,2].Value.ToString().Trim();
                        example.Product=worksheet.Cells[row,3].Value.ToString().Trim();
                        example.DiscountBrand = worksheet.Cells[row,4].Value.ToString().Trim();
                        example.UnitsSold =double.Parse(worksheet.Cells[row, 5].Value.ToString().Trim());
                        example.Manifactur = double.Parse(worksheet.Cells[row, 6].Value.ToString().Trim());
                        example.SalePrice = double.Parse(worksheet.Cells[row, 7].Value.ToString().Trim());
                        example.GrossSales=double.Parse(worksheet.Cells[row, 8].Value.ToString().Trim());
                        example.Discounts=double.Parse(worksheet.Cells[row, 9].Value.ToString().Trim());
                        example.Sales=double.Parse(worksheet.Cells[row,10].Value.ToString().Trim());
                        example.COGS=double.Parse(worksheet.Cells[row,11].Value.ToString().Trim());
                        example.Profit=double.Parse(worksheet.Cells[row,12].Value.ToString().Trim());
                        example.Date=DateTime.Parse(worksheet.Cells[row,13].Value.ToString().Trim());

                        await _con.AddAsync(example);
                    }

                }
                await _con.SaveChangesAsync(true);

            }
            return StatusCode(201,"datas saved to sql");
        }

        [HttpGet]
        public async Task<IActionResult> SendReport([FromQuery]SendFilter filter,[FromQuery] SendType type) 
        {
            

            _eservice.SendEmail(filter.AcceptorEmail,"salam","hesabatiniz");


            return Ok();
        }
    }
}
