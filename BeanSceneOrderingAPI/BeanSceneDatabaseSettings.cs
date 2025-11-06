namespace BeanSceneOrderingAPI
{
    /// <summary>
    /// The model containing the connection string and name of the MongoDB database.
    /// </summary>
    public class BeanSceneDatabaseSettings
    {
        /// <summary>
        /// Connection string to the MongoDB database.
        /// </summary>
        public string ConnectionString { get; set; } = null!;
        /// <summary>
        /// Name of the database.
        /// </summary>
        public string DatabaseName { get; set; } = null!;
    }
}
