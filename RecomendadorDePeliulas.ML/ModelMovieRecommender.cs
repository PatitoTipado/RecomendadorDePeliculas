using Google.Protobuf.WellKnownTypes;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace RecomendadorDePeliulas.ML
{
    public class ModelMovieRecommender
    {
        /*
         * Los datos de las clasificaciones de recomendación se dividen en 
         * conjuntos de datos Train y Test.
         * Los datos Train se usan para ajustar el modelo, mientras que los 
         * datos Test sirven para realizar predicciones con el modelo 
         * entrenado y evaluar el rendimiento del modelo. Los datos Train y Test
         * suelen dividirse con una proporción de 80/20.
         */

        private readonly MLContext _mlContext;
        private ITransformer _model;
        public ModelMovieRecommender(MLContext mlContext)
        {
            var modelDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var modelPath = Path.Combine(modelDirectory, "Data", "MovieRecommenderModel.zip");

            _mlContext = mlContext;

            if (!(System.IO.File.Exists(modelPath)))
            {
                BuildAndSaveModel();
            }
            else
            {
                _model = Load();
            }
        }
        //para cargar los datos del csv actualizado con los ratings para que entrene y ademas que guarde,
        //lo necesitaremos usar en 2 partesitas mas por eso tiene tanto este metodo
        public void BuildAndSaveModel()
        {
            (IDataView trainingDataView, IDataView testDataView) = LoadData();
            _model = BuildAndTrainModel(trainingDataView);
            SaveModel(trainingDataView.Schema, _model);
        }

        //solo una vez se ejecuta con la data cargada luego podre usar las dos interfaces 
        //hechas para otros metodos
        private (IDataView training, IDataView test) LoadData()
        {
            var rootPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var dataPath = Path.Combine(rootPath, "Data", "data-ratings.csv");

            // Cargar los datos desde el archivo CSV
            IDataView dataView = _mlContext.Data.LoadFromTextFile<MovieRating>(
                dataPath,
                hasHeader: true,
                separatorChar: ','
            );

            // Dividir los datos en entrenamiento (80%) y prueba (20%)
            var splitData = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            return (splitData.TrainSet, splitData.TestSet);
        }


        //primeravez que entreno el modelo 
        private ITransformer BuildAndTrainModel(IDataView trainingDataView)
        {
            IEstimator<ITransformer> estimator = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "userId")
    .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "movieIdEncoded", inputColumnName: "movieId"));
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "movieIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var trainerEstimator = estimator.Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

            //Console.WriteLine("=============== Training the model ===============");
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            return model;
        }
        //es una prueba que se realiza al modelo para saber si esta todo bien
        public void EvaluateModel(IDataView testDataView)
        {
            //Console.WriteLine("=============== Evaluating the model ===============");
            var prediction = _model.Transform(testDataView);
            var metrics = _mlContext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");
            //Console.WriteLine("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
            // Console.WriteLine("RSquared: " + metrics.RSquared.ToString());
        }
        //aca es la iteracion que va preguntando cual podria gustarle al usuario
        public MovieRating UseModelForSinglePrediction(int userId,int movieId)
        {
            //Console.WriteLine("=============== Making a prediction ===============");
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(_model);
            var testInput = new MovieRating { userId = userId, movieId = movieId };

            var movieRatingPrediction = predictionEngine.Predict(testInput);
            if (Math.Round(movieRatingPrediction.Score, 1) > 3.5)
            {
                return testInput;
            }
            else
            {
                return null;
            }

        }

        /*public List<MovieRating> GetTopRecommendations(int userId, List<int> candidateMovieIds)
        {
            Console.WriteLine("=============== Generando recomendaciones ===============");

            var predictions = candidateMovieIds
                .Select(movieId => new
                {
                    MovieId = movieId,
                    Score = _mlContext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(_model)
                             .Predict(new MovieRating { userId = userId, movieId = movieId }).Score
                })
                .Where(p => Math.Round(p.Score, 1) > 3.5) // Filtrar por un umbral de puntuación
                .OrderByDescending(p => p.Score)         // Ordenar por mejor puntuación
                .Select(p => new MovieRating { userId = userId, movieId = p.MovieId }) // Proyectar a MovieRating
                .ToList();

            return predictions;
        }*/

        //en un futuro mejorar
        public List<MovieRating> getPredictionsFor(int userId,int quantity)
        {

            List<MovieRating> predictions = new List<MovieRating>();
            int count = 1;
            while (predictions.Count() < quantity)
            {
                if (UseModelForSinglePrediction(userId,count)!=null)
                {
                    predictions.Add(UseModelForSinglePrediction(userId, count));
                    count++;
                }
            }

            return predictions;
        }

        public void insertRatingOnModel(float userId,float movieId,float rating)
        {

            var rootPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var dataPath = Path.Combine(rootPath, "Data", "data-ratings.csv");

            using (var writer = new StreamWriter(dataPath, true)) // Guardar nuevos ratings
            {
                writer.WriteLine($"{userId},{movieId},{rating}{DateTime.Now}");
            }
        }

        public void SaveModel(DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            var modelDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var modelPath = Path.Combine(modelDirectory, "Data", "MovieRecommenderModel.zip");

            Directory.CreateDirectory(modelDirectory);

            //Console.WriteLine("=============== Saving the model to a file ===============");
            _mlContext.Model.Save(model, trainingDataViewSchema, modelPath);
        }

        public ITransformer Load()
        {
            var modelDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            return _mlContext.Model.Load(Path.Combine(modelDirectory, "Data", "MovieRecommenderModel.zip"), out var inputSchema);
        }

    }

}
