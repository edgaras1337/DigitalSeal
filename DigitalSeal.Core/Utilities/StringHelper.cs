namespace DigitalSeal.Core.Utilities
{
    /// <summary>
    /// Helper class for managing strings.
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// Gets the name of the controller, without the "Controller" suffix.
        /// </summary>
        /// <typeparam name="T">Type of the controller class.</typeparam>
        /// <returns>Name of the controller.</returns>
        public static string ControllerName<T>() => ControllerName(typeof(T).Name);

        /// <summary>
        /// Gets the name of the controller, without the "Controller" suffix.
        /// </summary>
        /// <param name="name">Name of the controller class.</param>
        /// <returns>Name of the controller.</returns>
        public static string ControllerName(string name) => name.Replace("Controller", string.Empty);
    }
}
