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
        float RealizarPrediccionScore(int v, int v1);
        Pelicula ObtenerPeliculaPorId(int id);
        void GuardarResena(int usuarioId, int peliculaId, double calificacion, string comentario,string Genero);
        List<Historial> ObtenerHistorialDeUsuario(int userId);
        void EliminarResena(int usuarioId, int peliculaId);


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
        public float RealizarPrediccionScore(int v, int v1)
        {
           return  _modelRecomender.UseModelForSinglePredictionScore(v, v1);
        }


        public Pelicula ObtenerPeliculaPorId(int id)
        {
            return _peliculaLogica.ObtenerPeliculaPorId(id);
        }


        public void GuardarResena(int usuarioId, int peliculaId, double calificacion, string comentario,string genero)
        {
            var existente = _context.Historials
                .FirstOrDefault(h => h.UsuarioId == usuarioId && h.PeliculaId == peliculaId);

            if (existente != null)
            {
                
                existente.Calificacion = calificacion;
                existente.Comentario = comentario;
                existente.FechaReseña = DateTime.Now;

            }
            else
            {
                
                var historial = new Historial
                {
                    UsuarioId = usuarioId,
                    PeliculaId = peliculaId,
                    Calificacion = calificacion,
                    Comentario = comentario,
                    FechaReseña = DateTime.Now,
                    IsCalificada = true,
                    Generos = genero
                };

                _context.Historials.Add(historial);
            }

            _context.SaveChanges();
        }

        public void EliminarResena(int usuarioId, int peliculaId)
        {
            var reseña = _context.Historials.FirstOrDefault(h =>
                h.UsuarioId == usuarioId && h.PeliculaId == peliculaId);

            if (reseña != null)
            {
                reseña.IsCalificada = false;
                reseña.Calificacion = 0;
                reseña.Comentario = null;
                reseña.FechaReseña = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public List<Historial> ObtenerHistorialDeUsuario(int userId)
        {
            return _context.Historials
                .Where(h => h.UsuarioId == userId && h.IsCalificada)
                .ToList();
        }


    }
}
