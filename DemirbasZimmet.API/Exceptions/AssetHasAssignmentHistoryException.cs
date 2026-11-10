namespace DemirbasZimmet.API.Exceptions;

public class AssetHasAssignmentHistoryException : Exception
{
    public AssetHasAssignmentHistoryException(string message) : base(message) { }
}