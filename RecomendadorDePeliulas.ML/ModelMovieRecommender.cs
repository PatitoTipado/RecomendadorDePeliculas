using Google.Protobuf.WellKnownTypes;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace RecomendadorDePeliulas.ML
{
    public interface IModelMovieRecomender
    {
        public void BuildAndSaveModel();
        public (IDataView training, IDataView test) LoadData();
        public ITransformer BuildAndTrainModel(IDataView trainingDataView);
        public void EvaluateModel(IDataView testDataView);
        public MovieRating UseModelForSinglePrediction(int userId, int movieId);
        public List<MovieRating> getPredictionsFor(int userId, int quantity);
        public void insertRatingOnModel(float userId, float movieId, float rating);
        public void SaveModel(DataViewSchema trainingDataViewSchema, ITransformer model);
        public ITransformer Load();
    }

    public class ModelMovieRecommender: IModelMovieRecomender
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private string _dataPath;
        private string _modelPath;

        public ModelMovieRecommender(MLContext mlContext, string modelPath,string dataPath)
        {
            _mlContext = mlContext;
            _dataPath = dataPath;
            _modelPath = modelPath;

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
        public (IDataView training, IDataView test) LoadData()
        {
            // Cargar los datos desde el archivo CSV
            IDataView dataView = _mlContext.Data.LoadFromTextFile<MovieRating>(
                _dataPath,
                hasHeader: true,
                separatorChar: ','
            );

            // Dividir los datos en entrenamiento (80%) y prueba (20%)
            var splitData = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            return (splitData.TrainSet, splitData.TestSet);
        }


        //primeravez que entreno el modelo 
        public ITransformer BuildAndTrainModel(IDataView trainingDataView)
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
            Console.WriteLine("=============== Making a prediction ===============");
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(_model);
            var testInput = new MovieRating { userId = userId, movieId = movieId };

            var movieRatingPrediction = predictionEngine.Predict(testInput);
            if (Math.Round(movieRatingPrediction.Score, 1) > 3.5)
            {
                Console.WriteLine("recomendadisima");
                return testInput;
            }
            else
            {
                Console.WriteLine("no");

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
            using (var writer = new StreamWriter(_dataPath, true)) // Guardar nuevos ratings
            {
                writer.WriteLine($"{userId},{movieId},{rating}{DateTime.Now}");
            }
        }

        public void SaveModel(DataViewSchema trainingDataViewSchema, ITransformer model)
        {

            Directory.CreateDirectory(_modelPath);

            //Console.WriteLine("=============== Saving the model to a file ===============");
            _mlContext.Model.Save(model, trainingDataViewSchema, _modelPath);
        }

        public ITransformer Load()
        {

            return _mlContext.Model.Load(Path.Combine(_modelPath), out var inputSchema);
        }

    }

}
