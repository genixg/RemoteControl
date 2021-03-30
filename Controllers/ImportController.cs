using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteControl.Models;

namespace RemoteControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly KGNGPEmployeesContext _context;

        public ImportController(KGNGPEmployeesContext context)
        {
            _context = context;
        }

        [FromQuery]
        public string type { get; set; }

        [HttpPost]
        public IActionResult AddFile(IFormFileCollection filestruct)
        {
            switch (type)
            {
                case "employees":
                    foreach (var file in filestruct)
                    {
                        XMLDocumentEmployees result;
                        using (var fileStream = file.OpenReadStream())
                        {
                            try
                            {
                                XmlSerializer xml = new XmlSerializer(typeof(XMLDocumentEmployees));
                                result = (XMLDocumentEmployees)xml.Deserialize(fileStream);
                            }
                            catch(Exception ex)
                            {
                                return BadRequest(ex.Message);
                            }
                        }
                        
                    }

                    break;
                default:
                    throw new Exception("Неизвестный тип импорта");
            }

            return RedirectToAction("Index");
        }
    }
}
