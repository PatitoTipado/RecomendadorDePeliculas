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
        List<Pelicula> ObtenerPeliculasACalificarQueNoCalificoAntes(int userId, params string[] preferencias);
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
        public List<Pelicula> ObtenerPeliculasACalificarQueNoCalificoAntes(int userId, params string[] preferencias)
        {
            var reseñasUsuario = _context.Historials
                .Where(h => h.UsuarioId == userId)
                .ToList();

            List<int> excluir = reseñasUsuario.Select(r => r.PeliculaId).ToList();

            if (excluir.Count > 0)
            {
                return _peliculaLogica.obtenerPeliculas(excluir, preferencias);
            }

            return _peliculaLogica.obtenerPeliculas(preferencias);
        }

        public void RealizarPrediccion(int v, int v1)
        {
            _modelRecomender.UseModelForSinglePrediction(v, v1);
        }
    }
}
