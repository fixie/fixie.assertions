using static System.Environment;

namespace Fixie.Assertions;

public class SerializationDepthException(string interruptedOutput)
    : Exception(Introduction + NewLine + NewLine + interruptedOutput)
{
    const string Introduction =
        "A value could not be serialized because its object graph is too deep. " +
        "Below is the start of the message that was interrupted:";
}