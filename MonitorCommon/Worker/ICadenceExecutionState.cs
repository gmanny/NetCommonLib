namespace MonitorCommon.Worker;

public interface ICadenceExecutionState
{
    void Started();

    void Done();
}