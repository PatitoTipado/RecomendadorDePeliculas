

using Microsoft.ML;
using RecomendadorDePeliulas.ML;

MLContext mlContext = new MLContext();
ModelMovieRecommender recomender = new ModelMovieRecommender(mlContext);

