using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.V2.Controllers
{
    [ApiController]
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiVersion("2.0")]

    public class PalavrasController : ControllerBase
    {
        /// <summary>
        /// Operação que pega do banco de dados todas as palavras existentes.
        /// </summary>
        /// <param name="query">Filtro de pesquisa</param>
        /// <returns>Listagem de palavras </returns>
        [HttpGet("",Name ="ObterTodas")]
        public String Todas()
        {
           
            return "Versão 2.0";
        }

    }
}
