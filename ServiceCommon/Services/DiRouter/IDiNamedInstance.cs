namespace Monitor.ServiceCommon.Services.DiRouter;

public interface IDiNamedInstance<K> where K : notnull
{
    K DiName { get; }
}