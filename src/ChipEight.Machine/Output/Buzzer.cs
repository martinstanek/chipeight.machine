namespace ChipEight.Machine.Output;

public sealed class Buzzer
{
    private readonly IRemoteBuzzer _remoteBuzzer;
    
    public Buzzer()
    {
        _remoteBuzzer = new NullBuzzer();
    }

    public Buzzer(IRemoteBuzzer remoteBuzzer)
    {
        _remoteBuzzer = remoteBuzzer;
    }
    
    public void On()
    {
        if (IsOn)
        {
            return;
        }

        _remoteBuzzer.On();
        IsOn = true;
    }

    public void Off()
    {
        if (!IsOn)
        {
            return;
        }

        _remoteBuzzer.Off();
        IsOn = false;
    }
    
    public bool IsOn { get; private set; }
}