namespace DemirbasZimmet.API.Exceptions;

public class AssetHasActiveAssignmentException : Exception
{
    public AssetHasActiveAssignmentException(string message) : base(message) { }
}