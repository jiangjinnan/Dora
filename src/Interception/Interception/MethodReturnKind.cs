namespace Dora.Interception
{
    public enum MethodReturnKind
    {
        Void,
        Result,
        Task,
        TaskOfResult,
        ValueTask,
        ValueTaskOfResult
    }
}
