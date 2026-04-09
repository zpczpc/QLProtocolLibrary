namespace QLProtocolLibrary
{
    public static class QlProtocolKnownRouter
    {
        public static bool TryParse(QlProtocolFrame frame, out QlKnownParseResult? result)
        {
            foreach (IQlKnownOperation operation in QlKnownOperations.All)
            {
                if (operation.TryParse(frame, out result) && result != null)
                {
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
