namespace DigitalQueueManagement.ViewModel;

public abstract class QueuePageViewModel
{
    protected QueuePageViewModel(QueueSessionViewModel queueSession) =>
        QueueSession = queueSession;

    public QueueSessionViewModel QueueSession { get; }
}
