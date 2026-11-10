namespace DemirbasZimmet.API.Exceptions;

public class DuplicateAssetCodeException : Exception
{
    public DuplicateAssetCodeException(string message) : base(message) { }
}