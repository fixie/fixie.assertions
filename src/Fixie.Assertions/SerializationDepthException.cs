using static System.Environment;

namespace Fixie.Assertions;

public class SerializationDepthException : Exception
{
    const string Introduction =
        "A value could not be serialized because its object graph is too deep. " +
        "Below is the start of the message that was interrupted:";

    public SerializationDepthException(string interruptedOutput)
        : base(Introduction + NewLine + NewLine + interruptedOutput)
    {
    }
}