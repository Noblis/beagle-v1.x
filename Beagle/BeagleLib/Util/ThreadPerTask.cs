public class ThreadPerTask : TaskScheduler
{
    protected override void QueueTask(Task task)
    {
        new Thread(() => this.TryExecuteTask(task))
        {
            IsBackground = true
        }.Start();
    }

    protected override bool TryExecuteTaskInline(Task task,
        bool taskWasPreviouslyQueued) => false;

    protected override IEnumerable<Task> GetScheduledTasks() { yield break; }
}