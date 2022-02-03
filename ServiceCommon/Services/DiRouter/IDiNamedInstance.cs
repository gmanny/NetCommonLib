namespace Monitor.ServiceCommon.Services.DiRouter;

public interface IDiNamedInstance<K>
{
    K DiName { get; }
}