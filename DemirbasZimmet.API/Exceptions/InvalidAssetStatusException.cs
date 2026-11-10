namespace DemirbasZimmet.API.Exceptions;

public class InvalidAssetStatusException : Exception
{
    public InvalidAssetStatusException(string message) : base(message) { }
}