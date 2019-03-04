using System;
using System.Runtime.Serialization;

namespace RayCarrot.RCP.Metro
{
    [Serializable]
    public class SoundtrackUnplaybaleException : Exception
    {
        public SoundtrackUnplaybaleException() { }

        public SoundtrackUnplaybaleException(string message) : base(message) { }

        public SoundtrackUnplaybaleException(string message, Exception inner) : base(message, inner) { }

        protected SoundtrackUnplaybaleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}