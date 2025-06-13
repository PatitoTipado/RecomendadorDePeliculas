using CsvHelper;
using CsvHelper.Configuration;
using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliulas.ML;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RecomendadorDePeliculas.Logica
{
    public interface IRecomenderLogica
    {
        List<Pelicula> ObtenerPeliculasACalificarQueNoCalificoAntes(int userId, string preferencia, string preferenciaSecundaria);
        void RealizarPrediccion(int v, int v1);
    }
    public class RecomenderLogica : IRecomenderLogica
    {
        private IModelMovieRecomender _modelRecomender;
        private readonly RecomendadorPeliculasContext _context;
        private IPeliculasLogica _peliculaLogica;

        public RecomenderLogica(IModelMovieRecomender model, RecomendadorPeliculasContext context, IPeliculasLogica peliculasLogica)
        {
            _peliculaLogica= peliculasLogica;
            _modelRecomender=model;
            _context = context;
        }
        public List<Pelicula> ObtenerPeliculasACalificarQueNoCalificoAntes(int userId,string preferencia, string preferenciaSecundaria)
        {

            var reseñasUsuario = _context.Historials
                .Where(h => h.UsuarioId == userId)
                .ToList();

            if (reseñasUsuario.Count>0)
            {
                List<int> excluir = new List<int>();
                foreach (var historial in reseñasUsuario)
                {
                    excluir.Add(historial.PeliculaId);
                }

                return _peliculaLogica.obtenerPeliculas(excluir, preferencia, preferenciaSecundaria);
            }

            return _peliculaLogica.obtenerPeliculas(preferencia, preferenciaSecundaria); ;
        }

        public void RealizarPrediccion(int v, int v1)
        {
            _modelRecomender.UseModelForSinglePrediction(v, v1);
        }
    }
}
