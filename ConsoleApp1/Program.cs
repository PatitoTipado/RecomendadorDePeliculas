

using Microsoft.ML;
using RecomendadorDePeliulas.ML;

MLContext mlContext = new MLContext();
ModelMovieRecommender recomender = new ModelMovieRecommender(mlContext);

recomender.insertRatingOnModel(162535, 198609,3);

