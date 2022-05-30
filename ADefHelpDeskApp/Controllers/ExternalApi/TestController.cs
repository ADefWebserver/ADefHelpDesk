﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazorFileUploadSwagger.Controllers
{
    /// <summary>
    /// An Test controller for testing <code>multipart/form-data</code> submission
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        public TestController()
        {

        }

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="form">A form</param>
        /// <returns></returns>
        [HttpPost]
        public async void TestAPI([FromForm] TestForm form)
        {
            var id = form;
            await Task.Delay(1500);
        }

        /// <summary>
        /// View the form submission result
        /// </summary>
        public class TestForm
        {
            /// <summary>
            /// FormId
            /// </summary>
            public int FormId { get; set; }
        }
    }
}