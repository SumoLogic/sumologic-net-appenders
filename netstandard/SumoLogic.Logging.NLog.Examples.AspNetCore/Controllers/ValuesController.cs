using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace SumoLogic.Logging.NLog.Examples.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Logger.Debug("GET api/values was called");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            Logger.Debug($"GET api/values/{id} was called");

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
            Logger.Debug($"POST api/values/ with value {value} was called");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            Logger.Debug($"PUT api/values/{id} with value {value} was called");
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Logger.Debug($"DELETE api/values/{id} was called");
        }
    }
}
