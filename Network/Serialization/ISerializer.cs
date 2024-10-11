﻿namespace Network.Serialization
{
    public interface ISerializer
    {
        /// <summary>
        /// Given an object of a generic type, the method converts it into a serialized string and returns it
        /// </summary>
        /// <param name="genericObject">
        /// The object to be serialized
        /// </param>
        /// <returns>
        /// The serialized string of the generic object
        /// </returns>
        public string Serialize<T> (T genericObject);

        /// <summary>
        /// Given a serialized string, the method converts it into the original object and returns it
        /// </summary>
        /// <param name="serializedString">
        /// The string to be deserialized
        /// </param>
        /// <returns>
        /// The original object after deserializing the string
        /// </returns>
        public T Deserialize<T> (string serializedString);
    }
}
