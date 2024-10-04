namespace Tests;

class ContradictionException(string expected, string actual, string message)
    : Exception(message)
{
    public string Expected => expected;
    public string Actual => actual;
}