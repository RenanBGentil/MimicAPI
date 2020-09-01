using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.DataBases;
using MimicAPI.V1.Models.DTO;
using MimicAPI.Helpers;
using MimicAPI.V1.Models;
using MimicAPI.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.V1.Controllers
{
    [ApiController]
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.1")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Operação que pega do banco de dados todas as palavras existentes.
        /// </summary>
        /// <param name="query">Filtro de pesquisa</param>
        /// <returns>Listagem de palavras </returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult Todas ([FromQuery]PalavraUrlQuery query)
        {

            var item = _repository.ObterPalavras(query);

            if (item.Results.Count == 0)
                return NotFound();

            PaginationList<PalavraDTO> lista = CriarLinksListPalavrasDTO(query, item);
            return Ok(lista);
        }

        
        /// <summary>
        /// Operção que pega uma única palavra da base de dados
        /// </summary>
        /// <param name="Id">Código identificador da palavra</param>
        /// <returns>Um objeto de palavra </returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter (int Id){

            var obj = _repository.Obter(Id);

            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);
            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new {Id = palavraDTO.Id }), "GET")
               
                );

            palavraDTO.Links.Add(
                new LinkDTO("update", Url.Link("AtualizarPalavra", new { Id = palavraDTO.Id}), "PUT")
               
                );

            palavraDTO.Links.Add(
                new LinkDTO("delete",Url.Link("ExcluirPalavra", new { Id = palavraDTO.Id}), "DELETE")
               
                );

                return Ok (palavraDTO);
        }

        /// <summary>
        /// Operação que realiza o cadastro da palavra
        /// </summary>
        /// <param name="palavra">Um objeto palavra</param>
        /// <returns>Um objeto palavra com o seu Id</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpPost("", Name = "Cadastrar")]
        public ActionResult Cadastrar ([FromBody]Palavra palavra){

            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return  UnprocessableEntity(ModelState);


            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;
            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);

            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new { Id = palavraDTO.Id }), "GET")

                );

            return Created($"api/palavras/{palavra.Id}",palavraDTO);
        }

        /// <summary>
        /// Operação que realiza a substituição de uma palavra
        /// </summary>
        /// <param name="Id">Código identificador da palavra </param>
        /// <param name="palavra">Objeto palavra com dados para atualização </param>
        /// <returns></returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpPut("{id}",Name = "AtualizarPalavra")]
        public ActionResult Atualizar (int Id, [FromBody]Palavra palavra){

            var obj = _repository.Obter(Id);

            if (obj == null)
                return NotFound();

            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Id = Id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;
            _repository.Atualizar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new { Id = palavraDTO.Id }), "GET")

                );


            return Ok();
        }

        /// <summary>
        /// Operação que desativa uma palavra do sistema
        /// </summary>
        /// <param name="Id">Código identificador da palavra</param>
        /// <returns></returns>
        [MapToApiVersion("1.1")]
        [HttpDelete("{id}", Name = "ExcluirPalavra")]
        public ActionResult Delete (int Id){

            var palavra = _repository.Obter(Id);

            if (palavra == null)
                return NotFound();

            _repository.Deletar(Id);
        

            return NoContent();
        }
        private PaginationList<PalavraDTO> CriarLinksListPalavrasDTO(PalavraUrlQuery query, PaginationList<Palavra> item)
        {
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            foreach (var palavra in lista.Results)
            {
                palavra.Links = new List<LinkDTO>();
                palavra.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { Id = palavra.Id }), "GET"));
            }

            lista.Links.Add(new LinkDTO("self", Url.Link("ObterTodas", query), "GET"));

            if (item.Paginacao != null)
            {
                Response.Headers.Add("X-Registration", JsonConvert.SerializeObject(item.Paginacao));

                if (query.PagNumero + 1 <= item.Paginacao.TotalPaginas)
                {

                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }

                if (query.PagNumero - 1 > 0)
                {

                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }
            }

            return lista;
        }
    }
}